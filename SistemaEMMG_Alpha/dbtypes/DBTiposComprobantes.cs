using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;


namespace SistemaEMMG_Alpha
{
    public enum TipoComprobanteFlag
    {
        Gravado = 1 << 0,
        IVA = 1 << 1,
        NoGravado = 1 << 2,
        Percepcion = 1 << 3,
        Total = 1 << 4,
        Acredita = 1 << 5,
        Asociado = 1 << 6 //asociado a otro comprobante o no.
    }
    public struct TiposComprobantesData
    {
        public TiposComprobantesData(string nom, int bitflags)
        {
            tc_nombre = nom;
            tc_bitflags = bitflags;
        }
        public string tc_nombre { get; set; }

        public int tc_bitflags { get; set; }

        public static readonly string NameOf_tc_nombre = nameof(tc_nombre);

        public static readonly string NameOf_tc_bitflags = nameof(tc_bitflags);

        public void ResetFlags()
        {
            tc_bitflags = 0;
        }

        public void SetAllFlags()
        {
            tc_bitflags = 0;
            foreach (int flag in Enum.GetValues(typeof(TipoComprobanteFlag)))
            {
                tc_bitflags |= flag;
            }
        }

        public void SetFlag(TipoComprobanteFlag flag)
        {
            tc_bitflags |= (int)flag;
        }

        public void UnsetFlag(TipoComprobanteFlag flag)
        {
            tc_bitflags &= ~(int)flag;
        }

        public bool HasFlag(TipoComprobanteFlag flag)
        {
            return (tc_bitflags & (int)flag) == (int)flag;
        }
        public static TiposComprobantesData CreateFromReader(MySqlDataReader reader) => new TiposComprobantesData(reader.GetStringSafe(NameOf_tc_nombre), reader.GetInt32Safe(NameOf_tc_bitflags));

        public override string ToString() => $"Tipo: {tc_nombre}";
    }

    public class DBTiposComprobantes : DBBaseClass, IDBase<DBTiposComprobantes>, IDBDataType<DBTiposComprobantes>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_comprobantes";
        public const string NameOf_id = "tc_id";
        private TiposComprobantesData _data;
        private static readonly List<DBTiposComprobantes> _db_tipos_comprobantes = new List<DBTiposComprobantes>();
        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet) => $"SELECT {fieldsToGet} FROM {db_table}";
        string IDBase<DBTiposComprobantes>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBTiposComprobantes> UpdateAll(MySqlConnection conn)
        {
            List<DBTiposComprobantes> returnList = new List<DBTiposComprobantes>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_tipos_comprobantes.Clear();

                while (reader.Read())
                {
                    DBTiposComprobantes tipoComprobante = new DBTiposComprobantes(reader);
                    _db_tipos_comprobantes.Add(tipoComprobante);
                    returnList.Add(tipoComprobante);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los tipos de comprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        List<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBTiposComprobantes> GetAll() => new List<DBTiposComprobantes>(_db_tipos_comprobantes);

        List<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTiposComprobantes> GetAllLocal() => _db_tipos_comprobantes.Where(x => x.IsLocal()).ToList().AsReadOnly();
        IReadOnlyCollection<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.GetAllLocal() => GetAllLocal();

        public static DBTiposComprobantes GetByID(long tc_id) => _db_tipos_comprobantes.Find(x => x.GetID() == tc_id);

        public static DBTiposComprobantes GetByID(long tc_id, MySqlConnection conn)
        {
            DBTiposComprobantes returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {tc_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBTiposComprobantes(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBTiposComprobantes(long id, TiposComprobantesData newData) : base(id) { _data = newData; }

        public DBTiposComprobantes(TiposComprobantesData newData) : this(-1, newData) { }
        public DBTiposComprobantes(long id, string nombre, int bitflags) : this(id, new TiposComprobantesData(nombre, bitflags)) { }

        public DBTiposComprobantes(string nombre, int bitflags) : this(-1, nombre, bitflags) { }

        public DBTiposComprobantes(MySqlConnection conn, long id) : base (id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = TiposComprobantesData.CreateFromReader(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MakeLocal();
                MessageBox.Show("Error en el constructor de DBTiposComprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBTiposComprobantes(MySqlDataReader reader) : this (reader.GetInt64Safe(NameOf_id), TiposComprobantesData.CreateFromReader(reader)) { }

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
                    _data = TiposComprobantesData.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();
            } catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBTiposComprobantes::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET {TiposComprobantesData.NameOf_tc_nombre} = '{Regex.Replace(_data.tc_nombre.Trim(), @"\s+", " ")}', {TiposComprobantesData.NameOf_tc_bitflags} = {_data.tc_bitflags} WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
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
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TiposComprobantesData.NameOf_tc_nombre}, {TiposComprobantesData.NameOf_tc_bitflags}) VALUES ('{Regex.Replace(_data.tc_nombre.Trim(), @"\s+", " ")}', {_data.tc_bitflags})";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_tipos_comprobantes.Contains(this))
                    {
                        _db_tipos_comprobantes.Add(this);
                    }
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
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
                    _db_tipos_comprobantes.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error en DBTiposComprobantes::DeleteFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }
        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND UPPER({TiposComprobantesData.NameOf_tc_nombre}) = '{Regex.Replace(_data.tc_nombre.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBTiposComprobantes::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBTiposComprobantes::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.tc_nombre.Equals(newName);
            _data.tc_nombre = newName;
        }

        public string GetName() => _data.tc_nombre;

        public void SetFlag(TipoComprobanteFlag flag)
        {
            _shouldPush = _shouldPush || !_data.HasFlag(flag);
            _data.SetFlag(flag);
        }
        public void UnsetFlag(TipoComprobanteFlag flag)
        {
            _shouldPush = _shouldPush || _data.HasFlag(flag);
            _data.UnsetFlag(flag);
        }
        public bool HasFlag(TipoComprobanteFlag flag) => _data.HasFlag(flag);
        public void ResetFlags()
        {
            _shouldPush = _shouldPush || (_data.tc_bitflags != 0);
            _data.ResetFlags();
        }
        public void SetAllFlags()
        {
            _shouldPush = true;
            _data.SetAllFlags();
        }
        public void SetFlags(int flags)
        {
            _shouldPush = _shouldPush || (_data.tc_bitflags != flags);
            _data.tc_bitflags = flags;
        }
        public int GetFlags() => _data.tc_bitflags;


        public override DBBaseClass GetLocalCopy() => new DBTiposComprobantes(-1, _data);

        public override string ToString() => $"ID: {GetID()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBTiposComprobantes tipoComprobante in _db_tipos_comprobantes)
            {
                str += $"Tipo de Comprobante> {tipoComprobante}\n";
            }
            return str;
        }
        public static DBTiposComprobantes GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_tipos_comprobantes[r.Next(0, _db_tipos_comprobantes.Count)];
        }
    }
}