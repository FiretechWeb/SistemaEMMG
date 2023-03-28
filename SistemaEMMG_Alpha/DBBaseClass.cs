using System;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public abstract class DBBaseClass
    {
        protected long _id=-1;
        protected bool _shouldPush = false;

        protected DBBaseClass(long id)
        {
            _id = id;
            if (IsLocal())
            {
                _shouldPush = true;
            }
        }
        ///<summary>
        ///Check if element with same data exists in DB. This checks for data, and not ID. For checking if an element with the same ID exists in database, use ExistsInDatabase method.
        ///</summary>
        abstract public bool? DuplicatedExistsInDatabase(MySqlConnection conn);
        ///<summary>
        ///Check if element exists in the database or not. If it returns null, it means there was an error connecting with the DB.
        ///</summary>
        abstract public bool? ExistsInDatabase(MySqlConnection conn);
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
        virtual public bool PushToDatabase(MySqlConnection conn)
        {
            if (!ShouldPush())
            {
                return false;
            }
            bool? existsInDB = IsLocal() ? false : ExistsInDatabase(conn);
            if (existsInDB is null) //error with DB...
            {
                return false;
            }

            return Convert.ToBoolean(existsInDB) ? UpdateToDatabase(conn) : InsertIntoToDatabase(conn);
        }
        ///<summary>
        ///Refresh (pull) the information directly from DB.
        ///</summary>
        abstract public bool PullFromDatabase(MySqlConnection conn);

        ///<summary>
        ///Delete this element from the database.
        ///</summary>
        abstract public bool DeleteFromDatabase(MySqlConnection conn);
        ///<summary>
        ///Returns a Local copy of this element.
        ///</summary>
        abstract public DBBaseClass GetLocalCopy();
        ///<summary>
        ///Get the ID of the element as it is stored in the Database. Negative values means it is local data not stored in the database.
        ///</summary>
        virtual public long GetID() => _id;
        ///<summary>
        ///Change the ID of the element (private method only, for security reasons)
        ///</summary>
        virtual protected void ChangeID(long id)
        {
            _shouldPush = _shouldPush || (id != _id);
            _id = id;
        }
        ///<summary>
        ///Returns true if the entity changed after being read from the DB (or being created in general)
        ///</summary>
        virtual public bool ShouldPush() => _shouldPush;
        ///<summary>
        ///Returns true if the data was locally created and not present originally in the DB.
        ///</summary>
        virtual public bool IsLocal() => _id < 0;

        ///<summary>
        ///If this element was retrieved from DB, it makes it now local so it can be pushed into the DB as a new element. 
        ///(private method only, for security reasons)
        ///</summary>
        virtual protected void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }
    }

}
