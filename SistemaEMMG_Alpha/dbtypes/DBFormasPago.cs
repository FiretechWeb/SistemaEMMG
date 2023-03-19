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
        public FormasPagoData(string nom)
        {
            fp_nombre = nom;
        }
        public string fp_nombre { get; set; }

        public static readonly string NameOf_fp_nombre = nameof(fp_nombre);

        public override string ToString()
        {
            return $"Nombre: {fp_nombre}";
        }
    }
    public class DBFormasPago : DBBaseClass, IDBDataType<DBFormasPago>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public static readonly string db_table = "formas_pago";
        public static readonly string NameOf_id = "fp_id";
        private long _id;
        private FormasPagoData _data;
        private static readonly List<DBFormasPago> _db_formas_pago = new List<DBFormasPago>();

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
                    _db_formas_pago.Add(new DBFormasPago(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(FormasPagoData.NameOf_fp_nombre)));
                    returnList.Add(new DBFormasPago(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(FormasPagoData.NameOf_fp_nombre))); //Waste of performance but helps with making the code less propense to error.
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {fp_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBFormasPago(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(FormasPagoData.NameOf_fp_nombre));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener una forma de pago en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBFormasPago(long id, FormasPagoData newData)
        {
            _id = id;
            _data = newData;
        }

        public DBFormasPago(long id, string nombre) : this(id, new FormasPagoData(nombre)) { }

        public DBFormasPago(string nombre) : this(-1, nombre) { }

        public DBFormasPago(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _id = id;
                    _data = new FormasPagoData(reader.GetStringSafe(FormasPagoData.NameOf_fp_nombre));
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

        public override bool PushToDatabase(MySqlConnection conn)
        {
            bool? existsInDB = ExistsInDatabase(conn);
            if (existsInDB is null) //error with DB...
            {
                return false;
            }

            return Convert.ToBoolean(existsInDB) ? UpdateToDatabase(conn) : InsertIntoToDatabase(conn);
        }

        public override bool UpdateToDatabase(MySqlConnection conn)
        {
            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET {FormasPagoData.NameOf_fp_nombre} = '{_data.fp_nombre}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToUpdate)
                {
                    ChangeID(cmd.LastInsertedId);
                }
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBFormasPago::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({FormasPagoData.NameOf_fp_nombre}) VALUES ('{_data.fp_nombre}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBFormasPago::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_id} = {GetID()}";
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

        public override bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBFormasPago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public DBFormasPago Clone() => new DBFormasPago(_id, _data.fp_nombre);
        public override long GetID() => _id;
        protected override void ChangeID(long id) => _id = id;
        public string GetName() => _data.fp_nombre;

        public void SetName(string newName) => _data.fp_nombre = newName;

        public override string ToString()
        {
            return _data.ToString();
        }

    }
}
