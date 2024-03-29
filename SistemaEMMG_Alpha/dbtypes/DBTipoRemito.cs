﻿using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct TipoRemitoData
    {
        public TipoRemitoData(string nom)
        {
            ts_nombre = nom;
        }
        public string ts_nombre { get; set; }

        public static readonly string NameOf_ts_nombre = nameof(ts_nombre);

        public static TipoRemitoData CreateFromReader(MySqlDataReader reader) => new TipoRemitoData(reader.GetStringSafe(NameOf_ts_nombre));

        public override string ToString() => $"Tipo: {ts_nombre}";
    }
    public class DBTipoRemito : DBBaseClass, IDBase<DBTipoRemito>, IDBDataType<DBTipoRemito>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_remitos";
        public const string NameOf_id = "ts_id";
        private TipoRemitoData _data;
        private static readonly List<DBTipoRemito> _db_tipos_remitos = new List<DBTipoRemito>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet) => $"SELECT {fieldsToGet} FROM {db_table}";

        string IDBase<DBTipoRemito>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBTipoRemito> GenerateDefaultData()
        {
            List<DBTipoRemito> defaulData = new List<DBTipoRemito>();
            defaulData.Add(new DBTipoRemito("R"));

            return defaulData;
        }
        List<DBTipoRemito> IDBDataType<DBTipoRemito>.GenerateDefaultData() => GenerateDefaultData();

        public static bool PushDefaultData(MySqlConnection conn)
        {
            if ((User.GetCurrentUser() is null) || !User.GetCurrentUser().IsAdmin()) //Security measures.
            {
                return false;
            }

            List<DBTipoRemito> defaultData = GenerateDefaultData();
            foreach (DBTipoRemito tipoRemito in defaultData)
            {
                tipoRemito.PushToDatabase(conn);
            }

            return true;
        }

        bool IDBDataType<DBTipoRemito>.PushDefaultData(MySqlConnection conn) => PushDefaultData(conn);

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
                MessageBox.Show("Error en el método DBTipoRemito::ResetDBData. Error en la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return resetDataSuccess;
        }

        bool IDBDataType<DBTipoRemito>.ResetDBData(MySqlConnection conn) => ResetDBData(conn);

        public static List<DBTipoRemito> UpdateAll(MySqlConnection conn)
        {
            List<DBTipoRemito> returnList = new List<DBTipoRemito>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_tipos_remitos.Clear();

                while (reader.Read())
                {
                    DBTipoRemito tipoRemito = new DBTipoRemito(reader);
                    _db_tipos_remitos.Add(tipoRemito);
                    returnList.Add(tipoRemito);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los tipos de Remitos. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        List<DBTipoRemito> IDBDataType<DBTipoRemito>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBTipoRemito> GetAll() => new List<DBTipoRemito>(_db_tipos_remitos);

        List<DBTipoRemito> IDBDataType<DBTipoRemito>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTipoRemito> GetAllLocal() => _db_tipos_remitos.Where(x => x.IsLocal()).ToList().AsReadOnly();

        IReadOnlyCollection<DBTipoRemito> IDBDataType<DBTipoRemito>.GetAllLocal() => GetAllLocal();

        public static DBTipoRemito GetByID(long ts_id) => _db_tipos_remitos.Find(x => x.GetID() == ts_id);

        public static DBTipoRemito GetByID(long ts_id, MySqlConnection conn)
        {
            DBTipoRemito returnTipoRemito = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {ts_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnTipoRemito = new DBTipoRemito(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de Remito en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnTipoRemito;
        }

        public DBTipoRemito(long id, TipoRemitoData newData) : base(id) { _data = newData; }

        public DBTipoRemito(TipoRemitoData newData) : this(-1, newData) { }
        public DBTipoRemito(long id, string nombre) : this(id, new TipoRemitoData(nombre)) { }

        public DBTipoRemito(string nombre) : this(-1, nombre) { }

        public DBTipoRemito(MySqlConnection conn, long id) : base(id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _data = TipoRemitoData.CreateFromReader(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MakeLocal();
                MessageBox.Show("Error en el constructo de DBTipoRemito, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBTipoRemito(MySqlDataReader reader) : this(reader.GetInt64Safe(NameOf_id), TipoRemitoData.CreateFromReader(reader)) { }

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
                    _data = TipoRemitoData.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBTipoRemito::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET {TipoRemitoData.NameOf_ts_nombre} = '{Regex.Replace(_data.ts_nombre.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBTipoRemito::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"INSERT INTO {db_table} ({TipoRemitoData.NameOf_ts_nombre}) VALUES ('{Regex.Replace(_data.ts_nombre.Trim(), @"\s+", " ")}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_tipos_remitos.Contains(this))
                    {
                        _db_tipos_remitos.Add(this);
                    }
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBTipoRemito::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    _db_tipos_remitos.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTipoRemito: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND UPPER({TipoRemitoData.NameOf_ts_nombre}) = '{Regex.Replace(_data.ts_nombre.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBTipoRemito::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBTipoRemito::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.ts_nombre.Equals(newName);
            _data.ts_nombre = newName;
        }

        public string GetName() => _data.ts_nombre;

        public override string ToString() => $"ID: {GetID()} - {_data}";

        public override DBBaseClass GetLocalCopy() => new DBTipoRemito(-1, _data);

        public override void Update(DBBaseClass source)
        {
            if (!(source is DBTipoRemito)) return;
            DBTipoRemito sourceTipoRemito = (DBTipoRemito)source;
            SetName(sourceTipoRemito.GetName());
        }

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBTipoRemito tipoRemito in _db_tipos_remitos)
            {
                str += $"Tipo de Remito> {tipoRemito}\n";
            }
            return str;
        }
        public static DBTipoRemito GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_tipos_remitos[r.Next(0, _db_tipos_remitos.Count)];
        }
    }
}
