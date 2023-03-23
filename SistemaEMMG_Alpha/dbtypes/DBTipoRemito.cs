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
    public struct TipoRemitoData
    {
        public TipoRemitoData(string nom)
        {
            ts_nombre = nom;
        }
        public string ts_nombre { get; set; }

        public static readonly string NameOf_ts_nombre = nameof(ts_nombre);

        public static TipoRemitoData CreateFromReader(MySqlDataReader reader)
        {
            return new TipoRemitoData(reader.GetStringSafe(NameOf_ts_nombre));
        }

        public override string ToString()
        {
            return $"Tipo: {ts_nombre}";
        }
    }
    public class DBTipoRemito : DBBaseClass, IDBase<DBTipoRemito>, IDBDataType<DBTipoRemito>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_remitos";
        public const string NameOf_id = "ts_id";
        private long _id;
        private bool _shouldPush = false;
        private TipoRemitoData _data;
        private static readonly List<DBTipoRemito> _db_tipos_remitos = new List<DBTipoRemito>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
        string IDBase<DBTipoRemito>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

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

        public static List<DBTipoRemito> GetAll()
        {
            List<DBTipoRemito> returnList = new List<DBTipoRemito>();
            foreach (DBTipoRemito tipoRemito in _db_tipos_remitos)
            {
                returnList.Add(tipoRemito);
            }
            return returnList;
        }

        List<DBTipoRemito> IDBDataType<DBTipoRemito>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTipoRemito> GetAllLocal()
        {
            return _db_tipos_remitos.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBTipoRemito> IDBDataType<DBTipoRemito>.GetAllLocal() => GetAllLocal();

        public static DBTipoRemito GetByID(long te_id)
        {
            foreach (DBTipoRemito tmpTipo in _db_tipos_remitos)
            {
                if (tmpTipo.GetID() == te_id)
                {
                    return tmpTipo;
                }
            }
            return null;
        }

        public static DBTipoRemito GetByID(long te_id, MySqlConnection conn)
        {
            DBTipoRemito returnTipoRemito = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {te_id}";
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

        public DBTipoRemito(long id, TipoRemitoData newData)
        {
            _id = id;
            _data = newData;

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBTipoRemito(TipoRemitoData newData) : this(-1, newData) { }
        public DBTipoRemito(long id, string nombre) : this(id, new TipoRemitoData(nombre)) { }

        public DBTipoRemito(string nombre) : this(-1, nombre) { }

        public DBTipoRemito(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    _id = id;
                    _data = TipoRemitoData.CreateFromReader(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
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
                string query = $"UPDATE {db_table} SET {TipoRemitoData.NameOf_ts_nombre} = '{_data.ts_nombre}' WHERE {NameOf_id} = {GetID()}";
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
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TipoRemitoData.NameOf_ts_nombre}) VALUES ('{_data.ts_nombre}')";
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
                MessageBox.Show("Error DBTipoRemito::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBTipoRemito: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            return new DBTipoRemito(-1, _data);
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
