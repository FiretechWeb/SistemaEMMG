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
    public struct PagoData
    {
        public PagoData(double importe, string obs, DateTime? fecha)
        {
            pg_importe = importe;
            pg_obs = obs;
            pg_fecha = fecha;
        }
        public string pg_obs { get; set; }
        public double pg_importe { get; set; }
        public DateTime? pg_fecha { get; set; }

        public static readonly string NameOf_pg_obs = nameof(pg_obs);
        public static readonly string NameOf_pg_importe = nameof(pg_importe);
        public static readonly string NameOf_pg_fecha = nameof(pg_fecha);

        public static PagoData CreateFromReader(MySqlDataReader reader)
        {
            return new PagoData(reader.GetDoubleSafe(NameOf_pg_importe),
                                            reader.GetStringSafe(NameOf_pg_obs),
                                            reader.GetDateTimeSafe(NameOf_pg_fecha));
        }

        public override string ToString()
        {
            return $"Importe: {pg_importe} - Observación: {pg_obs} - Fecha: {pg_fecha}";
        }
    }
    //RECORDATORIO, TERMINAR DE REMPLAZAR LAS STRINGS LITERALES DE LAS CONSULTAS SQL POR las constantes NameOf de DBPago y PagoData
    public class DBPago : DBBaseClass, IDBase<DBPago>, IDBCuenta<DBCuenta>, IDBEntidadComercial<DBEntidades>, IDBComprobante<DBComprobantes>
    {
        public const string db_table = "comprobantes_pagos";
        public const string NameOf_pg_em_id = "pg_em_id";
        public const string NameOf_pg_ec_id = "pg_ec_id";
        public const string NameOf_pg_rc_id = "pg_rc_id";
        public const string NameOf_id = "pg_id";
        public const string NameOf_cp_fp_id = "cp_fp_id";
        private long _id;
        private bool _shouldPush = false;
        private PagoData _data;
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
                JOIN {cm_table} ON {cm_table}.{DBComprobantes.NameOf_id} = {db_table}.{NameOf_pg_rc_id} AND {cm_table}.{DBComprobantes.NameOf_cm_ec_id} = {db_table}.{NameOf_pg_ec_id} AND {cm_table}.{DBComprobantes.NameOf_cm_em_id} = {db_table}.{NameOf_pg_em_id} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {cm_table}.{DBComprobantes.NameOf_cm_tc_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_pg_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_pg_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} ";
        }
        string IDBase<DBPago>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static bool RemoveFromDB(MySqlConnection conn, DBCuenta cuenta, long ec_id, long cm_id, long id)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table}  WHERE {NameOf_pg_em_id} = {cuenta.GetID()} AND {NameOf_pg_ec_id} = {ec_id} AND {NameOf_pg_rc_id} = {cm_id} AND {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<static> Error tratando de eliminar una fila de la base de datos en DBPago: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }

        public static List<DBPago> GetAll(MySqlConnection conn, DBComprobantes comprobante)
        {
            List<DBPago> returnList = new List<DBPago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_pg_em_id} = {comprobante.GetCuentaID()} AND {NameOf_pg_ec_id} = {comprobante.GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {comprobante.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBPago(comprobante, new DBFormasPago(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de un comprobante. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBPago> GetAll(MySqlConnection conn, DBEntidades entidadComercial)
        {
            List<DBPago> returnList = new List<DBPago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_pg_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_pg_ec_id} = {entidadComercial.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBPago(new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), reader), new DBFormasPago(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBPago> GetAll(MySqlConnection conn, DBCuenta cuenta)
        {
            List<DBPago> returnList = new List<DBPago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE pg_em_id = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBPago(new DBComprobantes(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTiposComprobantes(reader), reader), new DBFormasPago(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBPago GetByID(MySqlConnection conn, DBComprobantes comprobante, long pg_id)
        {
            DBPago returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE pg_em_id = {comprobante.GetCuentaID()} AND pg_ec_id = {comprobante.GetEntidadComercialID()} AND pg_rc_id = {comprobante.GetID()} AND pg_id = {pg_id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBPago(comprobante, new DBFormasPago(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un pago de un comprobante en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }
        public static DBPago GetByID(MySqlConnection conn, DBCuenta cuenta, long pg_ec_id, long pg_rc_id, long pg_id)
        {
            return GetByID(conn, DBComprobantes.GetByID(conn, cuenta, pg_ec_id, pg_rc_id), pg_id);
        }

        public static DBPago GetByID(MySqlConnection conn, DBEntidades entidadComercial, long pg_rc_id, long pg_id)
        {
            return GetByID(conn, DBComprobantes.GetByID(conn, entidadComercial, pg_rc_id), pg_id);
        }

        public static DBPago GetByID(List<DBPago> listaPagos, DBCuenta cuenta, long pg_ec_id, long pg_rc_id, long pg_id)
        {
            foreach (DBPago pago in listaPagos)
            {
                if (pago.GetID() == pg_id && pago.GetComprobanteID() == pg_rc_id && pago.GetEntidadComercialID() == pg_ec_id && pago.GetCuentaID() == cuenta.GetID())
                {
                    return pago;
                }
            }

            return null;
        }

        public static DBPago GetByID(List<DBPago> listaPagos, DBEntidades entidadComercial, long pg_rc_id, long id)
        {
            return GetByID(listaPagos, entidadComercial.GetCuenta(), entidadComercial.GetID(), pg_rc_id, id);
        }

        public static DBPago GetByID(List<DBPago> listaPagos, DBComprobantes comprobante, long id)
        {
            return GetByID(listaPagos, comprobante.GetCuenta(), comprobante.GetEntidadComercialID(), comprobante.GetID(), id);
        }

        public static bool CheckIfExistsInList(List<DBPago> listaPagsComprobantes, DBPago ent)
        {
            foreach (DBPago pagoComprobante in listaPagsComprobantes)
            {
                if (pagoComprobante.GetCuentaID() == ent.GetCuentaID() && pagoComprobante.GetEntidadComercialID() == ent.GetEntidadComercialID() && pagoComprobante.GetComprobanteID() == ent.GetComprobanteID() && pagoComprobante.GetID() == ent.GetID())
                {
                    return true;
                }
            }
            return false;
        }

        public DBPago(DBComprobantes comprobante, long id, DBFormasPago formaDePago, PagoData newData)
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

        public DBPago(DBComprobantes comprobante, long id, long fp_id, PagoData newData)
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
        public DBPago(DBCuenta cuenta, long ec_id, long cm_id, long id, long fp_id, PagoData newData)
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

        public DBPago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long cm_id, long id, long fp_id, PagoData newData) //Directly from DB
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

        public DBPago(DBComprobantes comprobante, DBFormasPago formaPago, double importe, string obs, DateTime? fecha=null) : this(comprobante, -1, formaPago, new PagoData(importe, obs, fecha)) { }
        public DBPago(DBComprobantes comprobante, long id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(comprobante, id, fp_id, new PagoData(importe, obs, fecha)) { }
        public DBPago(DBComprobantes comprobante, long fp_id, double importe, string obs, DateTime? fecha = null) : this(comprobante, -1, fp_id,  importe, obs, fecha) { }

        public DBPago(DBCuenta cuenta, long ec_id, long cm_id, long id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, ec_id, cm_id, id, fp_id, new PagoData(importe, obs, fecha)) { }
        public DBPago(DBCuenta cuenta, long ec_id, long cm_id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, ec_id, cm_id, -1, fp_id, importe, obs, fecha) { }
        public DBPago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long cm_id, long id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, conn, id, ec_id, cm_id, fp_id, new PagoData(importe, obs, fecha)) { }
        public DBPago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long cm_id, long fp_id, double importe, string obs, DateTime? fecha = null) : this(cuenta, conn, ec_id, cm_id, -1, fp_id, importe, obs, fecha) { }
        
        public DBPago(DBComprobantes comprobante, DBFormasPago newFormaPago, MySqlDataReader reader) : this (
            comprobante,
            reader.GetInt64Safe(NameOf_id),
            newFormaPago,
            PagoData.CreateFromReader(reader)) { }

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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_de_pago_id = -1;
                long new_comprobante_id = -1;
                long new_entidad_comercial_id = -1;
                while (reader.Read())
                {
                    _data = PagoData.CreateFromReader(reader);
                    new_tipo_de_pago_id = reader.GetInt64Safe(NameOf_cp_fp_id);
                    new_comprobante_id = reader.GetInt64Safe(NameOf_pg_rc_id);
                    new_entidad_comercial_id = reader.GetInt64Safe(NameOf_pg_ec_id);
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
                string fechaPago = (_data.pg_fecha.HasValue) ? $"'{((DateTime)_data.pg_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_cp_fp_id} = {_formaDePago.GetID()}, 
                                {PagoData.NameOf_pg_importe} = {_data.pg_importe.ToString().Replace(",", ".")}, 
                                {PagoData.NameOf_pg_obs} = '{_data.pg_obs}', 
                                {PagoData.NameOf_pg_fecha} = {fechaPago} 
                                WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBPago::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string fechaPago = (_data.pg_fecha.HasValue) ? $"'{((DateTime)_data.pg_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"INSERT INTO {db_table} (
                                {NameOf_pg_em_id}, 
                                {NameOf_pg_ec_id}, 
                                {NameOf_pg_rc_id}, 
                                {NameOf_cp_fp_id}, 
                                {PagoData.NameOf_pg_importe}, 
                                {PagoData.NameOf_pg_obs}, 
                                {PagoData.NameOf_pg_fecha}) VALUES (
                                {GetCuentaID()}, 
                                {GetEntidadComercialID()}, 
                                {GetComprobanteID()}, 
                                {_formaDePago.GetID()}, 
                                {_data.pg_importe.ToString().Replace(",", ".")}, 
                                '{_data.pg_obs}', 
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
                MessageBox.Show("Error DBPago::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetComprobanteID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBPago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public string GetObservacion() => _data.pg_obs;

        public void SetFormaDePago(DBFormasPago newFormaDePago) => _formaDePago = newFormaDePago;

        public void SetFormaDePago(long fp_id) => _formaDePago = DBFormasPago.GetByID(fp_id);
        public void SetObservacion(string obs) => _data.pg_obs = obs;

        protected override void MakeLocal()
        {
            if (GetID() >= 0)
            {
                ChangeID(-1);
            }
        }
        public override DBBaseClass GetLocalCopy()
        {
            return new DBPago(_comprobante, -1, _formaDePago, _data);
        }

        public override string ToString()
        {
            return $"ID: {GetID()} - Forma de pago: {_formaDePago.GetName()} - {_data.ToString()}";
        }

        /**********************
         * DEBUG STUFF ONLY
         * ********************/


        public static DBPago GenerateRandom(DBComprobantes comprobante)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            DBFormasPago formaPago = DBFormasPago.GetRandom();
            DateTime fechaPago = new DateTime();
            DateTime? fechaFinal = null;
            string randomDateSTR = $"{r.Next(1, 28)}/{r.Next(1, 13)}/{r.Next(2010, 2024)}";
            if (DateTime.TryParse(randomDateSTR, out fechaPago))
            {
                fechaFinal = fechaPago;
            }

            return new DBPago(comprobante, DBFormasPago.GetRandom(), Math.Truncate(100.0*comprobante.GetTotal()*r.NextDouble())/100.0, "Sin información", fechaFinal);
        }
        
    }
}
