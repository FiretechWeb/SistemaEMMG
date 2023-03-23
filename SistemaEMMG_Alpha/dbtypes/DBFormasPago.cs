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
    public struct FormasPagoData
    {
        public FormasPagoData(string nom)
        {
            fp_nombre = nom;
        }
        public string fp_nombre { get; set; }

        public static readonly string NameOf_fp_nombre = nameof(fp_nombre);

        public static FormasPagoData CreateFromReader(MySqlDataReader reader) => new FormasPagoData(reader.GetStringSafe(NameOf_fp_nombre));
        public override string ToString() => $"Forma: {fp_nombre}";
    }
    public class DBFormasPago : DBBaseClass, IDBase<DBFormasPago>, IDBDataType<DBFormasPago>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "formas_pago";
        public const string NameOf_id = "fp_id";
        private FormasPagoData _data;
        private static readonly List<DBFormasPago> _db_formas_pago = new List<DBFormasPago>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet) => $"SELECT {fieldsToGet} FROM {db_table}";

        string IDBase<DBFormasPago>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

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

        public static IReadOnlyCollection<DBFormasPago> GetAllLocal() => _db_formas_pago.Where(x => x.IsLocal()).ToList().AsReadOnly();

        IReadOnlyCollection<DBFormasPago> IDBDataType<DBFormasPago>.GetAllLocal() => GetAllLocal();
        public static DBFormasPago GetByID(long fp_id)
        {
            foreach (DBFormasPago formaPago in _db_formas_pago)
            {
                if (formaPago.GetID() == fp_id)
                {
                    return formaPago;
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

        public DBFormasPago(long id, FormasPagoData newData) : base (id) { _data = newData; }

        public DBFormasPago(FormasPagoData newData) : this(-1, newData) { }
        public DBFormasPago(long id, string nombre) : this(id, new FormasPagoData(nombre)) { }

        public DBFormasPago(string nombre) : this(-1, nombre) { }

        public DBFormasPago(MySqlConnection conn, long id) : base (id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = FormasPagoData.CreateFromReader(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MakeLocal();
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
                    wasAbleToPull = true;
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


        public override bool UpdateToDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET {FormasPagoData.NameOf_fp_nombre} = '{Regex.Replace(_data.fp_nombre.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()}";
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
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({FormasPagoData.NameOf_fp_nombre}) VALUES ('{Regex.Replace(_data.fp_nombre.Trim(), @"\s+", " ")}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_formas_pago.Contains(this))
                    {
                        _db_formas_pago.Add(this);
                    }
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
                    _db_formas_pago.Remove(this);
                    MakeLocal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBFormasPago: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE UPPER({FormasPagoData.NameOf_fp_nombre}) = '{Regex.Replace(_data.fp_nombre.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBFormasPago::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBFormasPago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public string GetName() => _data.fp_nombre;

        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.fp_nombre.Equals(newName);
            _data.fp_nombre = newName;
        }

        public override DBBaseClass GetLocalCopy()
        {
            return new DBFormasPago(-1, _data);
        }

        public override string ToString() => $"ID: {GetID()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBFormasPago formaPago in _db_formas_pago)
            {
                str += $"Forma de Pago> {formaPago}\n";
            }
            return str;
        }
        public static DBFormasPago GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_formas_pago[r.Next(0, _db_formas_pago.Count)];
        }

    }
}
