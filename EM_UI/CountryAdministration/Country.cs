using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace EM_UI.CountryAdministration
{
    internal class Country
    {
        internal string _shortName = string.Empty;
        internal string _longName = string.Empty;
        internal bool _isAddOn = false;
        internal Image _flag = null;

        CountryConfigFacade _countryConfigFacade = null;
        DataConfigFacade _dataConfigFacade = null;

        const string DataFileExt = "_DataConfig";

        internal Country(string shortName = "", bool isAddOn = false, string longName = "", Image flag = null)
        {
            _shortName = shortName;
            _longName = longName;
            _isAddOn = isAddOn;
            _flag = flag;
        }

        internal string GetPath()
        {
            if (CountryAdministrator.ConsiderOldAddOnFileStructure(_isAddOn))
                return EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles);
            else
                return EMPath.AddSlash((_isAddOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)) + _shortName);
        }

        internal static string GetCountryXMLFileName(string shortName) { return shortName + ".xml"; }
        internal string GetCountryFileName(string alternativeName = "")
        {
            return (alternativeName == string.Empty) ? GetCountryXMLFileName(_shortName) : GetCountryXMLFileName(alternativeName);
        }

        internal static string GetDataConfigXMLFileName(string shortName) { return shortName + DataFileExt + ".xml"; }
        internal string GetDataConfigFileName(string alternativeName = "")
        {
            return (alternativeName == string.Empty) ? GetDataConfigXMLFileName(_shortName) : GetDataConfigXMLFileName(alternativeName);
        }

        internal CountryConfigFacade GetCountryConfigFacade(bool enforceReading = false, string alternativePath = "", bool informIfInconsistent = true)
        {
            try
            {
                if (_countryConfigFacade == null || enforceReading)
                {
                    if (alternativePath == string.Empty)
                        alternativePath = GetPath();
                    _countryConfigFacade = new CountryConfigFacade();
                    _countryConfigFacade.ReadXml(alternativePath, GetCountryFileName());

                    //file names (cc.xml and cc_DataConfig.xml) must correspond to ShortName stored in cc.xml, otherwise some functions (e.g. add parameter) do not work
                    CheckForCorrespondenceOfCountryShortName(informIfInconsistent);
                }                
            }
            catch (Exception e)
            {
                Tools.UserInfoHandler.ShowException(e);
                return null;
            }
            return _countryConfigFacade;
        }

        //try to correct if file names (cc.xml and cc_DataConfig.xml) do notcorrespond to ShortName stored in cc.xml
        void CheckForCorrespondenceOfCountryShortName(bool informIfInconsistent = true)
        {
            try
            {
                if (_countryConfigFacade.GetCountryShortName().ToLower() == _shortName.ToLower())
                    return; //short name corresponds - everything ok

                if (informIfInconsistent)
                    UserInfoHandler.ShowInfo("Filenames do not correspond to the short name stored within the files."
                        + Environment.NewLine + _countryConfigFacade.GetCountryShortName() + " versus " + _shortName
                        + Environment.NewLine + "Note that this will be corrected for you.");
                
                _countryConfigFacade.GetCountryRow().ShortName = _shortName;
                WriteXML();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        internal DataConfigFacade GetDataConfigFacade(bool enforceReading = false, string alternativePath = "")
        {
            if (_isAddOn)
                return null;

            try
            {
                if (_dataConfigFacade == null || enforceReading)
                {
                    if (alternativePath == string.Empty)
                        alternativePath = GetPath();
                    _dataConfigFacade = new DataConfigFacade();
                    _dataConfigFacade.ReadXml(alternativePath, GetDataConfigFileName());
                }
            }
            catch (Exception e)
            {
                Tools.UserInfoHandler.ShowException(e);
            }
            return _dataConfigFacade;
        }

        internal void SetDataConfigFacade(DataConfigFacade dataConfigFacade)
        {
            _dataConfigFacade = dataConfigFacade;
        }

        internal void SetCountryConfigFacade(CountryConfigFacade countryConfigFacade)
        {
            _countryConfigFacade = countryConfigFacade;
        }

        internal static bool CopyXMLFiles(string shortName, out string errorMessage,
                                          string sourceFolder = "", string destinationFolder = "", int addOn = -1)
        {
            try
            {
                bool isAddOn = addOn == -1 ? CountryAdministrator.IsAddOn(shortName) : (addOn == 0 ? false : true);
                string defaultFolder = isAddOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles);
                if (!CountryAdministrator.ConsiderOldAddOnFileStructure(isAddOn))
                    defaultFolder += shortName;

                if (destinationFolder == string.Empty) destinationFolder = defaultFolder;
                if (sourceFolder == string.Empty) sourceFolder = defaultFolder;

                File.Copy(EMPath.AddSlash(sourceFolder) + GetCountryXMLFileName(shortName),
                          EMPath.AddSlash(destinationFolder) + GetCountryXMLFileName(shortName), true);
                if (!isAddOn)
                    File.Copy(EMPath.AddSlash(sourceFolder) + GetDataConfigXMLFileName(shortName),
                              EMPath.AddSlash(destinationFolder) + GetDataConfigXMLFileName(shortName), true);
                
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        internal static bool DoesFolderContainValidXMLFiles(string shortName, out string errorMessage, string folderPath, bool isAddOn,
                                                            bool checkForCountryConsistence = false)
        {
            if (!IsValidCountryXMLFile(folderPath, shortName, out errorMessage, checkForCountryConsistence))
                return false;
            if (!isAddOn && !IsValidDataConfigXMLFile(folderPath, shortName, out errorMessage))
                return false;
            errorMessage = string.Empty;
            return true;
        }

        static bool IsValidCountryXMLFile(string folderPath, string countryShortName, out string errorMessage, bool checkForCountryConsistence = false)
        {
            try
            {
                CountryConfig countryConfig = new CountryConfig();
                string fileName = EMPath.AddSlash(folderPath) + GetCountryXMLFileName(countryShortName);
                if (!File.Exists(fileName)) { errorMessage = string.Format($"File '{fileName}' not found."); return false; } // this is for whatever reason not catched by the error handler and leads to a crash if not handled here
                using (StreamReader streamReader = new StreamReader(fileName, DefGeneral.DEFAULT_ENCODING))
                    countryConfig.ReadXml(streamReader);

                //trying to access country leads to an exception if this is not a valid country xml-file
                string xmlCountryShortName = countryConfig.Country.First().ShortName;

                if (checkForCountryConsistence)
                {
                    if (xmlCountryShortName.ToLower() != countryShortName.ToLower())
                        throw new System.ArgumentException("Name of country within file (" + xmlCountryShortName + 
                                                           ") does not correspond with name used for country file (" + countryShortName + ").");
                }

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        static bool IsValidDataConfigXMLFile(string folderPath, string countryShortName, out string errorMessage)
        {
            try
            {
                DataConfig dataConfig = new DataConfig();
                using (StreamReader streamReader = new StreamReader(EMPath.AddSlash(folderPath) + GetDataConfigXMLFileName(countryShortName), DefGeneral.DEFAULT_ENCODING))
                    dataConfig.ReadXml(streamReader); //this catches actually only if it's not a readable xml-file, but not it is no data-config file
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        internal void WriteXML(string alternativePath = "", string alternativeName = "", bool saveWithLineBreaks = true)
        {
            if (alternativePath == string.Empty)
                alternativePath = GetPath();

            if (_countryConfigFacade != null)
                _countryConfigFacade.WriteXml(alternativePath, GetCountryFileName(alternativeName), saveWithLineBreaks);
            if (_dataConfigFacade != null)
                _dataConfigFacade.WriteXml(alternativePath, GetDataConfigFileName(alternativeName), saveWithLineBreaks);
        }
    }
}
