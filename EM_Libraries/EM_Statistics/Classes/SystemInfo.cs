using EM_Common;
using EM_Crypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EM_Statistics
{
    internal class SystemInfo
    {
        private string fullFileName = string.Empty;
        private StringBuilder memoryData = null;

        internal SystemInfo(string _fullFileName) { fullFileName = _fullFileName; }
        internal void SetMemoryData(StringBuilder _memoryData) { memoryData = _memoryData; }

        internal string GetFileName() { return Path.GetFileNameWithoutExtension(fullFileName); }

        internal string GetSystemName() { return GetFileGenesis(out fileGenesis) ? fileGenesis.runInfo.systemName : GetFileName().Replace("_std", string.Empty); }

        internal List<string> GetDataRows()
        {
            IEnumerable<string> dataRows;
            // if reading from disk
            if (memoryData == null)
            {
                // if encrypted, decrypt the data
                if (!string.IsNullOrEmpty(SecureInfo.DataPassword))
                {
                    byte[] content = File.ReadAllBytes(fullFileName);
                    // if encrypted, then decrypt it
                    if (SimpleCrypt.IsEncrypted(content, out int nonSecretLength))
                        content = SimpleCrypt.SimpleDecryptWithPassword(content, SecureInfo.DataPassword, nonSecretLength);
                    // if decoding failed (e.g. wrong password) read the file as is, else use decrypted data
                    dataRows = content == null ? File.ReadLines(fullFileName) : Encoding.UTF8.GetString(content).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                }
                else    // not encrypted
                    dataRows = File.ReadLines(fullFileName);
            }
            else    // if getting from memory
                dataRows = memoryData.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return (from dr in dataRows where !string.IsNullOrEmpty(dr.Trim()) select dr).ToList(); // remove (last) empty line which may exist in StringBuilder
        }

        internal class FileGenesis { internal RunLogger.RunInfo runInfo; internal RunLogger.GeneralInfo generalInfo; internal RunLogger.PetInfo petInfo; }
        internal FileGenesis fileGenesis = null;
        internal bool fileGenesisNotAvailable = false;

        internal bool GetFileGenesis(out FileGenesis _fileGenesis)
        {
            if (fileGenesis == null && !fileGenesisNotAvailable)
            {
                try
                {
                    if (RunLogger.GetRunInfo(fullFileName, out RunLogger.RunInfo ri, out RunLogger.GeneralInfo gi, out RunLogger.PetInfo pi))
                        fileGenesis = new FileGenesis() { runInfo = ri, generalInfo = gi, petInfo = pi };
                    else fileGenesisNotAvailable = true;
                }
                catch { fileGenesisNotAvailable = true; }
            }
            _fileGenesis = fileGenesis; return _fileGenesis != null;
        }
    }
}
