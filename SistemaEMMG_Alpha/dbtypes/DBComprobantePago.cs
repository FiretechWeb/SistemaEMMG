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
    public struct ComprobantePagoData
    {
        public ComprobantePagoData(double importe, string obs, DateTime? fecha)
        {
            cp_importe = importe;
            cp_obs = obs;
            cp_fecha = fecha;
        }
        public string cp_obs { get; set; }
        public double cp_importe { get; set; }
        public DateTime? cp_fecha { get; set; }

        public static readonly string NameOf_cp_obs = nameof(cp_obs);
        public static readonly string NameOf_cp_importe = nameof(cp_importe);
        public static readonly string NameOf_cp_fecha = nameof(cp_fecha);

        public static ComprobantePagoData CreateFromReader(MySqlDataReader reader)
        {
            return new ComprobantePagoData(reader.GetDoubleSafe(NameOf_cp_importe),
                                            reader.GetStringSafe(NameOf_cp_obs),
                                            reader.GetDateTimeSafe(NameOf_cp_fecha));
        }

        public override string ToString()
        {
            return $"Importe: {cp_importe} - Observación: {cp_obs} - Fecha: {cp_fecha}";
        }
    }
    //RECORDATORIO, TERMINAR DE REMPLAZAR LAS STRINGS LITERALES DE LAS CONSULTAS SQL POR las constantes NameOf de DBComprobantePago y ComprobantePagoData
    public class DBComprobantePago : DBBaseClass, IDBase<DBComprobantePago>, IDBCuenta<DBCuenta>, IDBEntidadComercial<DBEntidades>, IDBComprobante<DBComprobantes>
    {
        public const string db_table = "comprobantes_pagos";
        public const string NameOf_cp_em_id = "cp_em_id";
        public const string NameOf_cp_ec_id = "cp_ec_id";
        public const string NameOf_cp_cm_id = "cp_cm_id";
        public const string NameOf_id = "cp_id";
        public const string NameOf_cp_fp_id = "cp_fp_id";
        private long _id;
        private bool _shouldPush = false;
        private ComprobantePagoData _data;
        private DBComprobantes _comprobante;
        private DBFormasPago _formaDePago;

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            string te_table = DBTipoEntidad.db_table;
            string ec_table = DBEntidades.db_table;
            string tc_table = DBTiposComprobantes.db_table;
            string fp_table = DBFormasPago.db_table;
            string cm_table = DBComprobantes.db_table;

            return $@"SELECT {fieldsToGet} FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.{DBFormasPago.NameOf_id} = {db_table}.{NameOf_cp_fp_id} 
                JOIN {cm_table} ON {cm_table}.{DBComprobantes.NameOf_id} = {db_table}.{NameOf_cp_cm_id} AND {cm_table}.{DBComprobantes.NameOf_cm_ec_id} = {db_table}.{NameOf_cp_ec_id} AND {cm_table}.{DBComprobantes.NameOf_cm_em_id} = {db_table}.{NameOf_cp_em_id} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {cm_table}.{DBComprobantes.NameOf_cm_tc_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_cp_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_cp_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} ";
        }
        string IDBase<DBComprobantePago>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static bool RemoveFromDB(MySqlConnection conn, DBCuenta cuenta, long ec_id, long cm_id, long id)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table}  WHERE {NameOf_cp_em_id} = {cuenta.GetID()} AND {NameOf_cp_ec_id} = {ec_id} AND {NameOf_cp_cm_id} = {cm_id} AND {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<static> Error tratando de eliminar una fila de la base de datos en DBComprobantePago: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }

        public static List<DBComprobantePago> GetAll(MySqlConnection conn, DBComprobantes comprobante)
        {
            List<DBComprobantePago> returnList = new List<DBComprobantePago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cp_em_id} = {comprobante.GetCuentaID()} AND {NameOf_cp_ec_id} = {comprobante.GetEntidadComercialID()} AND {NameOf_cp_cm_id} = {comprobante.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantePago(comprobante, new DBFormasPago(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de un comprobante. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBComprobantePago> GetAll(MySqlConnection conn, DBEntidades entidadComercial)
        {
            List<DBComprobantePago> returnList = new List<DBComprobantePago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cp_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cp_ec_id} = {entidadComercial.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantePago(new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), reader), new DBFormasPago(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBComprobantePago> GetAll(MySqlConnection conn, DBCuenta cuenta)
        {
            List<DBComprobantePago> returnList = new List<DBComprobantePago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE cp_em_id = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantePago(new DBComprobantes(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTiposComprobantes(reader), reader), new DBFormasPago(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBComprobantePago GetByID(MySqlConnection conn, DBComprobantes comprobante, long cp_id)
        {
            DBComprobantePago returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE cp_em_id = {comprobante.GetCuentaID()} AND cp_ec_id = {comprobante.GetEntidadComercialID()} AND cp_cm_id = {comprobante.GetID()} AND cp_id = {cp_id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBComprobantePago(comprobante, new DBFormasPago(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un pago de un comprobante en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }
        public static DBComprobantePago GetByID(MySqlConnection conn, DBCuenta cuenta, long cp_ec_id, long cp_cm_id, long cp_id)
        {
            return GetByID(conn, DBComprobantes.GetByID(conn, cuenta, cp_ec_id, cp_cm_id), cp_id);
        }

        public static DBComprobantePago GetByID(MySqlConnection conn, DBEntidades entidadComercial, long cp_cm_id, long cp_id)
        {
            return GetByID(conn, DBComprobantes.GetByID(conn, entidadComercial, cp_cm_id), cp_id);
        }

        public static DBComprobantePago GetByID(List<DBComprobantePago> listaPagos, DBCuenta cuenta, long cp_ec_id, long cp_cm_id, long cp_id)
        {
            foreach (DBComprobantePago pago in listaPagos)
            {
                if (pago.GetID() == cp_id && pago.GetComprobanteID() == cp_cm_id && pago.GetEntidadComercialID() == cp_ec_id && pago.GetCuentaID() == cuenta.GetID())
                {
                    return pago;
                }
            }

            return null;
        }

        public static DBComprobantePago GetByID(List<DBComprobantePago> listaPagos, DBEntidades entidadComercial, long cp_cm_id, long id)
        {
            return GetByID(listaPagos, entidadComercial.GetCuenta(), entidadComercial.GetID(), cp_cm_id, id);
        }

        public static DBComprobantePago GetByID(List<DBComprobantePago> listaPagos, DBComprobantes comprobante, long id)
        {
            return GetByID(listaPagos, comprobante.GetCuenta(), comprobante.GetEntidadComercialID(), comprobante.GetID(), id);
        }

        public static bool CheckIfExistsInList(List<DBComprobantePago> listaPagsComprobantes, DBComprobantePago ent)
        {
            foreach (DBComprobantePago pagoComprobante in listaPagsComprobantes)
            {
                if (pagoComprobante.GetCuentaID() == ent.GetCuentaID() && pagoComprobante.GetEntidadComercialID() == ent.GetEntidadComercialID() && pagoComprobante.GetComprobanteID() == ent.GetComprobanteID() && pagoComprobante.GetID() == ent.GetID())
                {
                    return true;
                }
            }
            return false;
        }

        public DBComprobantePago(DBComprobantes comprobante, long id, DBFormasPago formaDePago, ComprobantePagoData newData)
        {
            _id = id;
            if (formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _formaDePago = formaDePago;
            _comprobante = comprobante;
            _data = newData;

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBComprobantePago(DBComprobantes comprobante, long id, long fp_id, ComprobantePagoData newData)
        {
            _id = id;
            _formaDePago = DBFormasPago.GetByID(fp_id);
            if (_formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _comprobante = comprobante;
            _data = newData;

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }
        public DBComprobantePago(DBCuenta cuenta, long ec_id, long cm_id, long id, long fp_id, ComprobantePagoData newData)
        {
            _id = id;
            _formaDePago = DBFormasPago.GetByID(fp_id);
            if (_formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _comprobante = cuenta.GetComprobanteByID(ec_id, cm_id);
            _data = newData;

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBComprobantePago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long cm_id, long id, long fp_id, ComprobantePagoData newData) //Directly from DB
        {
            _id = id;
            _formaDePago = DBFormasPago.GetByID(fp_id, conn);
            if (_formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _comprobante = DBComprobantes.GetByID(conn, cuenta, ec_id, cm_id);
            _data = newData;

            if (IsLocal())
            {
                _shouldPush = true;
            }
        }

        public DBComprobantePago(DBComprobantes comprobante, long id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(comprobante, id, fp_id, new ComprobantePagoData(importe, obs, fecha)) { }
        public DBComprobantePago(DBComprobantes comprobante, long fp_id, double importe, string obs, DateTime? fecha = null) : this(comprobante, -1, fp_id,  importe, obs, fecha) { }

        public DBComprobantePago(DBCuenta cuenta, long ec_id, long cm_id, long id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, ec_id, cm_id, id, fp_id, new ComprobantePagoData(importe, obs, fecha)) { }
        public DBComprobantePago(DBCuenta cuenta, long ec_id, long cm_id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, ec_id, cm_id, -1, fp_id, importe, obs, fecha) { }
        public DBComprobantePago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long cm_id, long id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, conn, id, ec_id, cm_id, fp_id, new ComprobantePagoData(importe, obs, fecha)) { }
        public DBComprobantePago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long cm_id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, conn, ec_id, cm_id, -1, fp_id, importe, obs, fecha) { }
        
        public DBComprobantePago(DBComprobantes comprobante, DBFormasPago newFormaPago, MySqlDataReader reader) : this (
            comprobante,
            reader.GetInt64Safe(NameOf_id),
            newFormaPago,
            ComprobantePagoData.CreateFromReader(reader)) { }

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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_cp_em_id} = {GetCuentaID()} AND {NameOf_cp_ec_id} = {GetEntidadComercialID()} AND {NameOf_cp_cm_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_de_pago_id = -1;
                long new_comprobante_id = -1;
                long new_entidad_comercial_id = -1;
                while (reader.Read())
                {
                    _data = ComprobantePagoData.CreateFromReader(reader);
                    new_tipo_de_pago_id = reader.GetInt64Safe(NameOf_cp_fp_id);
                    new_comprobante_id = reader.GetInt64Safe(NameOf_cp_cm_id);
                    new_entidad_comercial_id = reader.GetInt64Safe(NameOf_cp_ec_id);
                    _shouldPush = false;
                }
                reader.Close();

                if (new_tipo_de_pago_id != -1)
                {
                    _formaDePago = DBFormasPago.GetByID(new_tipo_de_pago_id, conn);
                }
                if (new_entidad_comercial_id !=-1 && new_comprobante_id != -1)
                {
                    _comprobante = DBComprobantes.GetByID(conn, DBEntidades.GetByID(conn, GetCuenta(), new_entidad_comercial_id), new_comprobante_id);
                }
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
                string fechaPago = (_data.cp_fecha.HasValue) ? $"'{((DateTime)_data.cp_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_cp_fp_id} = {_formaDePago.GetID()}, 
                                {ComprobantePagoData.NameOf_cp_importe} = {_data.cp_importe.ToString().Replace(",", ".")}, 
                                {ComprobantePagoData.NameOf_cp_obs} = '{_data.cp_obs}', 
                                {ComprobantePagoData.NameOf_cp_fecha} = {fechaPago} 
                                WHERE {NameOf_cp_em_id} = {GetCuentaID()} AND {NameOf_cp_ec_id} = {GetEntidadComercialID()} AND {NameOf_cp_cm_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBComprobantePago::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string fechaPago = (_data.cp_fecha.HasValue) ? $"'{((DateTime)_data.cp_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"INSERT INTO {db_table} (
                                {NameOf_cp_em_id}, 
                                {NameOf_cp_ec_id}, 
                                {NameOf_cp_cm_id}, 
                                {NameOf_cp_fp_id}, 
                                {ComprobantePagoData.NameOf_cp_importe}, 
                                {ComprobantePagoData.NameOf_cp_obs}, 
                                {ComprobantePagoData.NameOf_cp_fecha}) VALUES (
                                {GetCuentaID()}, 
                                {GetEntidadComercialID()}, 
                                {GetComprobanteID()}, 
                                {_formaDePago.GetID()}, 
                                {_data.cp_importe.ToString().Replace(",", ".")}, 
                                '{_data.cp_obs}', 
                                {fechaPago})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    _comprobante.AddPago(this);
                }
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBComprobantePago::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_cp_em_id} = {GetCuentaID()} AND {NameOf_cp_ec_id} = {GetEntidadComercialID()} AND {NameOf_cp_cm_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _comprobante.RemovePago(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DeleteFromDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_cp_em_id} = {GetCuentaID()} AND {NameOf_cp_ec_id} = {GetEntidadComercialID()} AND {NameOf_cp_cm_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBComprobantePago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public override bool ShouldPush() => _shouldPush;
        public override bool IsLocal() => _id < 0;

        protected override void ChangeID(long id) => _id = id;
        public override long GetID() => _id;

        public long GetCuentaID() => _comprobante.GetCuentaID();
        public DBCuenta GetCuenta() => _comprobante.GetCuenta();

        public long GetEntidadComercialID() => _comprobante.GetEntidadComercialID();

        public DBEntidades GetEntidadComercial() => _comprobante.GetEntidadComercial();

        public long GetComprobanteID() => _comprobante.GetID();

        public DBComprobantes GetComprobante() => _comprobante;

        public DBFormasPago GetFormaDePago() => _formaDePago;

        public string GetObservacion() => _data.cp_obs;

        public void SetFormaDePago(DBFormasPago newFormaDePago) => _formaDePago = newFormaDePago;

        public void SetFormaDePago(long fp_id) => _formaDePago = DBFormasPago.GetByID(fp_id);
        public void SetObservacion(string obs) => _data.cp_obs = obs;

        protected override void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }
        public override DBBaseClass GetLocalCopy()
        {
            return new DBComprobantePago(_comprobante, -1, _formaDePago, _data);
        }

        public override string ToString()
        {
            return $"ID: {GetID()} - Forma de pago: {_formaDePago.GetName()} - {_data.ToString()}";
        }
    }
}
