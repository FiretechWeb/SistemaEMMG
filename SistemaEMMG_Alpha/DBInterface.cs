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
        ///<summary>
        ///Refresh and return all the elements of this type allocated in the database
        ///</summary>
        List<T> UpdateAll(MySqlConnection conn);

        ///<summary>
        ///Get all elements of this type allocated in the database (call UpdateAll() First)
        ///</summary>
        List<T> GetAll();
        ///<summary>
        ///Returns a clone of the element, no reference but value data duplicated.
        ///</summary>
        T Clone();
    }
    public interface DBInterface
    {
        ///<summary>
        ///Get the ID of the element as it is stored in the Database
        ///</summary>
        long GetID();
        ///<summary>
        ///Push this element to the database. If it exists it just update, if does not exists, it is inserted as a new element.
        ///</summary>
        bool PushToDatabase(MySqlConnection conn);
        ///<summary>
        ///Delete this element from the database.
        ///</summary>
        bool DeleteFromDatabase(MySqlConnection conn);
    }
}
