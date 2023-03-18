using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public interface IDBDataType<T> where T : class
    {
        List<T> UpdateAll(MySqlConnection conn);
        List<T> GetAll();
    }
    public interface DBInterface
    {
        long GetID();
        bool PushToDatabase(MySqlConnection conn);
        bool DeleteFromDatabase(MySqlConnection conn);
    }
}
