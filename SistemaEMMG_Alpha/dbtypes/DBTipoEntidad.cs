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

        public static TiposEntidadesData CreateFromReader(MySqlDataReader reader)
        {
            return new TiposEntidadesData(reader.GetStringSafe(NameOf_te_nombre));
        }

        public override string ToString()
        {
            return $"Tipo: {te_nombre}";
        }
    }

    public class DBTipoEntidad : DBBaseClass, IDBase<DBTipoEntidad>, IDBDataType<DBTipoEntidad>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_entidades";
        public const string NameOf_id = "te_id";
        private long _id;
        private bool _shouldPush = false;
        private TiposEntidadesData _data;
        private static readonly List<DBTipoEntidad> _db_tipos_entidades = new List<DBTipoEntidad>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
        string IDBase<DBTipoEntidad>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

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
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_tipos_entidades.Clear();

                while (reader.Read())
                {
                    DBTipoEntidad tipoEntidad = new DBTipoEntidad(reader);
                    _db_tipos_entidades.Add(tipoEntidad);
                    returnList.Add(tipoEntidad);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas los tipos de entidades, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        List<DBTipoEntidad> IDBDataType<DBTipoEntidad>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBTipoEntidad> GetAll()
        {
            List<DBTipoEntidad> returnList = new List<DBTipoEntidad>();
            foreach (DBTipoEntidad tipoEntidad in _db_tipos_entidades)
            {
                returnList.Add(tipoEntidad);
            }
            return returnList;
        }

        List<DBTipoEntidad> IDBDataType<DBTipoEntidad>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTipoEntidad> GetAllLocal()
        {
            return _db_tipos_entidades.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBTipoEntidad> IDBDataType<DBTipoEntidad>.GetAllLocal() => GetAllLocal();

        public static DBTipoEntidad GetByID(long te_id)
        {
            foreach (DBTipoEntidad tmpTipo in _db_tipos_entidades)
            {
                if (tmpTipo.GetID() == te_id)
                {
                    return tmpTipo;
                }
            }
            return null;
        }

        public static DBTipoEntidad GetByID(long te_id, MySqlConnection conn)
        {
            DBTipoEntidad returnTipoEntidad = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {te_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnTipoEntidad = new DBTipoEntidad(reader);
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

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBTipoEntidad(TiposEntidadesData newData) : this(-1, newData) { }
        public DBTipoEntidad(long id, string nombre) : this(id, new TiposEntidadesData(nombre)) { }

        public DBTipoEntidad(string nombre) : this(-1, nombre) { }

        public DBTipoEntidad(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _id = id;
                    _data = TiposEntidadesData.CreateFromReader(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructo de DBTipoEntidad, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBTipoEntidad(MySqlDataReader reader) : this (reader.GetInt64Safe(NameOf_id), TiposEntidadesData.CreateFromReader(reader)) { }

        public override bool PullFromDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }

            bool wasAbleToPull = false;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = TiposEntidadesData.CreateFromReader(reader);
                    _shouldPush = false;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBTipoEntidad::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPull;
        }
        public override bool PushToDatabase(MySqlConnection conn)
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

        public override bool UpdateToDatabase(MySqlConnection conn)
        {
            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET te_nombre = '{_data.te_nombre}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
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
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
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
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                }
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
        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.te_nombre.Equals(newName);
            _data.te_nombre = newName;
        }

        public string GetName() => _data.te_nombre;

        public override long GetID() => _id;

        protected override void ChangeID(long id)
        {
            _shouldPush = _shouldPush || (_id != id);
            _id = id;
        }
        public override bool ShouldPush() => _shouldPush;
        public override bool IsLocal() => _id < 0;

        protected override void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }
        public override string ToString()
        {
            return $"ID: {GetID()} - {_data.ToString()}";
        }

        public override DBBaseClass GetLocalCopy()
        {
            return new DBTipoEntidad(-1, _data);
        }

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBTipoEntidad tipoEntidad in _db_tipos_entidades)
            {
                str += $"Tipo de Entidad> {tipoEntidad}\n";
            }
            return str;
        }
        ~DBTipoEntidad()
        {
        }
    }
}
