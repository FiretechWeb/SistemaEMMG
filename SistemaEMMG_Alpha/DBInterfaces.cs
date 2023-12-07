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
        List<T> UpdateAll(MySqlConnection conn); //In future change to IReadOnlyCollection

        ///<summary>
        ///Get all elements of this type allocated in the database (call UpdateAll() First)
        ///</summary>
        List<T> GetAll(); //In future change to IReadOnlyCollection
        ///<summary>
        ///Returns a list with all the elements of this type that are locally created (not pushed into the DB yet)
        ///</summary>
        ///
        IReadOnlyCollection<T> GetAllLocal();
        List<T> GenerateDefaultData();

        bool PushDefaultData(MySqlConnection conn);

        bool ResetDBData(MySqlConnection conn);
    }

    public interface IDBRecibo<T>
    {
        ///<summary>
        /// Returns the bussiness recibo ID, as in the DB, that contains the data type implementing this Interface 
        ///</summary>
        long GetReciboID();

        ///<summary>
        /// Returns the bussiness recibo that contains the data type implementing this Interface 
        ///</summary>
        T GetRecibo();
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

    public interface IDBase<T>
    {
        ///<summary>
        /// Returns the SQL string used to join the table corresponding to this datatype with all its relations and the relations of its relations.
        ///</summary>
        string GetSQL_SelectQueryWithRelations(string fieldsToGet);
    }
}
