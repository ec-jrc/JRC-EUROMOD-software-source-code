__license__='''
Copyright 2024 European Commission
*
Licensed under the EUPL, Version 1.2;
You may not use this work except in compliance with the Licence.
You may obtain a copy of the Licence at:

*
   https://joinup.ec.europa.eu/software/page/eupl
*

Unless required by applicable law or agreed to in writing, software distributed under the Licence is distributed on an "AS IS" basis,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the Licence for the specific language governing permissions and limitations under the Licence.
'''

import os
import re
import json
import math
import numpy as np
import pandas as pd
import polars as pl
import clr
import System as SystemCs
from .utils._paths import DLL_PATH
from .utils.clr_array_convert import asNetArray
from .utils.utils import convert_numeric_columns_to_float64
from .container import Container

# Load EM_Statistics.dll via CLR
_EM_STATISTICS_DLL = os.path.join(DLL_PATH, "EM_Statistics.dll")
if not os.path.exists(_EM_STATISTICS_DLL):
    raise ImportError(
        f"[Statistics] Required DLL not found: {_EM_STATISTICS_DLL}\n"
        f"  Expected EM_Statistics.dll in: {DLL_PATH}"
    )
clr.AddReference(_EM_STATISTICS_DLL)

from EM_Statistics import XML_handling, Template, EM_TemplateCalculator, ErrorCollector, HardDefinitions
from EM_Statistics.ExternalStatistics import ExternalStatistic, ExternalStatisticAggregate, ExternalStatisticDistributional, ExternalStatisticAggregateValues

# ExportHandling (Excel export) depends on System.Drawing/EPPlus and is Windows-only;
# it is excluded from the cross-platform EM_Statistics build. Import it lazily so that
# statistics *computation* still works everywhere - only to_excel() needs it.
try:
    from EM_Statistics import ExportHandling as _ExportHandling
except ImportError:
    _ExportHandling = None

# CLR type for a 2D double array (double[,]). PrepareFromData expects
# List<double[,]>; a List<System.Array> is not convertible to it (generics are
# invariant), so the list must be built with this exact element type.
_DOUBLE_2D_TYPE = clr.GetClrType(SystemCs.Double).MakeArrayType(2)


class Statistics:
    """Calculates statistics from EUROMOD simulation output.

    Parameters
    ----------
    template_path : str
        Path to the template XML file.
    variable : str, optional
        Variable name for Variable-type templates.

    Raises
    ------
    FileNotFoundError
        If the template file does not exist.
    ValueError
        If the template XML cannot be parsed.
    """

    def __init__(self, template_path: str, variable: str = None):
        if not template_path:
            raise ValueError(
                "[Statistics] Template path must not be empty."
            )

        if not os.path.isfile(template_path):
            raise FileNotFoundError(
                f"[Statistics] Template file not found: {template_path}"
            )

        # Parse template XML via CLR
        success, self._template, error_collector = self._parse_template(template_path)

        if error_collector.HasErrors():
            raise ValueError(
                f"[Statistics] Failed to parse template '{template_path}': "
                f"{error_collector.GetErrorMessage()}"
            )

        if not success:
            raise ValueError(
                f"[Statistics] Failed to parse template '{template_path}'."
            )

        self._template_path = template_path
        self._variable = variable

        # If a variable is specified, configure the template for Variable-type usage
        if variable is not None and self._template.info.userVariables is not None:
            for uv in self._template.info.userVariables:
                if uv.inputType == HardDefinitions.UserInputType.VariableName:
                    uv.value = variable
                    break

    @staticmethod
    def _parse_template(template_path: str):
        """Parse a template XML file via the CLR XML_handling class.

        In C#, the signature is:
            bool ParseTemplate(string path, out Template template, out ErrorCollector errorCollector, bool readFromString = false)

        pythonnet returns out parameters as additional tuple values.

        Parameters
        ----------
        template_path : str
            Path to the template XML file.

        Returns
        -------
        tuple[bool, Template, ErrorCollector]
            Success flag, parsed template, and error collector.
        """
        result = XML_handling.ParseTemplate(template_path)
        # pythonnet returns (bool success, Template template, ErrorCollector errorCollector)
        return result

    def calculate(self, baseline, reforms=None, pages=None, tables=None):
        """Calculate statistics from simulation output.

        Parameters
        ----------
        baseline : Simulation or pandas.DataFrame or polars.DataFrame
            Baseline simulation output. A DataFrame must contain the variables
            required by the template (including idperson, idhh, dwt and dag).
            Note: passing a DataFrame skips the keep_clr_data zero-copy fast path,
            so the data is converted to CLR arrays on each call.
        reforms : list of Simulation or pandas.DataFrame or polars.DataFrame, optional
            Reform outputs for comparison templates. Entries may be Simulations or
            DataFrames and may be mixed with the baseline type.
        pages : list[str], optional
            If given, only these template pages are calculated (partial execution);
            the rest are skipped, which can be much faster. Use :meth:`list_pages`
            to discover names. Global prerequisite actions always run, but a kept
            page that depends on a skipped one may fail.
        tables : list[str], optional
            If given, only these tables are calculated. Can be combined with `pages`.
            Use :meth:`list_tables` to discover names.

        Returns
        -------
        StatisticsResult
            Calculation results with multiple access patterns.

        Raises
        ------
        ValueError
            If required variables are missing from the simulation output,
            or if template type requires reforms but none are provided.
        RuntimeError
            If the EM_TemplateCalculator reports errors during preparation
            or calculation.
        """
        template_type = self._template.info.templateType

        # Validate template type vs provided arguments
        if template_type == HardDefinitions.TemplateType.BaselineReform:
            if reforms is None or len(reforms) == 0:
                raise ValueError(
                    "[Statistics] BaselineReform template requires at least one reform Simulation."
                )
        if template_type == HardDefinitions.TemplateType.Multi:
            if reforms is None or len(reforms) == 0:
                raise ValueError(
                    "[Statistics] Multi template requires at least one additional Simulation "
                    "passed via the 'reforms' parameter."
                )

        # Validate variable exists in simulation columns for Variable-type templates
        if self._variable is not None:
            self._validate_variable_in_simulation(baseline)

        # Create .NET List types for the C# method parameters
        ListStr = SystemCs.Collections.Generic.List[SystemCs.String]
        ListListStr = SystemCs.Collections.Generic.List[ListStr]
        ListDoubleArray = SystemCs.Collections.Generic.List[_DOUBLE_2D_TYPE]

        # Build the data lists based on template type
        all_variable_names = ListListStr()
        all_data = ListDoubleArray()
        system_names = ListStr()

        if template_type == HardDefinitions.TemplateType.Default:
            # Default: single baseline system
            var_names, data_array = _simulation_to_clr_data(baseline)
            self._validate_required_variables(var_names)

            var_name_list = ListStr()
            for name in var_names:
                var_name_list.Add(name)
            all_variable_names.Add(var_name_list)
            all_data.Add(data_array)
            system_names.Add("Baseline")

        elif template_type == HardDefinitions.TemplateType.BaselineReform:
            # BaselineReform: baseline first, then reforms
            baseline_vars, baseline_data = _simulation_to_clr_data(baseline)
            self._validate_required_variables(baseline_vars)

            # Get baseline observation count for validation
            baseline_num_obs = _get_observation_count(baseline)

            # Add baseline
            baseline_var_list = ListStr()
            for name in baseline_vars:
                baseline_var_list.Add(name)
            all_variable_names.Add(baseline_var_list)
            all_data.Add(baseline_data)
            system_names.Add("Baseline")

            # Add each reform
            for i, reform in enumerate(reforms):
                reform_vars, reform_data = _simulation_to_clr_data(reform)
                reform_num_obs = _get_observation_count(reform)

                # Validate observation count matches baseline
                if reform_num_obs != baseline_num_obs:
                    raise ValueError(
                        f"[Statistics] Observation count mismatch: baseline has "
                        f"{baseline_num_obs} observations but reform {i} has "
                        f"{reform_num_obs} observations. "
                        f"All reforms must have the same number of observations as the baseline."
                    )

                reform_var_list = ListStr()
                for name in reform_vars:
                    reform_var_list.Add(name)
                all_variable_names.Add(reform_var_list)
                all_data.Add(reform_data)

                # Use output_filenames as the system name if available
                reform_name = _get_simulation_name(reform, default=f"Reform_{i}")
                system_names.Add(reform_name)

        elif template_type == HardDefinitions.TemplateType.Multi:
            # Multi: all simulations are treated as separate baseline systems
            baseline_vars, baseline_data = _simulation_to_clr_data(baseline)
            self._validate_required_variables(baseline_vars)

            # Add baseline as first system
            baseline_var_list = ListStr()
            for name in baseline_vars:
                baseline_var_list.Add(name)
            all_variable_names.Add(baseline_var_list)
            all_data.Add(baseline_data)

            baseline_name = _get_simulation_name(baseline, default="System_0")
            system_names.Add(baseline_name)

            # Add each additional system
            for i, sim in enumerate(reforms):
                sim_vars, sim_data = _simulation_to_clr_data(sim)

                sim_var_list = ListStr()
                for name in sim_vars:
                    sim_var_list.Add(name)
                all_variable_names.Add(sim_var_list)
                all_data.Add(sim_data)

                sim_name = _get_simulation_name(sim, default=f"System_{i + 1}")
                system_names.Add(sim_name)

        else:
            # Default/fallback: single system
            var_names, data_array = _simulation_to_clr_data(baseline)
            self._validate_required_variables(var_names)

            var_name_list = ListStr()
            for name in var_names:
                var_name_list.Add(name)
            all_variable_names.Add(var_name_list)
            all_data.Add(data_array)
            system_names.Add("Baseline")

        # Optionally restrict the calculation to a subset of pages/tables (partial
        # execution). KeepOnly snapshots the prior active state; RestoreActive (in the
        # finally below) puts it back, so the reused template is never left restricted.
        restricted = bool(pages) or bool(tables)
        if restricted:
            page_list = ListStr()
            for p in (pages or []):
                page_list.Add(p)
            table_list = ListStr()
            for t in (tables or []):
                table_list.Add(t)
            unmatched = self._template.KeepOnly(page_list, table_list)
            if unmatched is not None and len(unmatched) > 0:
                import warnings as _warnings
                _warnings.warn(
                    "[Statistics] These page/table names were not found and ignored: "
                    + ", ".join(str(u) for u in unmatched)
                )

        try:
            # Create EM_TemplateCalculator with the parsed template
            calculator = EM_TemplateCalculator(self._template)

            # Call PrepareFromData - out parameter returned as additional tuple value
            prepare_result = calculator.PrepareFromData(
                all_variable_names, all_data, system_names
            )

            # pythonnet returns (bool success, ErrorCollector errorCollector) for out param
            if isinstance(prepare_result, tuple):
                prepare_success = prepare_result[0]
                error_collector = prepare_result[1]
            else:
                prepare_success = prepare_result
                error_collector = None

            # Check for errors during preparation
            if not prepare_success:
                error_msg = ""
                if error_collector is not None and error_collector.HasErrors():
                    error_msg = error_collector.GetErrorMessage()
                # Check if the error mentions missing variables
                if "required variable" in error_msg.lower():
                    raise ValueError(
                        f"[Statistics] Missing required variables in simulation output: {error_msg}"
                    )
                raise RuntimeError(
                    f"[Statistics] PrepareFromData failed: {error_msg}"
                )

            # Call CalculateStatisticsFromPreparedData
            calc_success = calculator.CalculateStatisticsFromPreparedData()

            if not calc_success:
                error_msg = ""
                if error_collector is not None and error_collector.HasErrors():
                    error_msg = error_collector.GetErrorMessage()
                raise RuntimeError(
                    f"[Statistics] CalculateStatisticsFromPreparedData failed: {error_msg}"
                )

            # Collect errors and warnings from the ErrorCollector
            errors = []
            warnings = []
            if error_collector is not None:
                msg = error_collector.GetErrorMessage()
                if msg:
                    if error_collector.HasErrors():
                        # These are actual errors (calculation still succeeded if we reach here)
                        errors = [line.strip() for line in msg.split("\n\n") if line.strip()]
                    else:
                        # Non-fatal messages are treated as warnings
                        warnings = [line.strip() for line in msg.split("\n\n") if line.strip()]

            # Build StatisticsResult with proper constructor
            result = StatisticsResult(
                display_results=calculator.GetDisplayResults(),
                use_polars=_is_polars_simulation(baseline),
                errors=errors,
                warnings=warnings,
            )
        finally:
            # Undo any partial-execution restriction, restoring the authored active flags.
            if restricted:
                self._template.RestoreActive()

        return result

    def list_structure(self) -> dict:
        """Return the template structure as ``{page_name: [table_names]}``.

        Useful for discovering which pages/tables exist before requesting a subset
        via the ``pages``/``tables`` parameters of :meth:`calculate`.
        """
        structure = self._template.GetStructure()
        return {str(k): [str(t) for t in structure[k]] for k in structure.Keys}

    def list_pages(self) -> list:
        """Return the list of page names in the template."""
        return list(self.list_structure().keys())

    def list_tables(self, page: str = None) -> list:
        """Return table names in the template, or only those of ``page`` if given."""
        structure = self.list_structure()
        if page is None:
            return [t for tables in structure.values() for t in tables]
        return structure.get(page, [])

    def _validate_required_variables(self, available_vars):
        """Validate that all required template variables exist in the data.

        Parameters
        ----------
        available_vars : list[str]
            Variable names available in the simulation output.

        Raises
        ------
        ValueError
            If required variables are missing, with their names listed.
        """
        if self._template.info.requiredVariables is None:
            return

        available_lower = {v.lower() for v in available_vars}
        missing = []

        for req_var in self._template.info.requiredVariables:
            read_var = req_var.readVar if req_var.readVar else req_var.name
            if read_var.lower() not in available_lower:
                missing.append(read_var)

        if missing:
            raise ValueError(
                f"[Statistics] Missing required variables in simulation output: "
                f"{', '.join(missing)}\n"
                f"  Template '{os.path.basename(self._template_path)}' requires these variables. "
                f"Available: {', '.join(sorted(available_vars)[:20])}"
                + ("..." if len(available_vars) > 20 else "")
            )

    def _validate_variable_in_simulation(self, simulation):
        """Validate that the specified variable exists in the simulation output columns.

        Performs case-insensitive matching against the simulation's numeric column names.

        Parameters
        ----------
        simulation : Simulation or pandas.DataFrame or polars.DataFrame
            The simulation or DataFrame to check for the variable.

        Raises
        ------
        ValueError
            If the variable does not exist in the simulation columns,
            listing the variable name and available alternatives.
        """
        if _is_dataframe(simulation):
            df = simulation
        else:
            try:
                df = simulation.outputs[0]
            except (IndexError, KeyError):
                return  # Let downstream code handle missing outputs

        # Get column names from the DataFrame
        if isinstance(df, pd.DataFrame):
            columns = [str(c) for c in df.columns]
        elif isinstance(df, pl.DataFrame):
            columns = [str(c) for c in df.columns]
        else:
            return  # Let downstream code handle unsupported types

        # Case-insensitive check
        columns_lower = {c.lower() for c in columns}
        if self._variable.lower() not in columns_lower:
            available = sorted(columns)
            available_display = ", ".join(available[:20])
            if len(available) > 20:
                available_display += "..."
            raise ValueError(
                f"[Statistics] Variable '{self._variable}' not found in simulation output.\n"
                f"  Available columns: {available_display}"
            )

    def calculate_custom(self, baseline, aggregate_stats=None, distributional_stats=None):
        """Calculate custom statistics.

        Accepts aggregate and distributional statistic definitions as Python
        dicts, maps them to C# ExternalStatisticAggregate and
        ExternalStatisticDistributional objects, builds a programmatic template,
        and runs the calculation using the baseline Simulation data.

        If this Statistics instance was initialized with a template, the custom
        statistics are combined with the template-driven calculations in a
        single pass. Otherwise, a minimal template is constructed for the
        custom statistics alone.

        Parameters
        ----------
        baseline : Simulation or pandas.DataFrame or polars.DataFrame
            Simulation output, or a DataFrame containing the income-list variables
            (and idperson, idhh, dwt, dag) referenced by the requested statistics.
        aggregate_stats : list[dict], optional
            Aggregate statistic definitions. Each dict should have:
              - name (str): statistic name
              - income_list (str): income list variable name
              - description (str, optional): description
              - source (str, optional): data source
              - year (str, optional): year for year values
              - amount (str, optional): amount value for the year
              - beneficiaries (str, optional): beneficiaries count for the year
        distributional_stats : list[dict], optional
            Distributional statistic definitions. Each dict should have:
              - name (str): statistic name
              - income_list (str): income list variable name
              - description (str, optional): description
              - measures (list[str], optional): distributional measures to calculate.
                Supported: "gini", "s80s20", "poverty_rate", "median", "mean",
                "atkinson", "mean_log_deviation", "population_count"

        Returns
        -------
        StatisticsResult
            Calculation results with multiple access patterns.

        Raises
        ------
        ValueError
            If no custom statistics definitions are provided, or if definitions
            are invalid (missing required fields).
        RuntimeError
            If the EM_TemplateCalculator reports errors during preparation or
            calculation.
        """
        if not aggregate_stats and not distributional_stats:
            raise ValueError(
                "[Statistics] At least one of 'aggregate_stats' or "
                "'distributional_stats' must be provided."
            )

        # Validate the stat definitions
        aggregate_stats = aggregate_stats or []
        distributional_stats = distributional_stats or []
        self._validate_custom_stat_definitions(aggregate_stats, distributional_stats)

        # Build a programmatic template for the custom statistics
        custom_template = _build_custom_template(
            aggregate_stats, distributional_stats, baseline
        )

        # Create .NET List types for PrepareFromData
        ListStr = SystemCs.Collections.Generic.List[SystemCs.String]
        ListListStr = SystemCs.Collections.Generic.List[ListStr]
        ListDoubleArray = SystemCs.Collections.Generic.List[_DOUBLE_2D_TYPE]

        # Prepare data arrays from the baseline simulation
        var_names, data_array = _simulation_to_clr_data(baseline)

        # If this Statistics instance has a template, combine by running both:
        # first the template-driven calculation, then custom, and merge results.
        template_result = None
        if hasattr(self, '_template') and self._template is not None:
            # Run template-driven calculation first
            try:
                template_result = self.calculate(baseline)
            except (ValueError, RuntimeError):
                # Template calculation failed - proceed with custom only
                template_result = None

        # Run custom statistics calculation
        all_variable_names = ListListStr()
        all_data = ListDoubleArray()
        system_names = ListStr()

        var_name_list = ListStr()
        for name in var_names:
            var_name_list.Add(name)
        all_variable_names.Add(var_name_list)
        all_data.Add(data_array)
        system_names.Add("Baseline")

        # Create EM_TemplateCalculator with the custom template
        calculator = EM_TemplateCalculator(custom_template)

        # Call PrepareFromData
        prepare_result = calculator.PrepareFromData(
            all_variable_names, all_data, system_names
        )

        if isinstance(prepare_result, tuple):
            prepare_success = prepare_result[0]
            error_collector = prepare_result[1]
        else:
            prepare_success = prepare_result
            error_collector = None

        if not prepare_success:
            error_msg = ""
            if error_collector is not None and error_collector.HasErrors():
                error_msg = error_collector.GetErrorMessage()
            raise RuntimeError(
                f"[Statistics] PrepareFromData failed for custom statistics: {error_msg}"
            )

        # Calculate
        calc_success = calculator.CalculateStatisticsFromPreparedData()

        if not calc_success:
            error_msg = ""
            if error_collector is not None and error_collector.HasErrors():
                error_msg = error_collector.GetErrorMessage()
            raise RuntimeError(
                f"[Statistics] CalculateStatisticsFromPreparedData failed: {error_msg}"
            )

        # Collect errors and warnings
        errors = []
        warnings = []
        if error_collector is not None:
            msg = error_collector.GetErrorMessage()
            if msg:
                if error_collector.HasErrors():
                    errors = [line.strip() for line in msg.split("\n\n") if line.strip()]
                else:
                    warnings = [line.strip() for line in msg.split("\n\n") if line.strip()]

        # Build the custom statistics result
        custom_result = StatisticsResult(
            display_results=calculator.GetDisplayResults(),
            use_polars=_is_polars_simulation(baseline),
            errors=errors,
            warnings=warnings,
        )

        # If we have template results, combine them with custom results
        if template_result is not None:
            return _combine_results(template_result, custom_result)

        return custom_result

    @staticmethod
    def _validate_custom_stat_definitions(aggregate_stats, distributional_stats):
        """Validate custom statistic definitions have required fields.

        Parameters
        ----------
        aggregate_stats : list[dict]
            Aggregate statistic definitions.
        distributional_stats : list[dict]
            Distributional statistic definitions.

        Raises
        ------
        ValueError
            If any definition is missing required fields.
        """
        for i, stat in enumerate(aggregate_stats):
            if not isinstance(stat, dict):
                raise ValueError(
                    f"[Statistics] aggregate_stats[{i}] must be a dict, "
                    f"got {type(stat).__name__}."
                )
            if "name" not in stat:
                raise ValueError(
                    f"[Statistics] aggregate_stats[{i}] is missing required field 'name'."
                )
            if "income_list" not in stat:
                raise ValueError(
                    f"[Statistics] aggregate_stats[{i}] ('{stat.get('name', '')}') "
                    f"is missing required field 'income_list'."
                )

        for i, stat in enumerate(distributional_stats):
            if not isinstance(stat, dict):
                raise ValueError(
                    f"[Statistics] distributional_stats[{i}] must be a dict, "
                    f"got {type(stat).__name__}."
                )
            if "name" not in stat:
                raise ValueError(
                    f"[Statistics] distributional_stats[{i}] is missing required "
                    f"field 'name'."
                )
            if "income_list" not in stat:
                raise ValueError(
                    f"[Statistics] distributional_stats[{i}] ('{stat.get('name', '')}') "
                    f"is missing required field 'income_list'."
                )
            for measure in stat.get("measures", ["gini"]):
                if measure.lower() not in _DISTRIBUTIONAL_MEASURES:
                    supported = ", ".join(sorted(_DISTRIBUTIONAL_MEASURES))
                    raise ValueError(
                        f"[Statistics] distributional_stats[{i}] ('{stat.get('name', '')}') "
                        f"has unsupported measure '{measure}'. Supported measures: {supported}."
                    )


def _is_dataframe(obj):
    """Return True if obj is a pandas or polars DataFrame.

    Used throughout this module to let a DataFrame be passed directly anywhere a
    Simulation is accepted.
    """
    return isinstance(obj, (pd.DataFrame, pl.DataFrame))


def _is_polars_simulation(simulation):
    """Check whether the data source uses polars DataFrames.

    Parameters
    ----------
    simulation : Simulation or pandas.DataFrame or polars.DataFrame
        A Simulation object or a DataFrame.

    Returns
    -------
    bool
        True if the source is (or its first output is) a polars DataFrame.
    """
    if isinstance(simulation, pl.DataFrame):
        return True
    if isinstance(simulation, pd.DataFrame):
        return False
    try:
        df = simulation.outputs[0]
        return isinstance(df, pl.DataFrame)
    except (IndexError, KeyError):
        return False


def _get_simulation_name(simulation, default="System"):
    """Get a display name for a Simulation object.

    Uses the first output filename if available, otherwise falls back
    to the provided default name.

    Parameters
    ----------
    simulation : Simulation or pandas.DataFrame or polars.DataFrame
        A Simulation object or a DataFrame. A DataFrame has no filename and is
        named by the provided default (i.e. by position).
    default : str
        Default name if no output filename is available.

    Returns
    -------
    str
        A display name for the simulation.
    """
    if _is_dataframe(simulation):
        return default
    try:
        if hasattr(simulation, 'output_filenames') and simulation.output_filenames:
            return simulation.output_filenames[0]
    except (IndexError, AttributeError):
        pass
    return default


def _get_observation_count(simulation, output_index=0):
    """Get the number of observations (rows) in the source's output DataFrame.

    Parameters
    ----------
    simulation : Simulation or pandas.DataFrame or polars.DataFrame
        A Simulation object or a DataFrame. For a DataFrame, output_index is ignored.
    output_index : int, optional
        Index of the output DataFrame to check (default: 0).

    Returns
    -------
    int
        The number of rows in the DataFrame.

    Raises
    ------
    IndexError
        If output_index is out of range.
    """
    if _is_dataframe(simulation):
        return len(simulation) if isinstance(simulation, pd.DataFrame) else simulation.height

    try:
        df = simulation.outputs[output_index]
    except (IndexError, KeyError):
        raise IndexError(
            f"[Statistics] Output index {output_index} is out of range. "
            f"Simulation has {len(simulation.outputs)} output(s)."
        )

    if isinstance(df, pd.DataFrame):
        return len(df)
    elif isinstance(df, pl.DataFrame):
        return df.height
    else:
        return len(df)


def _dataframe_to_clr_data(df):
    """Convert a pandas or polars DataFrame to CLR-compatible arrays.

    Filters to numeric columns only and converts the data to a .NET
    double[,] array suitable for passing to EM_TemplateCalculator.PrepareFromData.

    Parameters
    ----------
    df : pandas.DataFrame or polars.DataFrame
        The DataFrame to convert.

    Returns
    -------
    tuple[list[str], System.Array]
        A tuple of (variable_names, net_array) where variable_names is a Python
        list of column name strings and net_array is a .NET double[,] array with
        shape [num_variables, num_observations].

    Raises
    ------
    TypeError
        If df is not a pandas or polars DataFrame.
    ValueError
        If no numeric columns are found in the DataFrame.
    """
    if isinstance(df, pd.DataFrame):
        # Filter to numeric columns only (matches _get_dataArray pattern in core.py)
        df_numeric = df.select_dtypes(['number'])
        cols = [str(x) for x in df_numeric.columns]
        data = df_numeric.to_numpy(np.float64).T
    elif isinstance(df, pl.DataFrame):
        # Use the existing utility that filters and casts numeric columns to Float64
        df_numeric = convert_numeric_columns_to_float64(df)
        cols = [str(x) for x in df_numeric.columns]
        data = df_numeric.to_numpy().T
    else:
        raise TypeError(
            "[Statistics] Data must be a pandas.DataFrame or polars.DataFrame, "
            f"got {type(df).__name__}."
        )

    if len(cols) == 0:
        raise ValueError(
            "[Statistics] No numeric columns found in the DataFrame. "
            "At least one numeric column is required."
        )

    # Ensure data is contiguous float64 for asNetArray
    if not data.flags.c_contiguous or data.dtype != np.float64:
        data = np.ascontiguousarray(data, dtype=np.float64)

    net_array = asNetArray(data)
    return cols, net_array


def _simulation_to_clr_data(simulation, output_index=0):
    """Convert a Simulation (or a DataFrame) to CLR-compatible arrays.

    A pandas or polars DataFrame is converted directly. For a Simulation that
    retains raw CLR data (via keep_clr_data=True in run()), the cached arrays are
    returned without re-conversion; otherwise the output DataFrame at the given
    index is extracted, filtered to numeric columns, and converted to a .NET
    double[,] array.

    Parameters
    ----------
    simulation : Simulation or pandas.DataFrame or polars.DataFrame
        A Simulation object or a DataFrame. For a DataFrame, output_index is ignored.
    output_index : int, optional
        Index of the output DataFrame to use (default: 0).

    Returns
    -------
    tuple[list[str], System.Array]
        A tuple of (variable_names, net_array) where variable_names is a Python
        list of column name strings and net_array is a .NET double[,] array with
        shape [num_variables, num_observations].

    Raises
    ------
    IndexError
        If output_index is out of range for the simulation's outputs.
    TypeError
        If the input is not a Simulation, pandas.DataFrame, or polars.DataFrame.
    ValueError
        If no numeric columns are found in the DataFrame.
    """
    # A bare DataFrame is the simplest case: convert it directly.
    if _is_dataframe(simulation):
        return _dataframe_to_clr_data(simulation)

    # Check for cached CLR data first (avoids DataFrame -> numpy -> CLR round-trip)
    if hasattr(simulation, '_clr_data') and simulation._clr_data:
        try:
            key = simulation.output_filenames[output_index]
            if key in simulation._clr_data:
                var_names_clr, clr_arr = simulation._clr_data[key]
                # Convert CLR List<string> to Python list if needed
                var_names = list(var_names_clr)
                return var_names, clr_arr
        except (IndexError, KeyError):
            pass  # Fall through to DataFrame conversion

    # Access the output DataFrame from the Simulation's Container
    try:
        df = simulation.outputs[output_index]
    except (IndexError, KeyError):
        raise IndexError(
            f"[Statistics] Output index {output_index} is out of range. "
            f"Simulation has {len(simulation.outputs)} output(s)."
        )
    except AttributeError:
        raise TypeError(
            "[Statistics] Expected a Simulation, pandas.DataFrame, or "
            f"polars.DataFrame, got {type(simulation).__name__}."
        )

    return _dataframe_to_clr_data(df)


# ---------------------------------------------------------------------------
# Custom statistics helper functions
# ---------------------------------------------------------------------------

# Supported distributional measures. Each maps to the C# CalculationType and the
# set of global prerequisite actions the engine needs before the cell action can
# run:
#   "deciles" -> a CreateDeciles action (S80/S20 reads a precomputed decile var)
#   "povline" -> a saved poverty line (60% of the median) for the poverty measures
#   "povflag" -> a CreateFlag marking units below the poverty line (headcount rate)
_DISTRIBUTIONAL_MEASURES = {
    "gini":               {"calc": "CalculateGini",             "needs": ()},
    "s80s20":             {"calc": "CalculateS8020",            "needs": ("deciles",)},
    "median":             {"calc": "CalculateMedian",           "needs": ()},
    "atkinson":           {"calc": "CalculateAtkinson",         "needs": ()},
    "mean_log_deviation": {"calc": "CalculateMeanLogDeviation", "needs": ()},
    "mld":                {"calc": "CalculateMeanLogDeviation", "needs": ()},
    "poverty_gap":        {"calc": "CalculatePovertyGap",       "needs": ("povline",)},
    "poverty_rate":       {"calc": "CalculatePopulationCount",  "needs": ("povline", "povflag")},
}


def _build_external_statistic(aggregate_stats, distributional_stats):
    """Build a C# ExternalStatistic object from Python dictionaries.

    Creates ExternalStatisticAggregate and ExternalStatisticDistributional
    CLR objects and populates them from the user-provided definitions.

    Parameters
    ----------
    aggregate_stats : list[dict]
        Aggregate statistic definitions.
    distributional_stats : list[dict]
        Distributional statistic definitions.

    Returns
    -------
    ExternalStatistic (CLR object)
        A populated C# ExternalStatistic object.
    """
    ext_stat = ExternalStatistic()

    for stat_def in aggregate_stats:
        name = stat_def["name"]
        income_list = stat_def["income_list"]
        description = stat_def.get("description", "")
        source = stat_def.get("source", "")
        comment = stat_def.get("comment", "")
        destination = stat_def.get("destination", "")

        agg = ExternalStatisticAggregate(
            income_list, name, description, source, comment, destination
        )

        # Add year values if provided
        year = stat_def.get("year")
        if year:
            year_vals = ExternalStatisticAggregateValues()
            year_vals.Amount = str(stat_def.get("amount", ""))
            year_vals.Beneficiares = str(stat_def.get("beneficiaries", ""))
            year_vals.Level = str(stat_def.get("level", ""))
            agg.YearValues.Add(str(year), year_vals)

        ext_stat.Aggregate.Add(name, agg)
        if year and str(year) not in list(ext_stat.Years):
            ext_stat.Years.Add(str(year))

    for stat_def in distributional_stats:
        name = stat_def["name"]
        income_list = stat_def["income_list"]
        description = stat_def.get("description", "")
        source = stat_def.get("source", "")
        comment = stat_def.get("comment", "")
        table_name = stat_def.get("table_name", income_list)

        dist = ExternalStatisticDistributional(
            table_name, name, description, source, comment
        )

        ext_stat.Distributional.Add(name, dist)

    return ext_stat


def _build_custom_template(aggregate_stats, distributional_stats, baseline):
    """Build a programmatic Template for custom statistics calculation.

    Creates a minimal Template XML string, parses it via CLR, and returns a
    Template object configured to calculate the requested aggregate and
    distributional statistics from the simulation data.

    Parameters
    ----------
    aggregate_stats : list[dict]
        Aggregate statistic definitions.
    distributional_stats : list[dict]
        Distributional statistic definitions.
    baseline : Simulation
        The baseline simulation (used to determine available variables).

    Returns
    -------
    Template (CLR object)
        A configured Template ready for EM_TemplateCalculator.
    """
    # Collect all income list variables that need to be required
    income_vars = set()
    for stat_def in aggregate_stats:
        income_vars.add(stat_def["income_list"])
    for stat_def in distributional_stats:
        income_vars.add(stat_def["income_list"])

    # Build the template XML string
    xml = _build_custom_template_xml(aggregate_stats, distributional_stats, income_vars)

    # Parse the XML string into a Template using CLR
    # C# signature: ParseTemplate(string pathOrInputString, out Template, out ErrorCollector, bool readFromString = false)
    # pythonnet returns the out params as extra tuple values; readFromString must
    # be passed by keyword (a positional bool would bind to the out Template param).
    result = XML_handling.ParseTemplate(xml, readFromString=True)
    # pythonnet returns (bool success, Template template, ErrorCollector errorCollector)
    success, template, error_collector = result

    if not success or (error_collector is not None and error_collector.HasErrors()):
        err_msg = error_collector.GetErrorMessage() if error_collector else "Unknown error"
        raise RuntimeError(
            f"[Statistics] Failed to build custom statistics template: {err_msg}"
        )

    return template


def _safe_var(name):
    """Sanitise a name into a token safe to use as a generated CLR variable name."""
    return re.sub(r"\W", "_", str(name))


def _emit_parameter(name, value, kind="var"):
    """Emit a <Parameter> element. kind is 'var', 'num', or 'bool'."""
    value_tag = {"var": "VarName", "num": "NumericValue", "bool": "BoolValue"}[kind]
    return (
        f'<Parameter><Name>{_xml_escape(name)}</Name>'
        f'<{value_tag}>{_xml_escape(value)}</{value_tag}></Parameter>'
    )


def _distributional_target(stat_def):
    """Return (target_var, key, equivalise) for a distributional stat definition.

    When equivalise is True (default) the measures are computed on the
    household-equivalised income variable; otherwise on the raw income list.
    `key` is a sanitised token used to name the generated helper variables.
    """
    income_list = stat_def["income_list"]
    equivalise = stat_def.get("equivalise", True)
    if equivalise:
        target = f"__eq_{_safe_var(income_list)}"
    else:
        target = income_list
    return target, _safe_var(target), equivalise


def _distributional_cell_action(measure, stat_def, target, key):
    """Build the <CellAction> XML for a single distributional measure."""
    m = measure.lower()
    calc = _DISTRIBUTIONAL_MEASURES[m]["calc"]

    if m == "poverty_rate":
        # Headcount ratio: weighted share of the population flagged as poor.
        return (
            f'<CellAction><CalculationType>{calc}</CalculationType>'
            f'<FormulaString>DATA_VAR[@__poor_{key}]</FormulaString></CellAction>'
        )

    params = []
    if m == "gini":
        params = [("GiniVar", target, "var"), ("GroupingVar", "idhh", "var")]
    elif m == "s80s20":
        params = [("S8020Var", target, "var"), ("DecileVar", f"__dec_{key}", "var")]
    elif m == "median":
        params = [("IncomeVar", target, "var")]
    elif m == "atkinson":
        epsilon = stat_def.get("epsilon", stat_def.get("inequality_aversion", 0.5))
        params = [("IncomeVar", target, "var"),
                  ("InequalityAversion", epsilon, "num")]
        if stat_def.get("recode_negatives", True):
            params.append(("RecodeNegatives", "true", "bool"))
    elif m in ("mean_log_deviation", "mld"):
        params = [("IncomeVar", target, "var")]
        if stat_def.get("recode_negatives", True):
            params.append(("RecodeNegatives", "true", "bool"))
    elif m == "poverty_gap":
        params = [("IncomeVar", target, "var"),
                  ("PovertyLine", f"__povline_{key}", "var")]

    params_xml = "".join(_emit_parameter(n, v, k) for n, v, k in params)
    return (
        f'<CellAction><CalculationType>{calc}</CalculationType>'
        f'<Parameters>{params_xml}</Parameters></CellAction>'
    )


def _build_global_actions(distributional_stats):
    """Build the <Globals><Actions> prerequisite actions for distributional stats.

    Creates, as needed and de-duplicated per income variable:
      - an OECD-modified equivalence scale (once),
      - equivalised income variables,
      - decile variables (for S80/S20),
      - a saved poverty line at 60% of the median (for poverty measures),
      - a poverty flag (for the headcount poverty rate).
    """
    actions = []
    oecd_added = False
    eq_done, dec_done, pov_done, flag_done = set(), set(), set(), set()

    for stat_def in distributional_stats:
        income_list = stat_def["income_list"]
        measures = [m.lower() for m in stat_def.get("measures", ["gini"])]
        target, key, equivalise = _distributional_target(stat_def)
        needs = set()
        for m in measures:
            needs.update(_DISTRIBUTIONAL_MEASURES.get(m, {}).get("needs", ()))

        if equivalise and target not in eq_done:
            if not oecd_added:
                actions.append(
                    '<Action><CalculationType>CreateOECDScale</CalculationType>'
                    '<OutputVar>__oecd</OutputVar></Action>'
                )
                oecd_added = True
            actions.append(
                '<Action><CalculationType>CreateEquivalized</CalculationType>'
                f'<OutputVar>{target}</OutputVar><Parameters>'
                f'<Parameter><VarName>{_xml_escape(income_list)}</VarName></Parameter>'
                f'{_emit_parameter("EquivalenceScale", "__oecd", "var")}'
                '</Parameters></Action>'
            )
            eq_done.add(target)

        if "deciles" in needs and target not in dec_done:
            actions.append(
                '<Action><CalculationType>CreateDeciles</CalculationType>'
                f'<OutputVar>__dec_{key}</OutputVar><Parameters>'
                f'{_emit_parameter("IncomeVar", target, "var")}'
                f'{_emit_parameter("GroupingVar", "idhh", "var")}'
                '</Parameters></Action>'
            )
            dec_done.add(target)

        if "povline" in needs and target not in pov_done:
            actions.append(
                '<Action><CalculationType>CalculateMedian</CalculationType>'
                f'<OutputVar>__med_{key}</OutputVar><SaveResults>true</SaveResults>'
                f'<Parameters>{_emit_parameter("IncomeVar", target, "var")}</Parameters>'
                '</Action>'
            )
            actions.append(
                '<Action><CalculationType>CalculateArithmetic</CalculationType>'
                f'<OutputVar>__povline_{key}</OutputVar><SaveResults>true</SaveResults>'
                f'<FormulaString>SAVED_VAR[@__med_{key}] * 0.6</FormulaString></Action>'
            )
            pov_done.add(target)

        if "povflag" in needs and target not in flag_done:
            actions.append(
                '<Action><CalculationType>CreateFlag</CalculationType>'
                f'<OutputVar>__poor_{key}</OutputVar>'
                f'<Filter><Name>__poorfilter_{key}</Name>'
                f'<FormulaString>DATA_VAR[@{target}] &lt; SAVED_VAR[@__povline_{key}]</FormulaString>'
                '</Filter></Action>'
            )
            flag_done.add(target)

    return actions


def _build_custom_template_xml(aggregate_stats, distributional_stats, income_vars):
    """Build a template XML string for custom statistics.

    Generates an engine-valid statistics template: required variables, the
    global prerequisite actions (equivalisation, deciles, poverty lines/flags),
    and one table per statistic with a cell action per measure.

    Aggregate stats are computed as a weighted mean of the income list.
    Distributional measures (gini, s80s20, median, atkinson, mean_log_deviation,
    poverty_gap, poverty_rate) are computed on the household-equivalised income
    by default (set ``equivalise=False`` in the stat dict to use the raw variable).

    Parameters
    ----------
    aggregate_stats : list[dict]
        Aggregate statistic definitions.
    distributional_stats : list[dict]
        Distributional statistic definitions.
    income_vars : set[str]
        Income list variable names that must be declared as required.

    Returns
    -------
    str
        A valid template XML string suitable for ParseTemplate.
    """
    lines = []
    lines.append('<Template>')
    lines.append('  <TemplateInfo>')
    lines.append('    <Name>CustomStatistics</Name>')
    lines.append('    <Title>Custom Statistics</Title>')
    lines.append('    <Button>Custom</Button>')
    lines.append('    <Description>Custom statistics from simulation output</Description>')
    lines.append('    <TemplateType>Default</TemplateType>')
    lines.append('    <RequiredVariables>')

    # Grouping/weight variables required by the calculation levels and measures.
    for name, read_var in (("idperson", "idperson"), ("idhh", "idhh"),
                           ("weight", "dwt"), ("age", "dag")):
        lines.append(f'      <RequiredVariable><Name>{name}</Name>'
                     f'<ReadVar>{read_var}</ReadVar></RequiredVariable>')

    # Income list variables used by the requested statistics.
    for var_name in sorted(income_vars):
        ev = _xml_escape(var_name)
        lines.append(f'      <RequiredVariable><Name>{ev}</Name>'
                     f'<ReadVar>{ev}</ReadVar></RequiredVariable>')

    lines.append('    </RequiredVariables>')
    lines.append('  </TemplateInfo>')

    # Globals: prerequisite actions for the distributional measures.
    lines.append('  <Globals>')
    lines.append('    <Filters></Filters>')
    lines.append('    <Actions>')
    for action in _build_global_actions(distributional_stats):
        lines.append(f'      {action}')
    lines.append('    </Actions>')
    lines.append('  </Globals>')

    # Pages / tables.
    lines.append('  <Pages>')
    lines.append('    <Page>')
    lines.append('      <Name>CustomStats</Name>')
    lines.append('      <Title>Custom Statistics</Title>')
    lines.append('      <Tables>')

    # Aggregate statistics table: one row per stat, weighted mean of the income list.
    if aggregate_stats:
        lines.append('        <Table>')
        lines.append('          <Name>AggregateStats</Name>')
        lines.append('          <Title>Aggregate Statistics</Title>')
        lines.append('          <Columns><Column><Name>value</Name><Title>Value</Title></Column></Columns>')
        lines.append('          <Rows>')
        for stat_def in aggregate_stats:
            stat_name = _xml_escape(stat_def["name"])
            description = _xml_escape(stat_def.get("description", stat_def["name"]))
            income_list = _xml_escape(stat_def["income_list"])
            lines.append(f'            <Row><Name>{stat_name}</Name><Title>{description}</Title>')
            lines.append(f'              <CellAction><CalculationType>CalculateWeightedAverage</CalculationType>'
                         f'<FormulaString>DATA_VAR[@{income_list}]</FormulaString></CellAction>')
            lines.append(f'            </Row>')
        lines.append('          </Rows>')
        lines.append('        </Table>')

    # Distributional statistics tables: one table per stat, one row per measure.
    for stat_def in distributional_stats:
        stat_name = stat_def["name"]
        description = stat_def.get("description", stat_name)
        measures = stat_def.get("measures", ["gini"])
        target, key, _ = _distributional_target(stat_def)

        lines.append('        <Table>')
        lines.append(f'          <Name>Dist_{_xml_escape(stat_name)}</Name>')
        lines.append(f'          <Title>{_xml_escape(description)}</Title>')
        lines.append('          <Columns><Column><Name>value</Name><Title>Value</Title></Column></Columns>')
        lines.append('          <Rows>')
        for measure in measures:
            m = measure.lower()
            if m not in _DISTRIBUTIONAL_MEASURES:
                continue
            row_name = _xml_escape(f"{stat_name}_{m}")
            cell_action = _distributional_cell_action(measure, stat_def, target, key)
            lines.append(f'            <Row><Name>{row_name}</Name><Title>{_xml_escape(measure)}</Title>')
            lines.append(f'              {cell_action}')
            lines.append(f'            </Row>')
        lines.append('          </Rows>')
        lines.append('        </Table>')

    lines.append('      </Tables>')
    lines.append('    </Page>')
    lines.append('  </Pages>')
    lines.append('</Template>')

    return '\n'.join(lines)


def _xml_escape(text):
    """Escape special XML characters in a string.

    Parameters
    ----------
    text : str
        Input text that may contain special XML characters.

    Returns
    -------
    str
        XML-safe text with &, <, >, ", ' escaped.
    """
    text = str(text)
    text = text.replace("&", "&amp;")
    text = text.replace("<", "&lt;")
    text = text.replace(">", "&gt;")
    text = text.replace('"', "&quot;")
    text = text.replace("'", "&apos;")
    return text


def _combine_results(template_result, custom_result):
    """Combine template-driven and custom statistics results.

    Merges two StatisticsResult objects into a single result that contains
    tables from both the template calculation and the custom statistics
    calculation. This supports requirement 5.3 (combining template-driven
    and custom statistics in a single calculation pass).

    Parameters
    ----------
    template_result : StatisticsResult
        Results from the template-driven calculation.
    custom_result : StatisticsResult
        Results from the custom statistics calculation.

    Returns
    -------
    StatisticsResult
        A combined result containing tables from both calculations.
    """
    # Create a combined result that holds both sets of tables
    combined = StatisticsResult(
        display_results=custom_result._display_results,
        use_polars=custom_result._use_polars,
        errors=template_result.errors + custom_result.errors,
        warnings=template_result.warnings + custom_result.warnings,
    )

    # Build combined tables from both results
    template_tables = template_result.tables
    custom_tables = custom_result.tables

    combined_container = Container()
    if template_tables is not None:
        for table in template_tables:
            key = table.name if table.name else table.title
            if not key:
                key = f"table_{len(combined_container)}"
            combined_container.add(key, table)

    if custom_tables is not None:
        for table in custom_tables:
            key = table.name if table.name else table.title
            if not key:
                key = f"table_{len(combined_container)}"
            combined_container.add(key, table)

    combined._tables = combined_container
    return combined


class StatisticsResult:
    """Container for statistics calculation results, ready for Python consumption.

    Parameters
    ----------
    display_results : DisplayResults (CLR object), optional
        The C# DisplayResults object from EM_TemplateCalculator.
    use_polars : bool, optional
        If True, DataFrames produced by this result will use polars (default: False).
    errors : list[str], optional
        Error messages from the ErrorCollector.
    warnings : list[str], optional
        Warning messages (non-fatal) from the ErrorCollector.
    """

    def __init__(self, display_results=None, use_polars=False, errors=None, warnings=None):
        self._tables = None
        self._summary = None
        self._errors = errors if errors is not None else []
        self._warnings = warnings if warnings is not None else []
        self._display_results = display_results
        self._use_polars = use_polars

    @property
    def tables(self):
        """All result tables as a Container (indexable by name or position).

        Tables are built lazily from the C# DisplayResults on first access.
        Each table is a StatisticsTable object with name, title, and cell data.
        """
        if self._tables is None and self._display_results is not None:
            self._tables = _build_tables_from_display_results(
                self._display_results, self._use_polars
            )
        return self._tables

    @property
    def errors(self) -> list:
        """Errors from calculation (empty if successful)."""
        return self._errors

    @property
    def warnings(self) -> list:
        """Non-fatal warnings from calculation."""
        return self._warnings

    @property
    def summary(self):
        """Flattened summary DataFrame with one row per cell across all tables.

        Columns: page, table, row_label, column_label, value, display_value.
        Ready for filtering, grouping, and plotting.

        Returns
        -------
        pandas.DataFrame or polars.DataFrame
            A flat DataFrame with one row per cell. Uses polars if the source
            Simulation was polars-based, otherwise pandas.
        """
        if self._summary is not None:
            return self._summary

        tables = self.tables
        if tables is None or len(tables) == 0:
            if self._use_polars:
                self._summary = pl.DataFrame({
                    "page": [],
                    "table": [],
                    "row_label": [],
                    "column_label": [],
                    "value": [],
                    "display_value": [],
                })
            else:
                self._summary = pd.DataFrame(
                    columns=["page", "table", "row_label", "column_label", "value", "display_value"]
                )
            return self._summary

        rows = []
        for table in tables:
            page_name = table._page_name
            table_name = table._name if table._name else table._title
            for i, row_label in enumerate(table._row_titles):
                if i >= len(table._cell_values):
                    continue
                for j, col_label in enumerate(table._col_titles):
                    if j >= len(table._cell_values[i]):
                        continue
                    value = table._cell_values[i][j]
                    display_value = (
                        table._cell_display_values[i][j]
                        if i < len(table._cell_display_values)
                        and j < len(table._cell_display_values[i])
                        else ""
                    )
                    rows.append((page_name, table_name, row_label, col_label, value, display_value))

        if self._use_polars:
            if rows:
                self._summary = pl.DataFrame(
                    {
                        "page": [r[0] for r in rows],
                        "table": [r[1] for r in rows],
                        "row_label": [r[2] for r in rows],
                        "column_label": [r[3] for r in rows],
                        "value": [r[4] for r in rows],
                        "display_value": [r[5] for r in rows],
                    }
                )
            else:
                self._summary = pl.DataFrame({
                    "page": [],
                    "table": [],
                    "row_label": [],
                    "column_label": [],
                    "value": [],
                    "display_value": [],
                })
        else:
            self._summary = pd.DataFrame(
                rows,
                columns=["page", "table", "row_label", "column_label", "value", "display_value"],
            )

        return self._summary

    def to_dataframes(self) -> dict:
        """Convert each table in results to a DataFrame, keyed by table name.

        Returns
        -------
        dict[str, pandas.DataFrame | polars.DataFrame]
            Dictionary mapping table name to its DataFrame representation.
            Uses the same DataFrame library (pandas or polars) as the input Simulation.
        """
        tables = self.tables
        if tables is None:
            return {}
        result = {}
        for table in tables:
            key = table.name if table.name else table.title
            if not key:
                key = f"table_{len(result)}"
            result[key] = table.dataframe
        return result

    def to_dict(self) -> dict:
        """Full results as a nested ``dict`` matching the SP_ExecutableCaller JSON structure.

        The returned dictionary has the following shape::

            {
                "info": {"title": str, "subtitle": str, "button": str, "description": str},
                "pages": [
                    {
                        "name": str, "title": str, "subtitle": str, "description": str,
                        "tables": [
                            {
                                "name": str, "title": str, "subtitle": str,
                                "columns": [{"name": str, "title": str}],
                                "rows": [{"name": str, "title": str}],
                                "cells": [[{"displayValue": str, "value": float, "isStringValue": bool}]]
                            }
                        ]
                    }
                ],
                "prepared": bool,
                "calculated": bool
            }

        Returns
        -------
        dict
            Nested dictionary matching the SP_ExecutableCaller JSON schema.
        """
        tables = self.tables

        # Build the info section from display_results if available
        info = {"title": "", "subtitle": "", "button": "", "description": ""}
        if self._display_results is not None and hasattr(self._display_results, 'info'):
            dr_info = self._display_results.info
            if dr_info is not None:
                info["title"] = str(dr_info.title) if dr_info.title else ""
                info["subtitle"] = str(dr_info.subtitle) if dr_info.subtitle else ""
                info["button"] = str(dr_info.button) if dr_info.button else ""
                info["description"] = str(dr_info.description) if dr_info.description else ""

        # Build pages from display_results
        pages = []
        if self._display_results is not None and self._display_results.displayPages is not None:
            for page in self._display_results.displayPages:
                page_dict = {
                    "name": str(page.name) if page.name else "",
                    "title": str(page.title) if page.title else "",
                    "subtitle": str(page.subtitle) if page.subtitle else "",
                    "description": str(page.description) if page.description else "",
                    "tables": [],
                }

                if page.displayTables is not None:
                    for display_table in page.displayTables:
                        table_dict = self._display_table_to_dict(display_table)
                        page_dict["tables"].append(table_dict)

                pages.append(page_dict)

        # Determine prepared/calculated status
        prepared = self._display_results is not None
        calculated = prepared and len(self._errors) == 0

        return {
            "info": info,
            "pages": pages,
            "prepared": prepared,
            "calculated": calculated,
        }

    @staticmethod
    def _display_table_to_dict(display_table) -> dict:
        """Convert a single C# DisplayTable to the JSON-compatible dict structure.

        Parameters
        ----------
        display_table : DisplayTable (CLR object)
            A C# DisplayTable object.

        Returns
        -------
        dict
            Dictionary with keys: name, title, subtitle, columns, rows, cells.
        """
        table_dict = {
            "name": str(display_table.name) if display_table.name else "",
            "title": str(display_table.title) if display_table.title else "",
            "subtitle": str(display_table.subtitle) if display_table.subtitle else "",
            "columns": [],
            "rows": [],
            "cells": [],
        }

        # Build columns
        if display_table.columns is not None:
            for col in display_table.columns:
                table_dict["columns"].append({
                    "name": str(col.name) if col.name else "",
                    "title": str(col.title) if col.title else "",
                })

        # Build rows
        if display_table.rows is not None:
            for row in display_table.rows:
                table_dict["rows"].append({
                    "name": str(row.name) if row.name else "",
                    "title": str(row.title) if row.title else "",
                })

        # Build cells (list of lists of cell dicts)
        if display_table.cells is not None:
            for row_cells in display_table.cells:
                row_list = []
                for cell in row_cells:
                    row_list.append({
                        "displayValue": str(cell.displayValue) if cell.displayValue else "",
                        "value": float(cell.value),
                        "isStringValue": bool(cell.isStringValue),
                    })
                table_dict["cells"].append(row_list)

        return table_dict

    def to_json(self, path: str) -> None:
        """Export results to JSON file in SP_ExecutableCaller-compatible format.

        Serializes the results using `to_dict()` and writes to the specified file.
        Applies sentinel value substitution for special float values:
        - NaN → 99999998
        - +Infinity → 99999999
        - -Infinity → -99999999

        Parameters
        ----------
        path : str
            File path where the JSON output will be written.

        Raises
        ------
        OSError
            If the file cannot be written to the specified path.
        """
        result_dict = self.to_dict()
        _apply_sentinel_substitution(result_dict)

        with open(path, "w", encoding="utf-8") as f:
            json.dump(result_dict, f, indent=2, ensure_ascii=False)

    def to_excel(self, path: str) -> None:
        """Export results to Excel file via EM_Statistics ExportHandling.

        Uses the C# ExportHandling.ExportSinglePackage method to generate an
        Excel workbook from the DisplayResults object, then writes the resulting
        MemoryStream to the specified file path.

        Parameters
        ----------
        path : str
            File path where the Excel (.xlsx) output will be written.

        Raises
        ------
        ValueError
            If no DisplayResults are available (calculation did not complete).
        RuntimeError
            If the C# ExportHandling reports an error during export.
        OSError
            If the file cannot be written to the specified path.
        """
        if self._display_results is None:
            raise ValueError(
                "[Statistics] Cannot export to Excel: no DisplayResults available. "
                "Ensure calculate() completed successfully."
            )

        if _ExportHandling is None:
            raise NotImplementedError(
                "[Statistics] Excel export (to_excel) is not available in this build. It "
                "requires the full Windows EUROMOD Statistics component (ExportHandling), "
                "which is excluded from the cross-platform EM_Statistics build bundled with "
                "the euromod package. Point EUROMOD_PATH at a full Windows EUROMOD "
                "installation to enable Excel export."
            )

        success, err_msg, excel_stream = _ExportHandling.ExportSinglePackage(
            self._display_results
        )

        if not success:
            raise RuntimeError(
                f"[Statistics] Excel export failed: {err_msg}"
            )

        try:
            # Write the MemoryStream bytes to the file
            excel_bytes = bytes(excel_stream.ToArray())
            with open(path, "wb") as f:
                f.write(excel_bytes)
        finally:
            if excel_stream is not None:
                excel_stream.Dispose()

    def __getitem__(self, key):
        """Direct table access: result['table_name'] or result[0].

        Delegates to the tables Container, which supports both string keys
        (table name) and integer indices (position).
        """
        tables = self.tables
        if tables is None:
            raise TypeError(
                "Cannot index StatisticsResult: no tables available. "
                "Ensure calculate() completed successfully."
            )
        return tables[key]

    def __len__(self):
        """Number of tables in the result."""
        tables = self.tables
        if tables is None:
            return 0
        return len(tables)

    def __iter__(self):
        """Iterate over tables."""
        tables = self.tables
        if tables is None:
            return iter([])
        return iter(tables)

    def __repr__(self) -> str:
        """Human-readable summary showing available tables and key values."""
        tables = self.tables
        if tables is None or len(tables) == 0:
            return "StatisticsResult(empty)"
        table_names = [t.name for t in tables]
        if len(table_names) <= 5:
            names_str = ", ".join(f"'{n}'" for n in table_names)
        else:
            names_str = ", ".join(f"'{n}'" for n in table_names[:5]) + ", ..."
        return f"StatisticsResult({len(tables)} tables: [{names_str}])"


def _apply_sentinel_substitution(obj):
    """Apply sentinel value substitution for special float values in-place.

    Recursively walks the dict/list structure and replaces:
    - NaN → 99999998
    - +Infinity → 99999999
    - -Infinity → -99999999

    This matches the SP_ExecutableCaller behavior for JSON serialization,
    since JSON does not support NaN or Infinity as valid number literals.

    Parameters
    ----------
    obj : dict or list
        The nested dictionary/list structure to process in-place.
    """
    if isinstance(obj, dict):
        for key, value in obj.items():
            if isinstance(value, float):
                if math.isnan(value):
                    obj[key] = 99999998
                elif math.isinf(value) and value > 0:
                    obj[key] = 99999999
                elif math.isinf(value) and value < 0:
                    obj[key] = -99999999
            elif isinstance(value, (dict, list)):
                _apply_sentinel_substitution(value)
    elif isinstance(obj, list):
        for i, item in enumerate(obj):
            if isinstance(item, float):
                if math.isnan(item):
                    obj[i] = 99999998
                elif math.isinf(item) and item > 0:
                    obj[i] = 99999999
                elif math.isinf(item) and item < 0:
                    obj[i] = -99999999
            elif isinstance(item, (dict, list)):
                _apply_sentinel_substitution(item)


def _build_tables_from_display_results(display_results, use_polars=False):
    """Convert C# DisplayResults to a Container of StatisticsTable objects.

    Iterates over all pages and their tables in the DisplayResults hierarchy,
    building a StatisticsTable for each DisplayTable found. Tables are stored
    in a Container indexed by both name (string) and position (integer).

    Parameters
    ----------
    display_results : DisplayResults (CLR object)
        The C# DisplayResults object returned by EM_TemplateCalculator after
        successful calculation. Contains displayPages, each with displayTables.
    use_polars : bool, optional
        If True, tables will be flagged to use polars DataFrames instead of
        pandas when building their dataframe property (default: False).

    Returns
    -------
    Container[StatisticsTable]
        A Container holding all StatisticsTable objects, accessible by name or index.
    """
    tables_container = Container()

    if display_results is None or display_results.displayPages is None:
        return tables_container

    for page in display_results.displayPages:
        if page.displayTables is None:
            continue

        for display_table in page.displayTables:
            table = _build_statistics_table(display_table, page.name, use_polars)
            # Use title as the key for Container access; fall back to name or index
            table_key = table.name if table.name else table.title
            if not table_key:
                table_key = f"table_{len(tables_container)}"
            tables_container.add(table_key, table)

    return tables_container


def _build_statistics_table(display_table, page_name="", use_polars=False):
    """Build a StatisticsTable from a single C# DisplayTable object.

    Extracts the table's title, name, column titles, row titles, and cell
    data. Cells are read from the DisplayTable.cells structure (list of lists
    of DisplayCell), with each cell providing a displayValue (string),
    value (double), and isStringValue (bool).

    Parameters
    ----------
    display_table : DisplayTable (CLR object)
        A single C# DisplayPage.DisplayTable object.
    page_name : str, optional
        The name of the page this table belongs to (for metadata).
    use_polars : bool, optional
        If True, the table's dataframe will be built as polars (default: False).

    Returns
    -------
    StatisticsTable
        A fully populated StatisticsTable object.
    """
    table = StatisticsTable()

    # Extract basic metadata
    table._name = str(display_table.name) if display_table.name else ""
    table._title = str(display_table.title) if display_table.title else ""
    table._page_name = page_name
    table._use_polars = use_polars

    # Extract column titles
    col_titles = []
    if display_table.columns is not None:
        for col in display_table.columns:
            col_titles.append(str(col.title) if col.title else "")

    # Extract row titles
    row_titles = []
    if display_table.rows is not None:
        for row in display_table.rows:
            row_titles.append(str(row.title) if row.title else "")

    table._col_titles = col_titles
    table._row_titles = row_titles

    # Extract cell data from the CLR cells structure
    # cells is List<List<DisplayCell>> indexed as cells[row][col]
    cell_values = []  # list of lists of float
    cell_display_values = []  # list of lists of str
    cell_is_string = []  # list of lists of bool

    if display_table.cells is not None:
        for row_cells in display_table.cells:
            row_values = []
            row_display = []
            row_is_str = []
            for cell in row_cells:
                row_values.append(float(cell.value))
                row_display.append(str(cell.displayValue) if cell.displayValue else "")
                row_is_str.append(bool(cell.isStringValue))
            cell_values.append(row_values)
            cell_display_values.append(row_display)
            cell_is_string.append(row_is_str)

    table._cell_values = cell_values
    table._cell_display_values = cell_display_values
    table._cell_is_string = cell_is_string

    # Build the nested dict for values property: values[row_title][col_title] -> float
    values_dict = {}
    for i, row_title in enumerate(row_titles):
        if i < len(cell_values):
            row_dict = {}
            for j, col_title in enumerate(col_titles):
                if j < len(cell_values[i]):
                    row_dict[col_title] = cell_values[i][j]
            values_dict[row_title] = row_dict
    table._values = values_dict

    return table


class StatisticsTable:
    """A single statistics table with Python-native data access."""

    def __init__(self):
        self._name = ""
        self._title = ""
        self._page_name = ""
        self._dataframe = None
        self._values = {}
        self._col_titles = []
        self._row_titles = []
        self._cell_values = []
        self._cell_display_values = []
        self._cell_is_string = []
        self._use_polars = False

    @property
    def name(self) -> str:
        """Table name from the template."""
        return self._name

    @property
    def title(self) -> str:
        """Display title."""
        return self._title

    @property
    def dataframe(self):
        """Table as a DataFrame. Numeric values as floats, row/column labels as index/headers.

        Built lazily on first access and cached. Uses polars if the source
        Simulation was polars-based, otherwise pandas.

        Returns
        -------
        pandas.DataFrame or polars.DataFrame
            DataFrame with row titles as index (pandas) or a 'row' column (polars),
            and column titles as column headers.
        """
        if self._dataframe is None and self._cell_values:
            if self._use_polars:
                self._dataframe = self._build_polars_dataframe()
            else:
                self._dataframe = self._build_pandas_dataframe()
        return self._dataframe

    def _build_pandas_dataframe(self):
        """Build a pandas DataFrame from cell values."""
        df = pd.DataFrame(
            self._cell_values,
            index=self._row_titles if self._row_titles else None,
            columns=self._col_titles if self._col_titles else None,
        )
        return df

    def _build_polars_dataframe(self):
        """Build a polars DataFrame from cell values."""
        data = {}
        for j, col_title in enumerate(self._col_titles):
            col_key = col_title if col_title else f"col_{j}"
            data[col_key] = [
                self._cell_values[i][j]
                for i in range(len(self._cell_values))
                if j < len(self._cell_values[i])
            ]
        df = pl.DataFrame(data)
        # Add row titles as the first column if available
        if self._row_titles:
            df = df.insert_column(0, pl.Series("row", self._row_titles))
        return df

    @property
    def values(self) -> dict:
        """Nested dict access: values['row_label']['column_label'] -> float."""
        return self._values

    def __getitem__(self, key):
        """Access a specific cell: table['Gini'] returns the value if unambiguous."""
        if key in self._values:
            return self._values[key]
        raise KeyError(f"Row '{key}' not found in table '{self._name}'.")

    def __repr__(self) -> str:
        """Pretty-print the table."""
        return f"StatisticsTable(name='{self._name}', title='{self._title}')"

    def _container_repr(self):
        return f"{self._name}"

    def _container_begin_repr(self):
        return f"{self._name}"

    def _container_middle_repr(self):
        return self._title

    def _container_end_repr(self):
        return ""

    def _short_repr(self):
        return f"{self._name}"
