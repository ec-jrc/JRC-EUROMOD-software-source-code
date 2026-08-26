from ..core import System,Model

import os
import polars as pl
import sympy 
from typing import Dict,List,Iterable
from abc import ABC, abstractmethod
import numba as nb
import json
import scipy as sp
import numpy as np


# =============================================================================
# CompiledFunction hierarchy
# =============================================================================

class CompiledFunction(ABC):
    """
    Base class for compiled symbolic functions.

    Compiles a SymPy expression into a fast NumPy/Numba callable,
    separating data variables (columns present in the dataframe) from
    free parameters (to be supplied at evaluation time).

    Subclasses define *how* symbols map to dataframe columns
    (:meth:`_resolve_vars`) and *how* the corresponding arrays are
    extracted from a concrete dataframe (:meth:`_extract_data`).

    Parameters
    ----------
    functional_form : sympy.Expr or str
        The symbolic expression to compile.
    df_vars : Iterable[str]
        Column names available in the dataframe (used to distinguish
        data variables from parameters).

    Attributes
    ----------
    functional_form : sympy.Expr
    data_vars : list[str]
        Symbol names classified as data variables.
    parameter_vars : list[str]
        Symbol names classified as free parameters.
    base_lambda : callable
        NumPy-backed callable produced by ``sympy.lambdify``.
    numba_function : callable
        Numba-compiled version of *base_lambda* (falls back to
        *base_lambda* if compilation fails).
    """

    def __init__(self, functional_form: sympy.Expr | str, df_vars: Iterable[str]):
        if isinstance(functional_form, str):
            functional_form = sympy.sympify(functional_form)
        self.functional_form = functional_form
        self.data_vars, self.parameter_vars = self._resolve_vars(
            functional_form, list(df_vars)
        )

        # Compile expression: sympy → lambdify → numba
        all_vars = self.data_vars + self.parameter_vars
        self.base_lambda = sympy.lambdify(all_vars, self.functional_form, "numpy")
        try:
            self.numba_function = nb.njit(self.base_lambda)
        except Exception:
            self.numba_function = self.base_lambda

    # ------------------------------------------------------------------
    # Abstract interface
    # ------------------------------------------------------------------

    @abstractmethod
    def _resolve_vars(
        self, expr: sympy.Expr, df_vars: list[str]
    ) -> tuple[list[str], list[str]]:
        """
        Classify the free symbols of *expr* into data variables and parameters.

        Returns
        -------
        (data_vars, parameter_vars)
            Two lists of symbol-name strings.
        """
        ...

    @abstractmethod
    def _extract_data(self, df: pl.DataFrame) -> list[np.ndarray]:
        """
        Extract one NumPy array per data variable from *df*.

        The order **must** match :attr:`data_vars`.
        """
        ...

    # ------------------------------------------------------------------
    # Evaluation
    # ------------------------------------------------------------------

    def __call__(self, params: list[float], df: pl.DataFrame) -> np.ndarray:
        """
        Evaluate the compiled function.

        Parameters
        ----------
        params : list[float]
            Values for each symbol in :attr:`parameter_vars` (same order).
        df : pl.DataFrame
            Dataframe supplying the data variables.

        Returns
        -------
        np.ndarray
        """
        data_arrays = self._extract_data(df)
        return self.numba_function(*data_arrays, *params)


class IndividualFunction(CompiledFunction):
    """
    Compiled function for **individual-level** (person-level) data.

    Each row represents one individual.  Symbols map directly to column
    names: symbol ``yem`` reads column ``yem``.

    Parameters
    ----------
    functional_form : sympy.Expr or str
        The symbolic expression.
    df_vars : Iterable[str]
        Available column names.
    """

    def _resolve_vars(self, expr, df_vars):
        data_vars = []
        parameter_vars = []
        for sym in expr.free_symbols:
            name = str(sym)
            if name in df_vars:
                data_vars.append(name)
            else:
                parameter_vars.append(name)
        return sorted(data_vars), sorted(parameter_vars)

    def _extract_data(self, df):
        return [df[var].to_numpy() for var in self.data_vars]


class HouseholdMemberFunction(CompiledFunction):
    """
    Compiled function for **household-member-level** data.

    Each row is one individual, but the expression references specific
    members of a household using a positional suffix:

    * ``yem_0`` → column ``yem``, primary earner (rank 0)
    * ``yem_1`` → column ``yem``, secondary earner (rank 1)

    Members are ranked within each household by the columns given in
    *sort_cols* (default: ``idhh ASC, yem DESC, dag DESC``).

    Parameters
    ----------
    functional_form : sympy.Expr or str
        Symbolic expression using ``<col>_<index>`` notation.
    df_vars : Iterable[str]
        Available column names (without the ``_N`` suffix).
    members : int, optional
        Maximum number of household members to consider (default 2).
    sort_cols : list[str] | None
        Columns used to sort within each household.
    sort_descending : list[bool] | None
        Sort direction for each column in *sort_cols*.
    """

    def __init__(
        self,
        functional_form: sympy.Expr | str,
        df_vars: Iterable[str],
        members: int = 2,
        sort_cols: list[str] | None = None,
        sort_descending: list[bool] | None = None,
    ):
        self.members = members
        self.sort_cols = sort_cols or ["idhh", "yem", "dag"]
        self.sort_descending = sort_descending or [False, True, True]
        # data_vars_indexed is populated by _resolve_vars, called from super().__init__
        self.data_vars_indexed: list[tuple[int, str]] = []
        super().__init__(functional_form, df_vars)

    def _resolve_vars(self, expr, df_vars):
        data_vars = []
        data_vars_indexed = []
        parameter_vars = []

        for sym in expr.free_symbols:
            var_name = str(sym)
            found = False
            for i in range(self.members):
                suffix = f"_{i}"
                if var_name.endswith(suffix):
                    base_var = var_name[: -len(suffix)]
                    if base_var in df_vars:
                        data_vars.append(var_name)
                        data_vars_indexed.append((i, base_var))
                        found = True
                        break
            if not found:
                parameter_vars.append(var_name)

        self.data_vars_indexed = data_vars_indexed
        return data_vars, parameter_vars

    def _extract_data(self, df):
        df_sorted = df.sort(self.sort_cols, descending=self.sort_descending)
        data_arrays = []
        for index, var_name in self.data_vars_indexed:
            df_filtered = (
                df_sorted.with_columns(
                    pl.int_range(0, pl.count()).over("idhh").alias("_rn")
                )
                .filter(pl.col("_rn") == index)
                .drop("_rn")
            )
            data_arrays.append(df_filtered[var_name].to_numpy())
        return data_arrays


class HouseholdFunction(CompiledFunction):
    """
    Compiled function for **pre-aggregated household-level** data.

    Each row represents one household (already aggregated).  Symbols map
    directly to column names, just like :class:`IndividualFunction`, but
    the semantic unit is a household rather than a person.

    Use this when your dataframe has been collapsed to one row per
    household (e.g. via ``groupby('idhh').sum()``).

    Parameters
    ----------
    functional_form : sympy.Expr or str
        The symbolic expression.
    df_vars : Iterable[str]
        Available column names.
    """

    def _resolve_vars(self, expr, df_vars):
        data_vars = []
        parameter_vars = []
        for sym in expr.free_symbols:
            name = str(sym)
            if name in df_vars:
                data_vars.append(name)
            else:
                parameter_vars.append(name)
        return sorted(data_vars), sorted(parameter_vars)

    def _extract_data(self, df):
        return [df[var].to_numpy() for var in self.data_vars]


# Backward compatibility alias
CompiledHHFunction = HouseholdMemberFunction


FUNCTION_TYPES: dict[str, type] = {
    "individual": IndividualFunction,
    "household_member": HouseholdMemberFunction,
    "household": HouseholdFunction,
}


class Calibration:
    """
    Encapsulates a calibrated functional form with group-specific parameter estimates.

    Delegates compilation and data extraction to the :class:`CompiledFunction`
    hierarchy, selected via *function_type*:

    * ``"individual"`` — one row per person, symbols = column names.
    * ``"household_member"`` — one row per person, symbols use ``<col>_<N>`` notation.
    * ``"household"`` — one row per household (pre-aggregated), symbols = column names.

    Responsibilities:
    - Store symbolic functional form and its per-group fitted parameters.
    - Provide fast numeric evaluation (NumPy / Numba) via :class:`CompiledFunction`.
    - Export/import calibration metadata to/from JSON.
    - Evaluate fitted vs actual target values and compute diagnostics.
    """

    def __init__(self, model_version: str, system: str, dataset_id: str,
                 functional_form: sympy.Expr,
                 data_vars: List[str], parameter_vars: List[str],
                 estimates: Dict[str, Dict[str, float]],
                 target_variable: str,
                 function_type: str = "individual",
                 group_filters: dict[str, str] | None = None,
                 group_filters_pl: dict[str, pl.Expr] | None = None,
                 constantsToOverwrite: dict = {},
                 variable_labels: dict[str, str] | None = None):
        """
        Initialize a Calibration instance.

        Args:
            model_version: Model version identifier.
            system: System name / code.
            dataset_id: Dataset identifier used for calibration.
            functional_form: SymPy expression representing the functional form.
            data_vars: Names of variables (columns) used as data inputs.
            parameter_vars: Names of free parameters calibrated.
            estimates: Mapping of group -> {parameter_name: value}.
            target_variable: Name of target variable (column name or expression string).
            function_type: One of ``"individual"``, ``"household_member"``, ``"household"``.
            group_filters: Mapping of group -> SQL query (table name expected as 'self').
            group_filters_pl: Mapping of group -> Polars expression (alternative to SQL).
            constantsToOverwrite: Constants forwarded to underlying system execution.
            variable_labels: Human-readable labels for variable names and parameters.
                Mapping of symbol/column name to descriptive text, e.g.
                ``{"ils_base_tin": "Taxable income", "tau": "Progressivity"}``.
                Stored in the calibration and exported/imported with JSON.
        """
        self.system = system
        self.model_version = model_version
        self.target_variable = target_variable
        self.dataset_id = dataset_id
        self.constantsToOverwrite = constantsToOverwrite
        self.data_vars = data_vars
        self.parameter_vars = parameter_vars
        self.functional_form = functional_form
        self.function_type = function_type
        self.group_filters = group_filters or {}
        self.group_filters_pl = group_filters_pl
        self.variable_labels: dict[str, str] = variable_labels or {}

        # Pretty-printed forms with substituted parameters
        self.pretty_function: dict = {}
        for k, v in estimates.items():
            repl_dict = {sympy.Symbol(p): round(float(val), 4) for p, val in v.items()}
            self.pretty_function[k] = self.functional_form.xreplace(repl_dict)
        self.estimates = estimates

        # Build compiled function via CompiledFunction hierarchy
        self._compiled = self._build_compiled(self.functional_form, self.data_vars)
        self.base_lambda = self._compiled.base_lambda
        self.numba_function = self._compiled.numba_function

        # Pre-bake group lambdas with parameters already bound
        self.group_lambdas: dict = {}
        for group_key, param_estimates in self.estimates.items():
            param_values = [param_estimates[p] for p in self.parameter_vars]
            self.group_lambdas[group_key] = (
                lambda params: lambda *data_args: self.numba_function(*data_args, *params)
            )(param_values)

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _derive_df_columns(self, data_vars: list[str]) -> list[str]:
        """Derive the column names needed to construct the CompiledFunction."""
        if self.function_type == "household_member":
            base_cols: set[str] = set()
            for v in data_vars:
                for i in range(10):
                    suffix = f"_{i}"
                    if v.endswith(suffix):
                        base_cols.add(v[: -len(suffix)])
                        break
            return list(base_cols)
        return list(data_vars)

    def _build_compiled(self, expr: sympy.Expr, data_vars: list[str]) -> CompiledFunction:
        """Build the right CompiledFunction subclass for this calibration."""
        if isinstance(expr, str):
            expr = sympy.sympify(expr)
        df_columns = self._derive_df_columns(data_vars)
        cls = FUNCTION_TYPES[self.function_type]
        return cls(expr, df_columns)

    def _build_target_function(self, target_str: str, df_columns: list[str]) -> CompiledFunction:
        """Build a CompiledFunction for a target expression (no free parameters)."""
        cls = FUNCTION_TYPES[self.function_type]
        fn = cls(sympy.sympify(target_str), df_columns)
        if fn.parameter_vars:
            raise ValueError(
                f"Target expression '{target_str}' has free parameters "
                f"{fn.parameter_vars}; targets must be fully determined by data."
            )
        return fn

    # ------------------------------------------------------------------
    # Evaluation
    # ------------------------------------------------------------------

    def predict(self, group_key: str, df: pl.DataFrame) -> np.ndarray:
        """
        Compute fitted values for a specific group on *df*.

        Uses the compiled function with proper data extraction for the
        configured *function_type*.
        """
        params = [self.estimates[group_key][p] for p in self.parameter_vars]
        return np.asarray(self._compiled(params, df), dtype=np.float64)

    def __call__(
        self,
        data,
        group_key: str = "all",
    ) -> np.ndarray:
        """
        Evaluate the calibrated function on *data*.

        Convenience wrapper around :meth:`predict` that accepts multiple
        input formats:

        * **numpy array** — only when the calibration has a single data
          variable (the common case).
        * **dict of numpy arrays** — ``{var_name: array}`` for each
          variable in ``data_vars``.
        * **pandas DataFrame** — converted to Polars internally.
        * **polars DataFrame** — used directly.

        Parameters
        ----------
        data : numpy.ndarray | dict[str, numpy.ndarray] | pandas.DataFrame | polars.DataFrame
            Input data.  Must supply values for every variable in
            ``data_vars``.
        group_key : str
            Which parameter group to use (default ``"all"``).

        Returns
        -------
        numpy.ndarray
            Fitted values, one per row.

        Examples
        --------
        >>> import numpy as np
        >>> cal = org["BE"][2022]["HSV_taxben_all"]
        >>> cal(np.array([0.5, 1.0, 2.0]))
        array([...])  # predicted average tax rates
        """
        if group_key not in self.estimates:
            available = list(self.estimates.keys())
            raise KeyError(
                f"Group '{group_key}' not found. Available groups: {available}"
            )

        # --- numpy array (single data variable) -------------------------
        if isinstance(data, np.ndarray):
            if len(self.data_vars) != 1:
                raise ValueError(
                    f"Plain numpy array only supported when the calibration "
                    f"has exactly 1 data variable, but this one has "
                    f"{len(self.data_vars)}: {self.data_vars}.  "
                    f"Pass a dict or DataFrame instead."
                )
            data = pl.DataFrame({self.data_vars[0]: data})
            return self.predict(group_key, data)

        # --- dict of numpy arrays ---------------------------------------
        if isinstance(data, dict):
            missing = set(self.data_vars) - set(data.keys())
            if missing:
                raise KeyError(
                    f"Missing data variables: {sorted(missing)}.  "
                    f"Required: {self.data_vars}"
                )
            data = pl.DataFrame({k: data[k] for k in self.data_vars})
            return self.predict(group_key, data)

        # --- pandas DataFrame -------------------------------------------
        import pandas as pd
        if isinstance(data, pd.DataFrame):
            data = pl.from_pandas(data)

        return self.predict(group_key, data)

    def get_value_function(self, group_key: str, data: Dict[str, np.ndarray]):
        """
        Build a callable that evaluates the calibrated functional form for a specific group.

        Args:
            group_key: Group identifier.
            data: Mapping variable name -> NumPy array for each data variable.

        Returns:
            A zero-argument function returning the fitted values for provided data arrays.

        Raises:
            ValueError: If group_key has no associated calibration.
        """
        group_lambda = self.group_lambdas.get(group_key)
        data_listed = [data[x] for x in self.data_vars]
        if group_lambda is not None:
            return lambda *args: group_lambda(*data_listed)
        raise ValueError(f"No value function found for group: {group_key}")

    def to_json(self, path: str):
        """Serialize calibration metadata to JSON."""
        with open(path, "w") as json_file:
            json.dump({
                "model_version": self.model_version,
                "system": self.system,
                "dataset_id": self.dataset_id,
                "functional_form": str(self.functional_form),
                "constantsToOverwrite": self.constantsToOverwrite,
                "parameter_vars": self.parameter_vars,
                "data_vars": self.data_vars,
                "estimates": self.estimates,
                "target_variable": self.target_variable,
                "function_type": self.function_type,
                "group_filters": self.group_filters,
                "variable_labels": self.variable_labels,
            }, json_file, indent=2)

    @staticmethod
    def from_json(system: System | None, path: str) -> "Calibration":
        """
        Reconstruct a Calibration from a JSON file.

        Args:
            system: System object (currently unused; kept for interface symmetry).
            path: Path to JSON file.
        """
        with open(path, "r") as json_file:
            data = json.load(json_file)
            functional_form = sympy.sympify(data["functional_form"])
            return Calibration(
                model_version=data["model_version"],
                system=data["system"],
                dataset_id=data["dataset_id"],
                functional_form=functional_form,
                data_vars=data["data_vars"],
                parameter_vars=data["parameter_vars"],
                estimates=data["estimates"],
                target_variable=data.get("target_variable", ""),
                function_type=data.get("function_type", "individual"),
                group_filters=data.get("group_filters", {}),
                constantsToOverwrite=data.get("constantsToOverwrite", {}),
                variable_labels=data.get("variable_labels", {}),
            )

    def get_estimates(self):
        """Return raw parameter estimates: group -> {param: value}."""
        return self.estimates

    def __repr__(self):
        s = f"Calibration for system {self.system} using dataset {self.dataset_id}:\n"
        s += f"  function_type: {self.function_type}\n"
        target_label = self.variable_labels.get(self.target_variable, self.target_variable)
        s += f"  target: {target_label}\n"
        if self.variable_labels:
            s += "  variables:\n"
            for var_name in self.data_vars + self.parameter_vars:
                label = self.variable_labels.get(var_name, var_name)
                s += f"    {var_name}: {label}\n"
        for k, v in self.pretty_function.items():
            s += f"  Group {k}: {v}\n"
        # Usage instructions
        groups = list(self.estimates.keys())
        s += "\n  Usage:\n"
        s += f"    import numpy as np\n"
        if len(self.data_vars) == 1:
            s += f"    x = np.array([...])  # {self.data_vars[0]}\n"
            s += f"    result = cal(x)  # returns numpy array of fitted values\n"
        else:
            cols = ', '.join(f'"{c}": np.array([...])' for c in self.data_vars)
            s += f"    data = {{{cols}}}\n"
            s += f"    result = cal(data)  # returns numpy array of fitted values\n"
        if len(groups) > 1:
            s += f"    result = cal(x, group_key=\"{groups[0]}\")  # specify group\n"
        s += f"    # Also accepts: dict of arrays, pandas DataFrame, polars DataFrame\n"
        s += f"    # Available groups: {groups}\n"
        s += f"    # Required data variables: {self.data_vars}\n"
        return s

    def evaluate(self,
                 output_data: pl.DataFrame,
                 target_variable: str | None = None,
                 group_filters: dict[str, str] | None = None,
                 system: System | None = None,
                 dataset_id: str | None = None) -> tuple[pl.DataFrame, pl.DataFrame]:
        """
        Evaluate fitted function against actual target values per group.

        The *target_variable* can be either a column name present in *output_data*
        or a SymPy-parseable expression string (e.g. ``"ils_taxin_0/ils_base_tin_0"``).
        In the latter case it is compiled using the same *function_type* as the
        calibrated expression.

        Args:
            output_data: DataFrame containing simulation outputs.
            target_variable: Column name **or** expression string.  Falls back to
                ``self.target_variable`` when *None*.
            group_filters: Mapping group -> SQL filter string; falls back to stored filters.
            system: Unused (kept for backward compatibility).
            dataset_id: Optional override of dataset identifier.

        Returns:
            metrics_df: DataFrame with per-group metrics (n, mse, rmse, mae, r2).
            comparison_df: Long-form DataFrame with columns (group, target, fitted, residual).
        """
        if dataset_id is None:
            dataset_id = self.dataset_id
        if not group_filters:
            group_filters = self.group_filters
        target_variable = target_variable or self.target_variable

        # Determine whether target is a simple column or an expression
        target_is_column = target_variable in output_data.columns

        metrics_rows: list[dict] = []
        comparison_frames: list[pl.DataFrame] = []

        output_data_lazy = output_data.lazy()

        for group_name, sql_filter in group_filters.items():
            if group_name not in self.estimates:
                continue

            group_df = output_data_lazy.sql(sql_filter).collect()
            if group_df.height == 0:
                continue

            # --- Fitted values (via CompiledFunction) ---
            params = [self.estimates[group_name][p] for p in self.parameter_vars]
            fitted = np.asarray(self._compiled(params, group_df), dtype=np.float64)

            # --- Actual target values ---
            if target_is_column:
                actual = group_df[target_variable].to_numpy().astype(np.float64)
            else:
                target_fn = self._build_target_function(target_variable, group_df.columns)
                actual = np.asarray(target_fn([], group_df), dtype=np.float64)

            residual = fitted - actual
            n = actual.size
            mse = float(np.mean(residual ** 2))
            rmse = float(np.sqrt(mse))
            mae = float(np.mean(np.abs(residual)))
            ss_res = float(np.sum(residual ** 2))
            ss_tot = float(np.sum((actual - actual.mean()) ** 2))
            r2 = float(1 - ss_res / ss_tot) if ss_tot > 0 else float("nan")

            metrics_rows.append({
                "group": group_name, "n": n,
                "mse": mse, "rmse": rmse, "mae": mae, "r2": r2,
            })
            comparison_frames.append(pl.DataFrame({
                "group": [group_name] * n,
                "target": actual, "fitted": fitted, "residual": residual,
            }))

        empty_metrics = {"group": [], "n": [], "mse": [], "rmse": [], "mae": [], "r2": []}
        empty_comp = {"group": [], "target": [], "fitted": [], "residual": []}
        metrics_df = pl.DataFrame(metrics_rows) if metrics_rows else pl.DataFrame(empty_metrics)
        comparison_df = (
            pl.concat(comparison_frames, how="vertical") if comparison_frames
            else pl.DataFrame(empty_comp)
        )
        return metrics_df, comparison_df



def calibrate_expression(
    expression: sympy.Expr | str,
    target: str | sympy.Expr,
    data,
    function_type: str = "individual",
    group_filters: dict[str, str] | None = None,
    group_filters_pl: dict[str, pl.Expr] | None = None,
    initial_params: list[float] | None = None,
    bounds: list[tuple[float, float]] | None = None,
    method: str = "Nelder-Mead",
    solver_shgo: bool = False,
    weight_var: str | None = None,
    model_version: str = "",
    system_name: str = "",
    dataset_id: str = "",
    constantsToOverwrite: dict | None = None,
    variable_labels: dict[str, str] | None = None,
) -> tuple[Calibration, pl.DataFrame]:
    """
    General-purpose calibration entry point.

    Calibrates *expression* against *target* on *data* with the specified
    *function_type* data-extraction mode.  Returns a :class:`Calibration`
    object ready for JSON export and a reference to the (unchanged) data.

    Args:
        expression: SymPy expression (or parseable string) representing the
            functional form to calibrate.  Free symbols not found as data
            columns are treated as parameters.
        target: Target expression or column name (string or SymPy).
        data: Polars (or pandas) DataFrame with simulation data.  If a pandas
            DataFrame is passed it is converted to Polars automatically.
        function_type: ``"individual"`` | ``"household_member"`` | ``"household"``.
        group_filters: Mapping group_name -> SQL string (table alias ``self``).
        group_filters_pl: Mapping group_name -> ``polars.Expr`` filter.
        initial_params: Starting values for the optimizer.  Defaults to
            ``[0.1] * n_params``.
        bounds: Sequence of ``(lo, hi)`` per parameter (required when
            *solver_shgo* is True).
        method: ``scipy.optimize.minimize`` method name.
        solver_shgo: If True, use ``scipy.optimize.shgo`` as global solver.
        weight_var: Optional column name for observation weights (e.g. ``"dwt"``).
        model_version: Metadata stored in the resulting Calibration.
        system_name: Metadata stored in the resulting Calibration.
        dataset_id: Metadata stored in the resulting Calibration.
        constantsToOverwrite: Metadata forwarded to the Calibration.
        variable_labels: Human-readable labels for variable names and parameters.
            Mapping of symbol/column name to descriptive text, e.g.
            ``{"ils_base_tin": "Taxable income", "tau": "Progressivity"}``.
            Stored in the calibration and exported/imported with JSON.

    Returns:
        (calibration, data): The fitted :class:`Calibration` and the input data.
    """
    import pandas as pd
    as_pd = False
    if isinstance(data, pd.DataFrame):
        data = pl.from_pandas(data)
        as_pd = True

    if isinstance(expression, str):
        expression = sympy.sympify(expression)
    target_str = str(target)
    if isinstance(target, str):
        target = sympy.sympify(target)

    constantsToOverwrite = constantsToOverwrite or {}

    cls = FUNCTION_TYPES[function_type]

    # Compile expression & target
    approx_fn = cls(expression, data.columns)
    target_fn = cls(target, data.columns)
    if target_fn.parameter_vars:
        raise ValueError(
            f"Target expression '{target_str}' has free parameters "
            f"{target_fn.parameter_vars}; targets must be fully determined by data."
        )

    n_params = len(approx_fn.parameter_vars)
    if initial_params is None:
        initial_params = [0.1] * n_params

    # Weight function (optional)
    weight_fn = None
    if weight_var and weight_var in data.columns:
        weight_fn = cls(sympy.sympify(weight_var), data.columns)

    # Group setup
    if group_filters_pl:
        grouper = group_filters_pl
        use_sql = False
    else:
        grouper = group_filters or {"all": "SELECT * FROM self"}
        use_sql = True
    if group_filters is None:
        group_filters = {"all": "SELECT * FROM self"}

    estimates: dict[str, dict[str, float]] = {}
    data_lazy = data.lazy()

    for group_name, group_filter in grouper.items():
        if use_sql:
            group_data = data_lazy.sql(str(group_filter)).collect()
        else:
            group_data = data_lazy.filter(group_filter).collect()

        if group_data.height == 0:
            print(f"  SKIP [{group_name}]: no records after filter")
            continue

        # Pre-compute target & weight arrays (constant during optimization)
        target_arr = np.asarray(target_fn([], group_data), dtype=np.float64)
        weight_arr = (
            np.asarray(weight_fn([], group_data), dtype=np.float64)
            if weight_fn else np.ones(target_arr.shape[0])
        )

        def objective(params, _gd=group_data, _ta=target_arr, _wa=weight_arr):
            fitted = approx_fn(list(params), _gd)
            if not np.all(np.isfinite(fitted)):
                return np.inf
            residual = _ta - fitted
            val = np.sum((residual ** 2) * _wa) / _wa.sum()
            return val if np.isfinite(val) else np.inf

        print(f"  Calibrating [{group_name}]  n={group_data.height}  params={n_params}")

        if solver_shgo:
            shgo_bounds = bounds or [(0.001, 2.0)] * n_params
            result = sp.optimize.shgo(
                objective,
                bounds=shgo_bounds,
                minimizer_kwargs={"method": method},
                options={"infty_constraints": False},
            )
        else:
            kw: dict = {
                "fun": objective,
                "x0": np.array(initial_params, dtype=float),
                "method": method,
                "options": {"maxiter": 10_000},
            }
            if bounds is not None:
                kw["bounds"] = bounds
            result = sp.optimize.minimize(**kw)

        if result.success:
            calibrated = dict(
                zip(approx_fn.parameter_vars, [float(x) for x in result.x])
            )
            estimates[group_name] = calibrated
            print(f"    -> {calibrated}")
        else:
            print(f"    WARNING: optimization did not converge — {result.message}")

    cal = Calibration(
        model_version=model_version,
        system=system_name,
        dataset_id=dataset_id,
        functional_form=expression,
        data_vars=approx_fn.data_vars,
        parameter_vars=approx_fn.parameter_vars,
        estimates=estimates,
        target_variable=target_str,
        function_type=function_type,
        group_filters=group_filters,
        group_filters_pl=group_filters_pl,
        constantsToOverwrite=constantsToOverwrite,
        variable_labels=variable_labels,
    )
    return cal, data


def calibrate_function(system, data, dataset_id, target_variable,
                       base, functional_form: sympy.Expr,
                       group_filters: dict[str, str] | None = None,
                       group_filters_pl: dict[str, pl.Expr] | None = None,
                       constantsToOverwrite: dict | None = None,
                       output_data: pl.DataFrame | None = None):
    """
    Legacy calibration entry point (individual-level).

    Runs the EUROMOD *system* on *data* (or uses *output_data* if provided),
    then delegates to :func:`calibrate_expression` with
    ``function_type="individual"``.

    Returns:
        (Calibration, output_data)
    """
    constantsToOverwrite = constantsToOverwrite or {}
    if output_data is None:
        simulation = system.run(data, dataset_id,
                                constantsToOverwrite=constantsToOverwrite)
        output_data = simulation.outputs[0]

    if not isinstance(functional_form, sympy.Expr):
        raise ValueError("functional_form must be a sympy expression")

    group_filters = group_filters or {"all": "SELECT * FROM self"}
    group_filters_pl = group_filters_pl or {}

    cal, _ = calibrate_expression(
        expression=functional_form,
        target=target_variable,
        data=output_data,
        function_type="individual",
        group_filters=group_filters,
        group_filters_pl=group_filters_pl if group_filters_pl else None,
        method="Nelder-Mead",
        model_version=getattr(getattr(system, "parent", None), "model", type("", (), {"model_path": ""})()).model_path
            if hasattr(system, "parent") else "",
        system_name=getattr(system, "name", str(system)),
        dataset_id=dataset_id,
        constantsToOverwrite=constantsToOverwrite,
    )
    return cal, output_data

def plot_calibration_comparison_plotly(
    output_data: pl.DataFrame,
    target_variable: str,
    calibrations: list[tuple[str, Calibration]],
    group_filters: dict[str, str],
    sample_size: int | None = 10000
):
    """
    Create interactive Plotly plots comparing actual vs fitted for multiple calibrations.
    
    Args:
        output_data: Simulation output data
        target_variable: Name of target variable
        calibrations: List of (name, Calibration) tuples
        group_filters: Dictionary of SQL queries for group filters
        sample_size: Max points to plot (None = all)
    """
    import plotly.graph_objects as go
    from plotly.subplots import make_subplots
    
    # Evaluate all calibrations
    results = []
    for name, calib in calibrations:
        metrics_df, comparison_df = calib.evaluate(
            output_data=output_data,
            target_variable=target_variable,
            group_filters=group_filters
        )
        results.append((name, metrics_df, comparison_df))
    
    # Create subplots: one per group
    groups = list(group_filters.keys())
    n_groups = len(groups)
    
    fig = make_subplots(
        rows=n_groups,
        cols=1,
        subplot_titles=[f"Group: {g}" for g in groups],
        vertical_spacing=0.08
    )
    
    colors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd', '#8c564b']
    
    output_data_lazy = output_data.lazy()
    
    for group_idx, group_name in enumerate(groups):
        row = group_idx + 1
        
        # Get actual values for this group using SQL query
        group_data = output_data_lazy.sql(group_filters[group_name]).collect()
        if sample_size and group_data.height > sample_size:
            group_data = group_data.sample(n=sample_size, shuffle=True, seed=42)
        
        actual = group_data[target_variable].to_numpy()
        
        # Plot actual data as scatter
        fig.add_trace(
            go.Scatter(
                x=actual,
                y=actual,
                mode='markers',
                name=f'Actual ({group_name})',
                marker=dict(size=4, color='gray', opacity=0.3),
                showlegend=(group_idx == 0),
                legendgroup='actual'
            ),
            row=row,
            col=1
        )
        
        # Plot each calibration's fitted values
        for calib_idx, (calib_name, metrics_df, comparison_df) in enumerate(results):
            # Filter comparison data for this group
            group_comp = comparison_df.filter(pl.col("group") == group_name)
            
            if group_comp.height == 0:
                continue
            
            if sample_size and group_comp.height > sample_size:
                group_comp = group_comp.sample(n=sample_size, shuffle=True, seed=42)
            
            actual_vals = group_comp["target"].to_numpy()
            fitted_vals = group_comp["fitted"].to_numpy()
            
            color = colors[calib_idx % len(colors)]
            
            fig.add_trace(
                go.Scatter(
                    x=actual_vals,
                    y=fitted_vals,
                    mode='markers',
                    name=f'{calib_name} ({group_name})',
                    marker=dict(size=4, color=color, opacity=0.5),
                    showlegend=(group_idx == 0),
                    legendgroup=calib_name
                ),
                row=row,
                col=1
            )
        
        # Add diagonal reference line
        if group_data.height > 0:
            min_val = float(min(actual.min(), *[
                comparison_df.filter(pl.col("group") == group_name)["fitted"].min()
                for _, _, comparison_df in results
                if comparison_df.filter(pl.col("group") == group_name).height > 0
            ]))
            max_val = float(max(actual.max(), *[
                comparison_df.filter(pl.col("group") == group_name)["fitted"].max()
                for _, _, comparison_df in results
                if comparison_df.filter(pl.col("group") == group_name).height > 0
            ]))
            
            fig.add_trace(
                go.Scatter(
                    x=[min_val, max_val],
                    y=[min_val, max_val],
                    mode='lines',
                    line=dict(color='black', dash='dash', width=1),
                    showlegend=False,
                    hoverinfo='skip'
                ),
                row=row,
                col=1
            )
        
        # Update axes
        fig.update_xaxes(title_text=f"Actual {target_variable}", row=row, col=1)
        fig.update_yaxes(title_text="Fitted", row=row, col=1)
    
    # Update layout
    fig.update_layout(
        height=400 * n_groups,
        title_text="Calibration Comparison: Actual vs Fitted",
        showlegend=True,
        hovermode='closest'
    )
    
    return fig


def plot_residuals_plotly(
    calibrations: list[tuple[str, Calibration]],
    output_data: pl.DataFrame,
    target_variable: str,
    group_filters: dict[str, str]
):
    """
    Create interactive Plotly histograms of residuals for each calibration.
    
    Args:
        group_filters: Dictionary of SQL queries for group filters
    """
    import plotly.graph_objects as go
    from plotly.subplots import make_subplots
    
    n_calibs = len(calibrations)
    groups = list(group_filters.keys())
    n_groups = len(groups)
    
    fig = make_subplots(
        rows=n_groups,
        cols=n_calibs,
        subplot_titles=[f"{calib_name} - {group}" 
                       for group in groups 
                       for calib_name, _ in calibrations],
        vertical_spacing=0.08,
        horizontal_spacing=0.05
    )
    
    for calib_idx, (calib_name, calib) in enumerate(calibrations):
        metrics_df, comparison_df = calib.evaluate(
            output_data=output_data,
            target_variable=target_variable,
            group_filters=group_filters
        )
        
        for group_idx, group_name in enumerate(groups):
            group_comp = comparison_df.filter(pl.col("group") == group_name)
            
            if group_comp.height == 0:
                continue
            
            residuals = group_comp["residual"].to_numpy()
            
            fig.add_trace(
                go.Histogram(
                    x=residuals,
                    nbinsx=50,
                    name=f"{calib_name} - {group_name}",
                    showlegend=False,
                    marker_color=f'rgba({31 + calib_idx * 50}, {119 + calib_idx * 50}, {180}, 0.7)'
                ),
                row=group_idx + 1,
                col=calib_idx + 1
            )
            
            fig.update_xaxes(title_text="Residual", row=group_idx + 1, col=calib_idx + 1)
            fig.update_yaxes(title_text="Count", row=group_idx + 1, col=calib_idx + 1)
    
    fig.update_layout(
        height=300 * n_groups,
        title_text="Residual Distributions",
        showlegend=False
    )
    
    return fig


def plot_metrics_comparison_plotly(
    calibrations: list[tuple[str, Calibration]],
    output_data: pl.DataFrame,
    target_variable: str,
    group_filters: dict[str, str]
):
    """
    Create bar chart comparing metrics (RMSE, MAE, R²) across calibrations.
    
    Args:
        group_filters: Dictionary of SQL queries for group filters
    """
    import plotly.graph_objects as go
    from plotly.subplots import make_subplots
    
    # Collect metrics for all calibrations
    all_metrics = []
    for calib_name, calib in calibrations:
        metrics_df, _ = calib.evaluate(
            output_data=output_data,
            target_variable=target_variable,
            group_filters=group_filters
        )
        metrics_df = metrics_df.with_columns(pl.lit(calib_name).alias("calibration"))
        all_metrics.append(metrics_df)
    
    combined_metrics = pl.concat(all_metrics, how="vertical")
    
    # Create subplots for different metrics
    fig = make_subplots(
        rows=1,
        cols=3,
        subplot_titles=("RMSE", "MAE", "R²"),
        horizontal_spacing=0.1
    )
    
    groups = combined_metrics["group"].unique().to_list()
    calibs = combined_metrics["calibration"].unique().to_list()
    
    colors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd']
    
    for metric_idx, metric in enumerate(["rmse", "mae", "r2"]):
        for calib_idx, calib_name in enumerate(calibs):
            calib_data = combined_metrics.filter(pl.col("calibration") == calib_name)
            
            values = [
                float(calib_data.filter(pl.col("group") == g)[metric].to_numpy()[0])
                if calib_data.filter(pl.col("group") == g).height > 0
                else 0
                for g in groups
            ]
            
            fig.add_trace(
                go.Bar(
                    name=calib_name,
                    x=groups,
                    y=values,
                    marker_color=colors[calib_idx % len(colors)],
                    showlegend=(metric_idx == 0)
                ),
                row=1,
                col=metric_idx + 1
            )
        
        fig.update_xaxes(title_text="Group", row=1, col=metric_idx + 1)
        fig.update_yaxes(title_text=metric.upper(), row=1, col=metric_idx + 1)
    
    fig.update_layout(
        height=400,
        title_text="Metrics Comparison",
        barmode='group'
    )
    
    return fig


# Add to __main__ section after calibrations:
if __name__ == "__main__":
    model_path= r"R:\B2\04 - EUROMOD JRC\06 - Working area\01 - Common\15 - Release checks\2025Q1\model\EUROMOD_MASTER_VERSION_J0.51"
    mod = Model(model_path)
    repository = r"R:\B2\04 - EUROMOD JRC\01 - Repository\03 - Datasets\All data\All countries"
    #example calibration
    import pandas as pd
    dataset = mod["HU"][-1].bestmatch_datasets[0].name
    df_pandas = pd.read_csv(os.path.join(repository,dataset + ".txt"),sep="\t")
    df = pl.from_pandas(df_pandas)
    
    # Define filters as SQL queries with table 'self'
    group_filters_sql = {
        "single_person_hh": "SELECT * FROM self WHERE idhh IN (SELECT idhh FROM (SELECT idhh, COUNT(idperson) as n_person FROM self GROUP BY idhh) as t WHERE t.n_person = 1)" ,
        "multi_person_hh": "SELECT * FROM self WHERE idhh IN (SELECT idhh FROM (SELECT idhh, COUNT(idperson) as n_person FROM (SELECT * FROM self WHERE yem > 0) as h GROUP BY idhh) as t WHERE t.n_person > 1)",
        "all": "SELECT * FROM self"
    }
    
    calibration,output_data = calibrate_function(
        mod["HU"][-1],
        df,
        dataset,
        "ils_taxin",
        None,
        sympy.sympify("ils_base_tin - g*ils_base_tin**(1-tau)"),
        group_filters_sql
    )
    print(calibration)

    evaluation = calibration.evaluate(
        output_data=output_data,
        target_variable="ils_taxin",
        group_filters=group_filters_sql
    )

    print(evaluation)

    calibration2,output_data = calibrate_function(
        mod["HU"][-1],
        df,
        dataset,
        "ils_taxin",
        None,
        sympy.sympify(" a_4*(ils_base_tin/100000)**4+ a_3*(ils_base_tin/100000)**3 + a_2*(ils_base_tin/100000)**2 + a_1*(ils_base_tin/100000) + a_0"),
        group_filters_sql
    )
    print(calibration2)

    evaluation2 = calibration2.evaluate(
        output_data=output_data,
        target_variable="ils_taxin",
        group_filters=group_filters_sql
    )

    print(evaluation2)

    # Create visualizations
    calibrations_list = [
        ("HSV function", calibration),
        ("Polynomial", calibration2)
    ]
    
    # Plot 1: Actual vs Fitted comparison
    fig1 = plot_calibration_comparison_plotly(
        output_data=output_data,
        target_variable="ils_taxin",
        calibrations=calibrations_list,
        group_filters=group_filters_sql,
        sample_size=5000
    )
    fig1.show()
    
    # Plot 2: Residual distributions
    fig2 = plot_residuals_plotly(
        calibrations=calibrations_list,
        output_data=output_data,
        target_variable="ils_taxin",
        group_filters=group_filters_sql
    )
    fig2.show()
    
    # Plot 3: Metrics comparison
    fig3 = plot_metrics_comparison_plotly(
        calibrations=calibrations_list,
        output_data=output_data,
        target_variable="ils_taxin",
        group_filters=group_filters_sql
    )
    fig3.show()


    hh_data = df.lazy().sql(group_filters_sql["multi_person_hh"]).collect()
    fun = CompiledHHFunction(sympy.sympify("(yem_0 +  yem_1)*a"),df.columns)

    

    
    
