using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public static class DBExtensions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
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
        public static string GetStringSafe(this MySqlDataReader reader, int colIndex)
        {
            return GetStringSafe(reader, colIndex, string.Empty);
        }
        public static string GetStringSafe(this MySqlDataReader reader, int colIndex, string defaultValue)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            else
                return defaultValue;
        }

        public static string GetStringSafe(this MySqlDataReader reader, string indexName)
        {
             return GetStringSafe(reader, reader.GetOrdinal(indexName));
        }

        public static string GetStringSafe(this MySqlDataReader reader, string indexName, string defaultValue)
        {
            return GetStringSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }
        public static int GetInt32Safe(this MySqlDataReader reader, int colIndex, int defaultValue)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt32(colIndex);
            else
                return defaultValue;
        }
        public static int GetInt32Safe(this MySqlDataReader reader, int colIndex)
        {
            return GetInt32Safe(reader, colIndex, 0);
        }

        public static int GetInt32Safe(this MySqlDataReader reader, string indexName)
        {
            return GetInt32Safe(reader, reader.GetOrdinal(indexName));
        }

        public static int GetInt32Safe(this MySqlDataReader reader, string indexName, int defaultValue)
        {
            return GetInt32Safe(reader, reader.GetOrdinal(indexName), defaultValue);
        }


        public static long GetInt64Safe(this MySqlDataReader reader, int colIndex, long defaultValue)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetInt64(colIndex);
            else
                return defaultValue;
        }
        public static long GetInt64Safe(this MySqlDataReader reader, int colIndex)
        {
            return GetInt64Safe(reader, colIndex, 0);
        }

        public static long GetInt64Safe(this MySqlDataReader reader, string indexName)
        {
            return GetInt64Safe(reader, reader.GetOrdinal(indexName));
        }

        public static long GetInt64Safe(this MySqlDataReader reader, string indexName, long defaultValue)
        {
            return GetInt64Safe(reader, reader.GetOrdinal(indexName), defaultValue);
        }

        public static DateTime? GetDateTimeSafe(this MySqlDataReader reader, int colIndex, DateTime? defaultValue)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetDateTime(colIndex);
            else
                return defaultValue;
        }
        public static DateTime? GetDateTimeSafe(this MySqlDataReader reader, int colIndex)
        {
            return GetDateTimeSafe(reader, colIndex, null);
        }

        public static DateTime? GetDateTimeSafe(this MySqlDataReader reader, string indexName)
        {
            return GetDateTimeSafe(reader, reader.GetOrdinal(indexName));
        }

        public static DateTime? GetDateTimeSafe(this MySqlDataReader reader, string indexName, DateTime? defaultValue)
        {
            return GetDateTimeSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }


        public static double GetDoubleSafe(this MySqlDataReader reader, int colIndex, double defaultValue)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetDouble(colIndex);
            else
                return defaultValue;
        }
        public static double GetDoubleSafe(this MySqlDataReader reader, int colIndex)
        {
            return GetDoubleSafe(reader, colIndex, 0.0);
        }

        public static double GetDoubleSafe(this MySqlDataReader reader, string indexName)
        {
            return GetDoubleSafe(reader, reader.GetOrdinal(indexName));
        }

        public static double GetDoubleSafe(this MySqlDataReader reader, string indexName, double defaultValue)
        {
            return GetDoubleSafe(reader, reader.GetOrdinal(indexName), defaultValue);
        }
    }
}
