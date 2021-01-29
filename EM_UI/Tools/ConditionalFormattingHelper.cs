using EM_Common;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace EM_UI.Tools
{
    class ConditionalFormattingHelper
    {
        const string _patternStart = "{";
        const string _patternEnd = "}";
        const string _patternSeparator = "} OR {";
        internal const string _noSpecialColor = "no special color";

        internal static string GetDisplayTextFromColor(Color color)
        {
            if (color == Color.Empty || color == Color.Transparent)
                return _noSpecialColor;
            if (color.IsNamedColor)
                return color.Name; //e.g. red = 'red', while ...
            return string.Format("{0:X}", color.ToArgb()); //... some obscure color = 'FFAB0DC6'
        }

        internal static Color GetColorFromDisplayText(string displayText)
        {
            if (displayText == _noSpecialColor || displayText == string.Empty)
                return Color.Empty;
            if (EM_Helpers.IsNumeric(displayText, true))
                return Color.FromArgb(Convert.ToInt32(displayText, 16));
            return Color.FromName(displayText);
        }

        internal static List<string> GetFormatConditionPatterns(string formatCondition)
        {
            List<string> patternList = new List<string>();
            if (formatCondition.StartsWith(_patternStart))
                formatCondition = formatCondition.Substring(_patternStart.Length); //remove { at begin
            if (formatCondition.EndsWith(_patternEnd))
                formatCondition = formatCondition.Substring(0, formatCondition.Length - _patternEnd.Length); //remove } at end
            formatCondition += _patternSeparator; //for simpler search-procedure add separator at end

            for (int iStart = 0, iEnd = 0; iStart < formatCondition.Length; iStart = iEnd + _patternSeparator.Length)
            {
                iEnd = formatCondition.IndexOf(_patternSeparator, iStart);
                patternList.Add(formatCondition.Substring(iStart, iEnd - iStart));
            }
            return patternList;
        }
    }
}
