using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct CuentaData
    {
        public CuentaData(long cuit, string rs)
        {
            em_cuit = cuit;
            em_rs = rs; //Event handler to check that rs is not longer than 64 characters
        }
        public long em_cuit { get; set; }
        public string em_rs { get; set; }

        public static readonly string NameOf_em_cuit = nameof(em_cuit);
        public static readonly string NameOf_em_rs = nameof(em_rs);
        public static CuentaData CreateFromReader(MySqlDataReader reader) => new CuentaData(reader.GetInt64Safe(NameOf_em_cuit), reader.GetStringSafe(NameOf_em_rs));

        public override string ToString() => $"Razón Social: {em_rs} - CUIT: {em_cuit}";
    }
    public class DBCuenta : DBBaseClass, IDBase<DBCuenta>, IDBDataType<DBCuenta>
    {
        /*************************
         * Global Static STUFFF *
         ************************/
        public const string db_table = "cuentas";
        public const string NameOf_id = "em_id";
        private static readonly List<DBCuenta> _db_cuentas = new List<DBCuenta>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet) => $"SELECT {fieldsToGet} FROM {db_table}";

        string IDBase<DBCuenta>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);


        public static List<DBCuenta> GenerateDefaultData()
        {
            List<DBCuenta> defaulData = new List<DBCuenta>();

            return defaulData;
        }
        List<DBCuenta> IDBDataType<DBCuenta>.GenerateDefaultData() => GenerateDefaultData();

        public static bool PushDefaultData(MySqlConnection conn)
        {
            if ((User.GetCurrentUser() is null) || !User.GetCurrentUser().IsAdmin()) //Security measures.
            {
                return false;
            }

            List<DBCuenta> defaultData = GenerateDefaultData();
            foreach (DBCuenta cuenta in defaultData)
            {
                cuenta.PushToDatabase(conn);
            }

            return true;
        }

        bool IDBDataType<DBCuenta>.PushDefaultData(MySqlConnection conn) => PushDefaultData(conn);

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
                MessageBox.Show("Error en el método DBCuenta::ResetDBData. Error en la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return resetDataSuccess;
        }

        bool IDBDataType<DBCuenta>.ResetDBData(MySqlConnection conn) => ResetDBData(conn);

        public static List<DBCuenta> UpdateAll(MySqlConnection conn)
        {
            List<DBCuenta> returnList = new List<DBCuenta>();
            try
            {
                string query = GetSQL_SelectQueryWithRelations("*");
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_cuentas.Clear();

                while (reader.Read())
                {
                    DBCuenta newCuenta = new DBCuenta(reader);
                    _db_cuentas.Add(newCuenta);
                    returnList.Add(newCuenta);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas las cuentas, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
        List<DBCuenta> IDBDataType<DBCuenta>.UpdateAll(MySqlConnection conn) => UpdateAll(conn);

        public static List<DBCuenta> GetAll() => new List<DBCuenta>(_db_cuentas);

        List<DBCuenta> IDBDataType<DBCuenta>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBCuenta> GetAllLocal()
        {
            return _db_cuentas.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBCuenta> IDBDataType<DBCuenta>.GetAllLocal() => GetAllLocal();

        public static DBCuenta GetByID(long tc_id) => _db_cuentas.Find(x => x.GetID() == tc_id);

        public static DBCuenta GetByID(long tc_id, MySqlConnection conn)
        {
            DBCuenta returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {tc_id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEnt = new DBCuenta(reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener una cuenta en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public static bool CuentaYaExiste(string str, long cuit, List<DBCuenta> cuentas)
        {
            foreach (DBCuenta cuenta in cuentas)
            {
                if (str.ToLower().Equals(cuenta.GetRazonSocial().ToLower()) || cuenta.GetCUIT() == cuit)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool CuentaYaExiste(string str, long cuit) => CuentaYaExiste(str, cuit, _db_cuentas);

        /***************
         * Local STUFF *
         ***************/

        private CuentaData _data;
        private readonly List<DBEntidades> _db_entidades_comerciales = new List<DBEntidades>();
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>(); //Useless, TO REMOVE in future. 
        public DBCuenta(long id, CuentaData newData) : base (id) { _data = newData; }

        public DBCuenta(long id, long cuit, string rs) : this (id, new CuentaData(cuit, rs)) { }
        public DBCuenta(long cuit, string rs) : this(-1, cuit, rs) { }

        public DBCuenta(MySqlDataReader reader) : this (reader.GetInt64Safe(NameOf_id), CuentaData.CreateFromReader(reader)) { }
        public DBCuenta(MySqlConnection conn, int id) : base (id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
               
                while (reader.Read())
                {
                    _data = CuentaData.CreateFromReader(reader);
                }

                reader.Close();
            } catch (Exception ex)
            {
                MakeLocal();
                MessageBox.Show("Error en el constructor de DBCuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                    _data = CuentaData.CreateFromReader(reader);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBCuenta::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"UPDATE {db_table} SET {CuentaData.NameOf_em_cuit} = {_data.em_cuit}, {CuentaData.NameOf_em_rs} = '{Regex.Replace(_data.em_rs.Trim(), @"\s+", " ")}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBCuenta::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"INSERT INTO {db_table} ({CuentaData.NameOf_em_cuit}, {CuentaData.NameOf_em_rs}) VALUES ({_data.em_cuit}, '{Regex.Replace(_data.em_rs.Trim(), @"\s+", " ")}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    if (!_db_cuentas.Contains(this))
                    {
                        _db_cuentas.Add(this);
                    }
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBCuenta::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        private bool DeleteAllRelatedData(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {DBRecibo.db_relation_table} WHERE rp_em_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM remitos_comprobantes WHERE rt_em_id = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBPago.db_table} WHERE {DBPago.NameOf_pg_em_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBRecibo.db_table} WHERE {DBRecibo.NameOf_rc_em_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM remitos WHERE rm_em_id = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBComprobantes.db_table} WHERE {DBComprobantes.NameOf_cm_em_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBEntidades.db_table} WHERE {DBEntidades.NameOf_ec_em_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error tratando de eliminar información relacionada a una cuenta en DBCuenta: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }
        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            if (!DeleteAllRelatedData(conn)) //remove all related data first...
            {
                return false;
            }
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _db_cuentas.Remove(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBCuenta: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_id} <> {GetID()} AND {CuentaData.NameOf_em_cuit} = {_data.em_cuit} AND UPPER({CuentaData.NameOf_em_rs}) = '{Regex.Replace(_data.em_rs.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBCuenta::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            } catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBCuenta::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public List<DBComprobantes> GetAllComprobantes(MySqlConnection conn) //Get directly from database
        {
            List<DBComprobantes> returnList = DBComprobantes.GetAll(conn, this);
            _db_comprobantes.Clear();
            foreach (DBComprobantes comprobante in returnList)
            {
                _db_comprobantes.Add(comprobante); 
            }
            return returnList;
        }
        public List<DBComprobantes> GetAllComprobantes() //Get CACHE
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            foreach (DBComprobantes comprobante in _db_comprobantes)
            {
                returnList.Add(comprobante); 
            }
            return returnList;
        }
        public DBComprobantes GetComprobanteByID(long ec_id, long cm_id)  //Delete in future
        {
            return DBComprobantes.GetByID(_db_comprobantes, this, ec_id, cm_id);
        }

        public DBComprobantes GetComprobanteByIndex(int index)  //Delete in future
        {
            if (index < 0 || index >= _db_comprobantes.Count)
            {
                return null;
            }
            return _db_comprobantes[index];
        }

        public DBComprobantes GetComprobanteByID(DBEntidades entidadComercial, long cm_id) //Delete in future
        {
            return entidadComercial.GetComprobanteByID(cm_id);
        }

        public List<DBEntidades> GetAllEntidadesComerciales(MySqlConnection conn) //Get directly from database
        {
            List<DBEntidades> returnList = DBEntidades.GetAll(conn, this);
            _db_entidades_comerciales.Clear();
            foreach (DBEntidades entidadComercial in returnList)
            {
                _db_entidades_comerciales.Add(entidadComercial);
            }
            return returnList;
        }
        public List<DBEntidades> GetAllEntidadesComerciales() //Get CACHE
        {
            List<DBEntidades> returnList = new List<DBEntidades>();

            foreach (DBEntidades entidadComercial in _db_entidades_comerciales)
            {
                returnList.Add(entidadComercial);
            }

            return returnList;
        }

        public DBEntidades GetEntidadByID(long ec_id) => DBEntidades.GetByID(_db_entidades_comerciales, this, ec_id);

        public bool AddEntidad(DBEntidades newEntidadComercial)
        {
            if (newEntidadComercial.GetCuentaID() != GetID())
            {
                return false; //Cannot add an entity from another account like this...
            }
            if (_db_entidades_comerciales.Contains(newEntidadComercial))
            {
                return false; //already in list
            }
            if (!newEntidadComercial.IsLocal())
            {
                int foundIndex = DBEntidades.FindInList(_db_entidades_comerciales, newEntidadComercial);
                if (foundIndex != -1) //Update old data with new in case of match.
                {
                    _db_entidades_comerciales[foundIndex] = newEntidadComercial;
                    return true;
                }
            }
            _db_entidades_comerciales.Add(newEntidadComercial);
            return true;
        }
        public void RemoveEntidad(DBEntidades entRemove)
        {
            if (_db_entidades_comerciales.Remove(entRemove))
            {
                return;
            }
            if (entRemove.IsLocal())
            {
                return;
            }
            _db_entidades_comerciales.RemoveAll(x => x.GetCuentaID() == entRemove.GetCuentaID() && x.GetID() == entRemove.GetID());
        }

        public bool AddComprobante(DBComprobantes entAdd)
        {
            if (entAdd.GetCuentaID() != GetID())
            {
                return false; //Cannot add an entity from another account like this...
            }
            if (_db_comprobantes.Contains(entAdd))
            {
                return false;
            }
            if (!entAdd.IsLocal())
            {
                int foundIndex = DBComprobantes.FindInList(_db_comprobantes, entAdd);
                if (foundIndex != -1) //Update old data with new in case of match.
                {
                    _db_comprobantes[foundIndex] = entAdd;
                    return true;
                }
            }

            _db_comprobantes.Add(entAdd);
            return true;
        }
        public void RemoveComprobante(DBComprobantes entRemove)
        {
            if (_db_comprobantes.Remove(entRemove))
            {
                return;
            }
            if (entRemove.IsLocal())
            {
                return;
            }
            _db_comprobantes.RemoveAll(x => x.GetCuentaID() == entRemove.GetCuentaID() && x.GetEntidadComercialID() == entRemove.GetEntidadComercialID() && x.GetID() == entRemove.GetID());

        }
        public void SetRazonSocial(string name)
        {
            _shouldPush = _shouldPush || !_data.em_rs.Equals(name);
            _data.em_rs = name;
        }
        public void SetCUIT(long cuit)
        {
            _shouldPush = _shouldPush || (_data.em_cuit != cuit);
            _data.em_cuit = cuit;
        }

        public long GetCUIT()
        {
            return _data.em_cuit;
        }
        public string GetRazonSocial()
        {
            return _data.em_rs;
        }

        public override DBBaseClass GetLocalCopy() => new DBCuenta(-1, _data);

        public override string ToString() => $"ID: {GetID()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/
        public static string PrintAll()
        {
            string str = "";
            foreach (DBCuenta cuenta in _db_cuentas)
            {
                str += $"Cuenta> {cuenta}\n";
            }
            return str;
        }
        public string PrintAllEntidades()
        {
            string str = "";
            foreach (DBEntidades entidadComercial in _db_entidades_comerciales)
            {
                str += $"Entidad comercial> {entidadComercial}\n";
            }
            return str;
        }

        //RANDOM Generator
        private static string[] randomRS_A =
        {
            "Industrias",
            "Maquinaria",
            "Club",
            "Sociedad",
            "Pararrayos",
            "Diseño",
            "Telecomunicaciones",
            "Armeria",
            "Ferroviaria"
        };
        private static string[] randomRS_B =
        {
            "Argentino",
            "Garcia y Hermanos",
            "Fernandez e Hijos",
            "Performance",
            "Profesional",
            "Atila",
            "Zhukov",
            "Viamonte"
        };
        private static string[] randomRS_C =
        {
            "SA",
            "SRL",
            "Tech",
            ""
        };

        private static long[] randomCUITMinMax = { 20100000001, 22420000003 };

        public static DBCuenta GenerateRandom()
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            long randomCuit = randomCUITMinMax[0] + Convert.ToInt64(Convert.ToDouble(randomCUITMinMax[1]- randomCUITMinMax[0]) *r.NextDouble()+0.5);
            string randomRs = $"{randomRS_A[r.Next(0, randomRS_A.Length)]} {randomRS_B[r.Next(0, randomRS_B.Length)]} {randomRS_C[r.Next(0, randomRS_C.Length)]}";

            return new DBCuenta(randomCuit, randomRs);
        }
    }
}
