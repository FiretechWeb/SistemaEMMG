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
    public struct TiposEntidadesData
    {
        public TiposEntidadesData(long id, string nom)
        {
            te_id = id;
            te_nombre = nom; //Event handler to check that nom is not longer than 32 characters
        }
        public long te_id { get; set; }
        public string te_nombre { get; set; }

        public override string ToString()
        {
            return $"ID: {te_id} - Tipo Entidad: {te_nombre}";
        }
    }

    public class DBTipoEntidad : DBInterface, IDBDataType<DBTipoEntidad>
    {
        private static readonly string db_table = "tipos_entidades";
        private TiposEntidadesData _data;
        private static readonly List<DBTipoEntidad> _db_tipos_entidades = new List<DBTipoEntidad>();

        public static bool TipoEntidadYaExiste(string tipoEntidadNombre, List<DBTipoEntidad> tiposEntidades)
        {
            foreach (DBTipoEntidad tipoEntidad in tiposEntidades)
            {
                if (tipoEntidadNombre.Trim().ToLower().Equals(tipoEntidad.GetName().Trim().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<DBTipoEntidad> UpdateAll(MySqlConnection conn)
        {
            List<DBTipoEntidad> returnList = new List<DBTipoEntidad>();
            try
            {
                string query = $"SELECT * FROM {db_table}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_tipos_entidades.Clear();

                while (reader.Read())
                {
                    _db_tipos_entidades.Add(new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre")));
                    returnList.Add(new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas los tipos de entidades, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBTipoEntidad> GetAll()
        {
            List<DBTipoEntidad> returnList = new List<DBTipoEntidad>();
            foreach (DBTipoEntidad tipoEntidad in _db_tipos_entidades)
            {
                returnList.Add(new DBTipoEntidad(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }
            return returnList;
        }


        /***************************
         * START: IDBDataType Implementation contract
         * *****************************/

        List<DBTipoEntidad> IDBDataType<DBTipoEntidad>.GetAll()
        {
            return DBTipoEntidad.GetAll();
        }

        List<DBTipoEntidad> IDBDataType<DBTipoEntidad>.UpdateAll(MySqlConnection conn)
        {
            return DBTipoEntidad.UpdateAll(conn);
        }

        /***************************
         * END: IDBDataType Implementation contract
         * *****************************/

        public static DBTipoEntidad GetByID(long te_id, bool clone = true)
        {
            foreach (DBTipoEntidad tmpTipo in _db_tipos_entidades)
            {
                if (tmpTipo.GetID() == te_id)
                {
                    return clone ? tmpTipo.Clone() : tmpTipo;
                }
            }
            return null;
        }

        public static DBTipoEntidad GetByID(long te_id, MySqlConnection conn)
        {
            DBTipoEntidad returnTipoEntidad = null;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE te_id = {te_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnTipoEntidad = new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnTipoEntidad;
        }

        public DBTipoEntidad(TiposEntidadesData newData)
        {
            _data = newData;
        }

        public DBTipoEntidad(long id, string nombre) : this(new TiposEntidadesData(id, nombre)) { }

        public DBTipoEntidad(string nombre) : this(-1, nombre) { }

        public DBTipoEntidad(MySqlConnection conn, int id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE te_id = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _data = new TiposEntidadesData(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructo de DBTipoEntidad, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE te_id = {_data.te_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //if exists already, just update
                if (recordsCount > 0)
                {
                    query = $"UPDATE {db_table} SET te_nombre = '{_data.te_nombre}' WHERE te_id = {_data.te_id}";

                }
                else //if does not exists, insert into
                {
                    query = $"INSERT INTO {db_table} (te_nombre) VALUES ('{_data.te_nombre}')";
                }
                //if not, add
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (recordsCount <= 0) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new TiposEntidadesData(cmd.LastInsertedId, _data.te_nombre);
                }
                wasAbleToPush = true;
            }
            catch (Exception ex)
            {
                wasAbleToPush = false;
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBTipoEntidad: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPush;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE te_id = {_data.te_id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTipoEntidad: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public TiposEntidadesData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public void SetName(string newName)
        {
            _data.te_nombre = newName;
        }

        public string GetName() => _data.te_nombre;

        public long GetID() => _data.te_id;

        public override string ToString()
        {
            return _data.ToString();
        }

        public DBTipoEntidad Clone()
        {
            return new DBTipoEntidad(_data.te_id, _data.te_nombre);
        }
    }
}
