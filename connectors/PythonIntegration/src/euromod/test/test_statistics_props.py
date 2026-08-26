"""Property-based tests for the statistics module.

Uses Hypothesis to verify properties of the DataFrame-to-CLR conversion
logic, focusing on pandas/polars input-output symmetry.
"""
# Feature: python-statistics-module, Property 16: Pandas/polars input-output symmetry

import os
import sys
import importlib
import json
import math
import numpy as np
import pandas as pd
import polars as pl
import pytest
from unittest.mock import patch, MagicMock

from hypothesis import given, settings, assume, HealthCheck
from hypothesis import strategies as st


# ---------------------------------------------------------------------------
# Strategies
# ---------------------------------------------------------------------------

# Strategy for valid column names (alphanumeric, no duplicates)
_col_name_chars = st.sampled_from(
    "abcdefghijklmnopqrstuvwxyz_0123456789"
)
st_col_name = st.text(_col_name_chars, min_size=1, max_size=12).filter(
    lambda s: not s[0].isdigit()
)

# Strategy for numeric column data (finite floats only for reliable comparison)
st_numeric_value = st.floats(
    min_value=-1e12, max_value=1e12, allow_nan=False, allow_infinity=False
)

# Strategy for non-numeric column data (strings)
st_string_value = st.text(
    st.sampled_from("abcdefghijklmnopqrstuvwxyz "), min_size=1, max_size=10
)


@st.composite
def st_mixed_dataframe(draw):
    """Generate a DataFrame specification with both numeric and non-numeric columns.

    Returns a dict with:
      - numeric_cols: dict[str, list[float]] for numeric columns
      - string_cols: dict[str, list[str]] for non-numeric columns
      - num_rows: int
    At least one numeric column is always present.
    """
    num_rows = draw(st.integers(min_value=1, max_value=50))
    num_numeric_cols = draw(st.integers(min_value=1, max_value=8))
    num_string_cols = draw(st.integers(min_value=0, max_value=4))

    # Generate unique column names
    all_col_names = draw(
        st.lists(
            st_col_name,
            min_size=num_numeric_cols + num_string_cols,
            max_size=num_numeric_cols + num_string_cols,
            unique=True,
        )
    )

    numeric_col_names = all_col_names[:num_numeric_cols]
    string_col_names = all_col_names[num_numeric_cols:]

    numeric_cols = {}
    for name in numeric_col_names:
        values = draw(
            st.lists(st_numeric_value, min_size=num_rows, max_size=num_rows)
        )
        numeric_cols[name] = values

    string_cols = {}
    for name in string_col_names:
        values = draw(
            st.lists(st_string_value, min_size=num_rows, max_size=num_rows)
        )
        string_cols[name] = values

    return {
        "numeric_cols": numeric_cols,
        "string_cols": string_cols,
        "num_rows": num_rows,
    }


# ---------------------------------------------------------------------------
# Module import helper (mocks CLR dependencies)
# ---------------------------------------------------------------------------

def _import_statistics_mocked():
    """Import statistics module with CLR mocked out for testing Python-side logic."""
    if 'euromod.statistics' in sys.modules:
        del sys.modules['euromod.statistics']

    mock_clr = MagicMock()

    # Mock asNetArray to return the numpy array unchanged (identity for testing)
    mock_as_net_array = MagicMock(side_effect=lambda x: x)

    with patch.dict('sys.modules', {
        'clr': mock_clr,
    }):
        original_exists = os.path.exists

        def patched_exists(path):
            if 'EM_Statistics.dll' in str(path):
                return True
            return original_exists(path)

        with patch('os.path.exists', side_effect=patched_exists):
            mock_em_stats_module = MagicMock()
            sys.modules['EM_Statistics'] = mock_em_stats_module
            sys.modules['EM_Statistics.ExternalStatistics'] = MagicMock()

            mock_clr_convert = MagicMock()
            mock_clr_convert.asNetArray = mock_as_net_array
            sys.modules['euromod.utils.clr_array_convert'] = mock_clr_convert
            sys.modules['euromod.utils'] = MagicMock()

            try:
                import euromod.statistics as stats_module
                importlib.reload(stats_module)
                stats_module.asNetArray = mock_as_net_array
                return stats_module
            finally:
                if 'EM_Statistics' in sys.modules:
                    del sys.modules['EM_Statistics']
                if 'EM_Statistics.ExternalStatistics' in sys.modules:
                    del sys.modules['EM_Statistics.ExternalStatistics']
                if 'euromod.utils.clr_array_convert' in sys.modules:
                    del sys.modules['euromod.utils.clr_array_convert']


# Import once at module level for property tests
_stats_module = _import_statistics_mocked()


# ---------------------------------------------------------------------------
# Property-based tests
# ---------------------------------------------------------------------------

# **Validates: Requirements 7.2, 7.4**


class TestDataFrameConversionProperty:
    """Property 16: Pandas/polars input-output symmetry.

    For any valid simulation data represented as both a pandas DataFrame and a
    polars DataFrame, _dataframe_to_clr_data produces the same column names and
    numerically equivalent arrays from both. Non-numeric columns are filtered
    out consistently.
    """

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_pandas_returns_correct_columns_and_shape(self, data):
        """Given a random pandas DataFrame with numeric columns,
        _dataframe_to_clr_data returns the correct column names and an array
        with shape [num_cols, num_rows].

        **Validates: Requirements 7.2**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]
        num_rows = data["num_rows"]

        # Build pandas DataFrame with all columns
        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)

        cols, arr = _stats_module._dataframe_to_clr_data(df_pd)

        # Column names should be exactly the numeric columns (order may differ)
        assert set(cols) == set(numeric_cols.keys())

        # Shape should be [num_numeric_cols, num_rows]
        assert arr.shape == (len(numeric_cols), num_rows)

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_polars_returns_correct_columns_and_shape(self, data):
        """Given a random polars DataFrame with numeric columns,
        _dataframe_to_clr_data returns the correct column names and an array
        with shape [num_cols, num_rows].

        **Validates: Requirements 7.2**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]
        num_rows = data["num_rows"]

        # Build polars DataFrame with all columns
        all_data = {**numeric_cols, **string_cols}
        df_pl = pl.DataFrame(all_data)

        cols, arr = _stats_module._dataframe_to_clr_data(df_pl)

        # Column names should be exactly the numeric columns
        assert set(cols) == set(numeric_cols.keys())

        # Shape should be [num_numeric_cols, num_rows]
        assert arr.shape == (len(numeric_cols), num_rows)

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_pandas_polars_symmetry_column_names(self, data):
        """Given the same data as both a pandas and polars DataFrame,
        _dataframe_to_clr_data returns the same column names from both.

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)
        df_pl = pl.DataFrame(all_data)

        cols_pd, _ = _stats_module._dataframe_to_clr_data(df_pd)
        cols_pl, _ = _stats_module._dataframe_to_clr_data(df_pl)

        assert set(cols_pd) == set(cols_pl)

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_pandas_polars_symmetry_numeric_values(self, data):
        """Given the same numeric data as both a pandas and polars DataFrame,
        _dataframe_to_clr_data produces numerically equivalent arrays (lossless
        conversion).

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)
        df_pl = pl.DataFrame(all_data)

        cols_pd, arr_pd = _stats_module._dataframe_to_clr_data(df_pd)
        cols_pl, arr_pl = _stats_module._dataframe_to_clr_data(df_pl)

        # Align arrays by column name for comparison (order may differ)
        for col_name in cols_pd:
            idx_pd = cols_pd.index(col_name)
            idx_pl = cols_pl.index(col_name)
            np.testing.assert_array_equal(
                arr_pd[idx_pd],
                arr_pl[idx_pl],
                err_msg=f"Mismatch for column '{col_name}'",
            )

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_non_numeric_columns_filtered_consistently(self, data):
        """Non-numeric columns are excluded from results consistently for both
        pandas and polars.

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)
        df_pl = pl.DataFrame(all_data)

        cols_pd, _ = _stats_module._dataframe_to_clr_data(df_pd)
        cols_pl, _ = _stats_module._dataframe_to_clr_data(df_pl)

        # No string columns should appear in the output
        for str_col in string_cols.keys():
            assert str_col not in cols_pd, f"String col '{str_col}' in pandas result"
            assert str_col not in cols_pl, f"String col '{str_col}' in polars result"

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_conversion_is_lossless(self, data):
        """Values in the input DataFrame are preserved exactly in the output
        array (lossless conversion).

        **Validates: Requirements 7.2**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)

        cols, arr = _stats_module._dataframe_to_clr_data(df_pd)

        # Verify each value matches the original input
        for col_name, original_values in numeric_cols.items():
            idx = cols.index(col_name)
            for row_idx, expected_val in enumerate(original_values):
                actual_val = arr[idx, row_idx]
                assert actual_val == expected_val, (
                    f"Mismatch at col='{col_name}', row={row_idx}: "
                    f"expected {expected_val}, got {actual_val}"
                )



class TestSimulationToClrDataCaching:
    """Property 16 (extended): _simulation_to_clr_data uses cached CLR data when available.

    For any valid simulation data, if the Simulation has cached CLR data
    (_clr_data dict), _simulation_to_clr_data SHALL return that cached data
    directly without re-converting from the DataFrame. This validates that
    the pandas/polars symmetry extends through the simulation-level function.

    **Validates: Requirements 7.2, 7.4**
    """

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_cached_clr_data_is_preferred_over_dataframe(self, data):
        """When a Simulation has _clr_data, _simulation_to_clr_data returns
        the cached data without touching the DataFrame.

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        num_rows = data["num_rows"]

        # Create a fake "cached" CLR data entry
        cached_var_names = list(numeric_cols.keys())
        cached_array = np.zeros((len(cached_var_names), num_rows), dtype=np.float64)
        for i, col_name in enumerate(cached_var_names):
            cached_array[i] = numeric_cols[col_name]

        # Create a mock simulation with _clr_data set
        mock_sim = MagicMock()
        mock_sim._clr_data = {"output_0": (cached_var_names, cached_array)}
        mock_sim.output_filenames = ["output_0"]

        # The DataFrame on .outputs should NOT be accessed if cache is used
        mock_df = MagicMock()
        mock_sim.outputs.__getitem__ = MagicMock(return_value=mock_df)

        cols, arr = _stats_module._simulation_to_clr_data(mock_sim, output_index=0)

        # Should return cached data
        assert set(cols) == set(cached_var_names)
        np.testing.assert_array_equal(arr, cached_array)
        # Verify that outputs was NOT accessed
        mock_sim.outputs.__getitem__.assert_not_called()

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_fallback_to_dataframe_when_no_cache(self, data):
        """When a Simulation has no _clr_data, _simulation_to_clr_data
        falls back to converting the DataFrame.

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)

        # Create a mock simulation WITHOUT _clr_data
        mock_sim = MagicMock()
        mock_sim._clr_data = {}  # Empty cache
        mock_sim.output_filenames = ["output_0"]
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df_pd)

        cols, arr = _stats_module._simulation_to_clr_data(mock_sim, output_index=0)

        # Should have fallen back to DataFrame conversion
        assert set(cols) == set(numeric_cols.keys())
        assert arr.shape == (len(numeric_cols), data["num_rows"])
        mock_sim.outputs.__getitem__.assert_called_once_with(0)

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_pandas_simulation_symmetry_through_simulation_func(self, data):
        """For a pandas-based Simulation, _simulation_to_clr_data produces the
        same result as calling _dataframe_to_clr_data directly on the DataFrame.

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pd = pd.DataFrame(all_data)

        # Direct call
        direct_cols, direct_arr = _stats_module._dataframe_to_clr_data(df_pd)

        # Via _simulation_to_clr_data (no cache)
        mock_sim = MagicMock()
        del mock_sim._clr_data  # Ensure hasattr returns False
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df_pd)

        sim_cols, sim_arr = _stats_module._simulation_to_clr_data(mock_sim, output_index=0)

        assert set(direct_cols) == set(sim_cols)
        # Compare values aligned by column name
        for col_name in direct_cols:
            idx_d = direct_cols.index(col_name)
            idx_s = sim_cols.index(col_name)
            np.testing.assert_array_equal(direct_arr[idx_d], sim_arr[idx_s])

    @given(data=st_mixed_dataframe())
    @settings(max_examples=100)
    def test_polars_simulation_symmetry_through_simulation_func(self, data):
        """For a polars-based Simulation, _simulation_to_clr_data produces the
        same result as calling _dataframe_to_clr_data directly on the DataFrame.

        **Validates: Requirements 7.2, 7.4**
        """
        numeric_cols = data["numeric_cols"]
        string_cols = data["string_cols"]

        all_data = {**numeric_cols, **string_cols}
        df_pl = pl.DataFrame(all_data)

        # Direct call
        direct_cols, direct_arr = _stats_module._dataframe_to_clr_data(df_pl)

        # Via _simulation_to_clr_data (no cache)
        mock_sim = MagicMock()
        del mock_sim._clr_data  # Ensure hasattr returns False
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df_pl)

        sim_cols, sim_arr = _stats_module._simulation_to_clr_data(mock_sim, output_index=0)

        assert set(direct_cols) == set(sim_cols)
        # Compare values aligned by column name
        for col_name in direct_cols:
            idx_d = direct_cols.index(col_name)
            idx_s = sim_cols.index(col_name)
            np.testing.assert_array_equal(direct_arr[idx_d], sim_arr[idx_s])


# ---------------------------------------------------------------------------
# Property 4: Missing variables produce descriptive errors
# Feature: python-statistics-module, Property 4: Missing variables produce descriptive errors
# ---------------------------------------------------------------------------

# Strategies for error condition tests

# Strategy for realistic EUROMOD-like variable names
st_euromod_var = st.text(
    st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
    min_size=3, max_size=15,
).filter(lambda s: s[0].isalpha() and not s.endswith("_"))


@st.composite
def st_required_and_available_vars(draw):
    """Generate a set of required variables and available columns where at least
    one required variable is missing from the available set.

    Returns a dict with:
      - required_vars: list[str] - variables the template requires
      - available_vars: list[str] - columns in the DataFrame (missing at least one required)
      - missing_var: str - the specific variable that's missing
    """
    # Generate required variable names (at least 1)
    num_required = draw(st.integers(min_value=1, max_value=5))
    required_vars = draw(
        st.lists(st_euromod_var, min_size=num_required, max_size=num_required, unique=True)
    )

    # Pick one variable to be missing
    missing_idx = draw(st.integers(min_value=0, max_value=len(required_vars) - 1))
    missing_var = required_vars[missing_idx]

    # Build available columns: include all required EXCEPT the missing one, plus some extras
    available_vars = [v for v in required_vars if v != missing_var]
    num_extra = draw(st.integers(min_value=1, max_value=5))
    extra_vars = draw(
        st.lists(
            st_euromod_var.filter(lambda x: x.lower() != missing_var.lower()),
            min_size=num_extra,
            max_size=num_extra,
            unique=True,
        ).filter(lambda xs: all(x.lower() != missing_var.lower() for x in xs))
    )
    available_vars.extend(extra_vars)

    # Ensure no duplicates (case-insensitive)
    seen = set()
    deduped = []
    for v in available_vars:
        if v.lower() not in seen:
            seen.add(v.lower())
            deduped.append(v)
    available_vars = deduped

    # Verify missing_var is truly not in available (case-insensitive)
    assume(missing_var.lower() not in {v.lower() for v in available_vars})

    return {
        "required_vars": required_vars,
        "available_vars": available_vars,
        "missing_var": missing_var,
    }


class TestMissingVariablesError:
    """Property 4: Missing variables produce descriptive errors.

    For any template that requires variable V, and any Simulation DataFrame
    that does not contain column V, calling calculate() SHALL raise an
    exception whose message contains the string V.
    """

    @given(data=st_required_and_available_vars())
    @settings(max_examples=100)
    def test_missing_variable_in_error_message(self, data):
        """Given a template requiring variables and a DataFrame missing one,
        _validate_required_variables raises ValueError containing the missing name.

        **Validates: Requirements 1.4**
        """
        required_vars = data["required_vars"]
        available_vars = data["available_vars"]
        missing_var = data["missing_var"]

        # Create a mock Statistics object with a mock template
        stats = object.__new__(_stats_module.Statistics)
        stats._template_path = "test_template.xml"

        # Mock the template's requiredVariables
        mock_template = MagicMock()
        mock_req_vars = []
        for var_name in required_vars:
            req_var = MagicMock()
            req_var.readVar = var_name
            req_var.name = var_name
            mock_req_vars.append(req_var)
        mock_template.info.requiredVariables = mock_req_vars
        stats._template = mock_template

        # Call _validate_required_variables with available vars missing the target
        with pytest.raises(ValueError) as exc_info:
            stats._validate_required_variables(available_vars)

        # The error message must contain the missing variable name
        assert missing_var in str(exc_info.value), (
            f"Expected '{missing_var}' in error message, got: {exc_info.value}"
        )


# ---------------------------------------------------------------------------
# Property 9: Mismatched observations raises error
# Feature: python-statistics-module, Property 9: Mismatched observations raises error
# ---------------------------------------------------------------------------

@st.composite
def st_mismatched_dataframes(draw):
    """Generate two DataFrames with different row counts for baseline and reform.

    Returns a dict with:
      - baseline_rows: int - number of rows in baseline
      - reform_rows: int - number of rows in reform (different from baseline)
      - columns: list[str] - shared column names
    """
    baseline_rows = draw(st.integers(min_value=1, max_value=100))
    reform_rows = draw(st.integers(min_value=1, max_value=100))
    assume(baseline_rows != reform_rows)

    num_cols = draw(st.integers(min_value=1, max_value=5))
    columns = draw(
        st.lists(st_col_name, min_size=num_cols, max_size=num_cols, unique=True)
    )

    return {
        "baseline_rows": baseline_rows,
        "reform_rows": reform_rows,
        "columns": columns,
    }


class TestMismatchedObservationsError:
    """Property 9: Mismatched observations raises error.

    For any baseline Simulation with M observations and reform Simulation with
    N observations where M != N, calling calculate() with a BaselineReform
    template SHALL raise an exception whose message indicates the observation
    count mismatch.
    """

    @given(data=st_mismatched_dataframes())
    @settings(max_examples=100)
    def test_observation_mismatch_raises_error(self, data):
        """Given baseline and reform with different row counts,
        calculate() raises ValueError mentioning both observation counts.

        **Validates: Requirements 3.3**
        """
        baseline_rows = data["baseline_rows"]
        reform_rows = data["reform_rows"]
        columns = data["columns"]

        # Build baseline DataFrame
        baseline_data = {col: np.random.rand(baseline_rows).tolist() for col in columns}
        baseline_df = pd.DataFrame(baseline_data)

        # Build reform DataFrame with different row count
        reform_data = {col: np.random.rand(reform_rows).tolist() for col in columns}
        reform_df = pd.DataFrame(reform_data)

        # Create mock Simulation objects
        baseline_sim = MagicMock()
        baseline_sim.outputs = [baseline_df]
        baseline_sim.output_filenames = ["baseline"]

        reform_sim = MagicMock()
        reform_sim.outputs = [reform_df]
        reform_sim.output_filenames = ["reform_0"]

        # Create a mock Statistics instance configured for BaselineReform
        stats = object.__new__(_stats_module.Statistics)
        stats._template_path = "test_template.xml"
        stats._variable = None

        mock_template = MagicMock()
        mock_template.info.templateType = "BaselineReform"
        mock_template.info.requiredVariables = None
        stats._template = mock_template

        # Mock HardDefinitions.TemplateType.BaselineReform to match
        with patch.object(_stats_module, 'HardDefinitions') as mock_hd:
            mock_hd.TemplateType.BaselineReform = "BaselineReform"
            mock_hd.TemplateType.Multi = "Multi"
            mock_hd.TemplateType.Default = "Default"

            # Mock the CLR list types and system module
            with patch.object(_stats_module, 'SystemCs') as mock_sys:
                mock_list_str = MagicMock()
                mock_list_list_str = MagicMock()
                mock_list_double_array = MagicMock()
                mock_sys.Collections.Generic.List.__getitem__ = MagicMock(
                    return_value=MagicMock(return_value=mock_list_str)
                )

                with pytest.raises(ValueError) as exc_info:
                    stats.calculate(baseline_sim, reforms=[reform_sim])

                error_msg = str(exc_info.value)
                # The error message must mention both observation counts
                assert str(baseline_rows) in error_msg, (
                    f"Expected baseline count '{baseline_rows}' in error: {error_msg}"
                )
                assert str(reform_rows) in error_msg, (
                    f"Expected reform count '{reform_rows}' in error: {error_msg}"
                )


# ---------------------------------------------------------------------------
# Property 11: Invalid variable raises descriptive error
# Feature: python-statistics-module, Property 11: Invalid variable raises descriptive error
# ---------------------------------------------------------------------------

@st.composite
def st_invalid_variable_scenario(draw):
    """Generate a variable name and DataFrame columns where the variable is not
    among the DataFrame columns.

    Returns a dict with:
      - variable: str - variable name that doesn't exist in the DataFrame
      - columns: list[str] - DataFrame column names (variable not present)
    """
    num_cols = draw(st.integers(min_value=1, max_value=8))
    columns = draw(
        st.lists(st_euromod_var, min_size=num_cols, max_size=num_cols, unique=True)
    )

    # Generate a variable name that is not in the columns (case-insensitive)
    columns_lower = {c.lower() for c in columns}
    variable = draw(
        st_euromod_var.filter(lambda v: v.lower() not in columns_lower)
    )

    return {
        "variable": variable,
        "columns": columns,
    }


class TestInvalidVariableError:
    """Property 11: Invalid variable raises descriptive error.

    For any variable name V that does not exist in the Simulation DataFrame
    columns, calling calculate() with a Variable-type template specifying V
    SHALL raise an exception whose message contains V.
    """

    @given(data=st_invalid_variable_scenario())
    @settings(max_examples=100)
    def test_invalid_variable_in_error_message(self, data):
        """Given a variable name not present in the simulation DataFrame,
        _validate_variable_in_simulation raises ValueError containing the variable name.

        **Validates: Requirements 4.2**
        """
        variable = data["variable"]
        columns = data["columns"]

        # Build a DataFrame with the generated columns (numeric data)
        num_rows = 5
        df_data = {col: np.random.rand(num_rows).tolist() for col in columns}
        df = pd.DataFrame(df_data)

        # Create mock Simulation object
        mock_simulation = MagicMock()
        mock_simulation.outputs = [df]

        # Create a mock Statistics instance with the variable set
        stats = object.__new__(_stats_module.Statistics)
        stats._template_path = "test_template.xml"
        stats._variable = variable

        mock_template = MagicMock()
        stats._template = mock_template

        # Call _validate_variable_in_simulation
        with pytest.raises(ValueError) as exc_info:
            stats._validate_variable_in_simulation(mock_simulation)

        error_msg = str(exc_info.value)
        # The error message must contain the variable name
        assert variable in error_msg, (
            f"Expected variable '{variable}' in error message, got: {error_msg}"
        )


# ---------------------------------------------------------------------------
# Property 2: Results schema conformance
# Feature: python-statistics-module, Property 2: Results schema conformance
# ---------------------------------------------------------------------------

@st.composite
def st_table_config(draw):
    """Generate a random table configuration with valid dimensions and cell data.

    Returns a dict with:
      - name: str - table name
      - title: str - table title
      - page_name: str - page name
      - num_rows: int - number of rows (>= 1)
      - num_cols: int - number of columns (>= 1)
      - row_titles: list[str] - row title labels
      - col_titles: list[str] - column title labels
      - cell_values: list[list[float]] - numeric cell values [rows x cols]
      - cell_display_values: list[list[str]] - display strings for each cell
      - cell_is_string: list[list[bool]] - string flag for each cell
    """
    num_rows = draw(st.integers(min_value=1, max_value=20))
    num_cols = draw(st.integers(min_value=1, max_value=10))

    name = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
        min_size=1, max_size=15,
    ).filter(lambda s: s[0].isalpha()))

    title = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
        min_size=1, max_size=30,
    ))

    page_name = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
        min_size=1, max_size=10,
    ).filter(lambda s: s[0].isalpha()))

    row_titles = draw(st.lists(
        st.text(st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789 "), min_size=1, max_size=15),
        min_size=num_rows, max_size=num_rows,
    ))

    col_titles = draw(st.lists(
        st.text(st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789 "), min_size=1, max_size=15),
        min_size=num_cols, max_size=num_cols,
        unique=True,
    ))

    cell_values = []
    cell_display_values = []
    cell_is_string = []
    for _ in range(num_rows):
        row_vals = draw(st.lists(
            st.floats(min_value=-1e12, max_value=1e12, allow_nan=False, allow_infinity=False),
            min_size=num_cols, max_size=num_cols,
        ))
        row_display = [f"{v:.4f}" for v in row_vals]
        row_is_str = draw(st.lists(
            st.booleans(),
            min_size=num_cols, max_size=num_cols,
        ))
        cell_values.append(row_vals)
        cell_display_values.append(row_display)
        cell_is_string.append(row_is_str)

    return {
        "name": name,
        "title": title,
        "page_name": page_name,
        "num_rows": num_rows,
        "num_cols": num_cols,
        "row_titles": row_titles,
        "col_titles": col_titles,
        "cell_values": cell_values,
        "cell_display_values": cell_display_values,
        "cell_is_string": cell_is_string,
    }


@st.composite
def st_multi_table_result(draw):
    """Generate a StatisticsResult configuration with 1-5 tables with unique names."""
    num_tables = draw(st.integers(min_value=1, max_value=5))
    # Generate unique table names first, then build configs with those names
    table_names = draw(st.lists(
        st.text(
            st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
            min_size=1, max_size=15,
        ).filter(lambda s: s[0].isalpha()),
        min_size=num_tables, max_size=num_tables,
        unique=True,
    ))
    tables = []
    for name in table_names:
        config = draw(st_table_config())
        config["name"] = name
        tables.append(config)
    return tables


def _build_mock_statistics_table(config, use_polars=False):
    """Build a StatisticsTable from a test config dict without CLR dependencies."""
    table = _stats_module.StatisticsTable()
    table._name = config["name"]
    table._title = config["title"]
    table._page_name = config["page_name"]
    table._use_polars = use_polars
    table._col_titles = config["col_titles"]
    table._row_titles = config["row_titles"]
    table._cell_values = config["cell_values"]
    table._cell_display_values = config["cell_display_values"]
    table._cell_is_string = config["cell_is_string"]
    values_dict = {}
    for i, row_title in enumerate(config["row_titles"]):
        if i < len(config["cell_values"]):
            row_dict = {}
            for j, col_title in enumerate(config["col_titles"]):
                if j < len(config["cell_values"][i]):
                    row_dict[col_title] = config["cell_values"][i][j]
            values_dict[row_title] = row_dict
    table._values = values_dict
    return table


def _build_mock_statistics_result(table_configs, use_polars=False):
    """Build a mock StatisticsResult from table configs without CLR dependencies."""
    result = _stats_module.StatisticsResult(
        display_results=None,
        use_polars=use_polars,
        errors=[],
        warnings=[],
    )
    from ..container import Container
    tables_container = Container()
    for config in table_configs:
        table = _build_mock_statistics_table(config, use_polars=use_polars)
        table_key = table._name if table._name else table._title
        if not table_key:
            table_key = f"table_{len(tables_container)}"
        tables_container.add(table_key, table)
    result._tables = tables_container
    return result


class TestResultsSchemaConformance:
    """Property 2: Results schema conformance.

    For any successful StatisticsResult, the internal table structure SHALL have:
    - Each table contains columns, rows, and cells.
    - Cells are a list of lists with displayValue (str), value (float), isStringValue (bool).

    **Validates: Requirements 1.2**
    """

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_each_table_has_columns_rows_and_cells(self, table_configs):
        """Each table SHALL have col_titles, row_titles, and cell data.

        **Validates: Requirements 1.2**
        """
        result = _build_mock_statistics_result(table_configs)
        for table in result.tables:
            assert isinstance(table._col_titles, list)
            assert len(table._col_titles) > 0
            assert isinstance(table._row_titles, list)
            assert len(table._row_titles) > 0
            assert isinstance(table._cell_values, list)
            assert len(table._cell_values) > 0

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_cells_have_display_value_as_string(self, table_configs):
        """Each cell displayValue SHALL be a string.

        **Validates: Requirements 1.2**
        """
        result = _build_mock_statistics_result(table_configs)
        for table in result.tables:
            for i, row_display in enumerate(table._cell_display_values):
                for j, display_val in enumerate(row_display):
                    assert isinstance(display_val, str)

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_cells_have_value_as_float(self, table_configs):
        """Each cell value SHALL be a float.

        **Validates: Requirements 1.2**
        """
        result = _build_mock_statistics_result(table_configs)
        for table in result.tables:
            for i, row_vals in enumerate(table._cell_values):
                for j, val in enumerate(row_vals):
                    assert isinstance(val, (int, float))

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_cells_have_is_string_value_as_bool(self, table_configs):
        """Each cell isStringValue SHALL be a boolean.

        **Validates: Requirements 1.2**
        """
        result = _build_mock_statistics_result(table_configs)
        for table in result.tables:
            for i, row_is_str in enumerate(table._cell_is_string):
                for j, is_str in enumerate(row_is_str):
                    assert isinstance(is_str, bool)

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_cells_dimensions_match_rows_and_columns(self, table_configs):
        """Cells grid dimensions SHALL match declared rows and columns.

        **Validates: Requirements 1.2**
        """
        result = _build_mock_statistics_result(table_configs)
        for table in result.tables:
            num_rows = len(table._row_titles)
            num_cols = len(table._col_titles)
            assert len(table._cell_values) == num_rows
            for row in table._cell_values:
                assert len(row) == num_cols
            assert len(table._cell_display_values) == num_rows
            for row in table._cell_display_values:
                assert len(row) == num_cols
            assert len(table._cell_is_string) == num_rows
            for row in table._cell_is_string:
                assert len(row) == num_cols


# ---------------------------------------------------------------------------
# Property 3: DataFrame conversion preserves table structure
# Feature: python-statistics-module, Property 3: DataFrame conversion preserves table structure
# ---------------------------------------------------------------------------


class TestDataFrameConversionPreservesStructure:
    """Property 3: DataFrame conversion preserves table structure.

    For any successful StatisticsResult containing N tables, calling
    to_dataframes() SHALL return a dict with N entries, where each value
    is a DataFrame whose shape matches the table's (rows x columns) dimensions.

    **Validates: Requirements 1.3**
    """

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_to_dataframes_returns_n_entries(self, table_configs):
        """to_dataframes() SHALL return a dict with exactly N entries.

        **Validates: Requirements 1.3**
        """
        result = _build_mock_statistics_result(table_configs, use_polars=False)
        dfs = result.to_dataframes()
        assert isinstance(dfs, dict)
        assert len(dfs) == len(table_configs)

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_pandas_dataframe_shape_matches_table_dimensions(self, table_configs):
        """Pandas DataFrame SHALL have shape (R, C) matching table dimensions.

        **Validates: Requirements 1.3**
        """
        result = _build_mock_statistics_result(table_configs, use_polars=False)
        dfs = result.to_dataframes()
        for config in table_configs:
            table_key = config["name"] if config["name"] else config["title"]
            assert table_key in dfs
            df = dfs[table_key]
            assert isinstance(df, pd.DataFrame)
            assert df.shape == (config["num_rows"], config["num_cols"])

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_polars_dataframe_shape_matches_table_dimensions(self, table_configs):
        """Polars DataFrame SHALL have height=R and appropriate width.

        **Validates: Requirements 1.3**
        """
        result = _build_mock_statistics_result(table_configs, use_polars=True)
        dfs = result.to_dataframes()
        for config in table_configs:
            table_key = config["name"] if config["name"] else config["title"]
            assert table_key in dfs
            df = dfs[table_key]
            assert isinstance(df, pl.DataFrame)
            assert df.height == config["num_rows"]
            has_row_titles = len(config["row_titles"]) > 0
            expected_width = config["num_cols"] + (1 if has_row_titles else 0)
            assert df.width == expected_width

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_pandas_dataframe_values_match_cell_values(self, table_configs):
        """Pandas DataFrame numeric values SHALL match cell_values exactly.

        **Validates: Requirements 1.3**
        """
        result = _build_mock_statistics_result(table_configs, use_polars=False)
        dfs = result.to_dataframes()
        for config in table_configs:
            table_key = config["name"] if config["name"] else config["title"]
            df = dfs[table_key]
            for i in range(config["num_rows"]):
                for j in range(config["num_cols"]):
                    expected = config["cell_values"][i][j]
                    actual = df.iloc[i, j]
                    assert actual == expected

    @given(table_configs=st_multi_table_result())
    @settings(max_examples=100)
    def test_to_dataframes_keys_match_table_names(self, table_configs):
        """Keys in to_dataframes() SHALL correspond to table names.

        **Validates: Requirements 1.3**
        """
        result = _build_mock_statistics_result(table_configs, use_polars=False)
        dfs = result.to_dataframes()
        expected_keys = set()
        for config in table_configs:
            key = config["name"] if config["name"] else config["title"]
            expected_keys.add(key)
        assert set(dfs.keys()) == expected_keys

# ---------------------------------------------------------------------------
# Property 14: JSON export schema matches SP_ExecutableCaller
# Feature: python-statistics-module, Property 14: JSON export schema matches SP_ExecutableCaller
# ---------------------------------------------------------------------------


@st.composite
def st_table_config_for_json(draw):
    """Generate a table configuration suitable for JSON export testing."""
    num_rows = draw(st.integers(min_value=1, max_value=10))
    num_cols = draw(st.integers(min_value=1, max_value=6))

    name = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
        min_size=1, max_size=12,
    ).filter(lambda s: s[0].isalpha()))

    title = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz ABCDE"),
        min_size=1, max_size=20,
    ))

    page_name = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
        min_size=1, max_size=10,
    ).filter(lambda s: s[0].isalpha()))

    row_titles = draw(st.lists(
        st.text(st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789 "), min_size=1, max_size=12),
        min_size=num_rows, max_size=num_rows,
    ))

    col_titles = draw(st.lists(
        st.text(st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789 "), min_size=1, max_size=12),
        min_size=num_cols, max_size=num_cols,
        unique=True,
    ))

    cell_values = []
    cell_display_values = []
    cell_is_string = []
    for _ in range(num_rows):
        row_vals = draw(st.lists(
            st.floats(min_value=-1e12, max_value=1e12, allow_nan=False, allow_infinity=False),
            min_size=num_cols, max_size=num_cols,
        ))
        row_display = [f"{v:.4f}" for v in row_vals]
        row_is_str = draw(st.lists(
            st.booleans(),
            min_size=num_cols, max_size=num_cols,
        ))
        cell_values.append(row_vals)
        cell_display_values.append(row_display)
        cell_is_string.append(row_is_str)

    return {
        "name": name,
        "title": title,
        "page_name": page_name,
        "num_rows": num_rows,
        "num_cols": num_cols,
        "row_titles": row_titles,
        "col_titles": col_titles,
        "cell_values": cell_values,
        "cell_display_values": cell_display_values,
        "cell_is_string": cell_is_string,
    }


@st.composite
def st_multi_table_result_for_json(draw):
    """Generate a StatisticsResult configuration with 1-4 tables for JSON testing."""
    num_tables = draw(st.integers(min_value=1, max_value=4))
    table_names = draw(st.lists(
        st.text(
            st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
            min_size=1, max_size=12,
        ).filter(lambda s: s[0].isalpha()),
        min_size=num_tables, max_size=num_tables,
        unique=True,
    ))
    tables = []
    for name in table_names:
        config = draw(st_table_config_for_json())
        config["name"] = name
        tables.append(config)
    return tables



def _build_json_dict_from_table_configs(table_configs):
    """Build a SP_ExecutableCaller-compatible JSON dict from table configs.

    This mirrors what StatisticsResult.to_dict() produces from CLR DisplayResults,
    but uses our Python-side mock table configs for property testing without CLR.
    """
    # Group tables by page_name
    pages_map = {}
    for config in table_configs:
        page_name = config["page_name"]
        if page_name not in pages_map:
            pages_map[page_name] = {
                "name": page_name,
                "title": page_name,
                "subtitle": "",
                "description": "",
                "tables": [],
            }
        table_dict = {
            "name": config["name"],
            "title": config["title"],
            "subtitle": "",
            "columns": [
                {"name": col, "title": col} for col in config["col_titles"]
            ],
            "rows": [
                {"name": row, "title": row} for row in config["row_titles"]
            ],
            "cells": [],
        }
        for i in range(config["num_rows"]):
            row_cells = []
            for j in range(config["num_cols"]):
                row_cells.append({
                    "displayValue": config["cell_display_values"][i][j],
                    "value": config["cell_values"][i][j],
                    "isStringValue": config["cell_is_string"][i][j],
                })
            table_dict["cells"].append(row_cells)
        pages_map[page_name]["tables"].append(table_dict)

    return {
        "info": {"title": "Test", "subtitle": "", "button": "", "description": ""},
        "pages": list(pages_map.values()),
        "prepared": True,
        "calculated": True,
    }


class TestJsonExportSchemaConformance:
    """Property 14: JSON export schema matches SP_ExecutableCaller.

    For any valid StatisticsResult, the JSON produced by to_json() SHALL be
    parseable and conform to the same schema as SP_ExecutableCaller output:
    top-level keys "info", "pages", "prepared", "calculated" with pages
    containing tables containing cells with "displayValue", "value",
    "isStringValue" fields.

    **Validates: Requirements 6.1**
    """

    # Feature: python-statistics-module, Property 14: JSON export schema matches SP_ExecutableCaller

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_json_has_required_top_level_keys(self, table_configs):
        """JSON output SHALL have top-level keys: info, pages, prepared, calculated.

        **Validates: Requirements 6.1**
        """
        import tempfile

        result = _build_mock_statistics_result(table_configs)
        json_dict = _build_json_dict_from_table_configs(table_configs)

        # Patch to_dict to return our mock dict
        result.to_dict = lambda: json_dict

        # Write to JSON file
        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            # Parse back and verify top-level keys
            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            assert "info" in parsed
            assert "pages" in parsed
            assert "prepared" in parsed
            assert "calculated" in parsed
        finally:
            os.unlink(json_path)

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_json_info_has_expected_fields(self, table_configs):
        """JSON info section SHALL have title, subtitle, button, description.

        **Validates: Requirements 6.1**
        """
        import tempfile

        result = _build_mock_statistics_result(table_configs)
        json_dict = _build_json_dict_from_table_configs(table_configs)
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            info = parsed["info"]
            assert "title" in info
            assert "subtitle" in info
            assert "button" in info
            assert "description" in info
        finally:
            os.unlink(json_path)

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_json_pages_contain_tables_with_cells(self, table_configs):
        """Each page SHALL contain tables, and each table SHALL contain cells.

        **Validates: Requirements 6.1**
        """
        import tempfile

        result = _build_mock_statistics_result(table_configs)
        json_dict = _build_json_dict_from_table_configs(table_configs)
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            assert isinstance(parsed["pages"], list)
            assert len(parsed["pages"]) > 0

            for page in parsed["pages"]:
                assert "tables" in page
                assert isinstance(page["tables"], list)
                assert len(page["tables"]) > 0

                for table in page["tables"]:
                    assert "cells" in table
                    assert isinstance(table["cells"], list)
                    assert len(table["cells"]) > 0
        finally:
            os.unlink(json_path)

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_json_cells_have_required_fields(self, table_configs):
        """Each cell SHALL have displayValue (str), value (number), isStringValue (bool).

        **Validates: Requirements 6.1**
        """
        import tempfile

        result = _build_mock_statistics_result(table_configs)
        json_dict = _build_json_dict_from_table_configs(table_configs)
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            for page in parsed["pages"]:
                for table in page["tables"]:
                    for row in table["cells"]:
                        assert isinstance(row, list)
                        for cell in row:
                            assert "displayValue" in cell
                            assert "value" in cell
                            assert "isStringValue" in cell
                            assert isinstance(cell["displayValue"], str)
                            assert isinstance(cell["value"], (int, float))
                            assert isinstance(cell["isStringValue"], bool)
        finally:
            os.unlink(json_path)

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_json_tables_have_columns_and_rows(self, table_configs):
        """Each table SHALL have columns and rows metadata arrays.

        **Validates: Requirements 6.1**
        """
        import tempfile

        result = _build_mock_statistics_result(table_configs)
        json_dict = _build_json_dict_from_table_configs(table_configs)
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            for page in parsed["pages"]:
                for table in page["tables"]:
                    assert "columns" in table
                    assert "rows" in table
                    assert isinstance(table["columns"], list)
                    assert isinstance(table["rows"], list)
        finally:
            os.unlink(json_path)

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_json_prepared_and_calculated_are_booleans(self, table_configs):
        """prepared and calculated fields SHALL be booleans.

        **Validates: Requirements 6.1**
        """
        import tempfile

        result = _build_mock_statistics_result(table_configs)
        json_dict = _build_json_dict_from_table_configs(table_configs)
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            assert isinstance(parsed["prepared"], bool)
            assert isinstance(parsed["calculated"], bool)
        finally:
            os.unlink(json_path)


# ---------------------------------------------------------------------------
# Property 15: Special float sentinel values in JSON
# Feature: python-statistics-module, Property 15: Special float sentinel values in JSON
# ---------------------------------------------------------------------------

# Strategy for floats including NaN, +Infinity, -Infinity
st_float_with_special = st.one_of(
    st.floats(min_value=-1e12, max_value=1e12, allow_nan=False, allow_infinity=False),
    st.just(float("nan")),
    st.just(float("inf")),
    st.just(float("-inf")),
)


@st.composite
def st_table_config_with_special_floats(draw):
    """Generate a table configuration where some cells contain NaN, +Inf, or -Inf."""
    num_rows = draw(st.integers(min_value=1, max_value=8))
    num_cols = draw(st.integers(min_value=1, max_value=5))

    name = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
        min_size=1, max_size=10,
    ).filter(lambda s: s[0].isalpha()))

    title = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz "),
        min_size=1, max_size=15,
    ))

    page_name = draw(st.text(
        st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
        min_size=1, max_size=8,
    ).filter(lambda s: s[0].isalpha()))

    row_titles = draw(st.lists(
        st.text(st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789"), min_size=1, max_size=10),
        min_size=num_rows, max_size=num_rows,
    ))

    col_titles = draw(st.lists(
        st.text(st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789"), min_size=1, max_size=10),
        min_size=num_cols, max_size=num_cols,
        unique=True,
    ))

    cell_values = []
    cell_display_values = []
    cell_is_string = []
    has_special = False
    for _ in range(num_rows):
        row_vals = draw(st.lists(
            st_float_with_special,
            min_size=num_cols, max_size=num_cols,
        ))
        # Check if any special floats are present
        for v in row_vals:
            if math.isnan(v) or math.isinf(v):
                has_special = True
        row_display = []
        for v in row_vals:
            if math.isnan(v):
                row_display.append("NaN")
            elif math.isinf(v) and v > 0:
                row_display.append("Infinity")
            elif math.isinf(v) and v < 0:
                row_display.append("-Infinity")
            else:
                row_display.append(f"{v:.4f}")
        row_is_str = draw(st.lists(
            st.booleans(),
            min_size=num_cols, max_size=num_cols,
        ))
        cell_values.append(row_vals)
        cell_display_values.append(row_display)
        cell_is_string.append(row_is_str)

    # Ensure at least one special float is present
    assume(has_special)

    return {
        "name": name,
        "title": title,
        "page_name": page_name,
        "num_rows": num_rows,
        "num_cols": num_cols,
        "row_titles": row_titles,
        "col_titles": col_titles,
        "cell_values": cell_values,
        "cell_display_values": cell_display_values,
        "cell_is_string": cell_is_string,
    }


class TestSpecialFloatSentinelValues:
    """Property 15: Special float sentinel values in JSON.

    For any StatisticsResult containing cells with NaN, +Infinity, or -Infinity
    values, the JSON export SHALL replace these with 99999998, 99999999, and
    -99999999 respectively (matching SP_ExecutableCaller behavior).

    **Validates: Requirements 6.3**
    """

    # Feature: python-statistics-module, Property 15: Special float sentinel values in JSON

    @given(table_config=st_table_config_with_special_floats())
    @settings(max_examples=100)
    def test_nan_replaced_with_sentinel(self, table_config):
        """NaN values SHALL be replaced with 99999998 in JSON output.

        **Validates: Requirements 6.3**
        """
        import tempfile

        result = _build_mock_statistics_result([table_config])
        json_dict = _build_json_dict_from_table_configs([table_config])
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            # Verify no NaN values remain (JSON doesn't support NaN, so json.load
            # would fail if they were present; sentinel should be used instead)
            for page in parsed["pages"]:
                for table in page["tables"]:
                    for row in table["cells"]:
                        for cell in row:
                            val = cell["value"]
                            # Must be a valid JSON number (not NaN)
                            assert not (isinstance(val, float) and math.isnan(val))
        finally:
            os.unlink(json_path)

    @given(table_config=st_table_config_with_special_floats())
    @settings(max_examples=100)
    def test_positive_infinity_replaced_with_sentinel(self, table_config):
        """Positive infinity SHALL be replaced with 99999999 in JSON output.

        **Validates: Requirements 6.3**
        """
        import tempfile

        result = _build_mock_statistics_result([table_config])
        json_dict = _build_json_dict_from_table_configs([table_config])
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            # Verify no Infinity values remain
            for page in parsed["pages"]:
                for table in page["tables"]:
                    for row in table["cells"]:
                        for cell in row:
                            val = cell["value"]
                            assert not (isinstance(val, float) and math.isinf(val))
        finally:
            os.unlink(json_path)

    @given(table_config=st_table_config_with_special_floats())
    @settings(max_examples=100)
    def test_sentinel_values_are_correct(self, table_config):
        """Sentinel substitution SHALL map NaN→99999998, +Inf→99999999, -Inf→-99999999.

        **Validates: Requirements 6.3**
        """
        import tempfile

        result = _build_mock_statistics_result([table_config])
        json_dict = _build_json_dict_from_table_configs([table_config])
        result.to_dict = lambda: json_dict

        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json_path = f.name
        try:
            result.to_json(json_path)

            with open(json_path, "r", encoding="utf-8") as f:
                parsed = json.load(f)

            # Walk through cells and verify sentinels match original special values
            for page_idx, page in enumerate(parsed["pages"]):
                for table_idx, table in enumerate(page["tables"]):
                    for row_idx, row in enumerate(table["cells"]):
                        for col_idx, cell in enumerate(row):
                            # Find the corresponding original value from our config
                            original_val = table_config["cell_values"][row_idx][col_idx]
                            actual_val = cell["value"]

                            if math.isnan(original_val):
                                assert actual_val == 99999998, (
                                    f"NaN at [{row_idx}][{col_idx}] should map to 99999998, "
                                    f"got {actual_val}"
                                )
                            elif math.isinf(original_val) and original_val > 0:
                                assert actual_val == 99999999, (
                                    f"+Inf at [{row_idx}][{col_idx}] should map to 99999999, "
                                    f"got {actual_val}"
                                )
                            elif math.isinf(original_val) and original_val < 0:
                                assert actual_val == -99999999, (
                                    f"-Inf at [{row_idx}][{col_idx}] should map to -99999999, "
                                    f"got {actual_val}"
                                )
                            else:
                                # Regular float values should be preserved
                                assert actual_val == original_val, (
                                    f"Regular value at [{row_idx}][{col_idx}] should be "
                                    f"{original_val}, got {actual_val}"
                                )
        finally:
            os.unlink(json_path)

    @given(table_config=st_table_config_with_special_floats())
    @settings(max_examples=100)
    def test_apply_sentinel_substitution_directly(self, table_config):
        """_apply_sentinel_substitution SHALL replace special floats in nested dicts.

        **Validates: Requirements 6.3**
        """
        json_dict = _build_json_dict_from_table_configs([table_config])

        # Apply sentinel substitution
        _stats_module._apply_sentinel_substitution(json_dict)

        # Verify all cells have correct sentinels
        for page in json_dict["pages"]:
            for table in page["tables"]:
                for row_idx, row in enumerate(table["cells"]):
                    for col_idx, cell in enumerate(row):
                        original_val = table_config["cell_values"][row_idx][col_idx]
                        actual_val = cell["value"]

                        if math.isnan(original_val):
                            assert actual_val == 99999998
                        elif math.isinf(original_val) and original_val > 0:
                            assert actual_val == 99999999
                        elif math.isinf(original_val) and original_val < 0:
                            assert actual_val == -99999999
                        else:
                            assert actual_val == original_val

    @given(table_configs=st_multi_table_result_for_json())
    @settings(max_examples=100)
    def test_regular_floats_unchanged_after_substitution(self, table_configs):
        """Regular (finite) float values SHALL not be modified by sentinel substitution.

        **Validates: Requirements 6.3**
        """
        json_dict = _build_json_dict_from_table_configs(table_configs)

        # Store original values (all finite since st_multi_table_result_for_json
        # uses st_table_config_for_json which only generates finite floats)
        original_values = []
        for page in json_dict["pages"]:
            for table in page["tables"]:
                for row in table["cells"]:
                    for cell in row:
                        original_values.append(cell["value"])

        # Apply sentinel substitution
        _stats_module._apply_sentinel_substitution(json_dict)

        # Verify no values changed (all were finite)
        idx = 0
        for page in json_dict["pages"]:
            for table in page["tables"]:
                for row in table["cells"]:
                    for cell in row:
                        assert cell["value"] == original_values[idx], (
                            f"Finite value {original_values[idx]} was modified to {cell['value']}"
                        )
                        idx += 1


# ---------------------------------------------------------------------------
# Property 12: Custom statistics produce results
# Feature: python-statistics-module, Property 12: Custom statistics produce results
# ---------------------------------------------------------------------------

# Strategies for custom stat definitions

# Derived from the implementation rather than restated here: a hand-written list
# drifts silently, and did -- it named "mean" and "population_count", which the
# module has never supported, so every example carrying one was rejected by
# _validate_custom_stat_definitions and failed the test for the wrong reason.
_VALID_MEASURES = sorted(_stats_module._DISTRIBUTIONAL_MEASURES)

st_income_var = st.text(
    st.sampled_from("abcdefghijklmnopqrstuvwxyz_"),
    min_size=3, max_size=15,
).filter(lambda s: s[0].isalpha() and not s.endswith("_"))

st_stat_name = st.text(
    st.sampled_from("abcdefghijklmnopqrstuvwxyz_0123456789"),
    min_size=3, max_size=20,
).filter(lambda s: s[0].isalpha() and not s.endswith("_"))


@st.composite
def st_aggregate_stat_definition(draw):
    """Generate a valid aggregate statistic definition dict.

    Returns a dict with required keys 'name' and 'income_list',
    plus optional fields.
    """
    name = draw(st_stat_name)
    income_list = draw(st_income_var)
    stat_def = {"name": name, "income_list": income_list}

    # Optionally add description
    if draw(st.booleans()):
        stat_def["description"] = draw(st.text(
            st.sampled_from("abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            min_size=1, max_size=30,
        ))

    # Optionally add source
    if draw(st.booleans()):
        stat_def["source"] = draw(st.text(
            st.sampled_from("abcdefghijklmnopqrstuvwxyz"),
            min_size=1, max_size=15,
        ))

    return stat_def


@st.composite
def st_distributional_stat_definition(draw):
    """Generate a valid distributional statistic definition dict.

    Returns a dict with required keys 'name' and 'income_list',
    plus a 'measures' list drawn from valid measure names.
    """
    name = draw(st_stat_name)
    income_list = draw(st_income_var)
    num_measures = draw(st.integers(min_value=1, max_value=4))
    measures = draw(st.lists(
        st.sampled_from(_VALID_MEASURES),
        min_size=num_measures, max_size=num_measures,
        unique=True,
    ))

    stat_def = {"name": name, "income_list": income_list, "measures": measures}

    # Optionally add description
    if draw(st.booleans()):
        stat_def["description"] = draw(st.text(
            st.sampled_from("abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            min_size=1, max_size=30,
        ))

    return stat_def


@st.composite
def st_custom_stat_scenario(draw):
    """Generate a scenario with aggregate and/or distributional stat definitions.

    At least one stat definition is always present.

    Returns a dict with:
      - aggregate_stats: list[dict]
      - distributional_stats: list[dict]
    """
    num_agg = draw(st.integers(min_value=0, max_value=3))
    num_dist = draw(st.integers(min_value=0, max_value=3))
    assume(num_agg + num_dist >= 1)  # At least one stat

    aggregate_stats = draw(st.lists(
        st_aggregate_stat_definition(),
        min_size=num_agg, max_size=num_agg,
    ))
    distributional_stats = draw(st.lists(
        st_distributional_stat_definition(),
        min_size=num_dist, max_size=num_dist,
    ))

    return {
        "aggregate_stats": aggregate_stats,
        "distributional_stats": distributional_stats,
    }


class TestCustomStatisticsProduceResults:
    """Property 12: Custom statistics produce results.

    For any valid aggregate or distributional statistic definition with a
    variable that exists in the Simulation data, calling calculate_custom()
    SHALL produce a non-empty result. We validate this by testing:
    - Valid stat definitions pass validation without errors
    - The generated template XML is valid XML with correct structure
    - _build_custom_template_xml produces XML containing the requested measures/variables

    **Validates: Requirements 5.1, 5.2**
    """

    @given(scenario=st_custom_stat_scenario())
    @settings(max_examples=100)
    def test_valid_definitions_pass_validation(self, scenario):
        """Valid aggregate and distributional stat definitions SHALL pass
        _validate_custom_stat_definitions without raising.

        **Validates: Requirements 5.1, 5.2**
        """
        aggregate_stats = scenario["aggregate_stats"]
        distributional_stats = scenario["distributional_stats"]

        # Should not raise
        _stats_module.Statistics._validate_custom_stat_definitions(
            aggregate_stats, distributional_stats
        )

    @given(scenario=st_custom_stat_scenario())
    @settings(max_examples=100)
    def test_generated_xml_is_well_formed(self, scenario):
        """_build_custom_template_xml SHALL produce well-formed XML for any valid
        custom stat definitions.

        **Validates: Requirements 5.1, 5.2**
        """
        import xml.etree.ElementTree as ET

        aggregate_stats = scenario["aggregate_stats"]
        distributional_stats = scenario["distributional_stats"]

        # Collect income variables
        income_vars = set()
        for stat in aggregate_stats:
            income_vars.add(stat["income_list"])
        for stat in distributional_stats:
            income_vars.add(stat["income_list"])

        xml_str = _stats_module._build_custom_template_xml(
            aggregate_stats, distributional_stats, income_vars
        )

        # Must be parseable as valid XML
        root = ET.fromstring(xml_str)
        assert root.tag == "Template"

    @given(scenario=st_custom_stat_scenario())
    @settings(max_examples=100)
    def test_generated_xml_contains_required_variables(self, scenario):
        """The generated template XML SHALL declare all income_list variables
        as RequiredVariables.

        **Validates: Requirements 5.1, 5.2**
        """
        import xml.etree.ElementTree as ET

        aggregate_stats = scenario["aggregate_stats"]
        distributional_stats = scenario["distributional_stats"]

        income_vars = set()
        for stat in aggregate_stats:
            income_vars.add(stat["income_list"])
        for stat in distributional_stats:
            income_vars.add(stat["income_list"])

        xml_str = _stats_module._build_custom_template_xml(
            aggregate_stats, distributional_stats, income_vars
        )

        root = ET.fromstring(xml_str)
        req_vars_elem = root.find(".//RequiredVariables")
        assert req_vars_elem is not None

        # Collect all declared required variable names
        declared_vars = set()
        for rv in req_vars_elem.findall("RequiredVariable"):
            read_var = rv.find("ReadVar")
            if read_var is not None and read_var.text:
                declared_vars.add(read_var.text)

        # All income_vars should be declared
        for var in income_vars:
            assert var in declared_vars, (
                f"Income variable '{var}' not found in RequiredVariables. "
                f"Declared: {declared_vars}"
            )

    @given(scenario=st_custom_stat_scenario())
    @settings(max_examples=100)
    def test_generated_xml_contains_aggregate_table(self, scenario):
        """When aggregate_stats are provided, the template XML SHALL contain
        an AggregateStats table with rows for each stat definition.

        **Validates: Requirements 5.1**
        """
        import xml.etree.ElementTree as ET

        aggregate_stats = scenario["aggregate_stats"]
        distributional_stats = scenario["distributional_stats"]

        if not aggregate_stats:
            return  # Skip if no aggregate stats in this scenario

        income_vars = set()
        for stat in aggregate_stats:
            income_vars.add(stat["income_list"])
        for stat in distributional_stats:
            income_vars.add(stat["income_list"])

        xml_str = _stats_module._build_custom_template_xml(
            aggregate_stats, distributional_stats, income_vars
        )

        root = ET.fromstring(xml_str)

        # Find the AggregateStats table
        tables = root.findall(".//Table")
        agg_table = None
        for table in tables:
            name_elem = table.find("Name")
            if name_elem is not None and name_elem.text == "AggregateStats":
                agg_table = table
                break

        assert agg_table is not None, "AggregateStats table not found in XML"

        # Each aggregate stat should have a corresponding row
        rows = agg_table.findall(".//Rows/Row")
        row_names = set()
        for row in rows:
            name_elem = row.find("Name")
            if name_elem is not None and name_elem.text:
                row_names.add(name_elem.text)

        for stat in aggregate_stats:
            assert stat["name"] in row_names, (
                f"Aggregate stat '{stat['name']}' not found in table rows. "
                f"Found: {row_names}"
            )

    @given(scenario=st_custom_stat_scenario())
    @settings(max_examples=100)
    def test_generated_xml_contains_distributional_tables(self, scenario):
        """When distributional_stats are provided, the template XML SHALL contain
        a table for each distributional stat with rows for each measure.

        **Validates: Requirements 5.2**
        """
        import xml.etree.ElementTree as ET

        aggregate_stats = scenario["aggregate_stats"]
        distributional_stats = scenario["distributional_stats"]

        if not distributional_stats:
            return  # Skip if no distributional stats in this scenario

        income_vars = set()
        for stat in aggregate_stats:
            income_vars.add(stat["income_list"])
        for stat in distributional_stats:
            income_vars.add(stat["income_list"])

        xml_str = _stats_module._build_custom_template_xml(
            aggregate_stats, distributional_stats, income_vars
        )

        root = ET.fromstring(xml_str)

        # Each distributional stat should produce a table named Dist_<stat_name>
        tables = root.findall(".//Table")
        table_names = set()
        for table in tables:
            name_elem = table.find("Name")
            if name_elem is not None and name_elem.text:
                table_names.add(name_elem.text)

        for stat in distributional_stats:
            expected_table = f"Dist_{stat['name']}"
            assert expected_table in table_names, (
                f"Distributional table '{expected_table}' not found. "
                f"Found: {table_names}"
            )

    @given(agg_stat=st_aggregate_stat_definition())
    @settings(max_examples=100)
    def test_generated_xml_cell_references_income_variable(self, agg_stat):
        """Each aggregate stat cell action SHALL reference the stat's income_list
        variable, and the variable SHALL be declared as required.

        **Validates: Requirements 5.1**
        """
        import xml.etree.ElementTree as ET

        income_list = agg_stat["income_list"]
        xml_str = _stats_module._build_custom_template_xml(
            [agg_stat], [], {income_list}
        )

        root = ET.fromstring(xml_str)

        # The builder references the income list in the cell action's formula, as
        # DATA_VAR[@<name>] -- not in a <VariableName> element, which it does not
        # emit at all.
        formulas = {e.text for e in root.iter("FormulaString") if e.text}
        assert f"DATA_VAR[@{income_list}]" in formulas, (
            f"income_list '{income_list}' not referenced by any cell action. "
            f"Found formulas: {formulas}"
        )

        # A referenced variable the engine is not told to read would fail at
        # calculation time, so it must also be declared.
        required = {e.text for e in root.iter("ReadVar") if e.text}
        assert income_list in required, (
            f"income_list '{income_list}' not declared as a required variable. "
            f"Found: {required}"
        )

    @given(data=st.data())
    @settings(max_examples=100)
    def test_missing_name_field_raises_validation_error(self, data):
        """A stat definition missing the 'name' field SHALL fail validation.

        **Validates: Requirements 5.1**
        """
        # Generate a stat without 'name'
        income_list = data.draw(st_income_var)
        stat_def = {"income_list": income_list}

        # Choose whether to put it in aggregate or distributional
        if data.draw(st.booleans()):
            with pytest.raises(ValueError, match="missing required field 'name'"):
                _stats_module.Statistics._validate_custom_stat_definitions(
                    [stat_def], []
                )
        else:
            with pytest.raises(ValueError, match="missing required field 'name'"):
                _stats_module.Statistics._validate_custom_stat_definitions(
                    [], [stat_def]
                )

    @given(data=st.data())
    @settings(max_examples=100)
    def test_missing_income_list_field_raises_validation_error(self, data):
        """A stat definition missing the 'income_list' field SHALL fail validation.

        **Validates: Requirements 5.1**
        """
        # Generate a stat without 'income_list'
        name = data.draw(st_stat_name)
        stat_def = {"name": name}

        if data.draw(st.booleans()):
            with pytest.raises(ValueError, match="missing required field 'income_list'"):
                _stats_module.Statistics._validate_custom_stat_definitions(
                    [stat_def], []
                )
        else:
            with pytest.raises(ValueError, match="missing required field 'income_list'"):
                _stats_module.Statistics._validate_custom_stat_definitions(
                    [], [stat_def]
                )


# ---------------------------------------------------------------------------
# Property 13: Combined template and custom statistics
# Feature: python-statistics-module, Property 13: Combined template and custom statistics
# ---------------------------------------------------------------------------


@st.composite
def st_light_table_config(draw):
    """Generate a lightweight table config for combine tests (smaller than st_table_config)."""
    num_rows = draw(st.integers(min_value=1, max_value=5))
    num_cols = draw(st.integers(min_value=1, max_value=3))

    name = draw(st.from_regex(r'[a-z][a-z_]{0,8}', fullmatch=True))
    title = name.replace("_", " ").title()
    page_name = draw(st.from_regex(r'[a-z][a-z_]{0,5}', fullmatch=True))

    row_titles = [f"row_{i}" for i in range(num_rows)]
    col_titles = [f"col_{j}" for j in range(num_cols)]

    cell_values = [[float(i * num_cols + j) for j in range(num_cols)] for i in range(num_rows)]
    cell_display_values = [[f"{v:.2f}" for v in row] for row in cell_values]
    cell_is_string = [[False] * num_cols for _ in range(num_rows)]

    return {
        "name": name,
        "title": title,
        "page_name": page_name,
        "num_rows": num_rows,
        "num_cols": num_cols,
        "row_titles": row_titles,
        "col_titles": col_titles,
        "cell_values": cell_values,
        "cell_display_values": cell_display_values,
        "cell_is_string": cell_is_string,
    }


@st.composite
def st_two_results_scenario(draw):
    """Generate two independent StatisticsResult table configs for testing
    _combine_results behavior.

    Returns a dict with:
      - template_tables: list[table_config] - tables from template result
      - custom_tables: list[table_config] - tables from custom result
      - template_errors: list[str]
      - template_warnings: list[str]
      - custom_errors: list[str]
      - custom_warnings: list[str]
    """
    num_template_tables = draw(st.integers(min_value=1, max_value=3))
    num_custom_tables = draw(st.integers(min_value=1, max_value=3))

    # Generate unique table names across both sets
    total_tables = num_template_tables + num_custom_tables
    all_names = draw(st.lists(
        st.from_regex(r'[a-z][a-z_]{1,8}', fullmatch=True),
        min_size=total_tables, max_size=total_tables,
        unique=True,
    ))

    template_names = all_names[:num_template_tables]
    custom_names = all_names[num_template_tables:]

    template_tables = []
    for name in template_names:
        config = draw(st_light_table_config())
        config["name"] = name
        template_tables.append(config)

    custom_tables = []
    for name in custom_names:
        config = draw(st_light_table_config())
        config["name"] = name
        custom_tables.append(config)

    # Generate errors and warnings
    num_template_errors = draw(st.integers(min_value=0, max_value=2))
    num_template_warnings = draw(st.integers(min_value=0, max_value=2))
    num_custom_errors = draw(st.integers(min_value=0, max_value=2))
    num_custom_warnings = draw(st.integers(min_value=0, max_value=2))

    template_errors = draw(st.lists(
        st.from_regex(r'[a-z ]{5,20}', fullmatch=True),
        min_size=num_template_errors, max_size=num_template_errors,
    ))
    template_warnings = draw(st.lists(
        st.from_regex(r'[a-z ]{5,20}', fullmatch=True),
        min_size=num_template_warnings, max_size=num_template_warnings,
    ))
    custom_errors = draw(st.lists(
        st.from_regex(r'[a-z ]{5,20}', fullmatch=True),
        min_size=num_custom_errors, max_size=num_custom_errors,
    ))
    custom_warnings = draw(st.lists(
        st.from_regex(r'[a-z ]{5,20}', fullmatch=True),
        min_size=num_custom_warnings, max_size=num_custom_warnings,
    ))

    return {
        "template_tables": template_tables,
        "custom_tables": custom_tables,
        "template_errors": template_errors,
        "template_warnings": template_warnings,
        "custom_errors": custom_errors,
        "custom_warnings": custom_warnings,
    }


class TestCombinedTemplateAndCustomStatistics:
    """Property 13: Combined template and custom statistics.

    For any valid template and valid custom statistic definitions, a single
    calculation pass SHALL produce results that contain both the template-driven
    statistics and the custom statistics. We validate this by testing:
    - _combine_results merges tables from both results correctly
    - Combined result contains all tables from both inputs
    - Errors and warnings from both are concatenated

    **Validates: Requirements 5.3**
    """

    @given(scenario=st_two_results_scenario())
    @settings(max_examples=100, suppress_health_check=[HealthCheck.too_slow])
    def test_combined_result_contains_all_tables(self, scenario):
        """_combine_results SHALL produce a result containing all tables from
        both template and custom results.

        **Validates: Requirements 5.3**
        """
        template_result = _build_mock_statistics_result(
            scenario["template_tables"],
            use_polars=False,
        )
        template_result._errors = scenario["template_errors"]
        template_result._warnings = scenario["template_warnings"]

        custom_result = _build_mock_statistics_result(
            scenario["custom_tables"],
            use_polars=False,
        )
        custom_result._errors = scenario["custom_errors"]
        custom_result._warnings = scenario["custom_warnings"]

        combined = _stats_module._combine_results(template_result, custom_result)

        # Combined should have all tables from both
        expected_count = len(scenario["template_tables"]) + len(scenario["custom_tables"])
        assert len(combined.tables) == expected_count, (
            f"Expected {expected_count} tables, got {len(combined.tables)}"
        )

    @given(scenario=st_two_results_scenario())
    @settings(max_examples=100, suppress_health_check=[HealthCheck.too_slow])
    def test_combined_result_preserves_template_table_names(self, scenario):
        """The combined result SHALL contain all table names from the template result.

        **Validates: Requirements 5.3**
        """
        template_result = _build_mock_statistics_result(
            scenario["template_tables"],
            use_polars=False,
        )
        template_result._errors = scenario["template_errors"]
        template_result._warnings = scenario["template_warnings"]

        custom_result = _build_mock_statistics_result(
            scenario["custom_tables"],
            use_polars=False,
        )
        custom_result._errors = scenario["custom_errors"]
        custom_result._warnings = scenario["custom_warnings"]

        combined = _stats_module._combine_results(template_result, custom_result)

        combined_table_names = set()
        for table in combined.tables:
            combined_table_names.add(table.name)

        for config in scenario["template_tables"]:
            assert config["name"] in combined_table_names, (
                f"Template table '{config['name']}' not in combined result. "
                f"Found: {combined_table_names}"
            )

    @given(scenario=st_two_results_scenario())
    @settings(max_examples=100, suppress_health_check=[HealthCheck.too_slow])
    def test_combined_result_preserves_custom_table_names(self, scenario):
        """The combined result SHALL contain all table names from the custom result.

        **Validates: Requirements 5.3**
        """
        template_result = _build_mock_statistics_result(
            scenario["template_tables"],
            use_polars=False,
        )
        template_result._errors = scenario["template_errors"]
        template_result._warnings = scenario["template_warnings"]

        custom_result = _build_mock_statistics_result(
            scenario["custom_tables"],
            use_polars=False,
        )
        custom_result._errors = scenario["custom_errors"]
        custom_result._warnings = scenario["custom_warnings"]

        combined = _stats_module._combine_results(template_result, custom_result)

        combined_table_names = set()
        for table in combined.tables:
            combined_table_names.add(table.name)

        for config in scenario["custom_tables"]:
            assert config["name"] in combined_table_names, (
                f"Custom table '{config['name']}' not in combined result. "
                f"Found: {combined_table_names}"
            )

    @given(scenario=st_two_results_scenario())
    @settings(max_examples=100, suppress_health_check=[HealthCheck.too_slow])
    def test_combined_result_concatenates_errors(self, scenario):
        """The combined result errors SHALL contain all errors from both
        template and custom results.

        **Validates: Requirements 5.3**
        """
        template_result = _build_mock_statistics_result(
            scenario["template_tables"],
            use_polars=False,
        )
        template_result._errors = scenario["template_errors"]
        template_result._warnings = scenario["template_warnings"]

        custom_result = _build_mock_statistics_result(
            scenario["custom_tables"],
            use_polars=False,
        )
        custom_result._errors = scenario["custom_errors"]
        custom_result._warnings = scenario["custom_warnings"]

        combined = _stats_module._combine_results(template_result, custom_result)

        expected_errors = scenario["template_errors"] + scenario["custom_errors"]
        assert combined.errors == expected_errors, (
            f"Expected errors {expected_errors}, got {combined.errors}"
        )

    @given(scenario=st_two_results_scenario())
    @settings(max_examples=100, suppress_health_check=[HealthCheck.too_slow])
    def test_combined_result_concatenates_warnings(self, scenario):
        """The combined result warnings SHALL contain all warnings from both
        template and custom results.

        **Validates: Requirements 5.3**
        """
        template_result = _build_mock_statistics_result(
            scenario["template_tables"],
            use_polars=False,
        )
        template_result._errors = scenario["template_errors"]
        template_result._warnings = scenario["template_warnings"]

        custom_result = _build_mock_statistics_result(
            scenario["custom_tables"],
            use_polars=False,
        )
        custom_result._errors = scenario["custom_errors"]
        custom_result._warnings = scenario["custom_warnings"]

        combined = _stats_module._combine_results(template_result, custom_result)

        expected_warnings = scenario["template_warnings"] + scenario["custom_warnings"]
        assert combined.warnings == expected_warnings, (
            f"Expected warnings {expected_warnings}, got {combined.warnings}"
        )

    @given(scenario=st_two_results_scenario())
    @settings(max_examples=100, suppress_health_check=[HealthCheck.too_slow])
    def test_combined_result_table_data_is_preserved(self, scenario):
        """Tables in the combined result SHALL retain their original cell data.

        **Validates: Requirements 5.3**
        """
        template_result = _build_mock_statistics_result(
            scenario["template_tables"],
            use_polars=False,
        )
        template_result._errors = scenario["template_errors"]
        template_result._warnings = scenario["template_warnings"]

        custom_result = _build_mock_statistics_result(
            scenario["custom_tables"],
            use_polars=False,
        )
        custom_result._errors = scenario["custom_errors"]
        custom_result._warnings = scenario["custom_warnings"]

        combined = _stats_module._combine_results(template_result, custom_result)

        # Verify template tables data
        for config in scenario["template_tables"]:
            table = combined.tables[config["name"]]
            assert table._cell_values == config["cell_values"]
            assert table._row_titles == config["row_titles"]
            assert table._col_titles == config["col_titles"]

        # Verify custom tables data
        for config in scenario["custom_tables"]:
            table = combined.tables[config["name"]]
            assert table._cell_values == config["cell_values"]
            assert table._row_titles == config["row_titles"]
            assert table._col_titles == config["col_titles"]
