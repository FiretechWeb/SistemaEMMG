using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

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
        private TiposEntidadesData _data;
        private static readonly List<DBTipoEntidad> _db_tipos_entidades = new List<DBTipoEntidad>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
        string IDBase<DBTipoEntidad>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBTipoEntidad> GenerateDefaultData()
        {
            List<DBTipoEntidad> defaulData = new List<DBTipoEntidad>();
            defaulData.Add(new DBTipoEntidad("Cliente"));
            defaulData.Add(new DBTipoEntidad("Proovedor"));

            return defaulData;
        }
        List<DBTipoEntidad> IDBDataType<DBTipoEntidad>.GenerateDefaultData() => GenerateDefaultData();

        public static bool PushDefaultData(MySqlConnection conn)
        {
            if ((User.GetCurrentUser() is null) || !User.GetCurrentUser().IsAdmin()) //Security measures.
            {
                return false;
            }

            List<DBTipoEntidad> defaultData = GenerateDefaultData();
            foreach (DBTipoEntidad tipoEntidad in defaultData)
            {
                tipoEntidad.PushToDatabase(conn);
            }

            return true;
        }

        bool IDBDataType<DBTipoEntidad>.PushDefaultData(MySqlConnection conn) => PushDefaultData(conn);

        public static bool ResetDBData(MySqlConnection conn)
        {
            if ((User.GetCurrentUser() is null) || !User.GetCurrentUser().IsAdmin()) //Security measures.
            {
                return false;
            }
            bool resetDataSuccess;
            try
            {
                string query = $"DELETE FROM {db_table}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                resetDataSuccess = true;
            }
            catch (Exception ex)
            {
                resetDataSuccess = false;
                MessageBox.Show("Error en el método DBTipoEntidad::ResetDBData. Error en la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return resetDataSuccess;
        }

        bool IDBDataType<DBTipoEntidad>.ResetDBData(MySqlConnection conn) => ResetDBData(conn);

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

        public static List<DBTipoEntidad> GetAll() => new List<DBTipoEntidad>(_db_tipos_entidades);

        List<DBTipoEntidad> IDBDataType<DBTipoEntidad>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTipoEntidad> GetAllLocal()
        {
            return _db_tipos_entidades.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBTipoEntidad> IDBDataType<DBTipoEntidad>.GetAllLocal() => GetAllLocal();

        public static DBTipoEntidad GetByID(long te_id) => _db_tipos_entidades.Find(x => x.GetID() == te_id);

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

        public static DBTipoEntidad GetByName(string te_nombre) => _db_tipos_entidades.Find(x => x.GetName().DeepNormalize().Equals(te_nombre.DeepNormalize()));

        public static DBTipoEntidad GetByName(string te_nombre, MySqlConnection conn)
        {
            DBTipoEntidad returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE LOWER({TiposEntidadesData.NameOf_te_nombre}) = '{te_nombre.Trim().ToLower()}'";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBTipoEntidad(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByName. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBTipoEntidad(long id, TiposEntidadesData newData) : base (id) { _data = newData; }

        public DBTipoEntidad(TiposEntidadesData newData) : this(-1, newData) { }
        public DBTipoEntidad(long id, string nombre) : this(id, new TiposEntidadesData(nombre)) { }

        public DBTipoEntidad(string nombre) : this(-1, nombre) { }

        public DBTipoEntidad(MySqlConnection conn, long id) : base (id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _data = TiposEntidadesData.CreateFromReader(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MakeLocal();
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
                    wasAbleToPull = true;
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

        public override bool UpdateToDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }

            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }

            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET te_nombre = '{Regex.Replace(_data.te_nombre.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()}";
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
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TiposEntidadesData.NameOf_te_nombre}) VALUES ('{Regex.Replace(_data.te_nombre.Trim(), @"\s+", " ")}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_tipos_entidades.Contains(this))
                    {
                        _db_tipos_entidades.Add(this);
                    }
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
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    _db_tipos_entidades.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTipoEntidad: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND UPPER({TiposEntidadesData.NameOf_te_nombre}) = '{Regex.Replace(_data.te_nombre.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBTipoEntidad::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                duplicatedExistsInDB = null;
            }
            return duplicatedExistsInDB;
        }

        public override bool? ExistsInDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
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

        public override string ToString()
        {
            return $"ID: {GetID()} - {_data.ToString()}";
        }

        //Shorthands (HARDCODED - Not ideal) FIXME
        public bool isProveedor()
        {
            return GetName().ToLower().Equals("proveedor") || GetName().ToLower().Equals("proveedores") || GetName().ToLower().Equals("supplier") || GetName().ToLower().Equals("suppliers");
        }

        public override DBBaseClass GetLocalCopy()
        {
            return new DBTipoEntidad(-1, _data);
        }

        public override void Update(DBBaseClass source)
        {
            if (!(source is DBTipoEntidad)) return;
            DBTipoEntidad sourceTipoEntidad = (DBTipoEntidad)source;
            SetName(sourceTipoEntidad.GetName());
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
        public static DBTipoEntidad GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_tipos_entidades[r.Next(0, _db_tipos_entidades.Count)];
        }
        ~DBTipoEntidad()
        {
        }
    }
}
