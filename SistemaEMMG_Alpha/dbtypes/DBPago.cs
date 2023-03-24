﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct PagoData
    {
        public PagoData(double importe, string obs, DateTime? fecha, double cambio)
        {
            pg_importe = importe;
            pg_obs = obs;
            pg_fecha = fecha;
            pg_cambio = cambio;
        }
        public string pg_obs { get; set; }
        public double pg_importe { get; set; }
        public DateTime? pg_fecha { get; set; }
        public double pg_cambio { get; set; }

        public static readonly string NameOf_pg_obs = nameof(pg_obs);
        public static readonly string NameOf_pg_importe = nameof(pg_importe);
        public static readonly string NameOf_pg_fecha = nameof(pg_fecha);
        public static readonly string NameOf_pg_cambio = nameof(pg_cambio);

        public static PagoData CreateFromReader(MySqlDataReader reader)
        {
            return new PagoData(reader.GetDoubleSafe(NameOf_pg_importe),
                                            reader.GetStringSafe(NameOf_pg_obs),
                                            reader.GetDateTimeSafe(NameOf_pg_fecha),
                                            reader.GetDoubleSafe(NameOf_pg_cambio));
        }

        public override string ToString() => $"Importe: {pg_importe} - Observación: {pg_obs} - Fecha: {pg_fecha} - Cambio: {pg_cambio}";
    }
    //RECORDATORIO, TERMINAR DE REMPLAZAR LAS STRINGS LITERALES DE LAS CONSULTAS SQL POR las constantes NameOf de DBPago y PagoData
    public class DBPago : DBBaseClass, IDBase<DBPago>, IDBCuenta<DBCuenta>, IDBEntidadComercial<DBEntidades>, IDBRecibo<DBRecibo>
    {
        public const string db_table = "pagos";
        public const string NameOf_pg_em_id = "pg_em_id";
        public const string NameOf_pg_ec_id = "pg_ec_id";
        public const string NameOf_pg_rc_id = "pg_rc_id";
        public const string NameOf_id = "pg_id";
        public const string NameOf_pg_fp_id = "pg_fp_id";
        public const string NameOf_pg_mn_id = "pg_mn_id";

        private PagoData _data;
        private DBRecibo _recibo; //ESTO Luego se cambia por DBRecibo
        private DBFormasPago _formaDePago;
        private DBMoneda _moneda;

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            string te_table = DBTipoEntidad.db_table;
            string ec_table = DBEntidades.db_table;
            string tc_table = DBTipoRecibo.db_table;
            string fp_table = DBFormasPago.db_table;
            string cm_table = DBRecibo.db_table;
            string mn_table = DBMoneda.db_table;

            return $@"SELECT {fieldsToGet} FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.{DBFormasPago.NameOf_id} = {db_table}.{NameOf_pg_fp_id} 
                JOIN {mn_table} ON {mn_table}.{DBMoneda.NameOf_id} = {db_table}.{NameOf_pg_mn_id} 
                JOIN {cm_table} ON {cm_table}.{DBRecibo.NameOf_id} = {db_table}.{NameOf_pg_rc_id} AND {cm_table}.{DBRecibo.NameOf_rc_ec_id} = {db_table}.{NameOf_pg_ec_id} AND {cm_table}.{DBRecibo.NameOf_rc_em_id} = {db_table}.{NameOf_pg_em_id} 
                JOIN {tc_table} ON {tc_table}.{DBTipoRecibo.NameOf_id} = {cm_table}.{DBRecibo.NameOf_rc_tr_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_pg_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_pg_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} ";
        }
        string IDBase<DBPago>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBPago> GetAll(MySqlConnection conn, DBRecibo Recibo)
        {
            List<DBPago> returnList = new List<DBPago>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_pg_em_id} = {Recibo.GetCuentaID()} AND {NameOf_pg_ec_id} = {Recibo.GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {Recibo.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBPago(Recibo, new DBFormasPago(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de un Recibo. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    returnList.Add(new DBPago(new DBRecibo(entidadComercial, new DBTipoRecibo(reader), reader), new DBFormasPago(reader), new DBMoneda(reader), reader));
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
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_pg_em_id} = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBPago(new DBRecibo(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTipoRecibo(reader), reader), new DBFormasPago(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBPago GetByID(MySqlConnection conn, DBRecibo Recibo, long pg_id)
        {
            DBPago returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_pg_em_id} = {Recibo.GetCuentaID()} AND {NameOf_pg_ec_id} = {Recibo.GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {Recibo.GetID()} AND {NameOf_id} = {pg_id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBPago(Recibo, new DBFormasPago(reader), new DBMoneda(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un pago de un Recibo en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }
        public static DBPago GetByID(MySqlConnection conn, DBCuenta cuenta, long pg_ec_id, long pg_rc_id, long pg_id) => GetByID(conn, DBRecibo.GetByID(conn, cuenta, pg_ec_id, pg_rc_id), pg_id);

        public static DBPago GetByID(MySqlConnection conn, DBEntidades entidadComercial, long pg_rc_id, long pg_id) => GetByID(conn, DBRecibo.GetByID(conn, entidadComercial, pg_rc_id), pg_id);

        public static DBPago GetByID(List<DBPago> listaPagos, DBCuenta cuenta, long pg_ec_id, long pg_rc_id, long pg_id)
        {
            foreach (DBPago pago in listaPagos)
            {
                if (pago.GetID() == pg_id && pago.GetReciboID() == pg_rc_id && pago.GetEntidadComercialID() == pg_ec_id && pago.GetCuentaID() == cuenta.GetID())
                {
                    return pago;
                }
            }

            return null;
        }

        public static DBPago GetByID(List<DBPago> listaPagos, DBEntidades entidadComercial, long pg_rc_id, long id) => GetByID(listaPagos, entidadComercial.GetCuenta(), entidadComercial.GetID(), pg_rc_id, id);

        public static DBPago GetByID(List<DBPago> listaPagos, DBRecibo Recibo, long id) => GetByID(listaPagos, Recibo.GetCuenta(), Recibo.GetEntidadComercialID(), Recibo.GetID(), id);

        public static bool CheckIfExistsInList(List<DBPago> listaPagsRecibos, DBPago ent)
        {
            foreach (DBPago pagoRecibo in listaPagsRecibos)
            {
                if (pagoRecibo.GetCuentaID() == ent.GetCuentaID() && pagoRecibo.GetEntidadComercialID() == ent.GetEntidadComercialID() && pagoRecibo.GetReciboID() == ent.GetReciboID() && pagoRecibo.GetID() == ent.GetID())
                {
                    return true;
                }
            }
            return false;
        }

        public DBPago(DBRecibo Recibo, long id, DBFormasPago formaDePago, DBMoneda moneda, PagoData newData) : base (id)
        {
            if (formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _moneda = moneda;
            _formaDePago = formaDePago;
            _recibo = Recibo;
            _data = newData;
        }

        public DBPago(DBRecibo Recibo, long id, long fp_id, DBMoneda moneda, PagoData newData) : base (id)
        {
            _formaDePago = DBFormasPago.GetByID(fp_id);
            if (_formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _moneda = moneda;
            _recibo = Recibo;
            _data = newData;
        }

        public DBPago(DBRecibo Recibo, long id, long fp_id, long mn_id, PagoData newData) : base(id)
        {
            _formaDePago = DBFormasPago.GetByID(fp_id);
            if (_formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _moneda = DBMoneda.GetByID(mn_id);
            _recibo = Recibo;
            _data = newData;
        }

        public DBPago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long rc_id, long id, long fp_id, DBMoneda moneda, PagoData newData) : base (id)
        {
            _formaDePago = DBFormasPago.GetByID(fp_id, conn);
            if (_formaDePago is null)
            {
                throw new Exception("Error here");
            }
            _moneda = moneda;
            _recibo = DBRecibo.GetByID(conn, cuenta, ec_id, rc_id);
            _data = newData;
        }

        public DBPago(DBRecibo Recibo, DBFormasPago formaPago, DBMoneda moneda, double importe, string obs, DateTime? fecha=null, double cambio=1.0) : this(Recibo, -1, formaPago, moneda, new PagoData(importe, obs, fecha, cambio)) { }
        public DBPago(DBRecibo Recibo, long id, long fp_id, DBMoneda moneda, double importe, string obs, DateTime? fecha = null, double cambio = 1.0) : this(Recibo, id, fp_id, moneda, new PagoData(importe, obs, fecha, cambio)) { }
        public DBPago(DBRecibo Recibo, long fp_id, DBMoneda moneda, double importe, string obs, DateTime? fecha = null, double cambio = 1.0) : this(Recibo, -1, fp_id, moneda, importe, obs, fecha, cambio) { }

        public DBPago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long rc_id, long id, long fp_id, DBMoneda moneda, double importe, string obs, DateTime? fecha = null, double cambio = 1.0) : this(cuenta, conn, id, ec_id, rc_id, fp_id, moneda, new PagoData(importe, obs, fecha, cambio)) { }
        public DBPago(DBCuenta cuenta, MySqlConnection conn, long ec_id, long rc_id, long fp_id, DBMoneda moneda, double importe, string obs, DateTime? fecha = null, double cambio = 1.0) : this(cuenta, conn, ec_id, rc_id, -1, fp_id, moneda, importe, obs, fecha, cambio) { }
        
        public DBPago(DBRecibo Recibo, DBFormasPago newFormaPago, DBMoneda moneda, MySqlDataReader reader) : this (
            Recibo,
            reader.GetInt64Safe(NameOf_id),
            newFormaPago,
            moneda,
            PagoData.CreateFromReader(reader)) { }


        public override bool PullFromDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }

            bool wasAbleToPull = false;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetReciboID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_de_pago_id = -1;
                long new_recibo_id = -1;
                long new_entidad_comercial_id = -1;
                while (reader.Read())
                {
                    _data = PagoData.CreateFromReader(reader);
                    new_tipo_de_pago_id = reader.GetInt64Safe(NameOf_pg_fp_id);
                    new_recibo_id = reader.GetInt64Safe(NameOf_pg_rc_id);
                    new_entidad_comercial_id = reader.GetInt64Safe(NameOf_pg_ec_id);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();

                if (new_tipo_de_pago_id != -1)
                {
                    _formaDePago = DBFormasPago.GetByID(new_tipo_de_pago_id, conn);
                }
                if (new_entidad_comercial_id !=-1 && new_recibo_id != -1)
                {
                    _recibo = DBRecibo.GetByID(conn, DBEntidades.GetByID(conn, GetCuenta(), new_entidad_comercial_id), new_recibo_id);
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
            if (IsLocal())
            {
                return false;
            }
            bool wasAbleToUpdate = false;
            try
            {
                string fechaPago = (_data.pg_fecha.HasValue) ? $"'{((DateTime)_data.pg_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_pg_fp_id} = {_formaDePago.GetID()}, 
                                {NameOf_pg_mn_id} = {_moneda.GetID()}, 
                                {PagoData.NameOf_pg_importe} = {_data.pg_importe.ToString().Replace(",", ".")}, 
                                {PagoData.NameOf_pg_obs} = '{_data.pg_obs}', 
                                {PagoData.NameOf_pg_fecha} = {fechaPago}, 
                                {PagoData.NameOf_pg_cambio} = {_data.pg_cambio.ToString().Replace(",", ".")} 
                                WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetReciboID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
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
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            bool wasAbleToInsert = false;
            try
            {
                string fechaPago = (_data.pg_fecha.HasValue) ? $"'{((DateTime)_data.pg_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"INSERT INTO {db_table} (
                                {NameOf_pg_em_id}, 
                                {NameOf_pg_ec_id}, 
                                {NameOf_pg_rc_id}, 
                                {NameOf_pg_fp_id}, 
                                {NameOf_pg_mn_id}, 
                                {PagoData.NameOf_pg_importe}, 
                                {PagoData.NameOf_pg_obs}, 
                                {PagoData.NameOf_pg_fecha}, 
                                {PagoData.NameOf_pg_cambio}) VALUES (
                                {GetCuentaID()}, 
                                {GetEntidadComercialID()}, 
                                {GetReciboID()}, 
                                {_formaDePago.GetID()}, 
                                {_moneda.GetID()}, 
                                {_data.pg_importe.ToString().Replace(",", ".")}, 
                                '{_data.pg_obs}', 
                                {fechaPago}, 
                                {_data.pg_cambio.ToString().Replace(",", ".")})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    _recibo.AddPago(this);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
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
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetReciboID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _recibo.RemovePago(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DeleteFromDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn) => false; //DBPagos can have duplicated data (not the same as duplicated IDs!)
        public override bool? ExistsInDatabase(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_pg_em_id} = {GetCuentaID()} AND {NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {NameOf_pg_rc_id} = {GetReciboID()} AND {NameOf_id} = {GetID()}";
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

        public long GetCuentaID() => _recibo.GetCuentaID();
        public DBCuenta GetCuenta() => _recibo.GetCuenta();

        public long GetEntidadComercialID() => _recibo.GetEntidadComercialID();

        public DBEntidades GetEntidadComercial() => _recibo.GetEntidadComercial();

        public long GetReciboID() => _recibo.GetID();

        public DBRecibo GetRecibo() => _recibo;

        public DBFormasPago GetFormaDePago() => _formaDePago;

        public DBMoneda GetMoneda() => _moneda;

        public string GetObservacion() => _data.pg_obs;

        public double GetImporte() => _data.pg_importe;

        public double GetCambio() => _data.pg_cambio;

        public DateTime? GetFecha() => _data.pg_fecha;

        public void SetFormaDePago(DBFormasPago newFormaDePago)
        {
            _shouldPush = _shouldPush || (newFormaDePago.GetID() != _formaDePago.GetID());
            _formaDePago = newFormaDePago;
        }

        public void SetFormaDePago(long fp_id)
        {
            _shouldPush = _shouldPush || (fp_id != _formaDePago.GetID());
            _formaDePago = DBFormasPago.GetByID(fp_id);
        }
        public void SetObservacion(string obs) {
            _shouldPush = _shouldPush || !_data.pg_obs.Equals(obs);
            _data.pg_obs = obs;
        }
        public void SetFecha(DateTime? fecha)
        {
            _shouldPush = _shouldPush || (fecha != _data.pg_fecha);
            _data.pg_fecha = fecha;
        }
        public void SetImporte(double importe)
        {
            _shouldPush = _shouldPush || (importe != _data.pg_importe);
            _data.pg_importe = importe;
        }
        public void SetCambio(double cambio)
        {
            _shouldPush = _shouldPush || (cambio != _data.pg_cambio);
            _data.pg_cambio = cambio;
        }

        public void SetMoneda(DBMoneda newMoneda)
        {
            _shouldPush = _shouldPush || (_moneda != newMoneda);
            _moneda = newMoneda;
        }
        public void SetMoneda(long mn_id)
        {
            _shouldPush = _shouldPush || (mn_id != _moneda.GetID());
            _moneda = DBMoneda.GetByID(mn_id);
        }
        public void SetMoneda(long mn_id, MySqlConnection conn)
        {
            _shouldPush = _shouldPush || (mn_id != _moneda.GetID());
            _moneda = DBMoneda.GetByID(mn_id, conn);
        }

        public override DBBaseClass GetLocalCopy() => new DBPago(_recibo, -1, _formaDePago, _moneda, _data);

        public override string ToString() => $"ID: {GetID()} - Forma de pago: {_formaDePago.GetName()} - Moneda: {_moneda.GetName()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/


        public static DBPago GenerateRandom(DBRecibo Recibo)
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
            DBMoneda moneda = DBMoneda.GetRandom();

            return new DBPago(Recibo, DBFormasPago.GetRandom(), DBMoneda.GetRandom(), Math.Truncate(100000.0*r.NextDouble())/100.0, "Sin información", fechaFinal, moneda.IsExtranjera() ? (200.0 + Math.Truncate(10000.0 * r.NextDouble()) / 100.0) : 1.0);
        }
        
    }
}
