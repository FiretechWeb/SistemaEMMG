using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SistemaEMMG_Alpha
{
    public static class StringExtensions
    {

        ///<summary>
        ///Remove all letters except numbers and the minus sign. Also removes filling zeros after minus sign and before another number.
        ///</summary>
        public static string KeepNumbersAndMinus(this string text) => Regex.Replace(Regex.Replace(text, "[^0-9-]", ""), "-0+(?=[1-9])", "-");

        ///<summary>
        ///Normalize to lower, remove accent, trim string and ensure only only space between characters.
        ///</summary>
        public static string DeepNormalize(this string text)
        {
            return (new Regex(@"[ ]{2,}", RegexOptions.None)).Replace(string.Concat(Regex.Replace(text.Trim().ToLower(), @"(?i)[\p{L}-[ña-z]]+", m =>
    m.Value.Normalize(NormalizationForm.FormD)).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)), @" ");
        }
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string CleanWhitespaces(this string text)
        {
            return Regex.Replace(text.Trim(), @"\s+", " ");
        }

        public static bool IsValidDateTime(this string text)
        {
            return DateTime.TryParse(text, out DateTime _tmpDateTime);
        }
    }
}
