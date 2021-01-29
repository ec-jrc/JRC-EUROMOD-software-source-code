using System;
using System.Collections.Generic;

namespace EM_Common
{
    public static class RandColor
    {
        private static Random rand = new Random();

        public static void Set()
        {
            List<ConsoleColor> all = new List<ConsoleColor>() { ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.White };
            ConsoleColor co; for (;;) { co = all[rand.Next(0, all.Count)]; if (co != Console.ForegroundColor) break; }
            Console.ForegroundColor = co;
        }

        public static void WriteLine(string text, bool changeAfter = false)
        {
            Set(); Console.WriteLine(text); if (changeAfter) Set();
        }

        public static void WriteLine(string text, ConsoleColor specColor, bool changeAfter = false)
        {
            Console.ForegroundColor = specColor; Console.WriteLine(text); if (changeAfter) Set();
        }

        public static void Write(string text)
        {
            Set(); Console.Write(text);
        }

        public static void Write(string text, ConsoleColor specColor)
        {
            Console.ForegroundColor = specColor; Console.Write(text);
        }
    }
}