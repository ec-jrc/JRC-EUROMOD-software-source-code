using EM_Common;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_UI.DataSets
{
    internal class HICPConfigFacade
    {
        private string _pathHICPConfig = string.Empty;

        private HICPConfig _HICPConfig = null;

        internal HICPConfigFacade(string pathHICPConfig)
        {
            _pathHICPConfig = pathHICPConfig;
        }

        internal HICPConfigFacade()
        {
            _pathHICPConfig = new EMPath(EM_AppContext.FolderEuromodFiles).GetHICPFilePath(true);
            LoadHICPConfig();
        }
 

        static readonly string[] _cDataElements = new string[] { "Comment" };

        internal bool LoadHICPConfig()
        {
            try
            {
                if (_HICPConfig == null)
                {
                    _HICPConfig = new HICPConfig();
                    if (File.Exists(_pathHICPConfig))
                        using (StreamReader streamReader = new StreamReader(_pathHICPConfig, DefGeneral.DEFAULT_ENCODING))
                            _HICPConfig.ReadXml(streamReader);
                    _HICPConfig.AcceptChanges();
                }
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal HICPConfig GetHICPConfig() { return _HICPConfig; }

        internal bool WriteXML()
        {
            try
            {
                _HICPConfig.AcceptChanges();
                Stream fileStream = new FileStream(new EMPath(EM_AppContext.FolderEuromodFiles).GetHICPFilePath(true), FileMode.Create);
                using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING, _cDataElements))
                    _HICPConfig.WriteXml(xmlWriter);
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal List<HICPConfig.HICPRow> GetHICPs() { return _HICPConfig.HICP.ToList(); }

        internal HICPConfig.HICPRow GetHICP(string country, string year)
        {
            int iYear; if (!int.TryParse(year, out iYear)) return null; return GetHICP(country, iYear);
        }

        internal HICPConfig.HICPRow GetHICP(string country, int year)
        {
            return (from h in _HICPConfig.HICP where h.Country.ToLower() == country.ToLower() & h.Year == year select h).FirstOrDefault();
        }

        internal bool RefreshHICPs(List<Tuple<string, int, double, string>> hicps)
        {
            try
            {
                _HICPConfig.HICP.Rows.Clear();
                _HICPConfig.AcceptChanges();
                foreach (var hicp in hicps) _HICPConfig.HICP.AddHICPRow(hicp.Item1, hicp.Item2, hicp.Item3, hicp.Item4);
                _HICPConfig.AcceptChanges();
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal bool SetHICPs(List<Tuple<string, int, double, string>> hicps) // this is in fact a combination of add and set value
        {
            foreach (HICPConfig.HICPRow hicp in _HICPConfig.HICP) // just add the existing ones unless they are already in the list (with perhaps new values)
                if ((from h in hicps where h.Item1.ToLower() == hicp.Country.ToLower() & h.Item2 == hicp.Year select h).Count() == 0)
                    hicps.Add(new Tuple<string, int, double, string>(hicp.Country, hicp.Year, hicp.Value, hicp.Comment));
            return RefreshHICPs(hicps);
        }

        internal static HICPConfig.HICPRow CopyHICPFromAnotherConfig(HICPConfig hicpConfig, HICPConfig.HICPRow originalHicp)
        {
            HICPConfig.HICPRow hicpRow = hicpConfig.HICP.AddHICPRow(originalHicp.Country, originalHicp.Year, originalHicp.Value, originalHicp.Comment);

            return hicpRow;
        }

        internal bool HasHICP(string country)
        {
            return _HICPConfig.HICP.Where(h => string.IsNullOrEmpty(country) || h.Country.ToLower() == country.ToLower()).Any();
        }
    }
}
