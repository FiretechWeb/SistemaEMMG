using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;


namespace SistemaEMMG_Alpha
{
    public struct MonedaData
    {
        public MonedaData(string nom, bool extranjera)
        {
            mn_name = nom;
            mn_extranjera = extranjera;
        }
        public string mn_name { get; set; }
        public bool mn_extranjera { get; set; }

        public static readonly string NameOf_mn_name = nameof(mn_name);
        public static readonly string NameOf_mn_extranjera = nameof(mn_extranjera);

        public static MonedaData CreateFromReader(MySqlDataReader reader) => new MonedaData(reader.GetStringSafe(NameOf_mn_name), Convert.ToBoolean(reader.GetInt32Safe(NameOf_mn_extranjera)));

        public override string ToString() => $"Moneda: {mn_name} - Extranjera: {mn_extranjera}";
    }

    public class DBMoneda : DBBaseClass, IDBase<DBMoneda>, IDBDataType<DBMoneda>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "monedas";
        public const string NameOf_id = "mn_id";
        private MonedaData _data;
        private static readonly List<DBMoneda> _db_monedas = new List<DBMoneda>();
        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet) => $"SELECT {fieldsToGet} FROM {db_table}";
        string IDBase<DBMoneda>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBMoneda> UpdateAll(MySqlConnection conn)
        {
            List<DBMoneda> returnList = new List<DBMoneda>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_monedas.Clear();

                while (reader.Read())
                {
                    DBMoneda tipoMoneda = new DBMoneda(reader);
                    _db_monedas.Add(tipoMoneda);
                    returnList.Add(tipoMoneda);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los tipos de monedas. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        List<DBMoneda> IDBDataType<DBMoneda>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBMoneda> GetAll() => new List<DBMoneda>(_db_monedas);

        List<DBMoneda> IDBDataType<DBMoneda>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBMoneda> GetAllLocal() => _db_monedas.Where(x => x.IsLocal()).ToList().AsReadOnly();
        IReadOnlyCollection<DBMoneda> IDBDataType<DBMoneda>.GetAllLocal() => GetAllLocal();

        public static DBMoneda GetByID(long mn_id) => _db_monedas.Find(x => x.GetID() == mn_id);

        public static DBMoneda GetByID(long mn_id, MySqlConnection conn)
        {
            DBMoneda returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {mn_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBMoneda(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBMoneda(long id, MonedaData newData) : base(id) { _data = newData; }

        public DBMoneda(MonedaData newData) : this(-1, newData) { }
        public DBMoneda(long id, string nombre, bool extranjera) : this(id, new MonedaData(nombre, extranjera)) { }

        public DBMoneda(string nombre, bool extranjera) : this(-1, nombre, extranjera) { }

        public DBMoneda(MySqlConnection conn, long id) : base(id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = MonedaData.CreateFromReader(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MakeLocal();
                MessageBox.Show("Error en el constructor de DBMoneda. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBMoneda(MySqlDataReader reader) : this(reader.GetInt64Safe(NameOf_id), MonedaData.CreateFromReader(reader)) { }

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
                    _data = MonedaData.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBMoneda::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET {MonedaData.NameOf_mn_extranjera} = {Convert.ToInt32(_data.mn_extranjera)}, {MonedaData.NameOf_mn_name} = '{Regex.Replace(_data.mn_name.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBMoneda::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"INSERT INTO {db_table} ({MonedaData.NameOf_mn_name}, {MonedaData.NameOf_mn_extranjera}) VALUES ('{Regex.Replace(_data.mn_name.Trim(), @"\s+", " ")}', {Convert.ToInt32(_data.mn_extranjera)})";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_monedas.Contains(this))
                    {
                        _db_monedas.Add(this);
                    }
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBMoneda::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    _db_monedas.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error en DBMoneda::DeleteFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }
        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND UPPER({MonedaData.NameOf_mn_name}) = '{Regex.Replace(_data.mn_name.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBMoneda::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBMoneda::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.mn_name.Equals(newName);
            _data.mn_name = newName;
        }

        public void SetExtranjera(bool isExtranjera)
        {
            _shouldPush = _shouldPush || (_data.mn_extranjera != isExtranjera);
            _data.mn_extranjera = isExtranjera;
        }

        public string GetName() => _data.mn_name;

        public bool IsExtranjera() => _data.mn_extranjera;

        public override DBBaseClass GetLocalCopy() => new DBMoneda(-1, _data);

        public override string ToString() => $"ID: {GetID()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBMoneda moneda in _db_monedas)
            {
                str += $"Moneda> {moneda}\n";
            }
            return str;
        }
        public static DBMoneda GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_monedas[r.Next(0, _db_monedas.Count)];
        }
    }
}