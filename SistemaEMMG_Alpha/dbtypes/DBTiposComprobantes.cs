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
    public struct TiposComprobantesData
    {
        public TiposComprobantesData(string nom)
        {
            tc_nombre = nom;
        }
        public string tc_nombre { get; set; }

        public static readonly string NameOf_tc_nombre = nameof(tc_nombre);

        public static TiposComprobantesData CreateFromReader(MySqlDataReader reader)
        {
            return new TiposComprobantesData(reader.GetStringSafe(NameOf_tc_nombre));
        }

        public override string ToString()
        {
            return $"Tipo: {tc_nombre}";
        }
    }

    public class DBTiposComprobantes : DBBaseClass, IDBase<DBTiposComprobantes>, IDBDataType<DBTiposComprobantes>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "tipos_comprobantes";
        public const string NameOf_id = "tc_id";
        private long _id;
        private bool _shouldPush=false;
        private TiposComprobantesData _data;
        private static readonly List<DBTiposComprobantes> _db_tipos_comprobantes = new List<DBTiposComprobantes>();
        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
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

        public static List<DBTiposComprobantes> GetAll()
        {
            List<DBTiposComprobantes> returnList = new List<DBTiposComprobantes>();
            foreach (DBTiposComprobantes tiposComprobantes in _db_tipos_comprobantes)
            {
                returnList.Add(tiposComprobantes);
            }
            return returnList;
        }
        List<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBTiposComprobantes> GetAllLocal()
        {
            return _db_tipos_comprobantes.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBTiposComprobantes> IDBDataType<DBTiposComprobantes>.GetAllLocal() => GetAllLocal();

        public static DBTiposComprobantes GetByID(long tc_id)
        {
            foreach (DBTiposComprobantes tipoComprobante in _db_tipos_comprobantes)
            {
                if (tipoComprobante.GetID() == tc_id)
                {
                    return tipoComprobante;
                }
            }
            return null;
        }

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

        public DBTiposComprobantes(long id, TiposComprobantesData newData)
        {
            _id = id;
            _data = newData;
            if (IsLocal()) //Locally created
            {
                _shouldPush = true;
            }
        }

        public DBTiposComprobantes(TiposComprobantesData newData) : this(-1, newData) { }
        public DBTiposComprobantes(long id, string nombre) : this(id, new TiposComprobantesData(nombre)) { }

        public DBTiposComprobantes(string nombre) : this(-1, nombre) { }

        public DBTiposComprobantes(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _id = id;
                    _data = TiposComprobantesData.CreateFromReader(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBTiposComprobantes. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBTiposComprobantes(MySqlDataReader reader) : this (reader.GetInt64Safe(NameOf_id), TiposComprobantesData.CreateFromReader(reader)) { }

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
            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET {TiposComprobantesData.NameOf_tc_nombre} = '{_data.tc_nombre}' WHERE {NameOf_id} = {GetID()}";
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
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({TiposComprobantesData.NameOf_tc_nombre}) VALUES ('{_data.tc_nombre}')";
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
                MessageBox.Show("Error DBTiposComprobantes::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en DBTiposComprobantes::DeleteFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBTiposComprobantes::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public override bool ShouldPush() => _shouldPush;
        public override bool IsLocal() => _id < 0;
        protected override void ChangeID(long id)
        {
            _shouldPush = _shouldPush || (id != _id);
            _id = id;
        }
        public override long GetID() => _id;

        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.tc_nombre.Equals(newName);
            _data.tc_nombre = newName;
        }

        public string GetName() => _data.tc_nombre;

        protected override void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }

        public override DBBaseClass GetLocalCopy()
        {
            return new DBTiposComprobantes(-1, _data);
        }
        public override string ToString()
        {
            return $"ID: {GetID()} - {_data.ToString()}";
        }

        ~DBTiposComprobantes()
        {

        }
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
