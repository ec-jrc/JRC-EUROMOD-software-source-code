using EM_Common;
using System.IO;

namespace EM_ExecutableCaller
{
    internal class Test
    {
        internal static string[] T1()
        {
            return new string[]
            {
                Path.Combine(new EMPath(GetEM3TestFolder.EM2EuromodFiles()).GetFolderTemp(), "EMConfigd9badded-0629-40b1-ba8a-0ed05dc8371b.xml")
            };
        }

        internal static string[] T2()
        {
            return new string[]
            {
                GetEM3TestFolder.EM2EuromodFiles(), "sl_demo", "sl_demo_v4.txt"
            };
        }

        internal static string[] T3()
        {
            return new string[]
            {
                GetEM3TestFolder.EM2EuromodFiles(), "mt_2017", "mt_2015_a1", GetEM3TestFolder.EM_Output(), GetEM3TestFolder.EM_Data()
            };
        }

        internal static string[] T4()
        {
            return new string[]
            {
                "-config", Path.Combine(new EMPath(GetEM3TestFolder.EM2EuromodFiles()).GetFolderTemp(), "EMConfigd9badded-0629-40b1-ba8a-0ed05dc8371b.xml")
            };
        }

        internal static string[] T5()
        {
            return new string[]
            {
                "-data", "sl_demo_v4.txt", "-emPath", GetEM3TestFolder.EM2EuromodFiles(), "-sys", "sl_demo"
            };
        }

        internal static string[] T6()
        {
            return new string[]
            {
                "-sys", "mt_2017", "-emPath", GetEM3TestFolder.EM2EuromodFiles(), "-data", "mt_2015_a1", "-dataPath", GetEM3TestFolder.EM_Data(),
                "-outPath", GetEM3TestFolder.Rel_EM3Output(new EMPath(GetEM3TestFolder.EM2EuromodFiles()).GetFolderOutput())
            };
        }

        internal static string[] T7()
        {
            return new string[]
            {
                "-emPath", @"C:\euromod\EM3_TESTDATA\EuromodFiles_I1.5",
                "-sys", "sl_demo",
                "-data", "training_data.txt",
                "-pathParModifications", @"C:\Users\Christine\Downloads\SimpleModi.txt"
            };
        }

        internal static string[] T8()
        {
            return new string[]
            {
                "-emPath", @"C:\euromod\EM3_TESTDATA\EuromodFiles_I1.66",
                "-sys", "uk_2019",
                "-data", "UK_2016_a1.txt",
                "-extSwitch", "BTA_??=off",
                "-dataPath", @"C:\euromod\EM3_TESTDATA\EM_Data",
                "-extSwitch", "PAA=off"
            };
        }

        internal static string[] TPlay()
        {
            return new string[]
            {
                "-config", @"C:\euromod\EM3_TESTDATA\EUROMOD_PET\XMLParam\Temp\EMConfig.xml"
            };
        }
    }
}
