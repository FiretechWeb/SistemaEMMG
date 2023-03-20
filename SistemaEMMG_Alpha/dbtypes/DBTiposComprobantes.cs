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
        public TiposComprobantesData(string nom)
        {
            tc_nombre = nom;
        }
        public string tc_nombre { get; set; }

        public static readonly string NameOf_tc_nombre = nameof(tc_nombre);

        public override string ToString()
        {
            return $"Nombre: {tc_nombre}";
        }
    }

    public class DBTiposComprobantes : DBBaseClass, IDBDataType<DBTiposComprobantes>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_comprobantes";
        public const string NameOf_id = "tc_id";
        private long _id;
        private TiposComprobantesData _data;
        private static readonly List<DBTiposComprobantes> _db_tipos_comprobantes = new List<DBTiposComprobantes>();

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
                    _db_tipos_comprobantes.Add(new DBTiposComprobantes(reader));
                    returnList.Add(new DBTiposComprobantes(reader));
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {tc_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBTiposComprobantes(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBTiposComprobantes(long id, TiposComprobantesData newData)
        {
            _id = id;
            _data = newData;
        }

        public DBTiposComprobantes(long id, string nombre) : this(id, new TiposComprobantesData(nombre)) { }

        public DBTiposComprobantes(string nombre) : this(-1, nombre) { }

        public DBTiposComprobantes(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _id = id;
                    _data = new TiposComprobantesData(reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBTiposComprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBTiposComprobantes(MySqlDataReader reader) : this (reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre)) { }

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
                string query = $"UPDATE {db_table} SET {TiposComprobantesData.NameOf_tc_nombre} = '{_data.tc_nombre}' WHERE {NameOf_id} = {GetID()}";
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
                MessageBox.Show("Error en DBTiposComprobantes::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TiposComprobantesData.NameOf_tc_nombre}) VALUES ('{_data.tc_nombre}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBTiposComprobantes::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBTiposComprobantes::DeleteFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBTiposComprobantes::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public TiposComprobantesData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }
        protected override void ChangeID(long id) => _id = id;

        public override long GetID() => _id;

        public void SetName(string newName) => _data.tc_nombre = newName;

        public string GetName() => _data.tc_nombre;

        public DBTiposComprobantes Clone()
        {
            return new DBTiposComprobantes(_id, _data.tc_nombre);
        }

        public override string ToString()
        {
            return _data.ToString();
        }
    }
}
