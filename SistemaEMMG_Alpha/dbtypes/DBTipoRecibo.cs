using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct TipoReciboData
    {
        public TipoReciboData(string nom)
        {
            tr_nombre = nom;
        }
        public string tr_nombre { get; set; }

        public static readonly string NameOf_tr_nombre = nameof(tr_nombre);

        public static TipoReciboData CreateFromReader(MySqlDataReader reader)
        {
            return new TipoReciboData(reader.GetStringSafe(NameOf_tr_nombre));
        }

        public override string ToString()
        {
            return $"Tipo: {tr_nombre}";
        }
    }
    public class DBTipoRecibo : DBBaseClass, IDBase<DBTipoRecibo>, IDBDataType<DBTipoRecibo>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_recibos";
        public const string NameOf_id = "tr_id";
        private long _id;
        private bool _shouldPush = false;
        private TipoReciboData _data;
        private static readonly List<DBTipoRecibo> _db_tipos_recibos = new List<DBTipoRecibo>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
        string IDBase<DBTipoRecibo>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBTipoRecibo> UpdateAll(MySqlConnection conn)
        {
            List<DBTipoRecibo> returnList = new List<DBTipoRecibo>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_tipos_recibos.Clear();

                while (reader.Read())
                {
                    DBTipoRecibo tipoRecibo = new DBTipoRecibo(reader);
                    _db_tipos_recibos.Add(tipoRecibo);
                    returnList.Add(tipoRecibo);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los tipos de recibos. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        List<DBTipoRecibo> IDBDataType<DBTipoRecibo>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBTipoRecibo> GetAll()
        {
            List<DBTipoRecibo> returnList = new List<DBTipoRecibo>();
            foreach (DBTipoRecibo tipoRecibo in _db_tipos_recibos)
            {
                returnList.Add(tipoRecibo);
            }
            return returnList;
        }

        List<DBTipoRecibo> IDBDataType<DBTipoRecibo>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTipoRecibo> GetAllLocal()
        {
            return _db_tipos_recibos.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBTipoRecibo> IDBDataType<DBTipoRecibo>.GetAllLocal() => GetAllLocal();

        public static DBTipoRecibo GetByID(long te_id)
        {
            foreach (DBTipoRecibo tmpTipo in _db_tipos_recibos)
            {
                if (tmpTipo.GetID() == te_id)
                {
                    return tmpTipo;
                }
            }
            return null;
        }

        public static DBTipoRecibo GetByID(long te_id, MySqlConnection conn)
        {
            DBTipoRecibo returnTipoRecibo = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {te_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnTipoRecibo = new DBTipoRecibo(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de recibo en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnTipoRecibo;
        }

        public DBTipoRecibo(long id, TipoReciboData newData)
        {
            _id = id;
            _data = newData;

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBTipoRecibo(TipoReciboData newData) : this(-1, newData) { }
        public DBTipoRecibo(long id, string nombre) : this(id, new TipoReciboData(nombre)) { }

        public DBTipoRecibo(string nombre) : this(-1, nombre) { }

        public DBTipoRecibo(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _id = id;
                    _data = TipoReciboData.CreateFromReader(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructo de DBTipoRecibo, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBTipoRecibo(MySqlDataReader reader) : this(reader.GetInt64Safe(NameOf_id), TipoReciboData.CreateFromReader(reader)) { }


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
                    _data = TipoReciboData.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBTipoRecibo::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET {TipoReciboData.NameOf_tr_nombre} = '{Regex.Replace(_data.tr_nombre.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBTipoRecibo::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            if (DuplicatedExistsInDatabase(conn) == true || DuplicatedExistsInDatabase(conn) == null)
            {
                return false;
            }
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TipoReciboData.NameOf_tr_nombre}) VALUES ('{Regex.Replace(_data.tr_nombre.Trim(), @"\s+", " ")}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_tipos_recibos.Contains(this))
                    {
                        _db_tipos_recibos.Add(this);
                    }
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBTipoRecibo::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    _db_tipos_recibos.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTipoRecibo: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE UPPER({TipoReciboData.NameOf_tr_nombre}) = '{Regex.Replace(_data.tr_nombre.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBTipoRecibo::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                duplicatedExistsInDB = null;
            }
            return duplicatedExistsInDB;
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
                MessageBox.Show("Error en el método DBTipoRecibo::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.tr_nombre.Equals(newName);
            _data.tr_nombre = newName;
        }

        public string GetName() => _data.tr_nombre;

        public override long GetID() => _id;

        protected override void ChangeID(long id)
        {
            _shouldPush = _shouldPush || (_id != id);
            _id = id;
        }
        protected override void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }
        public override bool ShouldPush() => _shouldPush;
        public override bool IsLocal() => _id < 0;

        public override string ToString()
        {
            return $"ID: {GetID()} - {_data.ToString()}";
        }

        public override DBBaseClass GetLocalCopy()
        {
            return new DBTipoRecibo(-1, _data);
        }

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBTipoRecibo tipoRecibo in _db_tipos_recibos)
            {
                str += $"Tipo de Recibo> {tipoRecibo}\n";
            }
            return str;
        }
        public static DBTipoRecibo GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_tipos_recibos[r.Next(0, _db_tipos_recibos.Count)];
        }
    }
}
