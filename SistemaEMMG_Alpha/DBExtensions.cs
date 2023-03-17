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
    }
}
