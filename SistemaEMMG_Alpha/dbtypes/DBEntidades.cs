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
    public struct EntidadesComercialesData
    {
        public EntidadesComercialesData(long id, long cuit, long dni, string rs, string email, string tel, string cel)
        {
            ec_id = id;
            ec_cuit = cuit;
            ec_dni = dni;
            ec_rs = rs;
            ec_email = email;
            ec_telefono = tel;
            ec_celular = cel;
        }
        //Class handles ec_em_id y ec_te_id since it is better to hold references to those dataTypes instead of values
        public long ec_id { get; }
        public long ec_cuit { get; set; }
        public long ec_dni { get; set; }
        public string ec_rs { get; set; }
        public string ec_email { get; set; }
        public string ec_telefono { get; set; }
        public string ec_celular { get; set; }
    }
    public class DBEntidades : DBInterface, IDBCuenta<DBEmpresa>
    {
        private static readonly string db_table = "ent_comerciales";
        ///<summary>
        ///Business Account associated with this commercial entity.
        ///</summary>
        private readonly DBEmpresa _cuentaEmpresa;
        private EntidadesComercialesData _data;
        private DBTipoEntidad _tipoEntidad = null;
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();


        public static string GetDBTableName() => db_table;
        string DBInterface.GetDBTableName() => GetDBTableName();

        public DBEntidades(DBEmpresa newCuenta, DBTipoEntidad newTipo, EntidadesComercialesData newData)
        {
            _tipoEntidad = newTipo.Clone(); //Maybe this is not welcomed
            _cuentaEmpresa = newCuenta;
            _data = newData;
        }
        public DBEntidades(DBEmpresa newCuenta, long te_id, EntidadesComercialesData newData)
        {
            _tipoEntidad = DBTipoEntidad.GetByID(te_id);
            _cuentaEmpresa = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBEmpresa newCuenta, MySqlConnection conn, long te_id, EntidadesComercialesData newData)
        {
            _tipoEntidad = DBTipoEntidad.GetByID(te_id, conn);
            _cuentaEmpresa = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBEmpresa newCuenta, DBTipoEntidad newTipo, long id, long cuit, string rs, long dni = -1, string email = "", string tel = "", string cel = "") : this(newCuenta, newTipo, new EntidadesComercialesData(id, cuit, dni, rs, email, tel, cel)) { }
        public DBEntidades(DBEmpresa newCuenta, DBTipoEntidad newTipo, long cuit, string rs, long dni = -1, string email = "", string tel = "", string cel = "") : this(newCuenta, newTipo, - 1, cuit, rs, dni, email, tel, cel) { }

        public DBEntidades(DBEmpresa newCuenta, long te_id, long id, long cuit, string rs, long dni = -1, string email = "", string tel = "", string cel = "") : this(newCuenta, te_id, new EntidadesComercialesData(id, cuit, dni, rs, email, tel, cel)) { }
        public DBEntidades(DBEmpresa newCuenta, long te_id, long cuit, string rs, long dni = -1, string email = "", string tel = "", string cel = "") : this(newCuenta, te_id, -1, cuit, rs, dni, email, tel, cel) { }

        public DBEntidades(DBEmpresa newCuenta, MySqlConnection conn, long te_id, long id, long cuit, string rs, long dni = -1, string email = "", string tel = "", string cel = "") : this(newCuenta, conn, te_id, new EntidadesComercialesData(id, cuit, dni, rs, email, tel, cel)) { }
        public DBEntidades(DBEmpresa newCuenta, MySqlConnection conn, long te_id, long cuit, string rs, long dni = -1, string email = "", string tel = "", string cel = "") : this(newCuenta, conn, te_id, -1, cuit, rs, dni, email, tel, cel) { }


        //GetDBTableName
        public static List<DBEntidades> GetAll(MySqlConnection conn, DBEmpresa empresa)
        {
            List<DBEntidades> returnList = new List<DBEntidades>();
            try
            {
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string query = $"SELECT * FROM {db_table} JOIN {te_table} ON {te_table}.te_id = {db_table}.ec_te_id WHERE ec_em_id = {empresa.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                

                //_db_tipos_entidades.Clear();
                
                while (reader.Read())
                {
                    // _db_tipos_entidades.Add(new DBTipoEntidad(reader.GetInt64("te_id"), reader.GetString("te_nombre")));
                   returnList.Add(new DBEntidades(empresa, new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre")), reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetStringSafe("ec_rs"), reader.GetInt64Safe("ec_dni"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular"))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas las entidades de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBEntidades GetByID(MySqlConnection conn, DBEmpresa cuenta, long id)
        {
            DBEntidades returnEntidad = null;
            try
            {
                string te_table = DBTipoEntidad.GetDBTableName();
                string query = $"SELECT * FROM {db_table} JOIN {te_table} ON {te_table}.te_id = {db_table}.ec_te_id WHERE ec_em_id = {cuenta.GetID()} AND ec_id = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEntidad = new DBEntidades(cuenta, new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre")), reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetStringSafe("ec_rs"), reader.GetInt64Safe("ec_dni"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular"));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener una entidad comercial en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEntidad;
        }
        public static DBEntidades GetByID(List<DBEntidades> listaEntidades, DBEmpresa cuenta, long id)
        {
            foreach (DBEntidades entidadComercial in listaEntidades)
            {
                if (entidadComercial.GetID() == id && entidadComercial.GetCuentaID() == cuenta.GetID())
                {
                    return entidadComercial;
                }
            }
            
            return null;
        }

        public static bool CheckIfExistsInList(List<DBEntidades> listaEntidades, DBEntidades ent, bool strictNameAndCUIT = false)
        {
            foreach (DBEntidades entidadComercial in listaEntidades)
            {
                if (entidadComercial.GetCuentaID() != ent.GetCuentaID())
                {
                    continue;
                }
                if (entidadComercial.GetID() == ent.GetID())
                {
                    return true;
                } else if (strictNameAndCUIT && entidadComercial.GetCUIT() == ent.GetCUIT() && entidadComercial.GetRazonSocial().Trim().ToLower().Equals(ent.GetRazonSocial().Trim().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        public EntidadesComercialesData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public DBTipoEntidad TipoEntidad
        {
            get => _tipoEntidad;
            set
            {
                _tipoEntidad = value;
            }
        }


        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE ec_em_id = {_cuentaEmpresa.GetID()} AND ec_id = {_data.ec_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //if exists already, just update
                if (recordsCount > 0)
                {
                    query = $"UPDATE {db_table} SET ec_te_id = {_tipoEntidad.GetID()}, ec_cuit = {_data.ec_cuit}, ec_dni = {_data.ec_dni}, ec_rs = '{_data.ec_rs}', ec_email ='{_data.ec_email}', ec_telefono='{_data.ec_telefono}', ec_celular='{_data.ec_celular}' WHERE ec_em_id = {_cuentaEmpresa.GetID()} AND ec_id = {_data.ec_id}";

                }
                else //if does not exists, insert into
                {
                    query = $"INSERT INTO {db_table} (ec_em_id, ec_te_id, ec_cuit, ec_dni, ec_rs, ec_email, ec_telefono, ec_celular) VALUES ({_cuentaEmpresa.GetID()}, {_tipoEntidad.GetID()}, {_data.ec_cuit}, {_data.ec_dni}, '{_data.ec_rs}', '{_data.ec_email}', '{_data.ec_telefono}', '{_data.ec_celular}')";
                }
                //if not, add
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (recordsCount <= 0) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new EntidadesComercialesData(cmd.LastInsertedId, _data.ec_cuit, _data.ec_dni, _data.ec_rs, _data.ec_email, _data.ec_telefono, _data.ec_celular);
                }
                wasAbleToPush = true;
            }
            catch (Exception ex)
            {
                wasAbleToPush = false;
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBEntidades: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPush;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE ec_em_id = {_cuentaEmpresa.GetID()} AND ec_id = {_data.ec_id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBEntidades: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (deletedCorrectly)
            {
                _cuentaEmpresa.RemoveEntidad(this);
            }
            return deletedCorrectly;
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
        public DBComprobantes GetComprobanteByID(long cm_id)
        {
            return DBComprobantes.GetByID(_db_comprobantes, this, cm_id);
        }

        public bool AddNewComprobante(DBComprobantes newComprobante)
        {
            if (newComprobante.GetCuentaID() != GetCuentaID() || newComprobante.GetEntidadComercialID() != GetID())
            {
                return false; //Cannot add an receipt from another account or entity like this...
            }
            if (DBComprobantes.CheckIfExistsInList(_db_comprobantes, newComprobante))
            {
                return false;
            }
            _db_comprobantes.Add(newComprobante);
            _cuentaEmpresa.AddNewComprobante(newComprobante);
            return true;
        }
        public void RemoveComprobante(DBComprobantes entRemove)
        {
            _db_comprobantes.Remove(entRemove);
            _cuentaEmpresa.RemoveComprobante(entRemove);
        }

        public long GetID() => _data.ec_id;

        public long GetCuentaID() => _cuentaEmpresa.GetID();

        public DBEmpresa GetCuenta() => _cuentaEmpresa;

        public long GetCUIT() => _data.ec_cuit;

        public long GetDNI() => _data.ec_dni;
        public string GetRazonSocial() => _data.ec_rs;

        public string GetEmail() => _data.ec_email;

        public string GetTelefono() => _data.ec_telefono;

        public string GetCelular() => _data.ec_celular;

        public DBTipoEntidad GetTipoEntidad() => _tipoEntidad.Clone();

        public void SetCuit(long cuit) => _data.ec_cuit = cuit;

        public void SetRazonSocial(string rs) => _data.ec_rs = rs;

        public void SetEmail(string email) => _data.ec_email = email;

        public void SetTelefono(string tel) => _data.ec_telefono = tel;

        public void SetCelular(string cel) => _data.ec_celular = cel;

        public void SetDNI(long dni) => _data.ec_dni = dni;

        public void SetTipoEntidad(DBTipoEntidad newType) => _tipoEntidad = newType.Clone();

        public void SetTipoEntidad(long te_id) => _tipoEntidad = DBTipoEntidad.GetByID(te_id);
    }
}
