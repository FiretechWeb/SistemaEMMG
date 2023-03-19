﻿using System;
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
    }
    public class DBEntidades : DBBaseClass, IDBCuenta<DBEmpresa>
    {
        ///<summary>
        ///Contains the name of the table where this element is stored at the Database.
        ///</summary>
        public static readonly string db_table = "ent_comerciales";
        public static readonly string NameOf_ec_te_id = "ec_te_id";
        public static readonly string NameOf_ec_em_id = "ec_em_id";
        public static readonly string NameOf_id = "ec_id";
        ///<summary>
        ///Business Account associated with this commercial entity.
        ///</summary>
        private readonly DBEmpresa _cuentaEmpresa;
        private long _id;
        private EntidadesComercialesData _data;
        private DBTipoEntidad _tipoEntidad = null;
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();

        public DBEntidades(DBEmpresa newCuenta, long id, DBTipoEntidad newTipo, EntidadesComercialesData newData)
        {
            _id = id;
            _tipoEntidad = newTipo;
            _cuentaEmpresa = newCuenta;
            _data = newData;
        }
        public DBEntidades(DBEmpresa newCuenta, long id, long te_id, EntidadesComercialesData newData)
        {
            _id = id;
            _tipoEntidad = DBTipoEntidad.GetByID(te_id);
            _cuentaEmpresa = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBEmpresa newCuenta, MySqlConnection conn, long id, long te_id, EntidadesComercialesData newData)
        {
            _id = id;
            _tipoEntidad = DBTipoEntidad.GetByID(te_id, conn);
            _cuentaEmpresa = newCuenta;
            _data = newData;
        }

        public DBEntidades(DBEmpresa newCuenta, DBTipoEntidad newTipo, long id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, id, newTipo, new EntidadesComercialesData(cuit, rs, email, tel, cel)) { }
        public DBEntidades(DBEmpresa newCuenta, DBTipoEntidad newTipo, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, newTipo, -1, cuit, rs, email, tel, cel) { }

        public DBEntidades(DBEmpresa newCuenta, long te_id, long id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, id, te_id, new EntidadesComercialesData(cuit, rs, email, tel, cel)) { }
        public DBEntidades(DBEmpresa newCuenta, long te_id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, te_id, -1, cuit, rs, email, tel, cel) { }

        public DBEntidades(DBEmpresa newCuenta, MySqlConnection conn, long te_id, long id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, conn, id, te_id, new EntidadesComercialesData(cuit, rs, email, tel, cel)) { }
        public DBEntidades(DBEmpresa newCuenta, MySqlConnection conn, long te_id, long cuit, string rs, string email = "", string tel = "", string cel = "") : this(newCuenta, conn, te_id, -1, cuit, rs, email, tel, cel) { }


        public static List<DBEntidades> GetAll(MySqlConnection conn, DBEmpresa cuenta)
        {
            List<DBEntidades> returnList = new List<DBEntidades>();
            try
            {
                string query = $"SELECT * FROM {db_table} JOIN {DBTipoEntidad.db_table} ON {DBTipoEntidad.db_table}.{DBTipoEntidad.NameOf_id} = {db_table}.{NameOf_ec_te_id} WHERE {NameOf_ec_em_id} = {cuenta.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                   returnList.Add(
                       new DBEntidades(
                           cuenta,
                           new DBTipoEntidad(
                               reader.GetInt64Safe(DBTipoEntidad.NameOf_id),
                               reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre)),
                           reader.GetInt64Safe(NameOf_id),
                           reader.GetInt64Safe(EntidadesComercialesData.NameOf_ec_cuit),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_rs),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_email),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_telefono),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_celular)
                  ));
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
                string query = $"SELECT * FROM {db_table} JOIN {DBTipoEntidad.db_table} ON {DBTipoEntidad.db_table}.{DBTipoEntidad.NameOf_id} = {db_table}.{NameOf_ec_te_id} WHERE {NameOf_ec_em_id} = {cuenta.GetID()} AND {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    returnEntidad = new DBEntidades(
                           cuenta,
                           new DBTipoEntidad(
                               reader.GetInt64Safe(DBTipoEntidad.NameOf_id),
                               reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre)),
                           reader.GetInt64Safe(NameOf_id),
                           reader.GetInt64Safe(EntidadesComercialesData.NameOf_ec_cuit),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_rs),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_email),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_telefono),
                           reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_celular));
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
                string query = $@"UPDATE {db_table} SET {NameOf_ec_te_id} = {_tipoEntidad.GetID()}, 
                                {EntidadesComercialesData.NameOf_ec_cuit} = {_data.ec_cuit}, 
                                {EntidadesComercialesData.NameOf_ec_rs} = '{_data.ec_rs}', 
                                {EntidadesComercialesData.NameOf_ec_email} = '{_data.ec_email}', 
                                {EntidadesComercialesData.NameOf_ec_telefono} ='{_data.ec_telefono}', 
                                {EntidadesComercialesData.NameOf_ec_celular} ='{_data.ec_celular}' 
                                WHERE {NameOf_ec_em_id} = {_cuentaEmpresa.GetID()} AND {NameOf_id} = {GetID()}";
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
                MessageBox.Show("Error en DBEntidades::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
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
                                VALUES ({_cuentaEmpresa.GetID()}, 
                                        {_tipoEntidad.GetID()}, 
                                        {_data.ec_cuit}, 
                                        '{_data.ec_rs}', 
                                        '{_data.ec_email}', 
                                        '{_data.ec_telefono}', 
                                        '{_data.ec_celular}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBEntidades::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_ec_em_id} = {_cuentaEmpresa.GetID()} AND {NameOf_id} = {GetID()}";
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

        public override bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_ec_em_id} = {_cuentaEmpresa.GetID()} AND {NameOf_id} = {GetID()}";
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

        protected override void ChangeID(long id) => _id = id;
        public override long GetID() => _id;

        public long GetCuentaID() => _cuentaEmpresa.GetID();

        public DBEmpresa GetCuenta() => _cuentaEmpresa;

        public long GetCUIT() => _data.ec_cuit;

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

        public void SetTipoEntidad(DBTipoEntidad newType) => _tipoEntidad = newType.Clone();

        public void SetTipoEntidad(long te_id) => _tipoEntidad = DBTipoEntidad.GetByID(te_id);
    }
}
