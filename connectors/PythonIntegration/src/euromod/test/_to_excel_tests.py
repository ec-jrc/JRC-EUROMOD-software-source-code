

class TestToExcel:
    """Test StatisticsResult.to_excel() method."""

    def _get_module(self):
        """Import statistics module with CLR mocked, including ExportHandling."""
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
                mock_em_stats_module.HardDefinitions.UserInputType.VariableName = "VariableName"
                sys.modules['EM_Statistics'] = mock_em_stats_module

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
        stats_module, _ = self._get_module()
        result = stats_module.StatisticsResult()

        with pytest.raises(ValueError, match="Cannot export to Excel"):
            result.to_excel("output.xlsx")

    def test_to_excel_raises_runtime_error_on_export_failure(self):
        """to_excel raises RuntimeError when ExportHandling reports failure."""
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        # Mock ExportHandling.ExportSinglePackage to return failure
        mock_em.ExportHandling.ExportSinglePackage.return_value = (
            False, "Export error: nothing to export", None
        )
        stats_module.ExportHandling = mock_em.ExportHandling

        with pytest.raises(RuntimeError, match="Excel export failed"):
            result.to_excel("output.xlsx")

    def test_to_excel_writes_file_on_success(self, tmp_path):
        """to_excel writes MemoryStream bytes to file on success."""
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        # Create a mock MemoryStream with known bytes
        mock_stream = MagicMock()
        excel_content = b"PK fake_excel_content"
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
        stats_module, mock_em = self._get_module()

        mock_display_results = MagicMock()
        result = stats_module.StatisticsResult(display_results=mock_display_results)

        # Create a mock MemoryStream that fails during ToArray
        mock_stream = MagicMock()
        mock_stream.ToArray.side_effect = Exception("Stream read error")
        mock_stream.Dispose = MagicMock()

        mock_em.ExportHandling.ExportSinglePackage.return_value = (
            True, "", mock_stream
        )
        stats_module.ExportHandling = mock_em.ExportHandling

        with pytest.raises(Exception, match="Stream read error"):
            result.to_excel(str(tmp_path / "output.xlsx"))

        # Stream should still be disposed
        mock_stream.Dispose.assert_called_once()

    def test_to_excel_calls_export_with_display_results(self):
        """to_excel passes _display_results to ExportSinglePackage."""
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

        import tempfile
        with tempfile.NamedTemporaryFile(suffix='.xlsx', delete=False) as f:
            output_path = f.name

        try:
            result.to_excel(output_path)
            mock_em.ExportHandling.ExportSinglePackage.assert_called_once_with(
                mock_display_results
            )
        finally:
            os.unlink(output_path)
