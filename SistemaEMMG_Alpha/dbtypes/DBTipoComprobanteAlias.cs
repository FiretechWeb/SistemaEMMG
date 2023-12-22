using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct TipoComprobanteAlias
    {
        public TipoComprobanteAlias(string alias)
        {
            afip_nombre = alias;
        }
        public string afip_nombre { get; set; }

        public static readonly string NameOf_afip_nombre = nameof(afip_nombre);

        public static TipoComprobanteAlias CreateFromReader(MySqlDataReader reader) =>
            new TipoComprobanteAlias(reader.GetStringSafe(NameOf_afip_nombre));

        public override string ToString() => $"Alias: {afip_nombre}";

    }
    public class DBTipoComprobanteAlias : DBBaseClass, IDBase<DBTipoComprobanteAlias>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "afip_tipos_comprobantes";
        public const string NameOf_id = "afip_id";
        public const string NameOf_tc_id = "afip_tc_id";
        private TipoComprobanteAlias _data;
        private DBTiposComprobantes _tipoComprobante = null;
        private static readonly List<DBTipoComprobanteAlias> _db_afip_tipos_alias = new List<DBTipoComprobanteAlias>();
        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            string tc_table = DBTiposComprobantes.db_table;
            return $@"SELECT {fieldsToGet} FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_tc_id}";
        }
        string IDBase<DBTipoComprobanteAlias>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

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
                MessageBox.Show("Error en el método DBTipoComprobanteAlias::ResetDBData. Error en la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return resetDataSuccess;
        }

        public static List<DBTipoComprobanteAlias> UpdateAll(MySqlConnection conn)
        {
            List<DBTipoComprobanteAlias> returnList = new List<DBTipoComprobanteAlias>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_afip_tipos_alias.Clear();

                while (reader.Read())
                {
                    DBTipoComprobanteAlias tipoComprobante = new DBTipoComprobanteAlias(reader);
                    _db_afip_tipos_alias.Add(tipoComprobante);
                    returnList.Add(tipoComprobante);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los aliases de comprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        public static List<DBTipoComprobanteAlias> GetAll() => new List<DBTipoComprobanteAlias>(_db_afip_tipos_alias);

        public static IReadOnlyCollection<DBTipoComprobanteAlias> GetAllLocal() => _db_afip_tipos_alias.Where(x => x.IsLocal()).ToList().AsReadOnly();
        public static DBTipoComprobanteAlias GetByID(long alias_id, long tc_id) => _db_afip_tipos_alias.Find(x => x.GetID() == alias_id && x.GetTipoComprobanteID() == tc_id);

        public static DBTipoComprobanteAlias GetByID(long alias_id, long tc_id, MySqlConnection conn)
        {
            DBTipoComprobanteAlias returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {alias_id} AND {NameOf_tc_id} = {tc_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBTipoComprobanteAlias(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un alias de comprobante en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public static DBTipoComprobanteAlias GetByAlias(string alias) => _db_afip_tipos_alias.Find(x => x.GetAlias().DeepNormalize().Equals(alias.DeepNormalize()));

        public static DBTipoComprobanteAlias GetByAlias(string alias, MySqlConnection conn, bool normalized=false)
        {
            DBTipoComprobanteAlias returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")}";
                if (!normalized)
                {
                    query += $" WHERE LOWER({TipoComprobanteAlias.NameOf_afip_nombre}) = '{alias.Trim().ToLower()}'";
                }
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTipoComprobanteAlias currentAlias = new DBTipoComprobanteAlias(reader);
                    if (!normalized)
                    {
                        returnEnt = currentAlias;
                        break;
                    } else
                    {
                        if (currentAlias.GetAlias().DeepNormalize().Equals(alias.DeepNormalize()))
                        {
                            returnEnt = currentAlias;
                            break;
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un alias de comprobante en GetByAlias. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBTipoComprobanteAlias(long id, DBTiposComprobantes tipoComprobante, TipoComprobanteAlias newData) : base(id) {
            _data = newData;
            _tipoComprobante = tipoComprobante;
        }

        public DBTipoComprobanteAlias(DBTiposComprobantes tipoComprobante, TipoComprobanteAlias newData) : this(-1, tipoComprobante, newData) { }
        public DBTipoComprobanteAlias(long id, DBTiposComprobantes tipoComprobante, string nombre) : this(id, tipoComprobante, new TipoComprobanteAlias(nombre)) { }

        public DBTipoComprobanteAlias(DBTiposComprobantes tipoComprobante, string nombre) : this(-1, tipoComprobante, nombre) { }

        public DBTipoComprobanteAlias(MySqlConnection conn, long id, long tc_id) : base(id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id} AND {NameOf_tc_id} = {tc_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = TipoComprobanteAlias.CreateFromReader(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MakeLocal();
                MessageBox.Show("Error en el constructor de DBTipoComprobanteAlias. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public DBTipoComprobanteAlias(MySqlDataReader reader) : this(reader.GetInt64Safe(NameOf_id), new DBTiposComprobantes(reader), TipoComprobanteAlias.CreateFromReader(reader)) { }

        public override bool PullFromDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }

            bool wasAbleToPull = false;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {GetID()} AND {NameOf_tc_id} = {GetTipoComprobanteID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = TipoComprobanteAlias.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBTipoComprobanteAlias::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPull;
        }

        public override bool UpdateToDatabase(MySqlConnection conn)
        {
            if (IsLocal() || GetTipoComprobante().IsLocal())
            {
                return false;
            }

            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }

            bool wasAbleToUpdate;

            try
            {
                string query = $"UPDATE {db_table} SET {TipoComprobanteAlias.NameOf_afip_nombre} = '{Regex.Replace(_data.afip_nombre.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()} AND {NameOf_tc_id} = {GetTipoComprobanteID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBTipoComprobanteAlias::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            if (GetTipoComprobante().IsLocal()) return false;

            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            bool wasAbleToInsert;
            try
            {
                string query = $"INSERT INTO {db_table} ({NameOf_tc_id}, {TipoComprobanteAlias.NameOf_afip_nombre}) VALUES ({GetTipoComprobanteID()}, '{Regex.Replace(_data.afip_nombre.Trim(), @"\s+", " ")}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_afip_tipos_alias.Contains(this))
                    {
                        _db_afip_tipos_alias.Add(this);
                    }
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBTipoComprobanteAlias::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_table} WHERE {NameOf_id} = {GetID()} AND {NameOf_tc_id} = {GetTipoComprobanteID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    _db_afip_tipos_alias.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error en DBTipoComprobanteAlias::DeleteFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }
        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_tc_id} = {GetTipoComprobanteID()} AND {NameOf_id} <> {GetID()} AND UPPER({TipoComprobanteAlias.NameOf_afip_nombre}) = '{Regex.Replace(_data.afip_nombre.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBTipoComprobanteAlias::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            bool? existsInDB;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} = {GetID()} AND {NameOf_tc_id} = {GetTipoComprobanteID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBTipoComprobanteAlias::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public DBTiposComprobantes GetTipoComprobante() => _tipoComprobante;

        public string GetAlias() => _data.afip_nombre;

        public long GetTipoComprobanteID() => _tipoComprobante.GetID();

        public void SetTipoComprobante(DBTiposComprobantes tipoComprobante)
        {
            _shouldPush = _shouldPush || (_tipoComprobante != tipoComprobante);
            _tipoComprobante = tipoComprobante;
        }

        public void SetAlias(string newAlias)
        {
            _shouldPush = _shouldPush || !newAlias.Trim().ToLower().Equals(GetAlias().Trim().ToLower());
            _data.afip_nombre = newAlias;
        }

        public override DBBaseClass GetLocalCopy() => new DBTipoComprobanteAlias(-1, GetTipoComprobante(), _data);

        public override void Update(DBBaseClass source)
        {
            if (!(source is DBTipoComprobanteAlias)) return;
            DBTipoComprobanteAlias sourceAliasTipoComp = (DBTipoComprobanteAlias)source;
            SetAlias(sourceAliasTipoComp.GetAlias());
            SetTipoComprobante(sourceAliasTipoComp.GetTipoComprobante());
        }

        public override string ToString() => $"ID: {GetID()} - {_data}";

    }
}
