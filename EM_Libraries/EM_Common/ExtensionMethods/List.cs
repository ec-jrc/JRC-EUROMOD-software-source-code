using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public static class ExtensionMethods_List
    {
        public static bool Contains(this List<string> list, string value, bool ignoreCase = false)
        {
            return ignoreCase ? list.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase)) : list.Contains(value);
        }

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex || 0 > oldIndex || oldIndex >= list.Count || 0 > newIndex || newIndex >= list.Count) return; // exit if positions are equal or outside array
            var i = 0; T tmp = list[oldIndex];
            if (oldIndex < newIndex) for (i = oldIndex; i < newIndex; i++) list[i] = list[i + 1]; // move element down and shift other elements up
            else for (i = oldIndex; i > newIndex; i--) list[i] = list[i - 1]; // move element up and shift other elements down
            list[newIndex] = tmp; // put element from position 1 to destination
        }
        public static void MoveUp<T>(this List<T> list, T item) { list.Move(list.IndexOf(item), list.IndexOf(item) - 1); }
        public static void MoveDown<T>(this List<T> list, T item) { list.Move(list.IndexOf(item), list.IndexOf(item) + 1); }

        public static void AddUnique(this List<string> list, string value, bool ignoreCase = false)
        {
            if (!list.Contains(value, ignoreCase)) list.Add(value);
        }
    }
}
