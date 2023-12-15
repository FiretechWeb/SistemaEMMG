using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public enum TipoFormaDePago
    {
        EFECTIVO = 0,
        CHEQUE,
        TRANSFERENCIA,
        OTRO
    }

    public struct FormasPagoData
    {
        public FormasPagoData(string nom, TipoFormaDePago type)
        {
            fp_nombre = nom;
            fp_type = type;
        }
        public string fp_nombre { get; set; }

        public TipoFormaDePago fp_type;

        public static readonly string NameOf_fp_nombre = nameof(fp_nombre);

        public static readonly string NameOf_fp_type = nameof(fp_type);

        public static FormasPagoData CreateFromReader(MySqlDataReader reader) => new FormasPagoData(
            reader.GetStringSafe(NameOf_fp_nombre),
            (TipoFormaDePago)reader.GetInt32Safe(NameOf_fp_type)
        );
        public override string ToString() => $"Forma: {fp_nombre} - Tipo: {fp_type}";
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

        public static List<DBFormasPago> GenerateDefaultData()
        {
            List<DBFormasPago> defaulData = new List<DBFormasPago>();
            defaulData.Add(new DBFormasPago("Cheque", TipoFormaDePago.CHEQUE));
            defaulData.Add(new DBFormasPago("Efectivo", TipoFormaDePago.EFECTIVO));
            defaulData.Add(new DBFormasPago("Transferencia Bancaria", TipoFormaDePago.TRANSFERENCIA));
            defaulData.Add(new DBFormasPago("Nota de crédito", TipoFormaDePago.OTRO));

            return defaulData;
        }
        List<DBFormasPago> IDBDataType<DBFormasPago>.GenerateDefaultData() => GenerateDefaultData();

        public static bool PushDefaultData(MySqlConnection conn)
        {
            if ((User.GetCurrentUser() is null) || !User.GetCurrentUser().IsAdmin()) //Security measures.
            {
                return false;
            }

            List<DBFormasPago> defaultData = GenerateDefaultData();

            foreach (DBFormasPago formaPago in defaultData)
            {
                formaPago.PushToDatabase(conn);
            }

            return true;
        }

        bool IDBDataType<DBFormasPago>.PushDefaultData(MySqlConnection conn) => PushDefaultData(conn);

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
                MessageBox.Show("Error en el método DBFormasPago::ResetDBData. Error en la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return resetDataSuccess;
        }

        bool IDBDataType<DBFormasPago>.ResetDBData(MySqlConnection conn) => ResetDBData(conn);

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

        public static List<DBFormasPago> GetAll() => new List<DBFormasPago>(_db_formas_pago);

        List<DBFormasPago> IDBDataType<DBFormasPago>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBFormasPago> GetAllLocal() => _db_formas_pago.Where(x => x.IsLocal()).ToList().AsReadOnly();

        IReadOnlyCollection<DBFormasPago> IDBDataType<DBFormasPago>.GetAllLocal() => GetAllLocal();
        public static DBFormasPago GetByID(long fp_id) => _db_formas_pago.Find(x => x.GetID() == fp_id);

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
        public DBFormasPago(long id, string nombre, TipoFormaDePago tipoForma) : this(id, new FormasPagoData(nombre, tipoForma)) { }

        public DBFormasPago(string nombre, TipoFormaDePago tipoForma) : this(-1, nombre, tipoForma) { }

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

            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }

            bool wasAbleToUpdate = false;
            try
            {
                string query = $@"UPDATE {db_table} SET
                                {FormasPagoData.NameOf_fp_nombre} = '{Regex.Replace(_data.fp_nombre.Trim(), @"\s+", " ")}',
                                {FormasPagoData.NameOf_fp_type} = {(int)_data.fp_type}
                                WHERE {NameOf_id} = {GetID()}";
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
                string query = $@"INSERT INTO {db_table}
                                ({FormasPagoData.NameOf_fp_nombre}, {FormasPagoData.NameOf_fp_type})
                                VALUES
                                ('{Regex.Replace(_data.fp_nombre.Trim(), @"\s+", " ")}', {(int)_data.fp_type})";
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND UPPER({FormasPagoData.NameOf_fp_nombre}) = '{Regex.Replace(_data.fp_nombre.Trim().ToUpper(), @"\s+", " ")}'";
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

        public TipoFormaDePago GetTipo() => _data.fp_type;


        public void SetTipo(TipoFormaDePago newType) => _data.fp_type = newType;

        public override DBBaseClass GetLocalCopy()
        {
            return new DBFormasPago(-1, _data);
        }

        public override void Update(DBBaseClass source)
        {
            if (!(source is DBFormasPago)) return;
            DBFormasPago sourceFormaDePago = (DBFormasPago)source;
            SetName(sourceFormaDePago.GetName());
            SetTipo(sourceFormaDePago.GetTipo());
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
