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

    public interface IDBComprobante<T>
    {
        ///<summary>
        /// Returns the bussiness receipt ID, as in the DB, that contains the data type implementing this Interface 
        ///</summary>
        long GetComprobanteID();

        ///<summary>
        /// Returns the bussiness receipt that contains the data type implementing this Interface 
        ///</summary>
        T GetComprobante();
    }

    public interface IDBEntidadComercial<T>
    {
        ///<summary>
        /// Returns the bussiness entity ID, as in the DB, that contains the data type implementing this Interface 
        ///</summary>
        long GetEntidadComercialID();

        ///<summary>
        /// Returns the bussiness entity that contains the data type implementing this Interface 
        ///</summary>
        T GetEntidadComercial();
    }

    public interface IDBCuenta<T>
    {
        ///<summary>
        /// Returns the ID, as in the DB, of the current account being used.
        ///</summary>
        long GetCuentaID();
        ///<summary>
        /// Returns an instance of the current business account being used.
        ///</summary>
        T GetCuenta();
    }
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
