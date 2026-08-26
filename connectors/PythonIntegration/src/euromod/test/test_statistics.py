"""Unit tests for the statistics module.

Tests the Statistics class initialization, template path validation,
and error handling.

Since statistics.py performs DLL loading at import time (consistent with
other modules in the connector), tests that don't need the CLR use
importlib to reload the module with mocked dependencies.
"""
import os
import sys
import importlib
import pytest
from unittest.mock import patch, MagicMock, PropertyMock


def _import_statistics_mocked():
    """Import statistics module with CLR mocked out.

    Patches clr, os.path.exists (for DLL check), and EM_Statistics
    to allow importing without .NET runtime.
    """
    # Remove cached module if it exists
    if 'euromod.statistics' in sys.modules:
        del sys.modules['euromod.statistics']

    # Create mock modules
    mock_clr = MagicMock()
    mock_em_stats = MagicMock()

    # Mock the EM_Statistics types
    mock_em_stats.HardDefinitions.UserInputType.VariableName = "VariableName"

    with patch.dict('sys.modules', {
        'clr': mock_clr,
    }):
        # We need os.path.exists to return True for the DLL check
        original_exists = os.path.exists

        def patched_exists(path):
            if 'EM_Statistics.dll' in str(path):
                return True
            return original_exists(path)

        with patch('os.path.exists', side_effect=patched_exists):
            # Mock the CLR import of EM_Statistics namespace
            mock_xml_handling = MagicMock()
            mock_template_class = MagicMock()
            mock_calculator = MagicMock()
            mock_error_collector_class = MagicMock()
            mock_hard_definitions = MagicMock()
            mock_hard_definitions.UserInputType.VariableName = "VariableName"

            # Patch the 'from EM_Statistics import ...' statement
            mock_em_stats_module = MagicMock()
            mock_em_stats_module.XML_handling = mock_xml_handling
            mock_em_stats_module.Template = mock_template_class
            mock_em_stats_module.EM_TemplateCalculator = mock_calculator
            mock_em_stats_module.ErrorCollector = mock_error_collector_class
            mock_em_stats_module.HardDefinitions = mock_hard_definitions

            sys.modules['EM_Statistics'] = mock_em_stats_module

            # Mock the ExternalStatistics sub-namespace
            mock_ext_stats_module = MagicMock()
            sys.modules['EM_Statistics.ExternalStatistics'] = mock_ext_stats_module

            try:
                import euromod.statistics as stats_module
                importlib.reload(stats_module)
                return stats_module, mock_xml_handling, mock_hard_definitions
            finally:
                # Clean up
                if 'EM_Statistics' in sys.modules:
                    del sys.modules['EM_Statistics']
                if 'EM_Statistics.ExternalStatistics' in sys.modules:
                    del sys.modules['EM_Statistics.ExternalStatistics']


class TestStatisticsInit:
    """Test Statistics.__init__() validation logic."""

    def test_empty_template_path_raises_valueerror(self):
        """Empty string template path raises ValueError."""
        stats_module, _, _ = _import_statistics_mocked()
        Statistics = stats_module.Statistics

        with pytest.raises(ValueError, match="Template path must not be empty"):
            Statistics("")

    def test_nonexistent_file_raises_filenotfounderror(self):
        """Non-existent template path raises FileNotFoundError."""
        stats_module, _, _ = _import_statistics_mocked()
        Statistics = stats_module.Statistics

        with pytest.raises(FileNotFoundError, match="Template file not found"):
            Statistics("/nonexistent/path/template.xml")

    def test_invalid_xml_raises_valueerror(self, tmp_path):
        """Invalid XML content raises ValueError during parsing."""
        bad_template = tmp_path / "bad_template.xml"
        bad_template.write_text("this is not valid xml")

        stats_module, mock_xml_handling, _ = _import_statistics_mocked()
        Statistics = stats_module.Statistics

        # Configure mock to simulate parse failure
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = True
        mock_error_collector.GetErrorMessage.return_value = "Invalid Xml-structure!"

        mock_xml_handling.ParseTemplate.return_value = (False, MagicMock(), mock_error_collector)

        with pytest.raises(ValueError, match="Failed to parse template"):
            Statistics(str(bad_template))

    def test_successful_parse_stores_template(self, tmp_path):
        """Successful template parse stores the template object."""
        template_file = tmp_path / "valid_template.xml"
        template_file.write_text("<Template><TemplateInfo></TemplateInfo></Template>")

        stats_module, mock_xml_handling, _ = _import_statistics_mocked()
        Statistics = stats_module.Statistics

        mock_template = MagicMock()
        mock_template.info.userVariables = None

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_xml_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        stats = Statistics(str(template_file))
        assert stats._template is mock_template
        assert stats._template_path == str(template_file)
        assert stats._variable is None

    def test_variable_parameter_stored_and_configured(self, tmp_path):
        """Variable parameter is stored and set on the template's userVariables."""
        template_file = tmp_path / "var_template.xml"
        template_file.write_text("<Template><TemplateInfo></TemplateInfo></Template>")

        stats_module, mock_xml_handling, mock_hard_defs = _import_statistics_mocked()
        Statistics = stats_module.Statistics

        # Set up a mock user variable that matches the VariableName type
        mock_user_var = MagicMock()
        mock_user_var.inputType = mock_hard_defs.UserInputType.VariableName
        mock_user_var.value = ""

        mock_template = MagicMock()
        mock_template.info.userVariables = [mock_user_var]

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_xml_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        stats = Statistics(str(template_file), variable="ils_dispy")
        assert stats._variable == "ils_dispy"
        assert mock_user_var.value == "ils_dispy"

    def test_parse_failure_without_errors_raises_valueerror(self, tmp_path):
        """If parse returns False but no errors, still raises ValueError."""
        template_file = tmp_path / "template.xml"
        template_file.write_text("<Template></Template>")

        stats_module, mock_xml_handling, _ = _import_statistics_mocked()
        Statistics = stats_module.Statistics

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_xml_handling.ParseTemplate.return_value = (False, MagicMock(), mock_error_collector)

        with pytest.raises(ValueError, match="Failed to parse template"):
            Statistics(str(template_file))


class TestStatisticsResult:
    """Test StatisticsResult class."""

    def test_empty_result_repr(self):
        """Empty result shows proper repr."""
        stats_module, _, _ = _import_statistics_mocked()
        result = stats_module.StatisticsResult()
        assert "empty" in repr(result)

    def test_errors_and_warnings_default_empty(self):
        """Errors and warnings default to empty lists."""
        stats_module, _, _ = _import_statistics_mocked()
        result = stats_module.StatisticsResult()
        assert result.errors == []
        assert result.warnings == []

    def test_getitem_with_no_tables_raises(self):
        """Accessing items with no tables raises TypeError."""
        stats_module, _, _ = _import_statistics_mocked()
        result = stats_module.StatisticsResult()
        with pytest.raises(TypeError):
            result["something"]

    def test_constructor_with_errors_and_warnings(self):
        """Constructor accepts errors and warnings parameters."""
        stats_module, _, _ = _import_statistics_mocked()
        errors = ["Error 1", "Error 2"]
        warnings = ["Warning A"]
        result = stats_module.StatisticsResult(
            errors=errors, warnings=warnings
        )
        assert result.errors == ["Error 1", "Error 2"]
        assert result.warnings == ["Warning A"]

    def test_constructor_with_display_results_and_polars(self):
        """Constructor accepts display_results and use_polars parameters."""
        stats_module, _, _ = _import_statistics_mocked()
        mock_dr = MagicMock()
        result = stats_module.StatisticsResult(
            display_results=mock_dr, use_polars=True
        )
        assert result._display_results is mock_dr
        assert result._use_polars is True

    def test_len_empty_result(self):
        """len() returns 0 for empty result."""
        stats_module, _, _ = _import_statistics_mocked()
        result = stats_module.StatisticsResult()
        assert len(result) == 0

    def test_iter_empty_result(self):
        """iter() returns empty iterator for empty result."""
        stats_module, _, _ = _import_statistics_mocked()
        result = stats_module.StatisticsResult()
        assert list(result) == []


class TestStatisticsTable:
    """Test StatisticsTable class."""

    def test_table_repr(self):
        """Table repr shows name and title."""
        stats_module, _, _ = _import_statistics_mocked()
        table = stats_module.StatisticsTable()
        table._name = "Inequality"
        table._title = "Inequality Indicators"
        assert "Inequality" in repr(table)
        assert "Inequality Indicators" in repr(table)

    def test_table_getitem_missing_key(self):
        """Accessing non-existent key raises KeyError."""
        stats_module, _, _ = _import_statistics_mocked()
        table = stats_module.StatisticsTable()
        table._name = "test_table"
        with pytest.raises(KeyError, match="not found"):
            table["nonexistent"]

    def test_table_getitem_existing_key(self):
        """Accessing existing key returns the value dict."""
        stats_module, _, _ = _import_statistics_mocked()
        table = stats_module.StatisticsTable()
        table._values = {"Gini": {"Baseline": 0.312}}
        result = table["Gini"]
        assert result == {"Baseline": 0.312}

    def test_table_properties(self):
        """Table properties return the correct values."""
        stats_module, _, _ = _import_statistics_mocked()
        table = stats_module.StatisticsTable()
        table._name = "Income"
        table._title = "Income Distribution"
        assert table.name == "Income"
        assert table.title == "Income Distribution"
        assert table.dataframe is None
        assert table.values == {}

    def test_table_container_methods(self):
        """Table provides container repr methods for Container compatibility."""
        stats_module, _, _ = _import_statistics_mocked()
        table = stats_module.StatisticsTable()
        table._name = "Income"
        table._title = "Income Distribution"
        assert table._container_begin_repr() == "Income"
        assert table._container_middle_repr() == "Income Distribution"
        assert table._container_end_repr() == ""
        assert table._short_repr() == "Income"


class TestDataframeToClrData:
    """Test _dataframe_to_clr_data utility function."""

    def _get_module(self):
        """Import statistics module with CLR mocked, including asNetArray mock."""
        import numpy as np

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()

        # Mock asNetArray to return the input numpy array (identity mock for testing)
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

                # Also mock utils.clr_array_convert.asNetArray
                mock_clr_convert = MagicMock()
                mock_clr_convert.asNetArray = mock_as_net_array
                sys.modules['euromod.utils.clr_array_convert'] = mock_clr_convert
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    # Patch asNetArray on the imported module
                    stats_module.asNetArray = mock_as_net_array
                    return stats_module, mock_as_net_array
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_pandas_numeric_columns_extracted(self):
        """Pandas DataFrame filters to numeric columns and returns correct names."""
        import numpy as np
        import pandas as pd

        stats_module, mock_net = self._get_module()

        df = pd.DataFrame({
            'income': [1000.0, 2000.0, 3000.0],
            'name': ['Alice', 'Bob', 'Charlie'],  # non-numeric, should be filtered
            'age': [30, 40, 50],
        })

        cols, data = stats_module._dataframe_to_clr_data(df)

        assert 'income' in cols
        assert 'age' in cols
        assert 'name' not in cols
        assert len(cols) == 2

    def test_pandas_data_shape_is_transposed(self):
        """Pandas data is transposed to [num_variables, num_observations]."""
        import numpy as np
        import pandas as pd

        stats_module, mock_net = self._get_module()

        df = pd.DataFrame({
            'var1': [1.0, 2.0, 3.0],
            'var2': [4.0, 5.0, 6.0],
        })

        cols, data = stats_module._dataframe_to_clr_data(df)

        # asNetArray mock returns the numpy array directly
        assert data.shape == (2, 3)  # 2 variables, 3 observations
        assert cols == ['var1', 'var2']

    def test_polars_numeric_columns_extracted(self):
        """Polars DataFrame filters to numeric columns and returns correct names."""
        import numpy as np
        import polars as pl

        stats_module, mock_net = self._get_module()

        df = pl.DataFrame({
            'income': [1000.0, 2000.0, 3000.0],
            'name': ['Alice', 'Bob', 'Charlie'],  # non-numeric, should be filtered
            'age': [30, 40, 50],
        })

        cols, data = stats_module._dataframe_to_clr_data(df)

        assert 'income' in cols
        assert 'age' in cols
        assert 'name' not in cols
        assert len(cols) == 2

    def test_polars_data_shape_is_transposed(self):
        """Polars data is transposed to [num_variables, num_observations]."""
        import numpy as np
        import polars as pl

        stats_module, mock_net = self._get_module()

        df = pl.DataFrame({
            'var1': [1.0, 2.0, 3.0],
            'var2': [4.0, 5.0, 6.0],
        })

        cols, data = stats_module._dataframe_to_clr_data(df)

        assert data.shape == (2, 3)  # 2 variables, 3 observations
        assert cols == ['var1', 'var2']

    def test_invalid_type_raises_typeerror(self):
        """Non-DataFrame input raises TypeError."""
        stats_module, _ = self._get_module()

        with pytest.raises(TypeError, match="must be a pandas.DataFrame or polars.DataFrame"):
            stats_module._dataframe_to_clr_data({"not": "a dataframe"})

    def test_no_numeric_columns_raises_valueerror(self):
        """DataFrame with no numeric columns raises ValueError."""
        import pandas as pd

        stats_module, _ = self._get_module()

        df = pd.DataFrame({
            'name': ['Alice', 'Bob'],
            'city': ['Paris', 'London'],
        })

        with pytest.raises(ValueError, match="No numeric columns found"):
            stats_module._dataframe_to_clr_data(df)

    def test_data_values_are_correct_pandas(self):
        """Data values are correctly converted for pandas."""
        import numpy as np
        import pandas as pd

        stats_module, _ = self._get_module()

        df = pd.DataFrame({
            'a': [1.5, 2.5],
            'b': [3.0, 4.0],
        })

        cols, data = stats_module._dataframe_to_clr_data(df)

        # Transposed: row 0 = var 'a', row 1 = var 'b'
        np.testing.assert_array_almost_equal(data[0], [1.5, 2.5])
        np.testing.assert_array_almost_equal(data[1], [3.0, 4.0])

    def test_data_values_are_correct_polars(self):
        """Data values are correctly converted for polars."""
        import numpy as np
        import polars as pl

        stats_module, _ = self._get_module()

        df = pl.DataFrame({
            'a': [1.5, 2.5],
            'b': [3.0, 4.0],
        })

        cols, data = stats_module._dataframe_to_clr_data(df)

        np.testing.assert_array_almost_equal(data[0], [1.5, 2.5])
        np.testing.assert_array_almost_equal(data[1], [3.0, 4.0])

    def test_asnetarray_called_with_float64(self):
        """asNetArray is called with a float64 contiguous array."""
        import numpy as np
        import pandas as pd

        stats_module, mock_net = self._get_module()

        df = pd.DataFrame({'x': [1, 2, 3]})

        stats_module._dataframe_to_clr_data(df)

        # Check asNetArray was called
        mock_net.assert_called_once()
        arr_arg = mock_net.call_args[0][0]
        assert arr_arg.dtype == np.float64
        assert arr_arg.flags.c_contiguous


class TestSimulationToClrData:
    """Test _simulation_to_clr_data utility function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_extracts_first_output_by_default(self):
        """Default output_index=0 extracts the first DataFrame."""
        import pandas as pd

        stats_module = self._get_module()

        # Mock a Simulation object with outputs Container
        mock_sim = MagicMock()
        df = pd.DataFrame({'income': [100.0, 200.0], 'weight': [1.0, 1.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        cols, data = stats_module._simulation_to_clr_data(mock_sim)

        assert 'income' in cols
        assert 'weight' in cols
        mock_sim.outputs.__getitem__.assert_called_once_with(0)

    def test_extracts_specified_output_index(self):
        """Specified output_index is used to access the outputs container."""
        import pandas as pd

        stats_module = self._get_module()

        mock_sim = MagicMock()
        df = pd.DataFrame({'var1': [10.0, 20.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        cols, data = stats_module._simulation_to_clr_data(mock_sim, output_index=2)

        mock_sim.outputs.__getitem__.assert_called_once_with(2)

    def test_invalid_output_index_raises_indexerror(self):
        """Out-of-range output_index raises IndexError."""
        stats_module = self._get_module()

        mock_sim = MagicMock()
        mock_sim.outputs.__getitem__ = MagicMock(side_effect=IndexError("list index out of range"))
        mock_sim.outputs.__len__ = MagicMock(return_value=1)

        with pytest.raises(IndexError, match="Output index .* is out of range"):
            stats_module._simulation_to_clr_data(mock_sim, output_index=5)

    def test_delegates_to_dataframe_to_clr_data(self):
        """_simulation_to_clr_data delegates to _dataframe_to_clr_data."""
        import pandas as pd

        stats_module = self._get_module()

        df = pd.DataFrame({'x': [1.0, 2.0, 3.0]})
        mock_sim = MagicMock()
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        cols, data = stats_module._simulation_to_clr_data(mock_sim)

        assert cols == ['x']
        assert data.shape == (1, 3)

    def test_pandas_dataframe_passed_directly(self):
        """A bare pandas DataFrame is converted directly (no .outputs access)."""
        import pandas as pd

        stats_module = self._get_module()

        df = pd.DataFrame({'a': [1.0, 2.0], 'b': [3.0, 4.0]})
        cols, data = stats_module._simulation_to_clr_data(df)

        assert cols == ['a', 'b']
        assert data.shape == (2, 2)

    def test_dataframe_matches_dataframe_to_clr_data(self):
        """A bare DataFrame yields the same result as _dataframe_to_clr_data."""
        import pandas as pd

        stats_module = self._get_module()

        df = pd.DataFrame({'x': [1.0, 2.0, 3.0], 'y': [4.0, 5.0, 6.0]})
        cols_a, data_a = stats_module._simulation_to_clr_data(df)
        cols_b, data_b = stats_module._dataframe_to_clr_data(df)

        assert cols_a == cols_b
        assert data_a.shape == data_b.shape

    def test_unsupported_type_raises_typeerror(self):
        """A non-Simulation, non-DataFrame input raises a clear TypeError."""
        stats_module = self._get_module()

        with pytest.raises(TypeError, match="Expected a Simulation"):
            stats_module._simulation_to_clr_data([1, 2, 3])

    def test_get_observation_count_pandas_dataframe(self):
        """_get_observation_count counts rows of a bare pandas DataFrame."""
        import pandas as pd

        stats_module = self._get_module()
        df = pd.DataFrame({'a': [1.0, 2.0, 3.0]})
        assert stats_module._get_observation_count(df) == 3

    def test_get_observation_count_polars_dataframe(self):
        """_get_observation_count counts rows of a bare polars DataFrame."""
        import polars as pl

        stats_module = self._get_module()
        df = pl.DataFrame({'a': [1.0, 2.0, 3.0, 4.0]})
        assert stats_module._get_observation_count(df) == 4


class TestCalculateMethod:
    """Test Statistics.calculate() method."""

    def _get_module(self):
        """Import statistics module with CLR mocked, including System namespace."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
        mock_as_net_array = MagicMock(side_effect=lambda x: x)

        # Mock System namespace for .NET List types
        mock_system = MagicMock()
        mock_list_class = MagicMock()
        mock_system.Collections.Generic.List.__getitem__ = MagicMock(return_value=mock_list_class)
        mock_system.String = str
        mock_system.Array = MagicMock()

        with patch.dict('sys.modules', {
            'clr': mock_clr,
            'System': mock_system,
        }):
            original_exists = os.path.exists

            def patched_exists(path):
                if 'EM_Statistics.dll' in str(path):
                    return True
                return original_exists(path)

            with patch('os.path.exists', side_effect=patched_exists):
                mock_em_stats_module = MagicMock()
                mock_hard_defs = MagicMock()
                mock_hard_defs.TemplateType.Default = "Default"
                mock_hard_defs.TemplateType.BaselineReform = "BaselineReform"
                mock_hard_defs.TemplateType.Multi = "Multi"
                mock_hard_defs.UserInputType.VariableName = "VariableName"
                mock_em_stats_module.HardDefinitions = mock_hard_defs
                mock_em_stats_module.EM_TemplateCalculator = MagicMock()
                mock_em_stats_module.XML_handling = MagicMock()
                mock_em_stats_module.Template = MagicMock()
                mock_em_stats_module.ErrorCollector = MagicMock()

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
                    stats_module.SystemCs = mock_system
                    return stats_module, mock_em_stats_module, mock_hard_defs, mock_system
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def _create_stats_instance(self, stats_module, mock_xml_handling, mock_hard_defs, template_type="Default"):
        """Create a Statistics instance with mocked template."""
        import tempfile, os

        # Create a temporary file to pass path validation
        tmp = tempfile.NamedTemporaryFile(suffix='.xml', delete=False, mode='w')
        tmp.write('<Template></Template>')
        tmp.close()

        mock_template = MagicMock()
        mock_template.info.templateType = template_type
        mock_template.info.requiredVariables = []
        mock_template.info.userVariables = None

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_xml_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        stats = stats_module.Statistics(tmp.name)
        os.unlink(tmp.name)
        return stats

    def test_default_template_calls_prepare_and_calculate(self):
        """Default template: calls PrepareFromData then CalculateStatisticsFromPreparedData."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        # Mock the calculator
        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        # Mock a Simulation object
        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'idhh': [1.0, 1.0], 'dwt': [1.0, 1.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        result = stats.calculate(mock_sim)

        assert isinstance(result, stats_module.StatisticsResult)
        mock_calculator_instance.PrepareFromData.assert_called_once()
        mock_calculator_instance.CalculateStatisticsFromPreparedData.assert_called_once()

    def test_default_template_accepts_pandas_dataframe_baseline(self):
        """A bare pandas DataFrame baseline reaches Prepare+Calculate."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.GetDisplayResults.return_value = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        # Pass a DataFrame directly instead of a Simulation
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'idhh': [1.0, 1.0], 'dwt': [1.0, 1.0]})
        result = stats.calculate(df)

        assert isinstance(result, stats_module.StatisticsResult)
        assert result._use_polars is False
        mock_calculator_instance.PrepareFromData.assert_called_once()
        mock_calculator_instance.CalculateStatisticsFromPreparedData.assert_called_once()

    def _stats_with_ok_calculator(self):
        """Helper: a Default Statistics instance wired to a succeeding mock calculator."""
        import pandas as pd
        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )
        calc = MagicMock()
        ec = MagicMock()
        ec.HasErrors.return_value = False
        ec.GetErrorMessage.return_value = ""
        calc.PrepareFromData.return_value = (True, ec)
        calc.CalculateStatisticsFromPreparedData.return_value = True
        calc.GetDisplayResults.return_value = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = calc
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'idhh': [1.0, 1.0], 'dwt': [1.0, 1.0]})
        return stats_module, stats, calc, df

    def test_pages_filter_calls_keeponly_and_restore(self):
        """pages= restricts via Template.KeepOnly and always restores afterwards."""
        stats_module, stats, calc, df = self._stats_with_ok_calculator()
        stats._template.KeepOnly = MagicMock(return_value=[])
        stats._template.RestoreActive = MagicMock()

        stats.calculate(df, pages=['Poverty'])

        stats._template.KeepOnly.assert_called_once()
        stats._template.RestoreActive.assert_called_once()

    def test_no_filter_leaves_activation_untouched(self):
        """Without pages/tables, KeepOnly/RestoreActive are not invoked."""
        stats_module, stats, calc, df = self._stats_with_ok_calculator()
        stats._template.KeepOnly = MagicMock(return_value=[])
        stats._template.RestoreActive = MagicMock()

        stats.calculate(df)

        stats._template.KeepOnly.assert_not_called()
        stats._template.RestoreActive.assert_not_called()

    def test_filter_restores_active_state_on_error(self):
        """A failure during a restricted calculation still restores the template."""
        stats_module, stats, calc, df = self._stats_with_ok_calculator()
        ec = MagicMock()
        ec.HasErrors.return_value = True
        ec.GetErrorMessage.return_value = "boom"
        calc.PrepareFromData.return_value = (False, ec)
        stats._template.KeepOnly = MagicMock(return_value=[])
        stats._template.RestoreActive = MagicMock()

        with pytest.raises(RuntimeError):
            stats.calculate(df, tables=['3.1 Income inequality indices'])

        stats._template.RestoreActive.assert_called_once()

    def test_list_structure_and_helpers(self):
        """list_structure/list_pages/list_tables read the C# GetStructure() map."""
        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        class _NetDict:  # mimics a .NET Dictionary<string, List<string>>
            def __init__(self, d): self._d = d
            @property
            def Keys(self): return list(self._d.keys())
            def __getitem__(self, k): return self._d[k]

        stats._template.GetStructure = MagicMock(
            return_value=_NetDict({'P1': ['T1', 'T2'], 'P2': ['T3']})
        )

        assert stats.list_structure() == {'P1': ['T1', 'T2'], 'P2': ['T3']}
        assert stats.list_pages() == ['P1', 'P2']
        assert stats.list_tables('P1') == ['T1', 'T2']
        assert stats.list_tables() == ['T1', 'T2', 'T3']

    def test_missing_required_variables_raises_valueerror(self):
        """Missing required variables raises ValueError with variable names."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        # Configure template to have required variables not in data
        mock_req_var = MagicMock()
        mock_req_var.name = "ils_dispy"
        mock_req_var.readVar = "ils_dispy"
        stats._template.info.requiredVariables = [mock_req_var]

        # Mock a Simulation with data that doesn't have the required variable
        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'income': [100.0, 200.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(ValueError, match="ils_dispy"):
            stats.calculate(mock_sim)

    def test_missing_variables_error_lists_all_missing(self):
        """Error message lists all missing variable names."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        # Multiple missing required variables
        mock_req1 = MagicMock()
        mock_req1.name = "ils_dispy"
        mock_req1.readVar = "ils_dispy"
        mock_req2 = MagicMock()
        mock_req2.name = "dhi"
        mock_req2.readVar = "dhi"
        stats._template.info.requiredVariables = [mock_req1, mock_req2]

        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'income': [100.0, 200.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(ValueError, match="ils_dispy") as exc_info:
            stats.calculate(mock_sim)
        assert "dhi" in str(exc_info.value)

    def test_baseline_reform_without_reforms_raises_valueerror(self):
        """BaselineReform template without reforms raises ValueError."""
        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "BaselineReform"
        )

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="BaselineReform.*requires"):
            stats.calculate(mock_sim)

    def test_multi_template_without_reforms_raises_valueerror(self):
        """Multi template without reforms raises ValueError."""
        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Multi"
        )

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="Multi.*requires"):
            stats.calculate(mock_sim)

    def test_prepare_from_data_failure_raises_runtime_error(self):
        """PrepareFromData failure raises RuntimeError."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = True
        mock_error_collector.GetErrorMessage.return_value = "Some preparation error"
        mock_calculator_instance.PrepareFromData.return_value = (False, mock_error_collector)
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0], 'income': [100.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(RuntimeError, match="PrepareFromData failed"):
            stats.calculate(mock_sim)

    def test_prepare_missing_variable_error_raises_valueerror(self):
        """PrepareFromData with missing variable error raises ValueError."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = True
        mock_error_collector.GetErrorMessage.return_value = "Data does not contain required variable(s) 'dwt'"
        mock_calculator_instance.PrepareFromData.return_value = (False, mock_error_collector)
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0], 'income': [100.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(ValueError, match="required variable"):
            stats.calculate(mock_sim)

    def test_calculate_statistics_failure_raises_runtime_error(self):
        """CalculateStatisticsFromPreparedData failure raises RuntimeError."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = False

        # Make error collector report error after failed calculation
        mock_error_collector.HasErrors.side_effect = [False, True]
        mock_error_collector.GetErrorMessage.side_effect = ["", "Calculation error"]

        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0], 'income': [100.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(RuntimeError, match="CalculateStatisticsFromPreparedData failed"):
            stats.calculate(mock_sim)

    def test_successful_calculate_returns_result_with_display_results(self):
        """Successful calculation stores displayResults on the result object."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_display_results = MagicMock()
        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.GetDisplayResults.return_value = mock_display_results
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0], 'income': [100.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        result = stats.calculate(mock_sim)

        assert result._display_results is mock_display_results

    def test_warnings_are_collected_on_success(self):
        """Non-fatal warnings from error collector are stored in result."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = "Some non-fatal warning"
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0], 'income': [100.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        result = stats.calculate(mock_sim)

        assert len(result.warnings) == 1
        assert "non-fatal warning" in result.warnings[0]

    def test_polars_simulation_detected(self):
        """Polars simulation sets _use_polars flag on result."""
        import polars as pl

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pl.DataFrame({'idperson': [1.0], 'income': [100.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        result = stats.calculate(mock_sim)

        assert result._use_polars is True

    def test_variable_validation_case_insensitive(self):
        """Variable validation is case-insensitive."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Default"
        )

        # Required variable with different case
        mock_req_var = MagicMock()
        mock_req_var.name = "ILS_DISPY"
        mock_req_var.readVar = "ILS_DISPY"
        stats._template.info.requiredVariables = [mock_req_var]

        # Data has variable in lowercase
        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'ils_dispy': [1000.0, 2000.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        # Should NOT raise - case insensitive match
        result = stats.calculate(mock_sim)
        assert isinstance(result, stats_module.StatisticsResult)

    def test_baseline_reform_calls_prepare_with_correct_data(self):
        """BaselineReform template passes baseline + reform data to PrepareFromData."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "BaselineReform"
        )

        # Mock the calculator
        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        # Baseline simulation
        mock_baseline = MagicMock()
        baseline_df = pd.DataFrame({'idperson': [1.0, 2.0], 'income': [100.0, 200.0]})
        mock_baseline.outputs.__getitem__ = MagicMock(return_value=baseline_df)
        mock_baseline.output_filenames = ["baseline_output"]

        # Reform simulation (same observation count)
        mock_reform = MagicMock()
        reform_df = pd.DataFrame({'idperson': [1.0, 2.0], 'income': [150.0, 250.0]})
        mock_reform.outputs.__getitem__ = MagicMock(return_value=reform_df)
        mock_reform.output_filenames = ["reform_output"]

        result = stats.calculate(mock_baseline, reforms=[mock_reform])

        assert isinstance(result, stats_module.StatisticsResult)
        mock_calculator_instance.PrepareFromData.assert_called_once()
        mock_calculator_instance.CalculateStatisticsFromPreparedData.assert_called_once()

    def test_baseline_reform_mismatched_observations_raises_valueerror(self):
        """BaselineReform with mismatched observation counts raises ValueError."""
        import pandas as pd
        import numpy as np

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "BaselineReform"
        )

        # Baseline simulation - 3 observations
        mock_baseline = MagicMock()
        baseline_df = pd.DataFrame({
            'idperson': [1.0, 2.0, 3.0],
            'income': [100.0, 200.0, 300.0]
        })
        mock_baseline.outputs.__getitem__ = MagicMock(return_value=baseline_df)

        # Reform simulation - 2 observations (different from baseline!)
        mock_reform = MagicMock()
        reform_df = pd.DataFrame({
            'idperson': [1.0, 2.0],
            'income': [150.0, 250.0]
        })
        mock_reform.outputs.__getitem__ = MagicMock(return_value=reform_df)

        with pytest.raises(ValueError, match="[Oo]bservation count mismatch"):
            stats.calculate(mock_baseline, reforms=[mock_reform])

    def test_baseline_reform_multiple_reforms(self):
        """BaselineReform with multiple reforms passes all to PrepareFromData."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "BaselineReform"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        # All have 2 observations
        mock_baseline = MagicMock()
        mock_baseline.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [100.0, 200.0]})
        )
        mock_baseline.output_filenames = []

        mock_reform1 = MagicMock()
        mock_reform1.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [110.0, 210.0]})
        )
        mock_reform1.output_filenames = ["reform_1"]

        mock_reform2 = MagicMock()
        mock_reform2.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [120.0, 220.0]})
        )
        mock_reform2.output_filenames = ["reform_2"]

        result = stats.calculate(mock_baseline, reforms=[mock_reform1, mock_reform2])

        assert isinstance(result, stats_module.StatisticsResult)
        mock_calculator_instance.PrepareFromData.assert_called_once()

    def test_multi_template_passes_all_systems(self):
        """Multi template passes all simulations as separate baseline systems."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "Multi"
        )

        mock_calculator_instance = MagicMock()
        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False
        mock_error_collector.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_error_collector)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        # Create multiple simulations
        mock_sim1 = MagicMock()
        mock_sim1.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [100.0, 200.0]})
        )
        mock_sim1.output_filenames = ["system_a"]

        mock_sim2 = MagicMock()
        mock_sim2.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [150.0, 250.0]})
        )
        mock_sim2.output_filenames = ["system_b"]

        mock_sim3 = MagicMock()
        mock_sim3.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [180.0, 280.0]})
        )
        mock_sim3.output_filenames = ["system_c"]

        result = stats.calculate(mock_sim1, reforms=[mock_sim2, mock_sim3])

        assert isinstance(result, stats_module.StatisticsResult)
        mock_calculator_instance.PrepareFromData.assert_called_once()
        mock_calculator_instance.CalculateStatisticsFromPreparedData.assert_called_once()

    def test_baseline_reform_second_reform_mismatched_raises(self):
        """BaselineReform: second reform with different obs count raises ValueError."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()
        stats = self._create_stats_instance(
            stats_module, mock_em.XML_handling, mock_hard_defs, "BaselineReform"
        )

        # Baseline: 3 observations
        mock_baseline = MagicMock()
        mock_baseline.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0, 3.0], 'income': [100.0, 200.0, 300.0]})
        )

        # Reform 1: 3 observations (matches)
        mock_reform1 = MagicMock()
        mock_reform1.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0, 3.0], 'income': [110.0, 210.0, 310.0]})
        )
        mock_reform1.output_filenames = ["reform_1"]

        # Reform 2: 2 observations (mismatch!)
        mock_reform2 = MagicMock()
        mock_reform2.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'idperson': [1.0, 2.0], 'income': [120.0, 220.0]})
        )
        mock_reform2.output_filenames = ["reform_2"]

        with pytest.raises(ValueError, match="[Oo]bservation count mismatch"):
            stats.calculate(mock_baseline, reforms=[mock_reform1, mock_reform2])

    def test_variable_template_valid_variable_succeeds(self):
        """Variable template with a valid variable proceeds without error."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()

        # Create a Statistics instance with variable parameter
        import tempfile
        tmp = tempfile.NamedTemporaryFile(suffix='.xml', delete=False, mode='w')
        tmp.write('<Template></Template>')
        tmp.close()

        mock_template = MagicMock()
        mock_template.info.templateType = "Default"
        mock_template.info.requiredVariables = []

        # Set up userVariables with VariableName type
        mock_user_var = MagicMock()
        mock_user_var.inputType = mock_hard_defs.UserInputType.VariableName
        mock_user_var.value = ""
        mock_template.info.userVariables = [mock_user_var]

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_em.XML_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        stats = stats_module.Statistics(tmp.name, variable="ils_dispy")
        os.unlink(tmp.name)

        # Mock the calculator for successful calculation
        mock_calculator_instance = MagicMock()
        mock_calc_error = MagicMock()
        mock_calc_error.HasErrors.return_value = False
        mock_calc_error.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_calc_error)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        # Simulation with the variable present
        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'ils_dispy': [1000.0, 2000.0], 'dwt': [1.0, 1.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        # Should succeed without raising
        result = stats.calculate(mock_sim)
        assert isinstance(result, stats_module.StatisticsResult)

    def test_variable_template_invalid_variable_raises_valueerror(self):
        """Variable template with an invalid variable raises ValueError with the variable name."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()

        # Create a Statistics instance with a variable that doesn't exist in the data
        import tempfile
        tmp = tempfile.NamedTemporaryFile(suffix='.xml', delete=False, mode='w')
        tmp.write('<Template></Template>')
        tmp.close()

        mock_template = MagicMock()
        mock_template.info.templateType = "Default"
        mock_template.info.requiredVariables = []

        mock_user_var = MagicMock()
        mock_user_var.inputType = mock_hard_defs.UserInputType.VariableName
        mock_user_var.value = ""
        mock_template.info.userVariables = [mock_user_var]

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_em.XML_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        stats = stats_module.Statistics(tmp.name, variable="nonexistent_var")
        os.unlink(tmp.name)

        # Simulation without the requested variable
        mock_sim = MagicMock()
        df = pd.DataFrame({'idperson': [1.0, 2.0], 'ils_dispy': [1000.0, 2000.0], 'dwt': [1.0, 1.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(ValueError, match="nonexistent_var"):
            stats.calculate(mock_sim)

    def test_variable_template_error_lists_available_columns(self):
        """ValueError message includes available column alternatives."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()

        import tempfile
        tmp = tempfile.NamedTemporaryFile(suffix='.xml', delete=False, mode='w')
        tmp.write('<Template></Template>')
        tmp.close()

        mock_template = MagicMock()
        mock_template.info.templateType = "Default"
        mock_template.info.requiredVariables = []

        mock_user_var = MagicMock()
        mock_user_var.inputType = mock_hard_defs.UserInputType.VariableName
        mock_user_var.value = ""
        mock_template.info.userVariables = [mock_user_var]

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_em.XML_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        stats = stats_module.Statistics(tmp.name, variable="bad_variable")
        os.unlink(tmp.name)

        # Simulation with known columns
        mock_sim = MagicMock()
        df = pd.DataFrame({'income': [1000.0], 'weight': [1.0], 'age': [30.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        with pytest.raises(ValueError) as exc_info:
            stats.calculate(mock_sim)

        error_msg = str(exc_info.value)
        # Should contain the variable name
        assert "bad_variable" in error_msg
        # Should list available columns as alternatives
        assert "income" in error_msg
        assert "weight" in error_msg
        assert "age" in error_msg

    def test_variable_template_case_insensitive_match(self):
        """Variable validation is case-insensitive (e.g., ILS_DISPY matches ils_dispy)."""
        import pandas as pd

        stats_module, mock_em, mock_hard_defs, mock_system = self._get_module()

        import tempfile
        tmp = tempfile.NamedTemporaryFile(suffix='.xml', delete=False, mode='w')
        tmp.write('<Template></Template>')
        tmp.close()

        mock_template = MagicMock()
        mock_template.info.templateType = "Default"
        mock_template.info.requiredVariables = []

        mock_user_var = MagicMock()
        mock_user_var.inputType = mock_hard_defs.UserInputType.VariableName
        mock_user_var.value = ""
        mock_template.info.userVariables = [mock_user_var]

        mock_error_collector = MagicMock()
        mock_error_collector.HasErrors.return_value = False

        mock_em.XML_handling.ParseTemplate.return_value = (True, mock_template, mock_error_collector)

        # Variable in uppercase, data in lowercase
        stats = stats_module.Statistics(tmp.name, variable="ILS_DISPY")
        os.unlink(tmp.name)

        # Mock the calculator for successful calculation
        mock_calculator_instance = MagicMock()
        mock_calc_error = MagicMock()
        mock_calc_error.HasErrors.return_value = False
        mock_calc_error.GetErrorMessage.return_value = ""
        mock_calculator_instance.PrepareFromData.return_value = (True, mock_calc_error)
        mock_calculator_instance.CalculateStatisticsFromPreparedData.return_value = True
        mock_calculator_instance.displayResults = MagicMock()
        mock_em.EM_TemplateCalculator.return_value = mock_calculator_instance

        mock_sim = MagicMock()
        df = pd.DataFrame({'ils_dispy': [1000.0, 2000.0], 'dwt': [1.0, 1.0]})
        mock_sim.outputs.__getitem__ = MagicMock(return_value=df)

        # Should NOT raise - case insensitive match
        result = stats.calculate(mock_sim)
        assert isinstance(result, stats_module.StatisticsResult)


class TestGetSimulationName:
    """Test _get_simulation_name helper function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                sys.modules['euromod.utils.clr_array_convert'] = MagicMock()
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    stats_module.asNetArray = mock_as_net_array
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']

    def test_returns_output_filename_when_available(self):
        """Returns first output_filename when available."""
        stats_module = self._get_module()
        mock_sim = MagicMock()
        mock_sim.output_filenames = ["my_system_output"]
        assert stats_module._get_simulation_name(mock_sim) == "my_system_output"

    def test_returns_default_when_no_output_filenames(self):
        """Returns default when output_filenames is empty."""
        stats_module = self._get_module()
        mock_sim = MagicMock()
        mock_sim.output_filenames = []
        assert stats_module._get_simulation_name(mock_sim, default="Reform_0") == "Reform_0"

    def test_returns_default_when_attr_missing(self):
        """Returns default when output_filenames attribute doesn't exist."""
        stats_module = self._get_module()
        mock_sim = MagicMock(spec=[])  # No attributes
        assert stats_module._get_simulation_name(mock_sim, default="System_1") == "System_1"

    def test_dataframe_returns_default(self):
        """A bare DataFrame has no filename and is named by the default."""
        import pandas as pd
        stats_module = self._get_module()
        df = pd.DataFrame({'x': [1.0]})
        assert stats_module._get_simulation_name(df, default="Reform_0") == "Reform_0"


class TestIsPolarsSimulation:
    """Test _is_polars_simulation helper function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                sys.modules['euromod.utils.clr_array_convert'] = MagicMock()
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    stats_module.asNetArray = mock_as_net_array
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']

    def test_pandas_simulation_returns_false(self):
        """Pandas-based simulation returns False."""
        import pandas as pd
        stats_module = self._get_module()
        mock_sim = MagicMock()
        mock_sim.outputs.__getitem__ = MagicMock(
            return_value=pd.DataFrame({'x': [1.0]})
        )
        assert stats_module._is_polars_simulation(mock_sim) is False

    def test_polars_simulation_returns_true(self):
        """Polars-based simulation returns True."""
        import polars as pl
        stats_module = self._get_module()
        mock_sim = MagicMock()
        mock_sim.outputs.__getitem__ = MagicMock(
            return_value=pl.DataFrame({'x': [1.0]})
        )
        assert stats_module._is_polars_simulation(mock_sim) is True

    def test_empty_outputs_returns_false(self):
        """Simulation with no outputs returns False."""
        stats_module = self._get_module()
        mock_sim = MagicMock()
        mock_sim.outputs.__getitem__ = MagicMock(side_effect=IndexError)
        assert stats_module._is_polars_simulation(mock_sim) is False

    def test_bare_pandas_dataframe_returns_false(self):
        """A bare pandas DataFrame is detected as non-polars."""
        import pandas as pd
        stats_module = self._get_module()
        assert stats_module._is_polars_simulation(pd.DataFrame({'x': [1.0]})) is False

    def test_bare_polars_dataframe_returns_true(self):
        """A bare polars DataFrame is detected as polars."""
        import polars as pl
        stats_module = self._get_module()
        assert stats_module._is_polars_simulation(pl.DataFrame({'x': [1.0]})) is True


class TestStatisticsResultTableBuilding:
    """Test StatisticsResult table building from DisplayResults."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                sys.modules['euromod.utils.clr_array_convert'] = MagicMock()
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    stats_module.asNetArray = mock_as_net_array
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']

    def _make_display_cell(self, value=0.0, display_value="", is_string_value=False):
        """Create a mock DisplayCell."""
        cell = MagicMock()
        cell.value = value
        cell.displayValue = display_value
        cell.isStringValue = is_string_value
        return cell

    def _make_display_table(self, name="", title="", col_titles=None, row_titles=None, cell_data=None):
        """Create a mock DisplayTable.

        Parameters
        ----------
        name : str
        title : str
        col_titles : list[str]
        row_titles : list[str]
        cell_data : list[list[tuple(float, str, bool)]]
            Each element is (value, displayValue, isStringValue)
        """
        table = MagicMock()
        table.name = name
        table.title = title

        # Build columns
        if col_titles is not None:
            columns = []
            for ct in col_titles:
                col = MagicMock()
                col.title = ct
                columns.append(col)
            table.columns = columns
        else:
            table.columns = None

        # Build rows
        if row_titles is not None:
            rows = []
            for rt in row_titles:
                row = MagicMock()
                row.title = rt
                rows.append(row)
            table.rows = rows
        else:
            table.rows = None

        # Build cells
        if cell_data is not None:
            cells = []
            for row_data in cell_data:
                row_cells = []
                for val, disp, is_str in row_data:
                    row_cells.append(self._make_display_cell(val, disp, is_str))
                cells.append(row_cells)
            table.cells = cells
        else:
            table.cells = None

        return table

    def _make_display_results(self, pages_data):
        """Create a mock DisplayResults.

        Parameters
        ----------
        pages_data : list[dict]
            Each dict has keys: 'name', 'tables' (list of DisplayTable mocks)
        """
        dr = MagicMock()
        pages = []
        for page_data in pages_data:
            page = MagicMock()
            page.name = page_data.get("name", "")
            page.displayTables = page_data.get("tables", [])
            pages.append(page)
        dr.displayPages = pages
        return dr

    def test_tables_built_from_display_results(self):
        """Tables are built lazily from DisplayResults on first access."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality",
            title="Inequality Indicators",
            col_titles=["Baseline"],
            row_titles=["Gini", "S80/S20"],
            cell_data=[
                [(0.312, "0.312", False)],
                [(4.5, "4.5", False)],
            ],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        tables = result.tables
        assert tables is not None
        assert len(tables) == 1

    def test_tables_accessible_by_name(self):
        """Tables can be accessed by name via Container."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality",
            title="Inequality Indicators",
            col_titles=["Baseline"],
            row_titles=["Gini"],
            cell_data=[[(0.312, "0.312", False)]],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        table = result.tables["Inequality"]
        assert table.name == "Inequality"
        assert table.title == "Inequality Indicators"

    def test_tables_accessible_by_index(self):
        """Tables can be accessed by integer index via Container."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality",
            title="Inequality Indicators",
            col_titles=["Baseline"],
            row_titles=["Gini"],
            cell_data=[[(0.312, "0.312", False)]],
        )

        table2 = self._make_display_table(
            name="Poverty",
            title="Poverty Indicators",
            col_titles=["Baseline"],
            row_titles=["Rate"],
            cell_data=[[(0.15, "15.0%", False)]],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1, table2]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        assert result.tables[0].name == "Inequality"
        assert result.tables[1].name == "Poverty"

    def test_getitem_delegates_to_tables(self):
        """__getitem__ delegates to the tables Container."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality",
            title="Inequality Indicators",
            col_titles=["Baseline"],
            row_titles=["Gini"],
            cell_data=[[(0.312, "0.312", False)]],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        # Access by name via __getitem__
        assert result["Inequality"].name == "Inequality"
        # Access by index via __getitem__
        assert result[0].name == "Inequality"

    def test_table_values_nested_dict(self):
        """StatisticsTable.values provides nested dict access: values[row][col] -> float."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality",
            title="Inequality Indicators",
            col_titles=["Baseline", "Reform"],
            row_titles=["Gini", "S80/S20"],
            cell_data=[
                [(0.312, "0.312", False), (0.290, "0.290", False)],
                [(4.5, "4.5", False), (4.1, "4.1", False)],
            ],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        table = result["Inequality"]
        assert table.values["Gini"]["Baseline"] == 0.312
        assert table.values["Gini"]["Reform"] == 0.290
        assert table.values["S80/S20"]["Baseline"] == 4.5
        assert table.values["S80/S20"]["Reform"] == 4.1

    def test_table_getitem_returns_row_dict(self):
        """StatisticsTable.__getitem__ returns the row dict for a given row title."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality",
            title="Inequality Indicators",
            col_titles=["Baseline"],
            row_titles=["Gini"],
            cell_data=[[(0.312, "0.312", False)]],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        table = result["Inequality"]
        assert table["Gini"] == {"Baseline": 0.312}

    def test_multiple_pages_produce_all_tables(self):
        """Tables from multiple pages are all collected into the Container."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="TableA",
            title="Table A",
            col_titles=["Col1"],
            row_titles=["Row1"],
            cell_data=[[(1.0, "1.0", False)]],
        )

        table2 = self._make_display_table(
            name="TableB",
            title="Table B",
            col_titles=["Col1"],
            row_titles=["Row1"],
            cell_data=[[(2.0, "2.0", False)]],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]},
            {"name": "Page2", "tables": [table2]},
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        assert len(result.tables) == 2
        assert result["TableA"].values["Row1"]["Col1"] == 1.0
        assert result["TableB"].values["Row1"]["Col1"] == 2.0

    def test_empty_display_results_produces_empty_container(self):
        """Empty displayPages produces an empty Container."""
        stats_module = self._get_module()

        display_results = self._make_display_results([])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        assert len(result.tables) == 0

    def test_none_display_results_tables_is_empty_container(self):
        """None displayPages produces an empty Container."""
        stats_module = self._get_module()

        dr = MagicMock()
        dr.displayPages = None

        result = stats_module.StatisticsResult()
        result._display_results = dr

        assert len(result.tables) == 0

    def test_page_with_none_display_tables_skipped(self):
        """Pages with None displayTables are skipped gracefully."""
        stats_module = self._get_module()

        page = MagicMock()
        page.name = "EmptyPage"
        page.displayTables = None

        dr = MagicMock()
        dr.displayPages = [page]

        result = stats_module.StatisticsResult()
        result._display_results = dr

        assert len(result.tables) == 0

    def test_table_with_none_cells_produces_empty_values(self):
        """Table with no cells produces empty values dict."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Empty",
            title="Empty Table",
            col_titles=["Col1"],
            row_titles=["Row1"],
            cell_data=None,
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        table = result["Empty"]
        assert table.values == {}

    def test_string_value_cells_are_handled(self):
        """Cells with isStringValue=True are handled and their float value is still stored."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Mixed",
            title="Mixed Table",
            col_titles=["Col1"],
            row_titles=["Label"],
            cell_data=[[(0.0, "Total", True)]],
        )

        display_results = self._make_display_results([
            {"name": "Page1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        table = result["Mixed"]
        # Even string-value cells have their numeric value stored
        assert table.values["Label"]["Col1"] == 0.0

    def test_result_len(self):
        """len(result) returns the number of tables."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="T1", title="T1",
            col_titles=["C"], row_titles=["R"],
            cell_data=[[(1.0, "1", False)]],
        )
        table2 = self._make_display_table(
            name="T2", title="T2",
            col_titles=["C"], row_titles=["R"],
            cell_data=[[(2.0, "2", False)]],
        )

        display_results = self._make_display_results([
            {"name": "P1", "tables": [table1, table2]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        assert len(result) == 2

    def test_result_iter(self):
        """Iterating over result yields tables."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="T1", title="T1",
            col_titles=["C"], row_titles=["R"],
            cell_data=[[(1.0, "1", False)]],
        )
        table2 = self._make_display_table(
            name="T2", title="T2",
            col_titles=["C"], row_titles=["R"],
            cell_data=[[(2.0, "2", False)]],
        )

        display_results = self._make_display_results([
            {"name": "P1", "tables": [table1, table2]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        table_names = [t.name for t in result]
        assert table_names == ["T1", "T2"]

    def test_result_repr_with_tables(self):
        """repr shows table count and names."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="Inequality", title="Inequality",
            col_titles=["C"], row_titles=["R"],
            cell_data=[[(1.0, "1", False)]],
        )

        display_results = self._make_display_results([
            {"name": "P1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        r = repr(result)
        assert "1 tables" in r
        assert "Inequality" in r

    def test_errors_and_warnings_exposed(self):
        """Errors and warnings stored on result are accessible."""
        stats_module = self._get_module()

        result = stats_module.StatisticsResult()
        result._errors = ["Error 1", "Error 2"]
        result._warnings = ["Warning 1"]

        assert result.errors == ["Error 1", "Error 2"]
        assert result.warnings == ["Warning 1"]

    def test_table_fallback_key_to_title(self):
        """When table name is empty, title is used as the Container key."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="",  # Empty name
            title="My Title",
            col_titles=["C"],
            row_titles=["R"],
            cell_data=[[(1.0, "1", False)]],
        )

        display_results = self._make_display_results([
            {"name": "P1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        # Access by title since name is empty
        table = result["My Title"]
        assert table.title == "My Title"

    def test_table_fallback_key_to_generated_index(self):
        """When both name and title are empty, a generated key is used."""
        stats_module = self._get_module()

        table1 = self._make_display_table(
            name="",
            title="",
            col_titles=["C"],
            row_titles=["R"],
            cell_data=[[(1.0, "1", False)]],
        )

        display_results = self._make_display_results([
            {"name": "P1", "tables": [table1]}
        ])

        result = stats_module.StatisticsResult()
        result._display_results = display_results

        # Should be accessible by index 0
        assert result[0] is not None
        # And by the generated key "table_0"
        assert result["table_0"] is not None


class TestStatisticsTableDataframe:
    """Test StatisticsTable.dataframe property."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_pandas_dataframe_correct_shape(self):
        """Pandas DataFrame has correct number of rows and columns."""
        import pandas as pd

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [1.0, 2.0, 3.0],
            [4.0, 5.0, 6.0],
        ]
        table._row_titles = ["Row1", "Row2"]
        table._col_titles = ["ColA", "ColB", "ColC"]
        table._use_polars = False

        df = table.dataframe

        assert isinstance(df, pd.DataFrame)
        assert df.shape == (2, 3)

    def test_pandas_dataframe_correct_index(self):
        """Pandas DataFrame uses row titles as index."""
        import pandas as pd

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [10.0, 20.0],
            [30.0, 40.0],
            [50.0, 60.0],
        ]
        table._row_titles = ["Gini", "P90/P10", "Mean"]
        table._col_titles = ["Baseline", "Reform"]
        table._use_polars = False

        df = table.dataframe

        assert list(df.index) == ["Gini", "P90/P10", "Mean"]

    def test_pandas_dataframe_correct_columns(self):
        """Pandas DataFrame uses column titles as column headers."""
        import pandas as pd

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [1.0, 2.0],
            [3.0, 4.0],
        ]
        table._row_titles = ["R1", "R2"]
        table._col_titles = ["Baseline", "Reform"]
        table._use_polars = False

        df = table.dataframe

        assert list(df.columns) == ["Baseline", "Reform"]

    def test_pandas_dataframe_values_match_cell_values(self):
        """Values in the pandas DataFrame match the original cell values."""
        import pandas as pd
        import numpy as np

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [0.312, 0.295],
            [4.5, 3.8],
        ]
        table._row_titles = ["Gini", "P90/P10"]
        table._col_titles = ["Baseline", "Reform"]
        table._use_polars = False

        df = table.dataframe

        np.testing.assert_array_almost_equal(
            df.values, [[0.312, 0.295], [4.5, 3.8]]
        )

    def test_polars_dataframe_correct_shape(self):
        """Polars DataFrame has correct number of rows and columns (including row column)."""
        import polars as pl

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [1.0, 2.0, 3.0],
            [4.0, 5.0, 6.0],
        ]
        table._row_titles = ["Row1", "Row2"]
        table._col_titles = ["ColA", "ColB", "ColC"]
        table._use_polars = True

        df = table.dataframe

        assert isinstance(df, pl.DataFrame)
        # 2 rows, 4 columns (row + ColA + ColB + ColC)
        assert df.shape == (2, 4)

    def test_polars_dataframe_correct_column_names(self):
        """Polars DataFrame has 'row' column plus the column titles."""
        import polars as pl

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [10.0, 20.0],
            [30.0, 40.0],
        ]
        table._row_titles = ["Gini", "Mean"]
        table._col_titles = ["Baseline", "Reform"]
        table._use_polars = True

        df = table.dataframe

        assert df.columns == ["row", "Baseline", "Reform"]

    def test_polars_dataframe_row_titles_in_row_column(self):
        """Polars DataFrame stores row titles in the 'row' column."""
        import polars as pl

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [1.0, 2.0],
            [3.0, 4.0],
        ]
        table._row_titles = ["Gini", "Mean"]
        table._col_titles = ["Baseline", "Reform"]
        table._use_polars = True

        df = table.dataframe

        assert df["row"].to_list() == ["Gini", "Mean"]

    def test_polars_dataframe_values_match_cell_values(self):
        """Values in the polars DataFrame match the original cell values."""
        import polars as pl

        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [
            [0.312, 0.295],
            [4.5, 3.8],
        ]
        table._row_titles = ["Gini", "P90/P10"]
        table._col_titles = ["Baseline", "Reform"]
        table._use_polars = True

        df = table.dataframe

        assert df["Baseline"].to_list() == [0.312, 4.5]
        assert df["Reform"].to_list() == [0.295, 3.8]

    def test_dataframe_is_cached(self):
        """The dataframe is built only once and cached on subsequent accesses."""
        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = [[1.0, 2.0]]
        table._row_titles = ["R1"]
        table._col_titles = ["C1", "C2"]
        table._use_polars = False

        df1 = table.dataframe
        df2 = table.dataframe
        assert df1 is df2

    def test_dataframe_returns_none_when_no_cell_values(self):
        """When cell_values is empty, dataframe returns None."""
        stats_module = self._get_module()
        table = stats_module.StatisticsTable()
        table._cell_values = []
        table._row_titles = []
        table._col_titles = []
        table._use_polars = False

        assert table.dataframe is None


class TestStatisticsResultToDataframes:
    """Test StatisticsResult.to_dataframes() method."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_to_dataframes_returns_dict(self):
        """to_dataframes() returns a dictionary."""
        import pandas as pd

        stats_module = self._get_module()
        Container = stats_module.Container

        # Build tables manually
        table1 = stats_module.StatisticsTable()
        table1._name = "Inequality"
        table1._cell_values = [[0.31, 0.29]]
        table1._row_titles = ["Gini"]
        table1._col_titles = ["Baseline", "Reform"]
        table1._use_polars = False

        table2 = stats_module.StatisticsTable()
        table2._name = "Poverty"
        table2._cell_values = [[15.2, 12.1]]
        table2._row_titles = ["Rate"]
        table2._col_titles = ["Baseline", "Reform"]
        table2._use_polars = False

        # Put tables in a Container
        tables = Container()
        tables.add("Inequality", table1)
        tables.add("Poverty", table2)

        result = stats_module.StatisticsResult()
        result._tables = tables

        dfs = result.to_dataframes()

        assert isinstance(dfs, dict)
        assert "Inequality" in dfs
        assert "Poverty" in dfs
        assert isinstance(dfs["Inequality"], pd.DataFrame)
        assert isinstance(dfs["Poverty"], pd.DataFrame)

    def test_to_dataframes_values_correct(self):
        """to_dataframes() returns DataFrames with correct values."""
        import pandas as pd
        import numpy as np

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Income"
        table._cell_values = [[100.0, 200.0], [300.0, 400.0]]
        table._row_titles = ["Mean", "Median"]
        table._col_titles = ["System1", "System2"]
        table._use_polars = False

        tables = Container()
        tables.add("Income", table)

        result = stats_module.StatisticsResult()
        result._tables = tables

        dfs = result.to_dataframes()

        np.testing.assert_array_almost_equal(
            dfs["Income"].values, [[100.0, 200.0], [300.0, 400.0]]
        )
        assert list(dfs["Income"].index) == ["Mean", "Median"]
        assert list(dfs["Income"].columns) == ["System1", "System2"]

    def test_to_dataframes_empty_result(self):
        """to_dataframes() returns empty dict when no tables exist."""
        stats_module = self._get_module()

        result = stats_module.StatisticsResult()
        # _tables is None and _display_results is None
        dfs = result.to_dataframes()

        assert dfs == {}

    def test_to_dataframes_polars_tables(self):
        """to_dataframes() returns polars DataFrames when use_polars is True."""
        import polars as pl

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Stats"
        table._cell_values = [[1.5, 2.5], [3.5, 4.5]]
        table._row_titles = ["A", "B"]
        table._col_titles = ["X", "Y"]
        table._use_polars = True

        tables = Container()
        tables.add("Stats", table)

        result = stats_module.StatisticsResult()
        result._tables = tables

        dfs = result.to_dataframes()

        assert isinstance(dfs["Stats"], pl.DataFrame)
        assert dfs["Stats"]["X"].to_list() == [1.5, 3.5]
        assert dfs["Stats"]["Y"].to_list() == [2.5, 4.5]


class TestStatisticsResultSummary:
    """Test StatisticsResult.summary property."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_summary_returns_pandas_dataframe(self):
        """summary property returns a pandas DataFrame by default."""
        import pandas as pd

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Inequality"
        table._page_name = "Results"
        table._cell_values = [[0.31]]
        table._cell_display_values = [["0.31"]]
        table._row_titles = ["Gini"]
        table._col_titles = ["Baseline"]
        table._use_polars = False

        tables = Container()
        tables.add("Inequality", table)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = False

        summary = result.summary

        assert isinstance(summary, pd.DataFrame)
        assert list(summary.columns) == ["page", "table", "row_label", "column_label", "value", "display_value"]

    def test_summary_returns_polars_dataframe_when_polars(self):
        """summary property returns a polars DataFrame when _use_polars is True."""
        import polars as pl

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Inequality"
        table._page_name = "Results"
        table._cell_values = [[0.31]]
        table._cell_display_values = [["0.31"]]
        table._row_titles = ["Gini"]
        table._col_titles = ["Baseline"]
        table._use_polars = True

        tables = Container()
        tables.add("Inequality", table)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = True

        summary = result.summary

        assert isinstance(summary, pl.DataFrame)
        assert summary.columns == ["page", "table", "row_label", "column_label", "value", "display_value"]

    def test_summary_flattens_all_tables(self):
        """summary flattens multiple tables into one DataFrame."""
        import pandas as pd

        stats_module = self._get_module()
        Container = stats_module.Container

        table1 = stats_module.StatisticsTable()
        table1._name = "Inequality"
        table1._page_name = "Page1"
        table1._cell_values = [[0.31, 0.29]]
        table1._cell_display_values = [["0.31", "0.29"]]
        table1._row_titles = ["Gini"]
        table1._col_titles = ["Baseline", "Reform"]
        table1._use_polars = False

        table2 = stats_module.StatisticsTable()
        table2._name = "Poverty"
        table2._page_name = "Page1"
        table2._cell_values = [[15.2, 12.1]]
        table2._cell_display_values = [["15.2%", "12.1%"]]
        table2._row_titles = ["Rate"]
        table2._col_titles = ["Baseline", "Reform"]
        table2._use_polars = False

        tables = Container()
        tables.add("Inequality", table1)
        tables.add("Poverty", table2)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = False

        summary = result.summary

        # table1: 1 row x 2 cols = 2 cells, table2: 1 row x 2 cols = 2 cells => 4 rows
        assert len(summary) == 4
        assert list(summary["page"]) == ["Page1", "Page1", "Page1", "Page1"]
        assert list(summary["table"]) == ["Inequality", "Inequality", "Poverty", "Poverty"]
        assert list(summary["row_label"]) == ["Gini", "Gini", "Rate", "Rate"]
        assert list(summary["column_label"]) == ["Baseline", "Reform", "Baseline", "Reform"]
        assert list(summary["value"]) == [0.31, 0.29, 15.2, 12.1]
        assert list(summary["display_value"]) == ["0.31", "0.29", "15.2%", "12.1%"]

    def test_summary_multiple_rows_and_columns(self):
        """summary correctly handles tables with multiple rows and columns."""
        import pandas as pd

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Income"
        table._page_name = "Analysis"
        table._cell_values = [[100.0, 200.0], [300.0, 400.0]]
        table._cell_display_values = [["100", "200"], ["300", "400"]]
        table._row_titles = ["Mean", "Median"]
        table._col_titles = ["System1", "System2"]
        table._use_polars = False

        tables = Container()
        tables.add("Income", table)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = False

        summary = result.summary

        # 2 rows x 2 cols = 4 cells
        assert len(summary) == 4
        assert list(summary["row_label"]) == ["Mean", "Mean", "Median", "Median"]
        assert list(summary["column_label"]) == ["System1", "System2", "System1", "System2"]
        assert list(summary["value"]) == [100.0, 200.0, 300.0, 400.0]

    def test_summary_empty_result_returns_empty_dataframe(self):
        """summary returns an empty DataFrame when no tables exist."""
        import pandas as pd

        stats_module = self._get_module()

        result = stats_module.StatisticsResult()
        result._use_polars = False

        summary = result.summary

        assert isinstance(summary, pd.DataFrame)
        assert len(summary) == 0
        assert list(summary.columns) == ["page", "table", "row_label", "column_label", "value", "display_value"]

    def test_summary_empty_result_polars_returns_empty_dataframe(self):
        """summary returns an empty polars DataFrame when no tables exist (polars mode)."""
        import polars as pl

        stats_module = self._get_module()

        result = stats_module.StatisticsResult()
        result._use_polars = True

        summary = result.summary

        assert isinstance(summary, pl.DataFrame)
        assert len(summary) == 0
        assert summary.columns == ["page", "table", "row_label", "column_label", "value", "display_value"]

    def test_summary_is_cached(self):
        """summary result is cached on repeated access."""
        import pandas as pd

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Test"
        table._page_name = "Page"
        table._cell_values = [[1.0]]
        table._cell_display_values = [["1.0"]]
        table._row_titles = ["Row1"]
        table._col_titles = ["Col1"]
        table._use_polars = False

        tables = Container()
        tables.add("Test", table)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = False

        summary1 = result.summary
        summary2 = result.summary

        # Should be the exact same object (cached)
        assert summary1 is summary2

    def test_summary_uses_table_name_fallback_to_title(self):
        """summary uses table title as fallback when name is empty."""
        import pandas as pd

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = ""
        table._title = "My Table Title"
        table._page_name = "Page"
        table._cell_values = [[5.0]]
        table._cell_display_values = [["5.0"]]
        table._row_titles = ["Row"]
        table._col_titles = ["Col"]
        table._use_polars = False

        tables = Container()
        tables.add("My Table Title", table)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = False

        summary = result.summary

        assert list(summary["table"]) == ["My Table Title"]

    def test_summary_polars_values_correct(self):
        """summary in polars mode has correct cell values."""
        import polars as pl

        stats_module = self._get_module()
        Container = stats_module.Container

        table = stats_module.StatisticsTable()
        table._name = "Stats"
        table._page_name = "Overview"
        table._cell_values = [[1.5, 2.5], [3.5, 4.5]]
        table._cell_display_values = [["1.5", "2.5"], ["3.5", "4.5"]]
        table._row_titles = ["A", "B"]
        table._col_titles = ["X", "Y"]
        table._use_polars = True

        tables = Container()
        tables.add("Stats", table)

        result = stats_module.StatisticsResult()
        result._tables = tables
        result._use_polars = True

        summary = result.summary

        assert summary["page"].to_list() == ["Overview", "Overview", "Overview", "Overview"]
        assert summary["table"].to_list() == ["Stats", "Stats", "Stats", "Stats"]
        assert summary["row_label"].to_list() == ["A", "A", "B", "B"]
        assert summary["column_label"].to_list() == ["X", "Y", "X", "Y"]
        assert summary["value"].to_list() == [1.5, 2.5, 3.5, 4.5]
        assert summary["display_value"].to_list() == ["1.5", "2.5", "3.5", "4.5"]


class TestToExcel:
    """Test StatisticsResult.to_excel() method."""

    def _get_module(self):
        """Import statistics module with CLR mocked, including ExportHandling."""
        import importlib
        import sys
        import os
        from unittest.mock import patch, MagicMock

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                mock_em_stats_module.HardDefinitions.UserInputType.VariableName = 'VariableName'
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
                    return stats_module, mock_em_stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_to_excel_raises_valueerror_when_no_display_results(self):
        """to_excel raises ValueError when _display_results is None."""
        import pytest
        stats_module, _ = self._get_module()
        result = stats_module.StatisticsResult()

        with pytest.raises(ValueError, match="Cannot export to Excel"):
            result.to_excel("output.xlsx")

    def test_to_excel_raises_runtime_error_on_export_failure(self):
        """to_excel raises RuntimeError when ExportHandling reports failure."""
        import pytest
        from unittest.mock import MagicMock
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        mock_em.ExportHandling.ExportSinglePackage.return_value = (
            False, "Export error: nothing to export", None
        )
        stats_module.ExportHandling = mock_em.ExportHandling

        with pytest.raises(RuntimeError, match="Excel export failed"):
            result.to_excel("output.xlsx")

    def test_to_excel_writes_file_on_success(self, tmp_path):
        """to_excel writes MemoryStream bytes to file on success."""
        from unittest.mock import MagicMock
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        mock_stream = MagicMock()
        excel_content = b"PK\x03\x04fake_excel_content"
        mock_stream.ToArray.return_value = excel_content
        mock_stream.Dispose = MagicMock()

        mock_em.ExportHandling.ExportSinglePackage.return_value = (
            True, "", mock_stream
        )
        stats_module.ExportHandling = mock_em.ExportHandling

        output_file = tmp_path / "result.xlsx"
        result.to_excel(str(output_file))

        assert output_file.exists()
        assert output_file.read_bytes() == excel_content
        mock_stream.Dispose.assert_called_once()

    def test_to_excel_disposes_stream_even_on_write_error(self, tmp_path):
        """to_excel disposes the MemoryStream even if file write fails."""
        import pytest
        from unittest.mock import MagicMock
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        mock_stream = MagicMock()
        mock_stream.ToArray.side_effect = Exception("Stream read error")
        mock_stream.Dispose = MagicMock()

        mock_em.ExportHandling.ExportSinglePackage.return_value = (
            True, "", mock_stream
        )
        stats_module.ExportHandling = mock_em.ExportHandling

        with pytest.raises(Exception, match="Stream read error"):
            result.to_excel(str(tmp_path / "output.xlsx"))

        mock_stream.Dispose.assert_called_once()

    def test_to_excel_calls_export_with_display_results(self):
        """to_excel passes _display_results to ExportSinglePackage."""
        import os
        import tempfile
        from unittest.mock import MagicMock
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        mock_stream = MagicMock()
        mock_stream.ToArray.return_value = b"data"
        mock_stream.Dispose = MagicMock()

        mock_em.ExportHandling.ExportSinglePackage.return_value = (
            True, "", mock_stream
        )
        stats_module.ExportHandling = mock_em.ExportHandling

        with tempfile.NamedTemporaryFile(suffix=".xlsx", delete=False) as f:
            output_path = f.name

        try:
            result.to_excel(output_path)
            mock_em.ExportHandling.ExportSinglePackage.assert_called_once_with(
                mock_display_results
            )
        finally:
            os.unlink(output_path)

class TestCalculateCustom:
    """Test Statistics.calculate_custom() method."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        import importlib
        import sys
        import os
        from unittest.mock import patch, MagicMock

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                mock_em_stats_module.HardDefinitions.UserInputType.VariableName = 'VariableName'
                mock_em_stats_module.HardDefinitions.TemplateType.Default = 'Default'
                mock_em_stats_module.HardDefinitions.TemplateType.BaselineReform = 'BaselineReform'
                mock_em_stats_module.HardDefinitions.TemplateType.Multi = 'Multi'
                sys.modules['EM_Statistics'] = mock_em_stats_module

                mock_ext_stats_module = MagicMock()
                sys.modules['EM_Statistics.ExternalStatistics'] = mock_ext_stats_module

                mock_clr_convert = MagicMock()
                mock_clr_convert.asNetArray = mock_as_net_array
                sys.modules['euromod.utils.clr_array_convert'] = mock_clr_convert
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    stats_module.asNetArray = mock_as_net_array
                    return stats_module, mock_em_stats_module, mock_ext_stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'EM_Statistics.ExternalStatistics' in sys.modules:
                        del sys.modules['EM_Statistics.ExternalStatistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_raises_valueerror_when_no_stats_provided(self):
        """calculate_custom raises ValueError when neither aggregate nor distributional stats provided."""
        stats_module, mock_em, _ = self._get_module()

        # Create a bare Statistics instance (bypass __init__)
        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="At least one of"):
            stats.calculate_custom(mock_sim)

    def test_raises_valueerror_when_empty_lists(self):
        """calculate_custom raises ValueError when both lists are empty."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="At least one of"):
            stats.calculate_custom(mock_sim, aggregate_stats=[], distributional_stats=[])

    def test_validates_aggregate_stats_require_name(self):
        """aggregate_stats without 'name' field raises ValueError."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="missing required field 'name'"):
            stats.calculate_custom(mock_sim, aggregate_stats=[{"income_list": "ils_dispy"}])

    def test_validates_aggregate_stats_require_income_list(self):
        """aggregate_stats without 'income_list' field raises ValueError."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="missing required field 'income_list'"):
            stats.calculate_custom(mock_sim, aggregate_stats=[{"name": "test_stat"}])

    def test_validates_distributional_stats_require_name(self):
        """distributional_stats without 'name' field raises ValueError."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="missing required field 'name'"):
            stats.calculate_custom(
                mock_sim, distributional_stats=[{"income_list": "ils_dispy"}]
            )

    def test_validates_distributional_stats_require_income_list(self):
        """distributional_stats without 'income_list' field raises ValueError."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="missing required field 'income_list'"):
            stats.calculate_custom(
                mock_sim, distributional_stats=[{"name": "gini_test"}]
            )

    def test_validates_aggregate_stats_must_be_dict(self):
        """Non-dict items in aggregate_stats raise ValueError."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="must be a dict"):
            stats.calculate_custom(mock_sim, aggregate_stats=["not_a_dict"])

    def test_validates_distributional_stats_must_be_dict(self):
        """Non-dict items in distributional_stats raise ValueError."""
        stats_module, mock_em, _ = self._get_module()

        stats = object.__new__(stats_module.Statistics)
        stats._template = None
        stats._template_path = None
        stats._variable = None

        mock_sim = MagicMock()

        with pytest.raises(ValueError, match="must be a dict"):
            stats.calculate_custom(mock_sim, distributional_stats=[42])


class TestBuildExternalStatistic:
    """Test _build_external_statistic helper function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        import importlib
        import sys
        import os
        from unittest.mock import patch, MagicMock

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                mock_em_stats_module.HardDefinitions.UserInputType.VariableName = 'VariableName'
                sys.modules['EM_Statistics'] = mock_em_stats_module

                mock_ext_stats_module = MagicMock()
                sys.modules['EM_Statistics.ExternalStatistics'] = mock_ext_stats_module

                mock_clr_convert = MagicMock()
                mock_clr_convert.asNetArray = mock_as_net_array
                sys.modules['euromod.utils.clr_array_convert'] = mock_clr_convert
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    stats_module.asNetArray = mock_as_net_array
                    return stats_module, mock_em_stats_module, mock_ext_stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'EM_Statistics.ExternalStatistics' in sys.modules:
                        del sys.modules['EM_Statistics.ExternalStatistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_creates_external_statistic_with_aggregate(self):
        """_build_external_statistic creates ExternalStatistic with aggregate entries."""
        stats_module, _, mock_ext = self._get_module()

        aggregate_stats = [
            {
                "name": "mean_income",
                "income_list": "ils_dispy",
                "description": "Mean disposable income",
                "source": "simulation",
                "year": "2023",
                "amount": "25000",
            }
        ]

        result = stats_module._build_external_statistic(aggregate_stats, [])

        # ExternalStatistic() should have been called
        mock_ext.ExternalStatistic.assert_called_once()
        # ExternalStatisticAggregate should have been called
        mock_ext.ExternalStatisticAggregate.assert_called_once_with(
            "ils_dispy", "mean_income", "Mean disposable income",
            "simulation", "", ""
        )

    def test_creates_external_statistic_with_distributional(self):
        """_build_external_statistic creates ExternalStatistic with distributional entries."""
        stats_module, _, mock_ext = self._get_module()

        distributional_stats = [
            {
                "name": "gini_dispy",
                "income_list": "ils_dispy",
                "description": "Gini coefficient",
                "measures": ["gini", "s80s20"],
            }
        ]

        result = stats_module._build_external_statistic([], distributional_stats)

        mock_ext.ExternalStatistic.assert_called_once()
        mock_ext.ExternalStatisticDistributional.assert_called_once_with(
            "ils_dispy", "gini_dispy", "Gini coefficient", "", ""
        )


class TestBuildCustomTemplateXml:
    """Test _build_custom_template_xml helper function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        import importlib
        import sys
        import os
        from unittest.mock import patch, MagicMock

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                mock_em_stats_module.HardDefinitions.UserInputType.VariableName = 'VariableName'
                sys.modules['EM_Statistics'] = mock_em_stats_module

                mock_ext_stats_module = MagicMock()
                sys.modules['EM_Statistics.ExternalStatistics'] = mock_ext_stats_module

                mock_clr_convert = MagicMock()
                mock_clr_convert.asNetArray = mock_as_net_array
                sys.modules['euromod.utils.clr_array_convert'] = mock_clr_convert
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'EM_Statistics.ExternalStatistics' in sys.modules:
                        del sys.modules['EM_Statistics.ExternalStatistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_generates_valid_xml_with_aggregate_stats(self):
        """XML output contains aggregate stats table with weighted average."""
        stats_module = self._get_module()

        aggregate_stats = [
            {
                "name": "mean_income",
                "income_list": "ils_dispy",
                "description": "Mean disposable income",
            }
        ]
        income_vars = {"ils_dispy"}

        xml = stats_module._build_custom_template_xml(aggregate_stats, [], income_vars)

        assert "<Template>" in xml
        assert "<TemplateType>Default</TemplateType>" in xml
        assert "<Name>ils_dispy</Name>" in xml
        assert "<ReadVar>ils_dispy</ReadVar>" in xml
        assert "<Name>AggregateStats</Name>" in xml
        assert "<CalculationType>CalculateWeightedAverage</CalculationType>" in xml
        assert "<FormulaString>DATA_VAR[@ils_dispy]</FormulaString>" in xml
        assert "<Name>mean_income</Name>" in xml
        assert "<Title>Mean disposable income</Title>" in xml

    def test_generates_valid_xml_with_distributional_stats(self):
        """XML output contains distributional stats tables with Gini and S8020."""
        stats_module = self._get_module()

        distributional_stats = [
            {
                "name": "gini_dispy",
                "income_list": "ils_dispy",
                "description": "Gini coefficient",
                "measures": ["gini", "s80s20"],
            }
        ]
        income_vars = {"ils_dispy"}

        xml = stats_module._build_custom_template_xml([], distributional_stats, income_vars)

        assert "<Name>Dist_gini_dispy</Name>" in xml
        assert "<CalculationType>CalculateGini</CalculationType>" in xml
        assert "<CalculationType>CalculateS8020</CalculationType>" in xml
        # Distributional measures default to household-equivalised income, so the
        # income list is referenced via the generated equivalised variable.
        assert "<CalculationType>CreateEquivalized</CalculationType>" in xml
        assert "<Name>GiniVar</Name><VarName>__eq_ils_dispy</VarName>" in xml

    def test_generates_xml_with_multiple_income_vars(self):
        """XML includes all unique income list variables as required."""
        stats_module = self._get_module()

        aggregate_stats = [
            {"name": "stat1", "income_list": "ils_dispy"},
            {"name": "stat2", "income_list": "ils_earns"},
        ]
        income_vars = {"ils_dispy", "ils_earns"}

        xml = stats_module._build_custom_template_xml(aggregate_stats, [], income_vars)

        assert "ils_dispy" in xml
        assert "ils_earns" in xml

    def test_escapes_xml_special_characters(self):
        """XML special characters in names/descriptions are properly escaped."""
        stats_module = self._get_module()

        aggregate_stats = [
            {
                "name": "stat_a&b",
                "income_list": "ils_dispy",
                "description": "Income <threshold>",
            }
        ]
        income_vars = {"ils_dispy"}

        xml = stats_module._build_custom_template_xml(aggregate_stats, [], income_vars)

        assert "stat_a&amp;b" in xml
        assert "Income &lt;threshold&gt;" in xml

    def test_unsupported_measures_skipped(self):
        """Unsupported measure names are silently skipped in the XML."""
        stats_module = self._get_module()

        distributional_stats = [
            {
                "name": "test",
                "income_list": "ils_dispy",
                "measures": ["gini", "unsupported_measure"],
            }
        ]
        income_vars = {"ils_dispy"}

        xml = stats_module._build_custom_template_xml([], distributional_stats, income_vars)

        assert "CalculateGini" in xml
        assert "unsupported_measure" not in xml


class TestXmlEscape:
    """Test _xml_escape helper function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        import importlib
        import sys
        import os
        from unittest.mock import patch, MagicMock

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()

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
                mock_ext_stats_module = MagicMock()
                sys.modules['EM_Statistics.ExternalStatistics'] = mock_ext_stats_module
                sys.modules['euromod.utils.clr_array_convert'] = MagicMock()
                sys.modules['euromod.utils'] = MagicMock()

                try:
                    import euromod.statistics as stats_module
                    importlib.reload(stats_module)
                    return stats_module
                finally:
                    if 'EM_Statistics' in sys.modules:
                        del sys.modules['EM_Statistics']
                    if 'EM_Statistics.ExternalStatistics' in sys.modules:
                        del sys.modules['EM_Statistics.ExternalStatistics']
                    if 'euromod.utils.clr_array_convert' in sys.modules:
                        del sys.modules['euromod.utils.clr_array_convert']

    def test_escapes_ampersand(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape("a&b") == "a&amp;b"

    def test_escapes_less_than(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape("a<b") == "a&lt;b"

    def test_escapes_greater_than(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape("a>b") == "a&gt;b"

    def test_escapes_double_quote(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape('a"b') == "a&quot;b"

    def test_escapes_single_quote(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape("a'b") == "a&apos;b"

    def test_no_escape_needed(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape("normal_text") == "normal_text"

    def test_multiple_special_chars(self):
        stats_module = self._get_module()
        assert stats_module._xml_escape("<a&b>") == "&lt;a&amp;b&gt;"


class TestCombineResults:
    """Test _combine_results helper function."""

    def _get_module(self):
        """Import statistics module with CLR mocked."""
        import importlib
        import sys
        import os
        from unittest.mock import patch, MagicMock

        if 'euromod.statistics' in sys.modules:
            del sys.modules['euromod.statistics']

        mock_clr = MagicMock()
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
                mock_em_stats_module.HardDefinitions.UserInputType.VariableName = 'VariableName'
                sys.modules['EM_Statistics'] = mock_em_stats_module

                mock_ext_stats_module = MagicMock()
                sys.modules['EM_Statistics.ExternalStatistics'] = mock_ext_stats_module

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

    def test_combines_tables_from_both_results(self):
        """_combine_results merges tables from both template and custom results."""
        stats_module = self._get_module()

        # Create two StatisticsResults with tables
        template_result = stats_module.StatisticsResult()
        custom_result = stats_module.StatisticsResult()

        # Set up template tables
        from ..container import Container
        template_tables = Container()
        table1 = stats_module.StatisticsTable()
        table1._name = "TemplateTable"
        table1._title = "Template Table"
        template_tables.add("TemplateTable", table1)
        template_result._tables = template_tables

        # Set up custom tables
        custom_tables = Container()
        table2 = stats_module.StatisticsTable()
        table2._name = "CustomTable"
        table2._title = "Custom Table"
        custom_tables.add("CustomTable", table2)
        custom_result._tables = custom_tables

        combined = stats_module._combine_results(template_result, custom_result)

        assert len(combined._tables) == 2
        assert combined._tables["TemplateTable"]._name == "TemplateTable"
        assert combined._tables["CustomTable"]._name == "CustomTable"

    def test_combines_errors_and_warnings(self):
        """_combine_results merges errors and warnings from both results."""
        stats_module = self._get_module()

        template_result = stats_module.StatisticsResult(
            errors=["error1"], warnings=["warn1"]
        )
        custom_result = stats_module.StatisticsResult(
            errors=["error2"], warnings=["warn2"]
        )
        template_result._tables = None
        custom_result._tables = None

        combined = stats_module._combine_results(template_result, custom_result)

        assert combined.errors == ["error1", "error2"]
        assert combined.warnings == ["warn1", "warn2"]

    def test_handles_none_tables_gracefully(self):
        """_combine_results handles None tables without error."""
        stats_module = self._get_module()

        template_result = stats_module.StatisticsResult()
        custom_result = stats_module.StatisticsResult()
        template_result._tables = None
        custom_result._tables = None

        combined = stats_module._combine_results(template_result, custom_result)
        assert len(combined._tables) == 0
