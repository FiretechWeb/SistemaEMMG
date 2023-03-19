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
    public struct EmpresasData
    {
        public EmpresasData(long cuit, string rs)
        {
            em_cuit = cuit;
            em_rs = rs; //Event handler to check that rs is not longer than 64 characters
        }
        public long em_cuit { get; set; }
        public string em_rs { get; set; }

        public static readonly string NameOf_em_cuit = nameof(em_cuit);
        public static readonly string NameOf_em_rs = nameof(em_rs);

        public override string ToString()
        {
            return $"Nombre Empresa: {em_rs} - CUIT: {em_cuit}";
        }
    }
    public class DBEmpresa : DBBaseClass, IDBDataType<DBEmpresa>
    {
        /*************************
         * Global Static STUFFF *
         ************************/
        public const string db_table = "empresas";
        public const string NameOf_id = "em_id";
        private static readonly List<DBEmpresa> _db_empresas = new List<DBEmpresa>();

        public static List<DBEmpresa> UpdateAll(MySqlConnection conn)
        {
            List<DBEmpresa> returnList = new List<DBEmpresa>();
            try
            {
                string query = $"SELECT * FROM {db_table}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_empresas.Clear();

                while (reader.Read())
                {
                    returnList.Add(new DBEmpresa(reader.GetInt64Safe(NameOf_id), reader.GetInt64Safe(EmpresasData.NameOf_em_cuit), reader.GetStringSafe(EmpresasData.NameOf_em_rs)));
                    _db_empresas.Add(new DBEmpresa(reader.GetInt64Safe(NameOf_id), reader.GetInt64Safe(EmpresasData.NameOf_em_cuit), reader.GetStringSafe(EmpresasData.NameOf_em_rs)));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas las empresas, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBEmpresa> GetAll()
        {
            List<DBEmpresa> returnList = new List<DBEmpresa>();
            foreach (DBEmpresa empresa in _db_empresas)
            {
                returnList.Add(empresa);
            }
            return returnList;
        }

        /***************************
         * START: IDBDataType Implementation contract
         * *****************************/

        List<DBEmpresa> IDBDataType<DBEmpresa>.UpdateAll(MySqlConnection conn)
        {
            return DBEmpresa.UpdateAll(conn);
        }

        List<DBEmpresa> IDBDataType<DBEmpresa>.GetAll()
        {
            return DBEmpresa.GetAll();
        }

        /***************************
         * END: IDBDataType Implementation contract
         * *****************************/

        public static bool EmpresaYaExiste(string str, long cuit, List<DBEmpresa> empresas)
        {
            foreach (DBEmpresa empresa in empresas)
            {
                if (str.ToLower().Equals(empresa.GetRazonSocial().ToLower()) || empresa.GetCUIT() == cuit)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool EmpresaYaExiste(string str, long cuit) => EmpresaYaExiste(str, cuit, _db_empresas);

        /***************
         * Local STUFF *
         ***************/

        private long _id;
        private EmpresasData _data;
        private readonly List<DBEntidades> _db_entidades_comerciales = new List<DBEntidades>();
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();
        public DBEmpresa(EmpresasData newData)
        {
            _data = newData;
        }

        public DBEmpresa(long id, long cuit, string rs)
        {
            _id = id;
            _data = new EmpresasData(cuit, rs);
        }
        public DBEmpresa(long cuit, string rs) : this(-1, cuit, rs) { }

        public DBEmpresa(MySqlConnection conn, int id)
        {
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
               
                while (reader.Read())
                {
                    _id = id;
                    _data = new EmpresasData(reader.GetInt64Safe(EmpresasData.NameOf_em_cuit), reader.GetStringSafe(EmpresasData.NameOf_em_rs));
                }

                reader.Close();
            } catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBEmpresa, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public override bool PushToDatabase(MySqlConnection conn)
        {
            bool? existsInDB = ExistsInDatabase(conn);
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
                string query = $"UPDATE {db_table} SET {EmpresasData.NameOf_em_cuit} = {_data.em_cuit}, {EmpresasData.NameOf_em_rs} = '{_data.em_rs}' WHERE {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToUpdate)
                {
                    ChangeID(cmd.LastInsertedId);
                }
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBEmpresa::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string query = $"INSERT INTO {db_table} ({EmpresasData.NameOf_em_cuit}, {EmpresasData.NameOf_em_rs}) VALUES ({_data.em_cuit}, '{_data.em_rs}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBEmpresa::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBEmpresa: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBEmpresa::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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



        public EmpresasData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        ///<summary>
        ///DO NOT USET! Warning this method will return null. It is not implemented yet!
        ///</summary>
        public DBEmpresa Clone()
        {
            return null;
        }

        public void SetRazonSocial(string name)
        {
            _data.em_rs = name;
        }
        public void SetCUIT(long cuit)
        {
            _data.em_cuit = cuit;
        }

        protected override void ChangeID(long id) => _id = id;

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

        public override string ToString()
        {
            return _data.ToString();
        }
    }
}
