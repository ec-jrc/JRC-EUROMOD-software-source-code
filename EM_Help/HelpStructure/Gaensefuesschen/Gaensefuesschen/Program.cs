using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaensefuesschen
{
    class Program
    {
        static void Main(string[] args)
        {
            char[] hhcContent= new char[50000];
            System.IO.StreamReader hhcReader = new System.IO.StreamReader(args[0]);
            int nChar = hhcReader.ReadBlock(hhcContent, 0, 50000);
            hhcReader.Close();

            System.IO.StreamWriter hhcWriter = new System.IO.StreamWriter(args[0]);
            for (int i = 0; i < nChar; ++i)
            {
                if (hhcContent[i] != '{')
                    hhcWriter.Write(hhcContent[i]);
                else
                    hhcWriter.Write('"');
            }
            hhcWriter.Close();
        }
    }
}
