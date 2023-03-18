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
    public struct TiposComprobantesData
    {
        public TiposComprobantesData(long id, string nom)
        {
            tc_id = id;
            tc_nombre = nom;
        }
        public long tc_id { get; }
        public string tc_nombre { get; set; }

        public override string ToString()
        {
            return $"ID: {tc_id} - Nombre: {tc_nombre}";
        }
    }

    public class DBTiposComprobantes : DBInterface, IDBDataType<DBTiposComprobantes>
    {
        private static readonly string db_table = "tipos_comprobantes";
        private TiposComprobantesData _data;
        private static readonly List<DBTiposComprobantes> _db_tipos_comprobantes = new List<DBTiposComprobantes>();

        public static string GetDBTableName() => db_table;

        public static bool TipoComprobanteExists(string tipoComprobanteNombre, List<DBTiposComprobantes> listaTiposComprobantes)
        {
            foreach (DBTiposComprobantes tipoComprobante in listaTiposComprobantes)
            {
                if (tipoComprobanteNombre.Trim().ToLower().Equals(tipoComprobante.GetName().Trim().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TipoComprobanteExists(string tipoComprobanteNombre)
        {
            return TipoComprobanteExists(tipoComprobanteNombre, _db_tipos_comprobantes);
        }

        public static List<DBTiposComprobantes> UpdateAll(MySqlConnection conn)
        {
            List<DBTiposComprobantes> returnList = new List<DBTiposComprobantes>();
            try
            {
                string query = $"SELECT * FROM {db_table}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_tipos_comprobantes.Clear();

                while (reader.Read())
                {
                    _db_tipos_comprobantes.Add(new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre")));
                    returnList.Add(new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"))); //Waste of performance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los tipos de comprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBTiposComprobantes> GetAll()
        {
            List<DBTiposComprobantes> returnList = new List<DBTiposComprobantes>();
            foreach (DBTiposComprobantes tiposComprobantes in _db_tipos_comprobantes)
            {
                returnList.Add(tiposComprobantes);
            }
            return returnList;
        }


        /***************************
         * START: IDBDataType Implementation contract
         * *****************************/

        List<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.UpdateAll(MySqlConnection conn)
        {
            return DBTiposComprobantes.UpdateAll(conn);
        }

        List<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.GetAll()
        {
            return DBTiposComprobantes.GetAll();
        }


        /***************************
         * END: IDBDataType Implementation contract
         * *****************************/

        public static DBTiposComprobantes GetByID(long tc_id, bool clone = true)
        {
            foreach (DBTiposComprobantes tipoComprobante in _db_tipos_comprobantes)
            {
                if (tipoComprobante.GetID() == tc_id)
                {
                    return clone ? tipoComprobante.Clone() : tipoComprobante;
                }
            }
            return null;
        }

        public static DBTiposComprobantes GetByID(long tc_id, MySqlConnection conn)
        {
            DBTiposComprobantes returnEnt = null;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE tc_id = {tc_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBTiposComprobantes(TiposComprobantesData newData)
        {
            _data = newData;
        }

        public DBTiposComprobantes(long id, string nombre) : this(new TiposComprobantesData(id, nombre)) { }

        public DBTiposComprobantes(string nombre) : this(-1, nombre) { }

        public DBTiposComprobantes(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE tc_id = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = new TiposComprobantesData(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBTiposComprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE tc_id = {_data.tc_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //if exists already, just update
                if (recordsCount > 0)
                {
                    query = $"UPDATE {db_table} SET tc_nombre = '{_data.tc_nombre}' WHERE tc_id = {_data.tc_id}";

                }
                else //if does not exists, insert into
                {
                    query = $"INSERT INTO {db_table} (tc_nombre) VALUES ('{_data.tc_nombre}')";
                }
                //if not, add
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (recordsCount <= 0) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new TiposComprobantesData(cmd.LastInsertedId, _data.tc_nombre);
                }
                wasAbleToPush = true;
            }
            catch (Exception ex)
            {
                wasAbleToPush = false;
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBTiposComprobantes: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPush;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE tc_id = {_data.tc_id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTiposComprobantes: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }

        public TiposComprobantesData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public long GetID() => _data.tc_id;

        public void SetName(string newName) => _data.tc_nombre = newName;

        public string GetName() => _data.tc_nombre;

        public DBTiposComprobantes Clone()
        {
            return new DBTiposComprobantes(_data.tc_id, _data.tc_nombre);
        }

        public override string ToString()
        {
            return _data.ToString();
        }
    }
}
