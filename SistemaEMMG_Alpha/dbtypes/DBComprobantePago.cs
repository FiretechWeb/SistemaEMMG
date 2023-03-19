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
        public ComprobantePagoData(long id, string obs)
        {
            cp_id = id;
            cp_obs = obs;
        }
        public long cp_id { get; }
        public string cp_obs { get; set; }

        public override string ToString()
        {
            return $"ID: {cp_id} - Observación: {cp_obs}";
        }
    }
    public class DBComprobantePago : DBInterface, IDBCuenta<DBEmpresa>, IDBEntidadComercial<DBEntidades>, IDBComprobante<DBComprobantes>
    {
        private static readonly string db_table = "comprobantes_pagos";
        private ComprobantePagoData _data;
        private DBComprobantes _comprobante;
        private DBFormasPago _formaDePago;

        public static string GetDBTableName() => db_table;
        string DBInterface.GetDBTableName() => GetDBTableName();

        public static bool RemoveFromDB(MySqlConnection conn, DBEmpresa cuenta, long ec_id, long cm_id, long id)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table}  WHERE cp_em_id = {cuenta.GetID()} AND cp_ec_id = {ec_id} AND cp_cm_id = {cm_id} AND cp_id = {id}";
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
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();
                string fp_table = DBFormasPago.GetDBTableName();
                string cm_table = DBComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.fp_id = {db_table}.cp_fp_id 
                JOIN {cm_table} ON {cm_table}.cm_id = {db_table}.cp_cm_id AND {cm_table}.cm_ec_id = {db_table}.cp_ec_id AND {cm_table}.cm_em_id = {db_table}.cp_em_id 
                JOIN {tc_table} ON {tc_table}.tc_id = {cm_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cp_ec_id AND {ec_table}.ec_em_id = {db_table}.cp_em_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cp_em_id = {comprobante.GetCuentaID()} AND cp_ec_id = {comprobante.GetEntidadComercialID()} AND cp_cm_id = {comprobante.GetID()}";

                Console.WriteLine("Aca llego");

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine("Leyendo data...");
                    DBFormasPago newFormaDePago = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                    returnList.Add(new DBComprobantePago(comprobante, newFormaDePago, new ComprobantePagoData(reader.GetInt64Safe("cp_id"), reader.GetStringSafe("cp_obs")))); //Waste of persformance but helps with making the code less propense to error.
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
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();
                string fp_table = DBFormasPago.GetDBTableName();
                string cm_table = DBComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.fp_id = {db_table}.cp_fp_id 
                JOIN {cm_table} ON {cm_table}.cm_id = {db_table}.cp_cm_id AND {cm_table}.cm_ec_id = {db_table}.cp_ec_id AND {cm_table}.cm_em_id = {db_table}.cp_em_id 
                JOIN {tc_table} ON {tc_table}.tc_id = {cm_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cp_ec_id AND {ec_table}.ec_em_id = {db_table}.cp_em_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cp_em_id = {entidadComercial.GetCuentaID()} AND cp_ec_id = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBFormasPago newFormaDePago = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    DBComprobantes newComprobante = new DBComprobantes(entidadComercial, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido")))); //Waste of persformance but helps with making the code less propense to error.

                    returnList.Add(new DBComprobantePago(newComprobante, newFormaDePago, new ComprobantePagoData(reader.GetInt64Safe("cp_id"), reader.GetStringSafe("cp_obs")))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBComprobantePago> GetAll(MySqlConnection conn, DBEmpresa cuenta)
        {
            List<DBComprobantePago> returnList = new List<DBComprobantePago>();
            try
            {
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();
                string fp_table = DBFormasPago.GetDBTableName();
                string cm_table = DBComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.fp_id = {db_table}.cp_fp_id 
                JOIN {cm_table} ON {cm_table}.cm_id = {db_table}.cp_cm_id AND {cm_table}.cm_ec_id = {db_table}.cp_ec_id AND {cm_table}.cm_em_id = {db_table}.cp_em_id 
                JOIN {tc_table} ON {tc_table}.tc_id = {cm_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cp_ec_id AND {ec_table}.ec_em_id = {db_table}.cp_em_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cp_em_id = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTipoEntidad newTipoEntidadComercial = new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"));
                    DBEntidades newEntidadComercial = new DBEntidades(cuenta, newTipoEntidadComercial, reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetStringSafe("ec_rs"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular"));
                    DBFormasPago newFormaDePago = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    DBComprobantes newComprobante = new DBComprobantes(newEntidadComercial, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido")))); //Waste of persformance but helps with making the code less propense to error.

                    returnList.Add(new DBComprobantePago(newComprobante, newFormaDePago, new ComprobantePagoData(reader.GetInt64Safe("cp_id"), reader.GetStringSafe("cp_obs")))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los pagos de una cuenta comercial. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBComprobantePago GetByID(MySqlConnection conn, DBEmpresa cuenta, long cp_ec_id, long cp_cm_id, long cp_id)
        {
            DBComprobantePago returnEnt = null;
            try
            {
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();
                string fp_table = DBFormasPago.GetDBTableName();
                string cm_table = DBComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.fp_id = {db_table}.cp_fp_id 
                JOIN {cm_table} ON {cm_table}.cm_id = {db_table}.cp_cm_id AND {cm_table}.cm_ec_id = {db_table}.cp_ec_id AND {cm_table}.cm_em_id = {db_table}.cp_em_id 
                JOIN {tc_table} ON {tc_table}.tc_id = {cm_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cp_ec_id AND {ec_table}.ec_em_id = {db_table}.cp_em_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cp_em_id = {cuenta.GetID()} AND cp_ec_id = {cp_ec_id} AND cp_cm_id = {cp_cm_id} AND cp_id = {cp_id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTipoEntidad newTipoEntidadComercial = new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"));
                    DBEntidades newEntidadComercial = new DBEntidades(cuenta, newTipoEntidadComercial, reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetStringSafe("ec_rs"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular"));
                    DBFormasPago newFormaDePago = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    DBComprobantes newComprobante = new DBComprobantes(newEntidadComercial, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido")))); //Waste of persformance but helps with making the code less propense to error.

                    returnEnt = new DBComprobantePago(newComprobante, newFormaDePago, new ComprobantePagoData(reader.GetInt64Safe("cp_id"), reader.GetStringSafe("cp_obs"))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un pago de un comprobante en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public static DBComprobantePago GetByID(MySqlConnection conn, DBEntidades entidadComercial, long cp_cm_id, long cp_id)
        {
            DBComprobantePago returnEnt = null;
            try
            {
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();
                string fp_table = DBFormasPago.GetDBTableName();
                string cm_table = DBComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.fp_id = {db_table}.cp_fp_id 
                JOIN {cm_table} ON {cm_table}.cm_id = {db_table}.cp_cm_id AND {cm_table}.cm_ec_id = {db_table}.cp_ec_id AND {cm_table}.cm_em_id = {db_table}.cp_em_id 
                JOIN {tc_table} ON {tc_table}.tc_id = {cm_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cp_ec_id AND {ec_table}.ec_em_id = {db_table}.cp_em_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cp_em_id = {entidadComercial.GetCuentaID()} AND cp_ec_id = {entidadComercial.GetID()} AND cp_cm_id = {cp_cm_id} AND cp_id = {cp_id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBFormasPago newFormaDePago = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    DBComprobantes newComprobante = new DBComprobantes(entidadComercial, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido")))); //Waste of persformance but helps with making the code less propense to error.

                    returnEnt = new DBComprobantePago(newComprobante, newFormaDePago, new ComprobantePagoData(reader.GetInt64Safe("cp_id"), reader.GetStringSafe("cp_obs"))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un pago de un comprobante en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public static DBComprobantePago GetByID(MySqlConnection conn, DBComprobantes comprobante, long cp_id)
        {
            DBComprobantePago returnEnt = null;
            try
            {
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();
                string fp_table = DBFormasPago.GetDBTableName();
                string cm_table = DBComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {fp_table} ON {fp_table}.fp_id = {db_table}.cp_fp_id 
                JOIN {cm_table} ON {cm_table}.cm_id = {db_table}.cp_cm_id AND {cm_table}.cm_ec_id = {db_table}.cp_ec_id AND {cm_table}.cm_em_id = {db_table}.cp_em_id 
                JOIN {tc_table} ON {tc_table}.tc_id = {cm_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cp_ec_id AND {ec_table}.ec_em_id = {db_table}.cp_em_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cp_em_id = {comprobante.GetCuentaID()} AND cp_ec_id = {comprobante.GetEntidadComercialID()} AND cp_cm_id = {comprobante.GetID()} AND cp_id = {cp_id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBFormasPago newFormaDePago = new DBFormasPago(reader.GetInt64Safe("fp_id"), reader.GetStringSafe("fp_nombre"));
                    returnEnt = new DBComprobantePago(comprobante, newFormaDePago, new ComprobantePagoData(reader.GetInt64Safe("cp_id"), reader.GetStringSafe("cp_obs"))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener un pago de un comprobante en GetByID. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }


        public static DBComprobantePago GetByID(List<DBComprobantePago> listaPagos, DBEmpresa cuenta, long cp_ec_id, long cp_cm_id, long cp_id)
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

        public DBComprobantePago(DBComprobantes comprobante, DBFormasPago formaDePago, ComprobantePagoData newData)
        {
            _formaDePago = formaDePago;
            _comprobante = comprobante;
            _data = newData;
        }

        public DBComprobantePago(DBComprobantes comprobante, long fp_id, ComprobantePagoData newData)
        {
            _formaDePago = DBFormasPago.GetByID(fp_id);
            _comprobante = comprobante;
            _data = newData;
        }
        public DBComprobantePago(DBEmpresa cuenta, long ec_id, long cm_id, long fp_id, ComprobantePagoData newData)
        {
            _formaDePago = DBFormasPago.GetByID(fp_id);
            _comprobante = cuenta.GetComprobanteByID(ec_id, cm_id);
            _data = newData;
        }

        public DBComprobantePago(DBEmpresa cuenta, MySqlConnection conn, long ec_id, long cm_id, long fp_id, ComprobantePagoData newData) //Directly from DB
        {
            _formaDePago = DBFormasPago.GetByID(fp_id, conn);
            _comprobante = DBComprobantes.GetByID(conn, cuenta, ec_id, cm_id);
            _data = newData;
        }

        public DBComprobantePago(DBComprobantes comprobante, long fp_id, long id, string obs) : this(comprobante, fp_id, new ComprobantePagoData(id, obs)) { }
        public DBComprobantePago(DBComprobantes comprobante, long fp_id, string obs) : this(comprobante, fp_id, new ComprobantePagoData(-1, obs)) { }
        public DBComprobantePago(DBEmpresa cuenta, long ec_id, long cm_id, long fp_id, long id, string obs) : this(cuenta, ec_id, cm_id, fp_id, new ComprobantePagoData(id, obs)) { }
        public DBComprobantePago(DBEmpresa cuenta, long ec_id, long cm_id, long fp_id, string obs) : this(cuenta, ec_id, cm_id, fp_id, new ComprobantePagoData(-1, obs)) { }
        public DBComprobantePago(DBEmpresa cuenta, MySqlConnection conn, long ec_id, long cm_id, long fp_id, long id, string obs) : this(cuenta, conn, ec_id, cm_id, fp_id, new ComprobantePagoData(id, obs)) { }
        public DBComprobantePago(DBEmpresa cuenta, MySqlConnection conn, long ec_id, long cm_id, long fp_id, string obs) : this(cuenta, conn, ec_id, cm_id, fp_id, new ComprobantePagoData(-1, obs)) { }

        public ComprobantePagoData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public DBFormasPago FormaDePago
        {
            get => _formaDePago;
            set
            {
                _formaDePago = value;
            }
        }

        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            bool? existsInDB = ExistsInDatabase(conn);
            if (existsInDB is null) //error with DB...
            {
                return false;
            }
            try
            {
                //First check if the record already exists in the DB
                string query;

                //if exists already, just update
                if (existsInDB == true)
                {
                    query = $"UPDATE {db_table} SET cp_fp_id = {_formaDePago.GetID()}, cp_obs = '{_data.cp_obs}' WHERE cp_em_id = {GetCuentaID()} AND cp_ec_id = {GetEntidadComercialID()} AND cp_cm_id = {GetComprobanteID()} AND cp_id = {_data.cp_id}";
                }
                else //if does not exists, insert into
                {
                    query = $"INSERT INTO {db_table} (cp_em_id, cp_ec_id, cp_cm_id, cp_fp_id, cp_obs) VALUES ({GetCuentaID()}, {GetEntidadComercialID()}, {GetComprobanteID()}, {_formaDePago.GetID()}, '{_data.cp_obs}')";
                }
                //if not, add
                var cmd = new MySqlCommand(query, conn);
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (existsInDB == false) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new ComprobantePagoData(cmd.LastInsertedId, _data.cp_obs);
                }
                wasAbleToPush = true;
            }
            catch (Exception ex)
            {
                wasAbleToPush = false;
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBComprobantes: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPush;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE cp_em_id = {GetCuentaID()} AND cp_ec_id = {GetEntidadComercialID()} AND cp_cm_id = {GetComprobanteID()} AND cp_id = {_data.cp_id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DeleteFromDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (deletedCorrectly)
            {
                //_comprobante.RemovePago(this);
            }
            return deletedCorrectly;
        }

        public bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? existsInDB = null;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE cp_em_id = {GetCuentaID()} AND cp_ec_id = {GetEntidadComercialID()} AND cp_cm_id = {GetComprobanteID()} AND cp_id = {_data.cp_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //if exists already, just update
                if (recordsCount > 0)
                {
                    existsInDB = true;
                }
                else
                {
                    existsInDB = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBComprobantePago::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        public void ResetID(long newId = -1) => _data = new ComprobantePagoData(newId, _data.cp_obs);
        public long GetID() => _data.cp_id;

        public long GetCuentaID() => _comprobante.GetCuentaID();
        public DBEmpresa GetCuenta() => _comprobante.GetCuenta();

        public long GetEntidadComercialID() => _comprobante.GetEntidadComercialID();

        public DBEntidades GetEntidadComercial() => _comprobante.GetEntidadComercial();

        public long GetComprobanteID() => _comprobante.GetID();

        public DBComprobantes GetComprobante() => _comprobante;

        public DBFormasPago GetFormaDePago() => _formaDePago;

        public string GetObservacion() => _data.cp_obs;

        public void SetFormaDePago(DBFormasPago newFormaDePago) => _formaDePago = newFormaDePago;

        public void SetFormaDePago(long fp_id) => _formaDePago = DBFormasPago.GetByID(fp_id);
        public void SetObservacion(string obs) => _data.cp_obs = obs;
    }
}
