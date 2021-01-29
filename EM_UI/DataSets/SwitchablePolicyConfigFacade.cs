using EM_Common;
using EM_UI.Tools;
using System;
using System.IO;

namespace EM_UI.DataSets
{
    internal class SwitchablePolicyConfigFacade
    {
        private string _pathSwitchablePolicyConfig = string.Empty;
        private SwitchablePolicyConfig _switchablePolicyConfig = null;

        internal SwitchablePolicyConfigFacade(string pathSwitchablePolicyConfig = null)
        {
            _pathSwitchablePolicyConfig = pathSwitchablePolicyConfig ?? new EMPath(EM_AppContext.FolderEuromodFiles).GetExtensionsFilePath(true);
            LoadSwitchablePolicyConfig();
        }

        static readonly string[] _cDataElements = new string[] { "NamePattern" };

        private bool LoadSwitchablePolicyConfig()
        {
            try
            {
                _switchablePolicyConfig = new SwitchablePolicyConfig();
                using (StreamReader streamReader = new StreamReader(_pathSwitchablePolicyConfig, DefGeneral.DEFAULT_ENCODING))
                    _switchablePolicyConfig.ReadXml(streamReader);
                AcceptChanges();
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal SwitchablePolicyConfig GetSwitchablePolicyConfig()
        {
            return _switchablePolicyConfig;
        }

        internal bool WriteXML(string filePath = null)
        {
            try
            {
                AcceptChanges();
                Stream fileStream = new FileStream(filePath ?? _pathSwitchablePolicyConfig, FileMode.Create);
                using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING, _cDataElements))
                    _switchablePolicyConfig.WriteXml(xmlWriter);
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        internal void RejectChanges() { _switchablePolicyConfig.RejectChanges(); }

        internal void AcceptChanges() { _switchablePolicyConfig.AcceptChanges(); }
    }
}
