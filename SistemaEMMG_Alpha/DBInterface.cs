using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
namespace SistemaEMMG_Alpha
{
    public interface DBInterface
    {
        bool PushToDatabase(MySqlConnection conn);
        bool DeleteFromDatabase(MySqlConnection conn);
    }
}
