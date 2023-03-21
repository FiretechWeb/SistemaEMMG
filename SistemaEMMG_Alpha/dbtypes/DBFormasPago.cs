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
    public struct FormasPagoData
    {
        public FormasPagoData(string nom)
        {
            fp_nombre = nom;
        }
        public string fp_nombre { get; set; }

        public static readonly string NameOf_fp_nombre = nameof(fp_nombre);

        public static FormasPagoData CreateFromReader(MySqlDataReader reader)
        {
            return new FormasPagoData(reader.GetStringSafe(NameOf_fp_nombre));
        }
        public override string ToString()
        {
            return $"Nombre: {fp_nombre}";
        }
    }
    public class DBFormasPago : DBBaseClass, IDBase<DBFormasPago>, IDBDataType<DBFormasPago>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "formas_pago";
        public const string NameOf_id = "fp_id";
        private long _id;
        private bool _shouldPush = false;
        private FormasPagoData _data;
        private static readonly List<DBFormasPago> _db_formas_pago = new List<DBFormasPago>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
        string IDBase<DBFormasPago>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static bool TipoFormaPagoExists(string formaPagoNombre, List<DBFormasPago> listaFormasPago)
        {
            foreach (DBFormasPago formaPago in listaFormasPago)
            {
                if (formaPagoNombre.Trim().ToLower().Equals(formaPago.GetName().Trim().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TipoFormaPagoExists(string formaPagoNombre)
        {
            return TipoFormaPagoExists(formaPagoNombre, _db_formas_pago);
        }

        public static List<DBFormasPago> UpdateAll(MySqlConnection conn)
        {
            List<DBFormasPago> returnList = new List<DBFormasPago>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_formas_pago.Clear();

                while (reader.Read())
                {
                    _db_formas_pago.Add(new DBFormasPago(reader));
                    returnList.Add(new DBFormasPago(reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los tipos formas de pago. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        List<DBFormasPago> IDBDataType<DBFormasPago>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBFormasPago> GetAll()
        {
            List<DBFormasPago> returnList = new List<DBFormasPago>();
            foreach (DBFormasPago formaPago in _db_formas_pago)
            {
                returnList.Add(formaPago);
            }
            return returnList;
        }

        List<DBFormasPago> IDBDataType<DBFormasPago>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBFormasPago> GetAllLocal()
        {
            return _db_formas_pago.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBFormasPago> IDBDataType<DBFormasPago>.GetAllLocal() => GetAllLocal();
        public static DBFormasPago GetByID(long fp_id, bool clone = true)
        {
            foreach (DBFormasPago formaPago in _db_formas_pago)
            {
                if (formaPago.GetID() == fp_id)
                {
                    return clone ? formaPago.Clone() : formaPago;
                }
            }
            return null;
        }

        public static DBFormasPago GetByID(long fp_id, MySqlConnection conn)
        {
            DBFormasPago returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {fp_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBFormasPago(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener una forma de pago en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBFormasPago(long id, FormasPagoData newData)
        {
            _id = id;
            _data = newData;
            if (IsLocal()) //Locally created
            {
                _shouldPush = true;
            }
        }

        public DBFormasPago(long id, string nombre) : this(id, new FormasPagoData(nombre)) { }

        public DBFormasPago(string nombre) : this(-1, nombre) { }

        public DBFormasPago(MySqlConnection conn, long id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _id = id;
                    _data = FormasPagoData.CreateFromReader(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBFormasPago. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public DBFormasPago(MySqlDataReader reader) : this(reader.GetInt64Safe(NameOf_id), FormasPagoData.CreateFromReader(reader)) { }
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
                    _data = FormasPagoData.CreateFromReader(reader);
                    _shouldPush = false;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBFormasPago::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET {FormasPagoData.NameOf_fp_nombre} = '{_data.fp_nombre}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBFormasPago::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({FormasPagoData.NameOf_fp_nombre}) VALUES ('{_data.fp_nombre}')";
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
                MessageBox.Show("Error DBFormasPago::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBFormasPago: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBFormasPago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public DBFormasPago Clone() => new DBFormasPago(_id, _data.fp_nombre);
        public override long GetID() => _id;
        protected override void ChangeID(long id)
        {
            _shouldPush = _shouldPush || (_id != id);
            _id = id;
        }
        public override bool ShouldPush() => _shouldPush;
        public override bool IsLocal() => _id < 0;
        public string GetName() => _data.fp_nombre;

        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.fp_nombre.Equals(newName);
            _data.fp_nombre = newName;
        }

        public override string ToString()
        {
            return _data.ToString();
        }

    }
}
