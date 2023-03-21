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
        public static CuentaData CreateFromReader(MySqlDataReader reader)
        {
            return new CuentaData(reader.GetInt64Safe(NameOf_em_cuit), reader.GetStringSafe(NameOf_em_rs));
        }
        public override string ToString()
        {
            return $"Razón Social: {em_rs} - CUIT: {em_cuit}";
        }
    }
    public class DBCuenta : DBBaseClass, IDBase<DBCuenta>, IDBDataType<DBCuenta>
    {
        /*************************
         * Global Static STUFFF *
         ************************/
        public const string db_table = "cuentas";
        public const string NameOf_id = "em_id";
        private static readonly List<DBCuenta> _db_cuentas = new List<DBCuenta>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table}";
        }
        string IDBase<DBCuenta>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

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

        public static List<DBCuenta> GetAll()
        {
            List<DBCuenta> returnList = new List<DBCuenta>();
            foreach (DBCuenta cuenta in _db_cuentas)
            {
                returnList.Add(cuenta);
            }
            return returnList;
        }
        List<DBCuenta> IDBDataType<DBCuenta>.GetAll() => GetAll();

        public static IReadOnlyCollection<DBCuenta> GetAllLocal()
        {
            return _db_cuentas.Where(x => x.IsLocal()).ToList().AsReadOnly();
        }
        IReadOnlyCollection<DBCuenta> IDBDataType<DBCuenta>.GetAllLocal() => GetAllLocal();

        public static DBCuenta GetByID(long tc_id)
        {
            foreach (DBCuenta cuenta in _db_cuentas)
            {
                if (cuenta.GetID() == tc_id)
                {
                    return cuenta;
                }
            }
            return null;
        }

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

        private long _id;
        private bool _shouldPush = false;
        private CuentaData _data;
        private readonly List<DBEntidades> _db_entidades_comerciales = new List<DBEntidades>();
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();
        public DBCuenta(long id, CuentaData newData)
        {
            _id = id;
            _data = newData;
            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBCuenta(long id, long cuit, string rs) : this (id, new CuentaData(cuit, rs)) { }
        public DBCuenta(long cuit, string rs) : this(-1, cuit, rs) { }

        public DBCuenta(MySqlDataReader reader) : this (reader.GetInt64Safe(NameOf_id), CuentaData.CreateFromReader(reader)) { }
        public DBCuenta(MySqlConnection conn, int id)
        {
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
               
                while (reader.Read())
                {
                    _id = id;
                    _data = CuentaData.CreateFromReader(reader);
                }

                reader.Close();
            } catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBCuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
            bool wasAbleToUpdate = false;
            try
            {
                string query = $"UPDATE {db_table} SET {CuentaData.NameOf_em_cuit} = {_data.em_cuit}, {CuentaData.NameOf_em_rs} = '{_data.em_rs}' WHERE {NameOf_id} = {GetID()}";
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
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({CuentaData.NameOf_em_cuit}, {CuentaData.NameOf_em_rs}) VALUES ({_data.em_cuit}, '{_data.em_rs}')";
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
                MessageBox.Show("Error DBCuenta::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBCuenta: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        public DBComprobantes GetComprobanteByID(long ec_id, long cm_id)
        {
            return DBComprobantes.GetByID(_db_comprobantes, this, ec_id, cm_id);
        }

        public DBComprobantes GetComprobanteByIndex(int index)
        {
            if (index < 0 || index >= _db_comprobantes.Count)
            {
                return null;
            }
            return _db_comprobantes[index];
        }

        public DBComprobantes GetComprobanteByID(DBEntidades entidadComercial, long cm_id)
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

        public DBEntidades GetEntidadByID(long ec_id)
        {
            return DBEntidades.GetByID(_db_entidades_comerciales, this, ec_id);
        }

        public bool AddNewEntidad(DBEntidades newEntidadComercial)
        {
            if (newEntidadComercial.GetCuentaID() != GetID())
            {
                return false; //Cannot add an entity from another account like this...
            }
            if (DBEntidades.CheckIfExistsInList(_db_entidades_comerciales, newEntidadComercial, true))
            {
                return false; //already exists, at least cuit and name-
            } 
            _db_entidades_comerciales.Add(newEntidadComercial);
            return true;
        }
        public void RemoveEntidad(DBEntidades entRemove)
        {
            _db_entidades_comerciales.Remove(entRemove);
        }

        public bool AddNewComprobante(DBComprobantes entAdd)
        {
            if (entAdd.GetCuentaID() != GetID())
            {
                return false; //Cannot add an entity from another account like this...
            }
            if (DBComprobantes.CheckIfExistsInList(_db_comprobantes, entAdd))
            {
                return false;
            }

            _db_comprobantes.Add(entAdd);
            return true;
        }
       public void RemoveComprobante(DBComprobantes entRemove)
        {
             _db_comprobantes.Remove(entRemove);
        }


        public override bool ShouldPush() => _shouldPush;
        public override bool IsLocal() => _id < 0;

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

        protected override void ChangeID(long id)
        {
            _shouldPush = _shouldPush || (id != _id);
            _id = id;
        }
        public override long GetID()
        {
            return _id;
        }
        public long GetCUIT()
        {
            return _data.em_cuit;
        }
        public string GetRazonSocial()
        {
            return _data.em_rs;
        }

        protected override void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }

        public DBCuenta GetLocalCopy()
        {
            return new DBCuenta(-1, _data);
        }

        public override string ToString()
        {
            return $"ID: {GetID()} - {_data.ToString()}";
        }

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
    }
}
