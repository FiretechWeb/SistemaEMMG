using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct BancosData
    {
        public BancosData(string name, int code)
        {
            bnk_name = name;
            bnk_code = code;
        }
        public string bnk_name { get; set; }
        public int bnk_code { get; set; }

        public static readonly string NameOf_bnk_name = nameof(bnk_name);
        public static readonly string NameOf_bnk_code = nameof(bnk_code);

        public static BancosData CreateFromReader(MySqlDataReader reader)
        {
            return new BancosData(
                reader.GetStringSafe(NameOf_bnk_name),
                reader.GetInt32Safe(NameOf_bnk_code));
        }

        public override string ToString()
        {
            return $"Código del banco: {bnk_code} - Nombre del banco: {bnk_name}";
        }
    }
    
    public class DBBancos : DBBaseClass, IDBase<DBBancos>, IDBDataType<DBBancos>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "bancos";
        public const string NameOf_id = "bnk_id";
        private BancosData _data;
        private static readonly List<DBBancos> _db_bancos = new List<DBBancos>();
        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }

        string IDBase<DBBancos>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBBancos> GenerateDefaultData()
        {
            List<DBBancos> defaulData = new List<DBBancos>();
            defaulData.Add(new DBBancos("BANCO DE LA NACION ARGENTINA", 11));
            defaulData.Add(new DBBancos("BANCO DE LA PROVINCIA DE BUENOS AIRES", 14));
            defaulData.Add(new DBBancos("INDUSTRIAL AND COMMERCIAL BANK OF CHINA", 15));
            defaulData.Add(new DBBancos("CITIBANK N.A.", 16));
            defaulData.Add(new DBBancos("BANCO BBVA ARGENTINA S.A.", 17));
            defaulData.Add(new DBBancos("BANCO DE LA PROVINCIA DE CORDOBA S.A.", 20));
            defaulData.Add(new DBBancos("BANCO SUPERVIELLE S.A.", 27));
            defaulData.Add(new DBBancos("BANCO DE LA CIUDAD DE BUENOS AIRES", 29));
            defaulData.Add(new DBBancos("BANCO PATAGONIA S.A.", 34));
            defaulData.Add(new DBBancos("BANCO HIPOTECARIO S.A.", 44));
            defaulData.Add(new DBBancos("BANCO DE SAN JUAN S.A.", 45));
            defaulData.Add(new DBBancos("BANCO MUNICIPAL DE ROSARIO", 65));
            defaulData.Add(new DBBancos("BANCO SANTANDER ARGENTINA S.A.", 72));
            defaulData.Add(new DBBancos("BANCO DEL CHUBUT S.A.", 83));
            defaulData.Add(new DBBancos("BANCO DE SANTA CRUZ S.A", 86));
            defaulData.Add(new DBBancos("BANCO DE LA PAMPA SOCIEDAD DE ECONOMÍA M", 93));
            defaulData.Add(new DBBancos("BANCO DE CORRIENTES S.A.", 94));
            defaulData.Add(new DBBancos("BANCO PROVINCIA DEL NEUQUÉN SOCIEDAD ANÓ", 97));
            defaulData.Add(new DBBancos("BANK OF CHINA LIMITED SUCURSAL BUENOS AI", 131));
            defaulData.Add(new DBBancos("BRUBANK S.A.U.", 143));
            defaulData.Add(new DBBancos("BIBANK S.A.", 147));
            defaulData.Add(new DBBancos("HSBC BANK ARGENTINA S.A.", 150));
            defaulData.Add(new DBBancos("OPEN BANK ARGENTINA S.A.", 158));
            defaulData.Add(new DBBancos("JPMORGAN CHASE BANK, NATIONAL ASSOCIATIO.", 165));
            defaulData.Add(new DBBancos("BANCO CREDICOOP COOPERATIVO LIMITADO", 191));

            return defaulData;
        }
        List<DBBancos> IDBDataType<DBBancos>.GenerateDefaultData() => GenerateDefaultData();

        public static bool PushDefaultData(MySqlConnection conn)
        {
            if ((User.GetCurrentUser() is null) || !User.GetCurrentUser().IsAdmin()) //Security measures.
            {
                return false;
            }

            List<DBBancos> defaultData = GenerateDefaultData();

            foreach (DBBancos banco in defaultData)
            {
                banco.PushToDatabase(conn);
            }

            return true;
        }

        bool IDBDataType<DBBancos>.PushDefaultData(MySqlConnection conn) => PushDefaultData(conn);

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
                MessageBox.Show("Erro en el método DBBancos::ResetDBData. Error en la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return resetDataSuccess;
        }

        bool IDBDataType<DBBancos>.ResetDBData(MySqlConnection conn) => ResetDBData(conn);

        public static List<DBBancos> UpdateAll(MySqlConnection conn)
        {
            List<DBBancos> returnList = new List<DBBancos>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_bancos.Clear();

                while(reader.Read())
                {
                    DBBancos banco = new DBBancos(reader);
                    _db_bancos.Add(banco);
                    returnList.Add(banco);
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de actualizar todos los bancos. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        List<DBBancos> IDBDataType<DBBancos>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBBancos> GetAll() => new List<DBBancos>(_db_bancos);

        List<DBBancos> IDBDataType<DBBancos>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBBancos> GetAllLocal() => _db_bancos;
        IReadOnlyCollection<DBBancos> IDBDataType<DBBancos>.GetAllLocal() => GetAllLocal();

        public static DBBancos GetByID(long bnk_id) => _db_bancos.Find(x => x.GetID() == bnk_id);

        public static DBBancos GetByID(long bnk_id, MySqlConnection conn)
        {
            DBBancos returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {bnk_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBBancos(reader);
                }
                reader.Close();

            } catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un tipo de entidad en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBBancos(long id, BancosData newData) : base(id) { _data = newData; }

        public DBBancos(BancosData newData) : this(-1, newData) { }
        public DBBancos(long id, string nombre, int code) : this(id, new BancosData(nombre, code)) { }

        public DBBancos(string nombre, int code) : this(-1, nombre, code) { }

        public DBBancos(MySqlConnection conn, long id) : base (id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = BancosData.CreateFromReader(reader);
                }
            } catch (Exception ex)
            {
                MakeLocal();
                MessageBox.Show("Error en el constructor de DBBancos. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public DBBancos(MySqlDataReader reader) : this(reader.GetInt64Safe(NameOf_id), BancosData.CreateFromReader(reader)) { }

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
                    _data = BancosData.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
            } catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBBancos::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return wasAbleToPull;
        }

        public override bool UpdateToDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }

            bool? doesDuplicatedExistDB = DuplicatedExistsInDatabase(conn);
            if (!doesDuplicatedExistDB.HasValue || doesDuplicatedExistDB == true)
            {
                return false;
            }

            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET {BancosData.NameOf_bnk_code} = {_data.bnk_code}, {BancosData.NameOf_bnk_name} = '{_data.bnk_name.CleanWhitespaces()}'";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            } catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBBancos::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $@"INSERT INTO {db_table} (
                                {BancosData.NameOf_bnk_name}, 
                                {BancosData.NameOf_bnk_code}) 
                                VALUES (
                                '{_data.bnk_name.CleanWhitespaces()}',
                                {_data.bnk_code})";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    if (!_db_bancos.Contains(this))
                    {
                        _db_bancos.Add(this);
                    }
                    ChangeID(cmd.LastInsertedId);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            } catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBBancos::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    _db_bancos.Remove(this);
                    MakeLocal();
                }
            } catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error en DBBancos::DeleteFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND UPPER({BancosData.NameOf_bnk_name}) = '{_data.bnk_name.CleanWhitespaces()}' AND {BancosData.NameOf_bnk_code} = {_data.bnk_code}";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            } catch(Exception ex)
            {
                MessageBox.Show("Error en el método DBBancos::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} = {GetID()}"; ;
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            } catch(Exception ex)
            {
                existsInDB = null;
                MessageBox.Show("Error en el método DBBancos::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return existsInDB;
        }

        public string GetName() => _data.bnk_name;
        public int GetCode() => _data.bnk_code;

        public void SetName(string newName)
        {
            _shouldPush = _shouldPush || !_data.bnk_name.Equals(newName);
            _data.bnk_name = newName;
        }

        public void SetCode(int newCode)
        {
            _shouldPush = _shouldPush || _data.bnk_code != newCode;
            _data.bnk_code = newCode;
        }

        public string GetCodeWithFormat() => _data.bnk_code.ToString("00000");

        public override DBBaseClass GetLocalCopy() => new DBBancos(-1, _data);

        public override string ToString() => $"ID: {GetID()} - {_data}";
        
        public static string PrintAll()
        {
            string str = "";
            foreach(DBBancos banco in _db_bancos)
            {
                str += $"Banco> {banco}";
            }
            return str;
        }

        public static DBBancos GetRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            return _db_bancos[r.Next(0, _db_bancos.Count)];
        }
    }
}
