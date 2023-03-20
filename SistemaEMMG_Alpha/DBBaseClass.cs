using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public abstract class DBBaseClass
    {
        ///<summary>
        ///Change the ID of the element (private method only, for security reasons)
        ///</summary>
        abstract protected void ChangeID(long id);
        ///<summary>
        ///Check if element exists in the database or not. If it returns null, it means there was an error connecting with the DB.
        ///</summary>
        abstract public bool? ExistsInDatabase(MySqlConnection conn);
        ///<summary>
        ///Get the ID of the element as it is stored in the Database
        ///</summary>
        abstract public long GetID();
        ///<summary>
        ///Update this element into the database.
        ///</summary>
        abstract public bool UpdateToDatabase(MySqlConnection conn);
        ///<summary>
        ///Insert this element to the database as a row ignoring it's current ID.
        ///</summary>
        abstract public bool InsertIntoToDatabase(MySqlConnection conn);
        ///<summary>
        ///Push this element to the database. If it exists then just updates it. If it does not exists, then it is inserted as a new element.
        ///</summary>
        abstract public bool PushToDatabase(MySqlConnection conn);
        ///<summary>
        ///Delete this element from the database.
        ///</summary>
        abstract public bool DeleteFromDatabase(MySqlConnection conn);
    }

}
