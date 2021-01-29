using EM_Common;
using EM_XmlHandler;
using System;

namespace EM_Executable
{
    /// <summary>
    /// this test class is initialised in Control.Run to allow test-programmes access to the executable's infoStore
    /// through methods of this class (and thus avoids to make the class InfoStore public)
    /// </summary>
    public static partial class EM_Executable_Test
    {
        internal static InfoStore infoStore
        {
            get
            {
                if (_infoStore == null) { throw new Exception("EM_Executable_Test: infoStore is not initialised"); }
                return _infoStore;
            }
            set { _infoStore = value; } // set in Control.Run to make infoStore available for test-programmes
        }
        private static InfoStore _infoStore = null;

        // return stuff that uses publicly known classes, i.e. test-programmes can evaluate it as they require
        public static ExeXml.Country GetXmlCountry() { return infoStore.country; }
        public static double GetExRate() { return infoStore.exRate; }

        // visual stuff
        private static ConsoleColor colorHeading = ConsoleColor.Cyan, colorInfo = ConsoleColor.Gray,
                                    colorContent1 = ConsoleColor.White, colorContent2 = ConsoleColor.Gray, currentContentColor;

        private static void PrintHeading(string heading)
        {
            RandColor.Write(Environment.NewLine + "* * *   ", colorHeading);
            foreach (char c in heading.ToUpper()) RandColor.Write($"{c} ", colorHeading);
            RandColor.WriteLine("  * * *", colorHeading);
        }
        private static void PrintSubHeading(string heading)
        {
            RandColor.WriteLine($"- - -   {heading}   - - -", colorHeading);
        }

        private static bool Exclude(ref int i, int n, int count) { ++i; return (i > n && i <= count - n); }
        private static ConsoleColor GetSwapColor()
        {
            currentContentColor = currentContentColor == colorContent1 ? colorContent2 : colorContent1;
            return currentContentColor;
        }

    }
}
