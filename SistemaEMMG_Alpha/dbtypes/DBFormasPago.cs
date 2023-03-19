using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows;

namespace SistemaEMMG_Alpha
{
    public struct FormasPagoData
    {
        public FormasPagoData(long id, string nom)
        {
            fp_id = id;
            fp_nombre = nom;
        }
        public long fp_id { get; }
        public string fp_nombre { get; set; }

        public override string ToString()
        {
            return $"ID: {fp_id} - Nombre: {fp_nombre}";
        }
    }
    public class DBFormasPago : DBInterface, IDBDataType<DBFormasPago>
    {
        private static readonly string db_table = "formas_pago";
        private FormasPagoData _data;
        private static readonly List<DBFormasPago> _db_formas_pago = new List<DBFormasPago>();

        public static string GetDBTableName() => db_table;
        string DBInterface.GetDBTableName() => GetDBTableName();
        public static bool TipoFormaPagoExists(string formaPagoNombre, List<DBFormasPago> listaFormasPago)
        {
            foreach (DBFormasPago formaPago in listaFormasPago)
            {
                if (formaPagoNombre.Trim().ToLower().Equals(formaPago.GetName().Trim().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TipoFormaPagoExists(string formaPagoNombre)
        {
            return TipoFormaPagoExists(formaPagoNombre, _db_formas_pago);
        }

        public static List<DBFormasPago> UpdateAll(MySqlConnection conn)
        {
            List<DBFormasPago> returnList = new List<DBFormasPago>();
            try
            {
                string query = $"SELECT * FROM {db_table}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_formas_pago.Clear();

                while (reader.Read())
                {
                    _db_formas_pago.Add(new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre")));
                    returnList.Add(new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"))); //Waste of performance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los tipos formas de pago. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBFormasPago> GetAll()
        {
            List<DBFormasPago> returnList = new List<DBFormasPago>();
            foreach (DBFormasPago formaPago in _db_formas_pago)
            {
                returnList.Add(formaPago);
            }
            return returnList;
        }

        /***************************
         * START: IDBDataType Implementation contract
         * *****************************/

        List<DBFormasPago> IDBDataType<DBFormasPago>.UpdateAll(MySqlConnection conn)
        {
            return DBFormasPago.UpdateAll(conn);
        }

        List<DBFormasPago> IDBDataType<DBFormasPago>.GetAll()
        {
            return DBFormasPago.GetAll();
        }


        /***************************
         * END: IDBDataType Implementation contract
         * *****************************/


        public static DBFormasPago GetByID(long fp_id, bool clone = true)
        {
            foreach (DBFormasPago formaPago in _db_formas_pago)
            {
                if (formaPago.GetID() == fp_id)
                {
                    return clone ? formaPago.Clone() : formaPago;
                }
            }
            return null;
        }

        public static DBFormasPago GetByID(long fp_id, MySqlConnection conn)
        {
            DBFormasPago returnEnt = null;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE tc_id = {fp_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener una forma de pago en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBFormasPago(FormasPagoData newData)
        {
            _data = newData;
        }

        public DBFormasPago(long id, string nombre) : this(new FormasPagoData(id, nombre)) { }

        public DBFormasPago(string nombre) : this(-1, nombre) { }

        public DBFormasPago(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE fp_id = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = new FormasPagoData(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBFormasPago. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public FormasPagoData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            bool? existsInDB = ExistsInDatabase(conn);
            if (existsInDB is null) //error with DB...
            {
                return false;
            }
            try
            {
                //First check if the record already exists in the DB
                string query;

                //if exists already, just update
                if (existsInDB == true)
                {
                    query = $"UPDATE {db_table} SET fp_nombre = '{_data.fp_nombre}' WHERE fp_id = {_data.fp_id}";

                }
                else //if does not exists, insert into
                {
                    query = $"INSERT INTO {db_table} (fp_nombre) VALUES ('{_data.fp_nombre}')";
                }
                var cmd = new MySqlCommand(query, conn);
                //if not, add
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (existsInDB == false) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new FormasPagoData(cmd.LastInsertedId, _data.fp_nombre);
                }
                wasAbleToPush = true;
            }
            catch (Exception ex)
            {
                wasAbleToPush = false;
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBFormasPago: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPush;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE fp_id = {_data.fp_id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBFormasPago: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? existsInDB = null;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE fp_id = {_data.fp_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //if exists already, just update
                if (recordsCount > 0)
                {
                    existsInDB = true;
                }
                else
                {
                    existsInDB = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBFormasPago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public DBFormasPago Clone() => new DBFormasPago(_data.fp_id, _data.fp_nombre);
        public long GetID() => _data.fp_id;

        public string GetName() => _data.fp_nombre;

        public void SetName(string newName) => _data.fp_nombre = newName;

        public override string ToString()
        {
            return _data.ToString();
        }

    }
}
