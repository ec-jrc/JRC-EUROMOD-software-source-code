using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EM_XmlHandler
{
    public static class EM_Info
    {
        public class CountryInfo
        {
            public string longName = string.Empty;
            public string shortName = string.Empty;
            public Dictionary<string, string> systems = new Dictionary<string, string>();
            public Dictionary<string, string> datasets = new Dictionary<string, string>();
            public Dictionary<string, // system-id
                              List<Tuple<string, bool>>> // list of data-id/best-match
                              runOptions = new Dictionary<string, List<Tuple<string, bool>>>();
        }

        // note: this assumes an existing EM3Translation-folder (as sub-folder of emPath) and takes only the translated countries in there into account
        public static bool GetInfoCountries(string emPath, out List<CountryInfo> countryInfo, out string errorsAndWarnings,
                                            bool getDatasets = true, bool getRunOptions = true)
        {
            errorsAndWarnings = string.Empty; countryInfo = new List<CountryInfo>();
            EMPath pathHelper = new EMPath(emPath);
            try
            {
                if (!Directory.Exists(pathHelper.GetFolderCountries())) { errorsAndWarnings = $"Folder {pathHelper.GetFolderCountries()} not found"; return false; }

                object writeLock = new object(); List<CountryInfo> localCountryInfo = new List<CountryInfo>(); string localErrors = string.Empty;
                Parallel.ForEach(new DirectoryInfo(pathHelper.GetFolderCountries()).GetDirectories(), folder =>
                {
                    lock (writeLock)
                    {
                        if (GetInfoCountry(pathHelper.GetCountryFilePath(folder.Name), out CountryInfo ci, out string error, getDatasets, getRunOptions))
                            localCountryInfo.Add(ci);
                        else localErrors += error + Environment.NewLine;
                    }
                });

                countryInfo = localCountryInfo; errorsAndWarnings = localErrors.Trim();
                return true;
            }
            catch (Exception exception)
            {
                errorsAndWarnings = exception.Message; return false;
            }
        }

        public static bool GetInfoCountry(string countryFileFullPath, out CountryInfo ccInfo, out string error, bool getDatasets = true, bool getRunOptions = true)
        {
            ccInfo = new CountryInfo(); error = string.Empty;
            try
            {
                if (!File.Exists(countryFileFullPath)) { error = $"Country file {countryFileFullPath} not found"; return false; }

                using (StreamReader sr = new StreamReader(countryFileFullPath, Encoding.UTF8))
                using (XmlReader mainReader = XmlReader.Create(sr))
                {
                    mainReader.ReadToDescendant(TAGS.COUNTRY);
                    XmlReader subReader = mainReader.ReadSubtree();
                    do
                    {
                        if (subReader.NodeType != XmlNodeType.Element) continue;
                        if (subReader.Name == TAGS.NAME) ccInfo.longName = subReader.ReadElementContentAsString();
                        if (subReader.Name == TAGS.SHORT_NAME) ccInfo.shortName = subReader.ReadElementContentAsString();
                    } while (subReader.Read());
                    subReader.Close();

                    while (mainReader.Read()) { if (mainReader.NodeType == XmlNodeType.Element && mainReader.Name == TAGS.Enclosure(TAGS.SYS)) break; }
                    subReader = mainReader.ReadSubtree(); string id = string.Empty, name = string.Empty;
                    do
                    {
                        if (subReader.NodeType != XmlNodeType.Element) continue;
                        if (subReader.Name == TAGS.ID) id = subReader.ReadElementContentAsString();
                        if (subReader.Name == TAGS.NAME) name = subReader.ReadElementContentAsString();
                        if (id != string.Empty && name != string.Empty) { ccInfo.systems.Add(id, name); id = name = string.Empty; }
                    } while (subReader.Read());
                    subReader.Close();

                    if (!getDatasets) return true;

                    while (mainReader.Read()) { if (mainReader.NodeType == XmlNodeType.Element && mainReader.Name == TAGS.Enclosure(TAGS.DATA)) break; }
                    subReader = mainReader.ReadSubtree(); id = name = string.Empty;
                    do
                    {
                        if (subReader.NodeType != XmlNodeType.Element) continue;
                        if (subReader.Name == TAGS.ID) id = subReader.ReadElementContentAsString();
                        if (subReader.Name == TAGS.NAME) name = subReader.ReadElementContentAsString();
                        if (id != string.Empty && name != string.Empty) { ccInfo.datasets.Add(id, name); id = name = string.Empty; }
                    } while (subReader.Read());
                    subReader.Close();

                    if (!getRunOptions) return true;

                    while (mainReader.Read()) { if (mainReader.NodeType == XmlNodeType.Element && mainReader.Name == TAGS.Enclosure(TAGS.SYS_DATA)) break; }
                    subReader = mainReader.ReadSubtree(); string sid = string.Empty, did = string.Empty, bm = string.Empty;
                    do
                    {
                        if (subReader.NodeType != XmlNodeType.Element) continue;
                        if (subReader.Name == TAGS.SYS_ID) sid = subReader.ReadElementContentAsString();
                        if (subReader.Name == TAGS.DATA_ID) did = subReader.ReadElementContentAsString();
                        if (subReader.Name == TAGS.BEST_MATCH) bm = subReader.ReadElementContentAsString();
                        if (sid != string.Empty && did != string.Empty && bm != string.Empty)
                        {
                            if (!ccInfo.runOptions.ContainsKey(sid)) ccInfo.runOptions.Add(sid, new List<Tuple<string, bool>>());
                            ccInfo.runOptions[sid].Add(new Tuple<string, bool>(did, bm == DefPar.Value.YES));
                            sid = did = bm = string.Empty;
                        }
                    } while (subReader.Read());
                    subReader.Close();
                }
                return true;
            }
            catch (Exception exception)
            {
                error = $"Reading country file {countryFileFullPath} failed: {exception.Message}"; return false;
            }
        }

        public static bool GetEM2Countries(string emPath, out List<string> countries, out string error)
        {
            countries = new List<string>(); error = string.Empty;
            try
            {
                EMPath pathHelper = new EMPath(emPath);
                string folderCountries = pathHelper.GetFolderCountries(true);
                if (!Directory.Exists(emPath)) { Err($"Folder '{folderCountries}' not found!"); return false; }

                foreach (DirectoryInfo di in new DirectoryInfo(folderCountries).GetDirectories())
                {
                    if (File.Exists(pathHelper.GetCountryFilePath(di.Name, true)) &&
                        File.Exists(pathHelper.GetEM2DataConfigFilePath(di.Name))) countries.Add(di.Name);
                }
                return true;
            }
            catch (Exception exception) { error = Err(exception.Message); return false; }

            string Err(string msg) { return $"Failure assessing EM2-countries: {msg}"; }
        }
    }
}
