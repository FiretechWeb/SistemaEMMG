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
        public TiposEntidadesData(string nom)
        {
            te_nombre = nom; //Event handler to check that nom is not longer than 32 characters
        }
        public string te_nombre { get; set; }

        public static readonly string NameOf_te_nombre = nameof(te_nombre);

        public override string ToString()
        {
            return $"Tipo Entidad: {te_nombre}";
        }
    }

    public class DBTipoEntidad : DBBaseClass, IDBDataType<DBTipoEntidad>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_entidades";
        public const string NameOf_id = "te_id";
        private long _id;
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
                    _db_tipos_entidades.Add(new DBTipoEntidad(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre)));
                    returnList.Add(new DBTipoEntidad(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre))); //Waste of persformance but helps with making the code less propense to error.
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {te_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnTipoEntidad = new DBTipoEntidad(reader.GetInt64Safe(NameOf_id), reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnTipoEntidad;
        }

        public DBTipoEntidad(long id, TiposEntidadesData newData)
        {
            _id = id;
            _data = newData;
        }

        public DBTipoEntidad(long id, string nombre) : this(id, new TiposEntidadesData(nombre)) { }

        public DBTipoEntidad(string nombre) : this(-1, nombre) { }

        public DBTipoEntidad(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _id = id;
                    _data = new TiposEntidadesData(reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructo de DBTipoEntidad, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET te_nombre = '{_data.te_nombre}' WHERE {NameOf_id} = {GetID()}";
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
                MessageBox.Show("Error en DBTipoEntidad::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TiposEntidadesData.NameOf_te_nombre}) VALUES ('{_data.te_nombre}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBTipoEntidad::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTipoEntidad: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBTipoEntidad::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
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

        public override long GetID() => _id;

        protected override void ChangeID(long id) => _id = id;

        public override string ToString()
        {
            return _data.ToString();
        }

        public DBTipoEntidad Clone()
        {
            return new DBTipoEntidad(_id, _data.te_nombre);
        }
    }
}
