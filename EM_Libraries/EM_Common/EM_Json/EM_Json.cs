using Newtonsoft.Json;
using System;
using System.IO;

namespace EM_Common
{
    public static partial class EM_Json
    {
        public static bool GetFileContent<T>(string filePath, out T content, out string error)
        {
            error = string.Empty; content = default(T);
            try
            {
                if (!File.Exists(filePath))
                {
                    error = $"File {filePath} not found";
                    return false;
                }

                using (StreamReader sr = new StreamReader(filePath))
                {
                    content = (T)JsonConvert.DeserializeObject(sr.ReadToEnd(), typeof(T));
                }
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }
    }
}
