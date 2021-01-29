using System;
using System.IO;

namespace EM_Common
{
    public partial class EM_Helpers
    {
        public static bool AreFileHashesEqual(string filePath1, string filePath2)
        {
            byte[] hash1 = GenerateHash(filePath1); byte[] hash2 = GenerateHash(filePath2);
            if (hash1.Length != hash2.Length) return false;
            for (int i = 0; i < hash1.Length; i++) if (hash1[i] != hash2[i]) return false; return true;
        }

        public static byte[] GenerateHash(string filePath) // could probably be unified with GetFileMD5Hash (?)
        {
            System.Security.Cryptography.MD5 crypto = System.Security.Cryptography.MD5.Create();
            using (System.IO.FileStream stream = System.IO.File.OpenRead(filePath)) return crypto.ComputeHash(stream);
        }

        public static string GetFileMD5Hash(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
