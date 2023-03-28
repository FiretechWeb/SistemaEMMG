using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct EntidadesComercialesData
    {
        public EntidadesComercialesData(long cuit, string rs, string email, string tel, string cel)
        {
            ec_cuit = cuit;
            ec_rs = rs;
            ec_email = email;
            ec_telefono = tel;
            ec_celular = cel;
        }
        public long ec_cuit { get; set; }
        public string ec_rs { get; set; }
        public string ec_email { get; set; }
        public string ec_telefono { get; set; }
        public string ec_celular { get; set; }

        public static readonly string NameOf_ec_cuit = nameof(ec_cuit);
        public static readonly string NameOf_ec_rs = nameof(ec_rs);
        public static readonly string NameOf_ec_email = nameof(ec_email);
        public static readonly string NameOf_ec_telefono = nameof(ec_telefono);
        public static readonly string NameOf_ec_celular = nameof(ec_celular);
        public static EntidadesComercialesData CreateFromReader(MySqlDataReader reader)
        {
            return new EntidadesComercialesData(reader.GetInt64Safe(NameOf_ec_cuit),
                                reader.GetStringSafe(NameOf_ec_rs),
                                reader.GetStringSafe(NameOf_ec_email),
                                reader.GetStringSafe(NameOf_ec_telefono),
                                reader.GetStringSafe(NameOf_ec_celular));
        }

        public override string ToString() => $"CUIT: {ec_cuit} - Razón Social: {ec_rs} - Email: {ec_email} - Teléfono: {ec_telefono} - Celular: {ec_celular}";
    }
    public class DBEntidades : DBBaseClass, IDBase<DBEntidades>, IDBCuenta<DBCuenta>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public const string db_table = "ent_comerciales";
        public const string NameOf_ec_te_id = "ec_te_id";
        public const string NameOf_ec_em_id = "ec_em_id";
        public const string NameOf_id = "ec_id";
        ///<summary>
        ///Business Account associated with this commercial entity.
        ///</summary>
        private readonly DBCuenta _cuenta;
        private EntidadesComercialesData _data;
        private DBTipoEntidad _tipoEntidad = null;
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();
        private readonly List<DBRecibo> _db_recibos = new List<DBRecibo>();
        private readonly List<DBRemito> _db_remitos = new List<DBRemito>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            return $"SELECT {fieldsToGet} FROM {db_table} JOIN {DBTipoEntidad.db_table} ON {DBTipoEntidad.db_table}.{DBTipoEntidad.NameOf_id} = {db_table}.{NameOf_ec_te_id}";
        }
        string IDBase<DBEntidades>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public DBEntidades(DBCuenta newCuenta, long id, DBTipoEntidad newTipo, EntidadesComercialesData newData) : base(id)
        {
            _tipoEntidad = newTipo;
            _cuenta = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBCuenta newCuenta, long id, long te_id, EntidadesComercialesData newData) : base(id)
        {
            _tipoEntidad = DBTipoEntidad.GetByID(te_id);
            _cuenta = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBCuenta newCuenta, MySqlConnection conn, long id, long te_id, EntidadesComercialesData newData) : base(id)
        {
            _tipoEntidad = DBTipoEntidad.GetByID(te_id, conn);
            _cuenta = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBCuenta newCuenta, DBTipoEntidad newTipo, long id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, id, newTipo, new EntidadesComercialesData(cuit, rs, email, tel, cel)) { }
        public DBEntidades(DBCuenta newCuenta, DBTipoEntidad newTipo, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, newTipo, -1, cuit, rs, email, tel, cel) { }

        public DBEntidades(DBCuenta newCuenta, long te_id, long id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, id, te_id, new EntidadesComercialesData(cuit, rs, email, tel, cel)) { }
        public DBEntidades(DBCuenta newCuenta, long te_id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, te_id, -1, cuit, rs, email, tel, cel) { }

        public DBEntidades(DBCuenta newCuenta, MySqlConnection conn, long te_id, long id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, conn, id, te_id, new EntidadesComercialesData(cuit, rs, email, tel, cel)) { }
        public DBEntidades(DBCuenta newCuenta, MySqlConnection conn, long te_id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, conn, te_id, -1, cuit, rs, email, tel, cel) { }

        public DBEntidades(DBCuenta newCuenta, DBTipoEntidad newTipo, MySqlDataReader reader) : this(
            newCuenta,
            reader.GetInt64Safe(NameOf_id),
            newTipo,
            EntidadesComercialesData.CreateFromReader(reader)
        ) {}

        public static List<DBEntidades> GetAll(MySqlConnection conn, DBCuenta cuenta)
        {
            List<DBEntidades> returnList = new List<DBEntidades>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_ec_em_id} = {cuenta.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    returnList.Add(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas las entidades de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBEntidades GetByID(MySqlConnection conn, DBCuenta cuenta, long id)
        {
            DBEntidades returnEntidad = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_ec_em_id} = {cuenta.GetID()} AND {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEntidad = new DBEntidades(cuenta, new DBTipoEntidad(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener una entidad comercial en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEntidad;
        }
        public static DBEntidades GetByID(List<DBEntidades> listaEntidades, DBCuenta cuenta, long id)
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

        public static List<DBEntidades> Search(MySqlConnection conn, DBCuenta cuenta, string toFind)
        {
            List<DBEntidades> returnList = new List<DBEntidades>();
            try
            {
                string te_table = DBTipoEntidad.db_table;
                string query = $"SELECT * FROM {db_table} JOIN {te_table} ON {te_table}.te_id = {db_table}.ec_te_id WHERE ec_em_id = {cuenta.GetID()} AND (LOWER(ec_rs) LIKE '%{toFind.Trim().ToLower()}%' OR ec_cuit LIKE '%{toFind.Trim().ToLower()}%')";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error SQL en  DBEntidades::Search" + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_ec_em_id} = {_cuenta.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_entidad_id = -1;
                while (reader.Read())
                {
                    _data = EntidadesComercialesData.CreateFromReader(reader);
                    new_tipo_entidad_id = reader.GetInt64Safe(NameOf_ec_te_id);
                    wasAbleToPull = true;
                    _shouldPush = false;
                }
                reader.Close();
                if (new_tipo_entidad_id != -1)
                {
                    _tipoEntidad = DBTipoEntidad.GetByID(new_tipo_entidad_id, conn);
                }
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBEntidades::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $@"UPDATE {db_table} SET {NameOf_ec_te_id} = {_tipoEntidad.GetID()}, 
                                {EntidadesComercialesData.NameOf_ec_cuit} = {_data.ec_cuit}, 
                                {EntidadesComercialesData.NameOf_ec_rs} = '{Regex.Replace(_data.ec_rs.Trim(), @"\s+", " ")}', 
                                {EntidadesComercialesData.NameOf_ec_email} = '{_data.ec_email.Trim()}', 
                                {EntidadesComercialesData.NameOf_ec_telefono} ='{_data.ec_telefono.Trim()}', 
                                {EntidadesComercialesData.NameOf_ec_celular} ='{_data.ec_celular.Trim()}' 
                                WHERE {NameOf_ec_em_id} = {_cuenta.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBEntidades::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                                {NameOf_ec_em_id}, {NameOf_ec_te_id}, 
                                {EntidadesComercialesData.NameOf_ec_cuit}, 
                                {EntidadesComercialesData.NameOf_ec_rs}, 
                                {EntidadesComercialesData.NameOf_ec_email}, 
                                {EntidadesComercialesData.NameOf_ec_telefono}, 
                                {EntidadesComercialesData.NameOf_ec_celular}) 
                                VALUES ({_cuenta.GetID()}, 
                                        {_tipoEntidad.GetID()}, 
                                        {_data.ec_cuit}, 
                                        '{Regex.Replace(_data.ec_rs.Trim(), @"\s+", " ")}', 
                                        '{_data.ec_email.Trim()}', 
                                        '{_data.ec_telefono.Trim()}', 
                                        '{_data.ec_celular.Trim()}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    _cuenta.AddNewEntidad(this); //safe to add to since now it belongs to de DB.
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBEntidades::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {DBRecibo.db_relation_table} WHERE rp_em_id = {GetCuentaID()} AND rp_ec_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM remitos_comprobantes WHERE rt_em_id = {GetCuentaID()} AND rt_ec_id = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBPago.db_table} WHERE {DBPago.NameOf_pg_em_id} = {GetCuentaID()} AND {DBPago.NameOf_pg_ec_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM remitos WHERE rm_em_id = {GetCuentaID()} AND rm_ec_id = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBRecibo.db_table} WHERE {DBRecibo.NameOf_rc_em_id} = {GetCuentaID()} AND {DBRecibo.NameOf_rc_ec_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBComprobantes.db_table} WHERE {DBComprobantes.NameOf_cm_em_id} = {GetCuentaID()} AND {DBComprobantes.NameOf_cm_ec_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error tratando de eliminar información relacionada a una entidad comercial en DBEntidades: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }
        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            if (!DeleteAllRelatedData(conn))
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_ec_em_id} = {_cuenta.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _cuenta.RemoveEntidad(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBEntidades: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_ec_em_id} = {GetCuentaID()} AND {NameOf_id} <> {GetID()} AND {EntidadesComercialesData.NameOf_ec_cuit} = {_data.ec_cuit} AND UPPER({EntidadesComercialesData.NameOf_ec_rs}) = '{Regex.Replace(_data.ec_rs.Trim().ToUpper(), @"\s+", " ")}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBEntidades::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_ec_em_id} = {_cuenta.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBEntidades::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        public DBComprobantes GetComprobanteByID(long cm_id) => DBComprobantes.GetByID(_db_comprobantes, this, cm_id);

        public bool AddNewComprobante(DBComprobantes newComprobante)
        {
            if (newComprobante is null)
            {
                return false;
            }
            if (newComprobante.GetCuentaID() != GetCuentaID() || newComprobante.GetEntidadComercialID() != GetID())
            {
                return false; //Cannot add an receipt from another account or entity like this...
            }
            if (_db_comprobantes.Contains(newComprobante))
            {
                return false;
            }
            _db_comprobantes.Add(newComprobante);
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

        public List<DBRecibo> GetAllRecibos(MySqlConnection conn) //Get directly from database
        {
            List<DBRecibo> returnList = DBRecibo.GetAll(conn, this);
            _db_recibos.Clear();
            foreach (DBRecibo recibo in returnList)
            {
                _db_recibos.Add(recibo);
            }
            return returnList;
        }
        public List<DBRecibo> GetAllRecibos() //Get CACHE
        {
            List<DBRecibo> returnList = new List<DBRecibo>();
            foreach (DBRecibo recibo in _db_recibos)
            {
                returnList.Add(recibo);
            }
            return returnList;
        }
        public DBRecibo GetReciboByID(long rc_id) => DBRecibo.GetByID(_db_recibos, this, rc_id);

        public bool AddNewRecibo(DBRecibo newRecibo)
        {
            if (newRecibo is null)
            {
                return false;
            }
            if (newRecibo.GetCuentaID() != GetCuentaID() || newRecibo.GetEntidadComercialID() != GetID())
            {
                return false; //Cannot add an receipt from another account or entity like this...
            }
            if (_db_recibos.Contains(newRecibo))
            {
                return false;
            }
            _db_recibos.Add(newRecibo);
            return true;
        }
        public void RemoveRecibo(DBRecibo entRemove)
        {
            if (_db_recibos.Remove(entRemove))
            {
                return;
            }
            if (entRemove.IsLocal())
            {
                return;
            }
            _db_recibos.RemoveAll(x => x.GetCuentaID() == entRemove.GetCuentaID() && x.GetEntidadComercialID() == entRemove.GetEntidadComercialID() && x.GetID() == entRemove.GetID());
        }

        public List<DBRemito> GetAllRemitos(MySqlConnection conn) //Get directly from database
        {
            List<DBRemito> returnList = DBRemito.GetAll(conn, this);
            _db_remitos.Clear();
            foreach (DBRemito recibo in returnList)
            {
                _db_remitos.Add(recibo);
            }
            return returnList;
        }
        public List<DBRemito> GetAllRemitos() //Get CACHE
        {
            List<DBRemito> returnList = new List<DBRemito>();
            foreach (DBRemito recibo in _db_remitos)
            {
                returnList.Add(recibo);
            }
            return returnList;
        }
        public DBRemito GetRemitoByID(long rc_id) => DBRemito.GetByID(_db_remitos, this, rc_id);

        public bool AddNewRemito(DBRemito newRecibo)
        {
            if (newRecibo is null)
            {
                return false;
            }
            if (newRecibo.GetCuentaID() != GetCuentaID() || newRecibo.GetEntidadComercialID() != GetID())
            {
                return false; //Cannot add an receipt from another account or entity like this...
            }
            if (_db_remitos.Contains(newRecibo))
            {
                return false;
            }
            _db_remitos.Add(newRecibo);
            return true;
        }
        public void RemoveRemito(DBRemito entRemove)
        {
            if (_db_remitos.Remove(entRemove))
            {
                return;
            }
            if (entRemove.IsLocal())
            {
                return;
            }
            _db_remitos.RemoveAll(x => x.GetCuentaID() == entRemove.GetCuentaID() && x.GetEntidadComercialID() == entRemove.GetEntidadComercialID() && x.GetID() == entRemove.GetID());
        }
        public long GetCuentaID() => _cuenta.GetID();

        public DBCuenta GetCuenta() => _cuenta;

        public long GetCUIT() => _data.ec_cuit;

        public string GetRazonSocial() => _data.ec_rs;

        public string GetEmail() => _data.ec_email;

        public string GetTelefono() => _data.ec_telefono;

        public string GetCelular() => _data.ec_celular;

        public DBTipoEntidad GetTipoEntidad() => _tipoEntidad;

        public void SetCuit(long cuit)
        {
            _shouldPush = _shouldPush || (_data.ec_cuit != cuit);
            _data.ec_cuit = cuit;
        }


        public void SetRazonSocial(string rs)
        {
            _shouldPush = _shouldPush || !_data.ec_rs.Equals(rs);
            _data.ec_rs = rs;
        }

        public void SetEmail(string email)
        {
            _shouldPush = _shouldPush || !_data.ec_email.Equals(email);
            _data.ec_email = email;
        }
        public void SetTelefono(string tel)
        {
            _shouldPush = _shouldPush || !_data.ec_telefono.Equals(tel);
            _data.ec_telefono = tel;
        }
        public void SetCelular(string cel)
        {
            _shouldPush = _shouldPush || !_data.ec_celular.Equals(cel);
            _data.ec_celular = cel;
        }
        public void SetTipoEntidad(DBTipoEntidad newType)
        {
            _shouldPush = _shouldPush || (_tipoEntidad.GetID() != newType.GetID());
            _tipoEntidad = newType;
        }
        public void SetTipoEntidad(long te_id)
        {
            _shouldPush = _shouldPush || (_tipoEntidad.GetID() != te_id);
            _tipoEntidad = DBTipoEntidad.GetByID(te_id);
        }

        public override DBBaseClass GetLocalCopy() => new DBEntidades(_cuenta, -1, _tipoEntidad, _data);

        public override string ToString() => $"ID: {GetID()} - Tipo Entidad: {_tipoEntidad.GetName()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/

        public string PrintAllComprobantes()
        {
            string str = "";
            foreach (DBComprobantes comprobante in _db_comprobantes)
            {
                str += $"Comprobante> {comprobante}\n";
            }
            return str;
        }
        public string PrintAllRecibos()
        {
            string str = "";
            foreach (DBRecibo recibo in _db_recibos)
            {
                str += $"Recibo> {recibo}\n";
            }
            return str;
        }
        public string PrintAllRemitos()
        {
            string str = "";
            foreach (DBRemito remito in _db_remitos)
            {
                str += $"Remito> {remito}\n";
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
            "Ferroviaria",
            "Motores"
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
            "Viamonte",
            "Pucará"
        };
        private static string[] randomRS_C =
        {
            "SA",
            "SRL",
            "Tech",
            ""
        };
        private static string[] randomEmail_A =
        {
            "administracion",
            "info",
            "comercio",
            "contacto"
        };
        private static string[] randomEmail_B =
        {
            "argentino",
            "garciahnos",
            "fernandez_e_hijos",
            "performance",
            "profesional",
            "atila",
            "gzhukov",
            "viamonte94",
            "pucaramalvinas"
        };
        private static string[] randomEmail_C =
        {
            ".com",
            ".com.ar",
            ".gov.ar"
        };

        private static long[] randomCUITMinMax = { 20100000001, 22420000003 };

        public static DBEntidades GenerateRandom(DBCuenta cuenta)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            //DBTipoEntidad tipEntidad = DBTipoEntidad.G
            long randomCuit = randomCUITMinMax[0] + Convert.ToInt64(Convert.ToDouble(randomCUITMinMax[1] - randomCUITMinMax[0]) * r.NextDouble() + 0.5);
            int randB = r.Next(0, randomRS_B.Length);
            string randomRs = $"{randomRS_A[r.Next(0, randomRS_A.Length)]} {randomRS_B[randB]} {randomRS_C[r.Next(0, randomRS_C.Length)]}";
            string randomEmail = $"{randomEmail_A[r.Next(0, randomEmail_A.Length)]}@{randomEmail_B[randB]}{randomEmail_C[r.Next(0, randomEmail_C.Length)]}";

            return new DBEntidades(cuenta, DBTipoEntidad.GetRandom(), randomCuit, randomRs, randomEmail, $"{r.Next(100, 9999)}-{r.Next(1000, 9999)}", $"11{r.Next(10000000, 99999999)}");
        }
    }
}
