﻿using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct ComprobantesData
    {
        public ComprobantesData(DateTime? fecha, string numero, double gravado, double iva, double no_gravado, double percepcion, bool emitido, double cambio, string obs, double subtotal, double extentas, double otro_tributo)
        {
            cm_fecha = fecha;
            cm_numero = numero;
            cm_gravado = gravado;
            cm_iva = iva;
            cm_no_gravado = no_gravado;
            cm_percepcion = percepcion;
            cm_emitido = emitido;
            cm_cambio = cambio;
            cm_obs = obs;
            cm_subtotal = subtotal;
            cm_extentas = extentas;
            cm_otributos = otro_tributo;

        }
        public DateTime? cm_fecha { get; set; }
        public string cm_numero { get; set; }
        public double cm_gravado { get; set; }
        public double cm_iva { get; set; }
        public double cm_no_gravado { get; set; }
        public double cm_percepcion { get; set; }
        public bool cm_emitido { get; set; }
        public double cm_cambio { get; set; }
        public double cm_subtotal { get; set; }
        public double cm_extentas { get; set; }
        public double cm_otributos { get; set; }
        public string cm_obs { get; set; }

        public static readonly string NameOf_cm_fecha = nameof(cm_fecha);
        public static readonly string NameOf_cm_numero = nameof(cm_numero);
        public static readonly string NameOf_cm_gravado = nameof(cm_gravado);
        public static readonly string NameOf_cm_iva = nameof(cm_iva);
        public static readonly string NameOf_cm_no_gravado = nameof(cm_no_gravado);
        public static readonly string NameOf_cm_percepcion = nameof(cm_percepcion);
        public static readonly string NameOf_cm_emitido = nameof(cm_emitido);
        public static readonly string NameOf_cm_cambio = nameof(cm_cambio);
        public static readonly string NameOf_cm_obs = nameof(cm_obs);
        public static readonly string NameOf_cm_subtotal = nameof(cm_subtotal);
        public static readonly string NameOf_cm_extentas = nameof(cm_extentas);
        public static readonly string NameOf_cm_otributos = nameof(cm_otributos);

        public static ComprobantesData CreateFromReader(MySqlDataReader reader)
        {
            return new ComprobantesData(reader.GetDateTimeSafe(NameOf_cm_fecha),
                                        reader.GetStringSafe(NameOf_cm_numero),
                                        reader.GetDoubleSafe(NameOf_cm_gravado),
                                        reader.GetDoubleSafe(NameOf_cm_iva),
                                        reader.GetDoubleSafe(NameOf_cm_no_gravado),
                                        reader.GetDoubleSafe(NameOf_cm_percepcion),
                                        Convert.ToBoolean(reader.GetInt32Safe(NameOf_cm_emitido)),
                                        reader.GetDoubleSafe(NameOf_cm_cambio),
                                        reader.GetStringSafe(NameOf_cm_obs), 
                                        reader.GetDoubleSafe(NameOf_cm_subtotal),
                                        reader.GetDoubleSafe(NameOf_cm_extentas),
                                        reader.GetDoubleSafe(NameOf_cm_otributos));
        }
        public override string ToString()
        {
            return $"Emitido: {cm_emitido} - Fecha: {cm_fecha} - Número: {cm_numero} - Gravado: {cm_gravado} - IVA: {cm_iva} - No Gravado: {cm_no_gravado} - Percepción: {cm_percepcion} - Cambio: {cm_cambio} - Subtotal: {cm_subtotal} - Imp. Op. Extentas: {cm_extentas} - Otros Tributos: {cm_otributos} - Observación: {cm_obs}";
        }
    }
    public class DBComprobantes : DBBaseClass, IDBase<DBComprobantes>, IDBCuenta<DBCuenta>, IDBEntidadComercial<DBEntidades>
    {
        public const string db_table = "comprobantes";
        public const string db_recibos_relation_table = "recibos_comprobantes";
        public const string db_remitos_relation_table = "remitos_comprobantes";
        public const string NameOf_cm_em_id = "cm_em_id";
        public const string NameOf_cm_ec_id = "cm_ec_id";
        public const string NameOf_cm_tc_id = "cm_tc_id";
        public const string NameOf_cm_mn_id = "cm_mn_id";
        public const string NameOf_cm_cm_id = "cm_cm_id"; //comprobante asociado
        public const string NameOf_id = "cm_id";
        ///<summary>
        ///Commercial entity associated with this business receipt.
        ///</summary>
        private DBEntidades _entidadComercial; //this can change actually... (Maybe the DB's design it's flawed... it is what it is LOL)
        private DBMoneda _moneda;
        private ComprobantesData _data;
        private DBTiposComprobantes _tipoComprobante = null;
        private DBComprobantes _comprobanteAsociado = null; //comprobante al cual este comprobante (nota de crédito o débito) está asociado.
        private readonly List<DBComprobantes> _db_comprobantes_asociados = new List<DBComprobantes>(); //Notas de crédito o débitos asociados a este comprobante.
        private readonly List<DBRecibo> _db_recibos = new List<DBRecibo>();
        private readonly List<DBRemito> _db_remitos = new List<DBRemito>();
        private long _comprobante_asociado_id = -1;

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            string te_table = DBTipoEntidad.db_table;
            string ec_table = DBEntidades.db_table;
            string tc_table = DBTiposComprobantes.db_table;
            string mn_table = DBMoneda.db_table;

            return $@"SELECT {fieldsToGet} FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {mn_table} ON {mn_table}.{DBMoneda.NameOf_id} = {db_table}.{NameOf_cm_mn_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_cm_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_cm_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id}";
        }
        string IDBase<DBComprobantes>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBRecibo recibo)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string tc_table = DBTiposComprobantes.db_table;
                string mn_table = DBMoneda.db_table;
                string rc_table = DBRecibo.db_table;
                string query = $@"SELECT * FROM {db_recibos_relation_table} 
                JOIN {db_table} ON {db_recibos_relation_table}.rp_em_id = {db_table}.{NameOf_cm_em_id} AND {db_recibos_relation_table}.rp_ec_id = {db_table}.{NameOf_cm_ec_id} AND {db_recibos_relation_table}.rp_cm_id = {db_table}.{NameOf_id} 
                JOIN {rc_table} ON {db_recibos_relation_table}.rp_em_id = {rc_table}.{DBRecibo.NameOf_rc_em_id} AND {db_recibos_relation_table}.rp_ec_id = {rc_table}.{DBRecibo.NameOf_rc_ec_id} AND {db_recibos_relation_table}.rp_rc_id = {rc_table}.{DBRecibo.NameOf_id} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {mn_table} ON {mn_table}.{DBMoneda.NameOf_id} = {db_table}.{NameOf_cm_mn_id} 
                WHERE rp_em_id = {recibo.GetCuentaID()} AND rp_ec_id = {recibo.GetEntidadComercialID()} AND rp_rc_id = {recibo.GetID()}";
                //string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(recibo.GetEntidadComercial(), new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }
            return returnList;
        }

        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBRemito remito)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string tc_table = DBTiposComprobantes.db_table;
                string mn_table = DBMoneda.db_table;
                string rm_table = DBRemito.db_table;
                string query = $@"SELECT * FROM {db_remitos_relation_table} 
                JOIN {db_table} ON {db_remitos_relation_table}.rt_em_id = {db_table}.{NameOf_cm_em_id} AND {db_remitos_relation_table}.rt_ec_id = {db_table}.{NameOf_cm_ec_id} AND {db_remitos_relation_table}.rt_cm_id = {db_table}.{NameOf_id} 
                JOIN {rm_table} ON {db_remitos_relation_table}.rt_em_id = {rm_table}.{DBRemito.NameOf_rm_em_id} AND {db_remitos_relation_table}.rt_ec_id = {rm_table}.{DBRemito.NameOf_rm_ec_id} AND {db_remitos_relation_table}.rt_rm_id = {rm_table}.{DBRemito.NameOf_id} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {mn_table} ON {mn_table}.{DBMoneda.NameOf_id} = {db_table}.{NameOf_cm_mn_id} 
                WHERE rt_em_id = {remito.GetCuentaID()} AND rt_ec_id = {remito.GetEntidadComercialID()} AND rt_rm_id = {remito.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(remito.GetEntidadComercial(), new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }
            return returnList;
        }


        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBCuenta cuenta)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }
            return returnList;
        }

        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBEntidades entidadComercial)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }
            return returnList;
        }

        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBComprobantes comprobanteAsociado)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {comprobanteAsociado.GetCuentaID()} AND {NameOf_cm_ec_id} = {comprobanteAsociado.GetEntidadComercialID()} AND {NameOf_cm_cm_id} = {comprobanteAsociado.GetID()} AND {ComprobantesData.NameOf_cm_emitido} = {Convert.ToInt32(comprobanteAsociado.IsEmitido())} AND {NameOf_id} <> {comprobanteAsociado.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(comprobanteAsociado.GetEntidadComercial(), new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes asociados a otro comprobante, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }
            return returnList;
        }


        public static DBComprobantes GetByID(MySqlConnection conn, DBEntidades entidadComercial, long id)
        {
            DBComprobantes returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()} AND {NameOf_id} = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), new DBMoneda(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (!(returnEnt is null))
            {
                returnEnt.RefreshComprobanteAsociado(conn);
            }
            return returnEnt;
        }
        public static DBComprobantes GetByID(MySqlConnection conn, DBCuenta cuenta, long ec_id, long id) => GetByID(conn, DBEntidades.GetByID(conn, cuenta, ec_id), id);

        public static DBComprobantes GetByID(List<DBComprobantes> listaComprobantes, DBCuenta cuenta, long ec_id, long id)
        {
            foreach (DBComprobantes comprobante in listaComprobantes)
            {
                if (comprobante.GetID() == id && comprobante.GetEntidadComercialID() == ec_id && comprobante.GetCuentaID() == cuenta.GetID())
                {
                    return comprobante;
                }
            }
            return null;
        }

        public static DBComprobantes GetByID(List<DBComprobantes> listaComprobantes, DBEntidades entidadComercial, long id)
        {
            foreach (DBComprobantes comprobante in listaComprobantes)
            {
                if (comprobante.GetID() == id && comprobante.GetEntidadComercialID() == entidadComercial.GetID() && comprobante.GetCuentaID() == entidadComercial.GetCuentaID())
                {
                    return comprobante;
                }
            }

            return null;
        }

        public static int FindInList(List<DBComprobantes> listaComprobantes, DBComprobantes ent)
        {
            for (int i=0; i < listaComprobantes.Count; i++)
            {
                if (listaComprobantes[i].GetCuentaID() == ent.GetCuentaID() && listaComprobantes[i].GetEntidadComercialID() == ent.GetEntidadComercialID() && listaComprobantes[i].GetID() == ent.GetID())
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool CheckIfExistsInList(List<DBComprobantes> listaComprobantes, DBComprobantes ent) => FindInList(listaComprobantes, ent) != -1;

        /***********************
         * Filter/Search methods
         * *********************/

        /************
         *  estadoEmision:
         *      -1: Todos
         *      0: Recibido
         *      1: Emitido
         *  estadoPago
         *      -1: Todos
         *      0: No pago
         *      1: Pago
         *************/
        public static List<DBComprobantes> Search(MySqlConnection conn, DBCuenta cuenta, int estadoEmision, DateTime? fechaComienzo, DateTime? fechaFinal, long CUIT, int estadoPago, long cm_tc_id, long ec_te_id, string numero)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {cuenta.GetID()} ";
                estadoEmision -= 1;
                estadoPago -= 1;
                if (cm_tc_id > -1)
                {
                    query += $"AND {NameOf_cm_tc_id} = {cm_tc_id} ";
                }
                if (ec_te_id > -1)
                {
                    query += $"AND {DBEntidades.NameOf_ec_te_id} = {ec_te_id} ";
                }
                if (estadoEmision >= 0)
                {
                    query += $"AND {ComprobantesData.NameOf_cm_emitido} = {estadoEmision} ";
                }
                if (!(fechaComienzo is null))
                {
                    string fechaComienzoStr = ((DateTime)fechaComienzo).ToString("yyyy/MM/dd");
                    query += $"AND {ComprobantesData.NameOf_cm_fecha} >= '{fechaComienzoStr}' ";
                }
                if (!(fechaFinal is null))
                {
                    string fechaFinalStr = ((DateTime)fechaFinal).ToString("yyyy/MM/dd");
                    query += $"AND {ComprobantesData.NameOf_cm_fecha} <= '{fechaFinalStr}' ";
                }
                if (!string.IsNullOrEmpty(numero.Trim()))
                {
                    query += $"AND UPPER({ComprobantesData.NameOf_cm_numero}) LIKE '%{numero.Trim().ToUpper()}%'";
                }
                if (CUIT > 0)
                {
                    //'%{toFind.Trim().ToLower()}%'
                    query += $"AND {EntidadesComercialesData.NameOf_ec_cuit} LIKE '%{CUIT}%'";
                }
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBComprobantes::Search. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (estadoPago == 0)
            {
                returnList = returnList.Where(x => !x.IsPago(conn)).ToList();
            } else if (estadoPago == 1)
            {
                returnList = returnList.Where(x => x.IsPago(conn)).ToList();
            }

            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }

            return returnList;
        }

        public static List<DBComprobantes> SearchByNumber(MySqlConnection conn, DBEntidades entidadComercial, string numeroComprobante, bool isEmitido, int asociado = -1)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()} AND {ComprobantesData.NameOf_cm_emitido} = {Convert.ToInt32(isEmitido)} AND UPPER({ComprobantesData.NameOf_cm_numero}) LIKE '%{numeroComprobante.Trim().ToUpper()}%'";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), new DBMoneda(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBComprobantes::SearchByNumber. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (asociado != -1)
            {
                if (asociado == 0)
                {
                    returnList.RemoveAll(x => x.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Asociado));
                } else
                {
                    returnList.RemoveAll(x => !x.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Asociado));
                }
            }

            foreach (DBComprobantes comprobante in returnList)
            {
                comprobante.RefreshComprobanteAsociado(conn);
            }
            return returnList;
        }

        public static DBComprobantes GetByNumberNormalized(MySqlConnection conn, DBEntidades entidadComercial, string numeroComprobante, bool isEmitido)
        {
            DBComprobantes returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()}";
                
                if (!isEmitido)
                {
                    query += $" AND {NameOf_cm_ec_id} = {entidadComercial.GetID()}";
                }
                query += $" AND {ComprobantesData.NameOf_cm_emitido} = {Convert.ToInt32(isEmitido)}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBComprobantes dbComprobante = new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), new DBMoneda(reader), reader);
                    if (numeroComprobante.KeepNumbersAndMinus().Equals(dbComprobante.GetNumeroComprobante().KeepNumbersAndMinus()))
                    {
                        returnEnt = dbComprobante;
                        break; //entity found
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBComprobantes::GetByNumberNormalized. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return returnEnt;
        }

        public static DBComprobantes GetByNumber(MySqlConnection conn, DBEntidades entidadComercial, string numeroComprobante, bool isEmitido, int asociado = -1)
        { 
            DBComprobantes returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()} AND {ComprobantesData.NameOf_cm_emitido} = {Convert.ToInt32(isEmitido)} AND UPPER({ComprobantesData.NameOf_cm_numero}) = '{numeroComprobante.Trim().ToUpper()}'";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBComprobantes(entidadComercial, new DBTiposComprobantes(reader), new DBMoneda(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBComprobantes::GetByNumber. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            if (!(returnEnt is null))
            {
                if (asociado != -1)
                {
                    if (asociado == 0)
                    {
                        if (returnEnt.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Asociado)) {
                            return null;
                        }
                    }
                    else if (!returnEnt.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Asociado))
                    {
                        return null;
                    }
                }
                returnEnt.RefreshComprobanteAsociado(conn);
            }

            return returnEnt;
        }

        /****************
         * Constructors 
         * **************/
        public DBComprobantes(DBEntidades entidadComercial, long id, DBTiposComprobantes newTipo, DBMoneda newMoneda, ComprobantesData newData, DBComprobantes comprobanteAsociado=null) : base (id)
        {
            _entidadComercial = entidadComercial;
            _moneda = newMoneda;
            _tipoComprobante = newTipo;
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public DBComprobantes(DBEntidades entidadComercial, long id, DBTiposComprobantes newTipo, DBMoneda newMoneda, ComprobantesData newData, long comprobanteAsociadoID) : base(id)
        {
            _entidadComercial = entidadComercial;
            _moneda = newMoneda;
            _tipoComprobante = newTipo;
            _data = newData;
            _comprobanteAsociado = null;
            _comprobante_asociado_id = comprobanteAsociadoID;
        }

        public DBComprobantes(DBEntidades entidadComercial, long id, long tc_id, DBMoneda newMoneda, ComprobantesData newData, DBComprobantes comprobanteAsociado = null) : base(id)
        {
            _entidadComercial = entidadComercial;
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _moneda = newMoneda;
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public DBComprobantes(DBEntidades entidadComercial, long id, long tc_id, long mn_id, ComprobantesData newData, DBComprobantes comprobanteAsociado = null) : base(id)
        {
            _entidadComercial = entidadComercial;
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _moneda = DBMoneda.GetByID(mn_id);
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public DBComprobantes(DBCuenta cuentaSeleccioanda, long id, long ec_id, DBTiposComprobantes newTipo, DBMoneda newMoneda, ComprobantesData newData, DBComprobantes comprobanteAsociado = null) : base(id)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoComprobante = newTipo;
            _moneda = newMoneda;
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public DBComprobantes(DBCuenta cuentaSeleccioanda, long id, long ec_id, long tc_id, long mn_id, ComprobantesData newData, DBComprobantes comprobanteAsociado = null) : base(id)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _moneda = DBMoneda.GetByID(mn_id);
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }
        public DBComprobantes(DBCuenta cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, long tc_id, long mn_id, ComprobantesData newData, DBComprobantes comprobanteAsociado = null) : base(id)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _moneda = DBMoneda.GetByID(mn_id);
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public DBComprobantes(DBCuenta cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, DBTiposComprobantes newTipo, DBMoneda newMoneda, ComprobantesData newData, DBComprobantes comprobanteAsociado = null) : base(id)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoComprobante = newTipo;
            _moneda = newMoneda;
            _data = newData;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public DBComprobantes(
            DBEntidades entidadComercial,
            DBTiposComprobantes newTipo,
            DBMoneda newMoneda,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio=1.0,
            string obs="",
            double subtotal=0.0,
            double op_extentas=0.0,
            double otros_tributos=0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            entidadComercial,
            id,
            newTipo,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        ) { }

        public DBComprobantes(
            DBEntidades entidadComercial,
            DBTiposComprobantes newTipo,
            DBMoneda newMoneda,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            entidadComercial,
            -1,
            newTipo,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }

        public DBComprobantes(
            DBEntidades entidadComercial,
            long tc_id,
            DBMoneda newMoneda,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            entidadComercial,
            id,
            tc_id,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }
        public DBComprobantes(
            DBEntidades entidadComercial,
            long tc_id,
            DBMoneda newMoneda,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            entidadComercial,
            -1,
            tc_id,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }
        public DBComprobantes(
            DBCuenta cuentaSeleccioanda,
            long ec_id,
            DBTiposComprobantes newTipo,
            DBMoneda newMoneda,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            cuentaSeleccioanda,
            id,
            ec_id,
            newTipo,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }
        public DBComprobantes(
            DBCuenta cuentaSeleccioanda,
            long ec_id,
            DBTiposComprobantes newTipo,
            DBMoneda newMoneda,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            cuentaSeleccioanda,
            -1,
            ec_id,
            newTipo,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }
        public DBComprobantes(
            DBCuenta cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            DBTiposComprobantes newTipo,
            DBMoneda newMoneda,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            cuentaSeleccioanda,
            conn,
            id,
            ec_id,
            newTipo,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }
        public DBComprobantes(
            DBCuenta cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            DBTiposComprobantes newTipo,
            DBMoneda newMoneda,
            bool emitido,
            DateTime? fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0,
            double cambio = 1.0,
            string obs = "",
            double subtotal = 0.0,
            double op_extentas = 0.0,
            double otros_tributos = 0.0,
            DBComprobantes comprobanteAsociado = null
        ) : this(
            cuentaSeleccioanda,
            conn,
            -1,
            ec_id,
            newTipo,
            newMoneda,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido, cambio, obs, subtotal, op_extentas, otros_tributos),
            comprobanteAsociado
        )
        { }
        public DBComprobantes(DBEntidades entidadComercial, DBTiposComprobantes newTipo, DBMoneda newMoneda, MySqlDataReader reader) : this(
            entidadComercial,
            reader.GetInt64Safe(NameOf_id),
            newTipo,
            newMoneda,
            ComprobantesData.CreateFromReader(reader), 
            reader.GetInt64Safe(NameOf_cm_cm_id)) { }

        public bool PushToDatabase(MySqlConnection conn, long old_cm_ec_id)
        {
            long old_cm_id = GetID();
            bool wasAbleToPushAndDelete = false;
            if (!IsLocal() && old_cm_ec_id != GetEntidadComercialID())
            {
                MakeLocal();
            } else
            {
                return PushToDatabase(conn);
            }
            if (InsertIntoToDatabase(conn, old_cm_ec_id, old_cm_id))
            {
                DBComprobantes oldComprobante = GetByID(conn, GetCuenta(), old_cm_ec_id, old_cm_id);
                if (!(oldComprobante is null))
                {
                    wasAbleToPushAndDelete = oldComprobante.DeleteFromDatabase(conn);
                } else
                {
                    wasAbleToPushAndDelete = true;
                }
                if (!wasAbleToPushAndDelete)
                {
                    DeleteFromDatabase(conn);
                }
            }
            return wasAbleToPushAndDelete;
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_cm_em_id} = {GetCuentaID()} AND {NameOf_cm_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_comprobante_id = -1;
                long new_entidad_comercial_id = -1;
                long new_comprobante_asociado_id = -1;
                long new_moneda_id = -1;
                while (reader.Read())
                {
                    _data = ComprobantesData.CreateFromReader(reader);
                    new_tipo_comprobante_id = reader.GetInt64Safe(NameOf_cm_tc_id);
                    new_entidad_comercial_id = reader.GetInt64Safe(NameOf_cm_ec_id);
                    new_moneda_id = reader.GetInt64Safe(NameOf_cm_mn_id);
                    new_comprobante_asociado_id = reader.GetInt64Safe(NameOf_cm_cm_id);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();

                if (new_tipo_comprobante_id != -1)
                {
                    _tipoComprobante = DBTiposComprobantes.GetByID(new_tipo_comprobante_id, conn);
                }
                if (new_entidad_comercial_id != -1)
                {
                    _entidadComercial = DBEntidades.GetByID(conn, GetCuenta(), new_entidad_comercial_id);
                }
                if (new_moneda_id != -1)
                {
                    _moneda = DBMoneda.GetByID(new_moneda_id, conn);
                }
                if (new_comprobante_asociado_id != -1 && new_comprobante_asociado_id != GetID())
                {
                    _comprobanteAsociado = GetByID(conn, GetEntidadComercial(), new_comprobante_asociado_id);
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
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            bool wasAbleToUpdate = false;
            try
            {
                long comprobante_asociado_id = -1;
                if (!(_comprobanteAsociado is null))
                {
                    comprobante_asociado_id = _comprobanteAsociado.GetID();
                }
                string fechaEmitido = (_data.cm_fecha.HasValue) ? $"'{((DateTime)_data.cm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_cm_tc_id} = {_tipoComprobante.GetID()}, 
                                {NameOf_cm_mn_id} = {_moneda.GetID()}, 
                                {NameOf_cm_cm_id} = {comprobante_asociado_id}, 
                                {ComprobantesData.NameOf_cm_fecha} = {fechaEmitido}, 
                                {ComprobantesData.NameOf_cm_numero} = '{Regex.Replace(_data.cm_numero.Trim(), @"\s+", " ")}', 
                                {ComprobantesData.NameOf_cm_gravado} = {_data.cm_gravado.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_iva} = {_data.cm_iva.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_no_gravado} = {_data.cm_no_gravado.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_percepcion} = {_data.cm_percepcion.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_emitido} = {Convert.ToInt32(_data.cm_emitido)}, 
                                {ComprobantesData.NameOf_cm_cambio} = {_data.cm_cambio.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_subtotal} = {_data.cm_subtotal.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_obs} = '{_data.cm_obs}', 
                                {ComprobantesData.NameOf_cm_extentas} = {_data.cm_extentas.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_otributos} = {_data.cm_otributos.ToString().Replace(",", ".")} 
                                WHERE {NameOf_cm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBComprobantes::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        private bool InsertIntoToDatabase_Raw(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string fechaEmitido = (_data.cm_fecha.HasValue) ? $"'{((DateTime)_data.cm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                long comprobante_asociado_id = -1;
                if (!(_comprobanteAsociado is null))
                {
                    comprobante_asociado_id = _comprobanteAsociado.GetID();
                }
                string query = $@"INSERT INTO {db_table} (
                                {NameOf_cm_em_id}, 
                                {NameOf_cm_ec_id}, 
                                {NameOf_cm_tc_id}, 
                                {NameOf_cm_mn_id}, 
                                {NameOf_cm_cm_id}, 
                                {ComprobantesData.NameOf_cm_fecha}, 
                                {ComprobantesData.NameOf_cm_numero}, 
                                {ComprobantesData.NameOf_cm_gravado}, 
                                {ComprobantesData.NameOf_cm_iva}, 
                                {ComprobantesData.NameOf_cm_no_gravado}, 
                                {ComprobantesData.NameOf_cm_percepcion}, 
                                {ComprobantesData.NameOf_cm_emitido}, 
                                {ComprobantesData.NameOf_cm_cambio}, 
                                {ComprobantesData.NameOf_cm_subtotal}, 
                                {ComprobantesData.NameOf_cm_obs}, 
                                {ComprobantesData.NameOf_cm_extentas}, 
                                {ComprobantesData.NameOf_cm_otributos}) 
                                VALUES (
                                {_entidadComercial.GetCuentaID()}, 
                                {_entidadComercial.GetID()}, 
                                {_tipoComprobante.GetID()}, 
                                {_moneda.GetID()}, 
                                {comprobante_asociado_id}, 
                                {fechaEmitido}, 
                                '{Regex.Replace(_data.cm_numero.Trim(), @"\s+", " ")}', 
                                {_data.cm_gravado.ToString().Replace(",", ".")}, 
                                {_data.cm_iva.ToString().Replace(",", ".")}, 
                                {_data.cm_no_gravado.ToString().Replace(",", ".")}, 
                                {_data.cm_percepcion.ToString().Replace(",", ".")}, 
                                {Convert.ToInt32(_data.cm_emitido)}, 
                                {_data.cm_cambio.ToString().Replace(",", ".")}, 
                                {_data.cm_subtotal.ToString().Replace(",", ".")}, 
                                '{_data.cm_obs}', 
                                {_data.cm_extentas.ToString().Replace(",", ".")}, 
                                {_data.cm_otributos.ToString().Replace(",", ".")})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    _entidadComercial.AddComprobante(this); //safe to add to since now it belongs to de DB.
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBComprobantes::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            return InsertIntoToDatabase_Raw(conn);
        }

        public bool InsertIntoToDatabase(MySqlConnection conn, long ignore_ec_id, long ignore_cm_id)
        {
            bool? doesDuplicateExistsDB = DuplicatedExistsInDatabase(conn, ignore_ec_id, ignore_cm_id);
            if (doesDuplicateExistsDB == true || doesDuplicateExistsDB == null)
            {
                return false;
            }
            return InsertIntoToDatabase_Raw(conn);
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
                List<DBComprobantes> compAsociados = GetAllComprobantesAsociados(conn);
                foreach (DBComprobantes cmpAsociado in compAsociados)
                {
                    cmpAsociado.AsociarAComprobante(null);
                    cmpAsociado.PushToDatabase(conn);
                }

                string query = $"DELETE FROM {db_recibos_relation_table} WHERE rp_em_id = {GetCuentaID()} AND rp_ec_id = {GetEntidadComercialID()} AND rp_cm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {db_remitos_relation_table} WHERE rt_em_id = {GetCuentaID()} AND rt_ec_id = {GetEntidadComercialID()} AND rt_cm_id = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error tratando de eliminar información relacionada a un comprobante en DBComprobante: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_table} WHERE {NameOf_cm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _entidadComercial.RemoveComprobante(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBComprobantes: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }
        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = "";
                if (_data.cm_emitido) //IF emitido, then the number should be unique across ALL DBEntidades belonging to this account
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_cm_em_id} = {GetCuentaID()} AND {NameOf_id} <> {GetID()} AND UPPER({ComprobantesData.NameOf_cm_numero}) = '{Regex.Replace(_data.cm_numero.Trim().ToUpper(), @"\s+", " ")}'";
                }
                else //If not emitido (recibido) then the number should be unique only for this specific DBEntidades
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_cm_em_id} = {GetCuentaID()} AND {NameOf_cm_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} <> {GetID()} AND UPPER({ComprobantesData.NameOf_cm_numero}) = '{Regex.Replace(_data.cm_numero.Trim().ToUpper(), @"\s+", " ")}'";
                }
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

        public bool? DuplicatedExistsInDatabase(MySqlConnection conn, long ignore_ec_id, long ignore_cm_id)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = "";
                if (_data.cm_emitido) //IF emitido, then the number should be unique across ALL DBEntidades belonging to this account
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_cm_em_id} = {GetCuentaID()} AND (({NameOf_cm_ec_id} <> {ignore_ec_id}) OR ({NameOf_id} <> {ignore_cm_id})) AND {NameOf_id} <> {GetID()} AND UPPER({ComprobantesData.NameOf_cm_numero}) = '{Regex.Replace(_data.cm_numero.Trim().ToUpper(), @"\s+", " ")}'";
                }
                else //If not emitido (recibido) then the number should be unique only for this specific DBEntidades
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_cm_em_id} = {GetCuentaID()} AND {NameOf_cm_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} <> {GetID()} AND (({NameOf_cm_ec_id} <> {ignore_ec_id}) OR ({NameOf_id} <> {ignore_cm_id})) AND UPPER({ComprobantesData.NameOf_cm_numero}) = '{Regex.Replace(_data.cm_numero.Trim().ToUpper(), @"\s+", " ")}'";
                }
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_cm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBComprobantes::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        private bool? CheckIfRelatioshipWithRemitoExistsDB(MySqlConnection conn, DBRemito remito)
        {
            if (IsLocal())
            {
                return false;
            }
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_remitos_relation_table} WHERE rt_em_id = {_entidadComercial.GetCuentaID()} AND rt_ec_id = {_entidadComercial.GetID()} AND rt_cm_id = {GetID()} AND rt_rm_id = {remito.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBComprobantes::CheckIfRelatioshipWithReciboExistsDB: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public bool PushRelationshipRemitoDB(MySqlConnection conn, DBRemito newRemito)
        {
            if (IsLocal() || newRemito.IsLocal())
            {
                return false;
            }
            if (newRemito.GetEntidadComercialID() != GetEntidadComercialID() || newRemito.GetCuentaID() != GetCuentaID())
            {
                MessageBox.Show("Error en el método DBComprobantes::PushRelationshipRemitoDB.\nImposible relacionar un Remito de otra entidad comercial a la del comprobante.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (newRemito.IsEmitido() != IsEmitido())
            {
                return false;
            }
            if ((newRemito.ExistsInDatabase(conn) != true) || (ExistsInDatabase(conn) != true))
            {
                MessageBox.Show("Error en el método DBComprobantes::PushRelationshipRemitoDB.\n Parece que uno de los datos a relacionar no existe en la base de datos.\nRecuerde llamar esta función solo si los datos están en la base de datos.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CheckIfRelatioshipWithRemitoExistsDB(conn, newRemito) != false)
            {
                return false;
            }

            bool wasAbleToInsertRelation = false;
            try
            {
                string query = $@"INSERT INTO {db_remitos_relation_table} (
                                rt_em_id,
                                rt_ec_id,
                                rt_rm_id,
                                rt_cm_id) 
                                VALUES (
                                {_entidadComercial.GetCuentaID()},
                                {_entidadComercial.GetID()},
                                {newRemito.GetID()},
                                {GetID()})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsertRelation = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsertRelation = false;
                MessageBox.Show("Error DBComprobantes::PushRelationshipRemitoDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return wasAbleToInsertRelation;
        }
        public bool RemoveRelationshipRemitoDB(MySqlConnection conn, long rm_id)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_remitos_relation_table} WHERE rt_em_id = {_entidadComercial.GetCuentaID()} AND rt_ec_id = {_entidadComercial.GetID()} AND rt_cm_id = {GetID()} AND rt_rm_id = {rm_id}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBComprobantes::RemoveRelationshipRemitoDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                deletedCorrectly = false;
            }
            return deletedCorrectly;
        }
        public bool RemoveRelationshipRemitoDB(MySqlConnection conn, DBRemito remito)
        {
            if (IsLocal())
            {
                return false;
            }
            if (remito.GetEntidadComercialID() != GetEntidadComercialID() || remito.GetCuentaID() != GetCuentaID())
            {
                MessageBox.Show("Error en el método DBComprobantes::RemoveRelationshipRemitoDB.\nImposible relacionar un Remito de otra entidad comercial a la del comprobante.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            bool deletedCorrectly = RemoveRelationshipRemitoDB(conn, remito.GetID());
            if (deletedCorrectly)
            {
                RemoveRemito(remito);
            }
            return deletedCorrectly;
        }

        public bool RemoveAllRelationshipsWithRemitosDB(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_remitos_relation_table} WHERE rt_em_id = {_entidadComercial.GetCuentaID()} AND rt_ec_id = {_entidadComercial.GetID()} AND rt_cm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    _db_remitos.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRemito::RemoveAllRelationshipsWithRemitosDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                deletedCorrectly = false;
            }
            return deletedCorrectly;
        }
        public void PushAllRelationshipsWithRemitosDB(MySqlConnection conn)
        {
            foreach (DBRemito remito in _db_remitos)
            {
                PushRelationshipRemitoDB(conn, remito);
            }
        }

        private bool? CheckIfRelatioshipWithReciboExistsDB(MySqlConnection conn, DBRecibo newRecibo)
        {
            if (IsLocal())
            {
                return false;
            }
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_recibos_relation_table} WHERE rp_em_id = {_entidadComercial.GetCuentaID()} AND rp_ec_id = {_entidadComercial.GetID()} AND rp_cm_id = {GetID()} AND rp_rc_id = {newRecibo.GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBComprobantes::CheckIfRelatioshipWithReciboExistsDB: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public bool PushRelationshipReciboDB(MySqlConnection conn, DBRecibo newRecibo)
        {
            if (IsLocal() || newRecibo.IsLocal())
            {
                return false;
            }
            if (newRecibo.GetEntidadComercialID() != GetEntidadComercialID() || newRecibo.GetCuentaID() != GetCuentaID())
            {
                MessageBox.Show("Error en el método DBComprobantes::PushRelationshipReciboDB.\nImposible relacionar un recibo de otra entidad comercial a la del comprobante.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (newRecibo.IsEmitido() != IsEmitido())
            {
                return false;
            }
            if ((newRecibo.ExistsInDatabase(conn) != true) || (ExistsInDatabase(conn) != true))
            {
                MessageBox.Show("Error en el método DBComprobantes::PushRelationshipReciboDB.\n Parece que uno de los datos a relacionar no existe en la base de datos.\nRecuerde llamar esta función solo si los datos están en la base de datos.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CheckIfRelatioshipWithReciboExistsDB(conn, newRecibo) != false)
            {
                return false;
            }

            bool wasAbleToInsertRelation = false;
            try
            {
                string query = $@"INSERT INTO {db_recibos_relation_table} (
                                rp_em_id,
                                rp_ec_id,
                                rp_rc_id,
                                rp_cm_id) 
                                VALUES (
                                {_entidadComercial.GetCuentaID()},
                                {_entidadComercial.GetID()},
                                {newRecibo.GetID()},
                                {GetID()})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsertRelation = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsertRelation = false;
                MessageBox.Show("Error DBComprobantes::PushRelationshipReciboDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return wasAbleToInsertRelation;
        }
        public bool RemoveRelationshipReciboDB(MySqlConnection conn, long rc_id)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_recibos_relation_table} WHERE rp_em_id = {_entidadComercial.GetCuentaID()} AND rp_ec_id = {_entidadComercial.GetID()} AND rp_cm_id = {GetID()} AND rp_rc_id = {rc_id}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRecibo::RemoveRelationshipComprobanteDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                deletedCorrectly = false;
            }
            return deletedCorrectly;
        }
        public bool RemoveRelationshipReciboDB(MySqlConnection conn, DBRecibo recibo)
        {
            if (IsLocal())
            {
                return false;
            }
            if (recibo.GetEntidadComercialID() != GetEntidadComercialID() || recibo.GetCuentaID() != GetCuentaID())
            {
                MessageBox.Show("Error en el método DBComprobantes::RemoveRelationshipReciboDB.\nImposible relacionar un recibo de otra entidad comercial a la del comprobante.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            bool deletedCorrectly = RemoveRelationshipReciboDB(conn, recibo.GetID());
            if (deletedCorrectly)
            {
                RemoveRecibo(recibo);
            }
            return deletedCorrectly;
        }

        public bool RemoveAllRelationshipsWithRecibosDB(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_recibos_relation_table} WHERE rp_em_id = {_entidadComercial.GetCuentaID()} AND rp_ec_id = {_entidadComercial.GetID()} AND rp_cm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    _db_recibos.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRecibo::RemoveAllRelationshipsWithRecibosDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                deletedCorrectly = false;
            }
            return deletedCorrectly;
        }
        public void PushAllRelationshipsWithRecibosDB(MySqlConnection conn)
        {
            foreach (DBRecibo recibo in _db_recibos)
            {
                PushRelationshipReciboDB(conn, recibo);
            }
        }

        public List<DBRecibo> GetAllRecibos(MySqlConnection conn)
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
        public DBRecibo GetReciboByID(long rc_id) => DBRecibo.GetByID(_db_recibos, GetEntidadComercial(), rc_id);

        public bool AddRecibo(DBRecibo newRecibo)
        {
            if (newRecibo is null)
            {
                return false;
            }
            if (newRecibo.GetCuentaID() != GetCuentaID() || newRecibo.GetEntidadComercialID() != GetEntidadComercialID())
            {
                return false; //Cannot add an payament from another account or entity like this...
            }
            if (newRecibo.IsEmitido() != IsEmitido())
            {
                return false;
            }
            if (_db_recibos.Contains(newRecibo))
            {
                return false;
            }
            if (!newRecibo.IsLocal())
            {
                int foundIndex = DBRecibo.FindInList(_db_recibos, newRecibo);
                if (foundIndex != -1) //Update old data with new in case of match.
                {
                    _db_recibos[foundIndex] = newRecibo;
                    return true;
                }
            }
            _db_recibos.Add(newRecibo);
            return true;
        }
        public bool AddRecibo(MySqlConnection conn, long rc_id) => AddRecibo(DBRecibo.GetByID(conn, GetEntidadComercial(), rc_id));

        public void RemoveRecibo(long rc_id) => _db_recibos.RemoveAll(x => x.GetID() == rc_id);

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
        public List<DBRemito> GetAllRemitos(MySqlConnection conn)
        {
            List<DBRemito> returnList = DBRemito.GetAll(conn, this);
            _db_remitos.Clear();
            foreach (DBRemito Remito in returnList)
            {
                _db_remitos.Add(Remito);
            }
            return returnList;
        }
        public List<DBRemito> GetAllRemitos() //Get CACHE
        {
            List<DBRemito> returnList = new List<DBRemito>();
            foreach (DBRemito Remito in _db_remitos)
            {
                returnList.Add(Remito);
            }
            return returnList;
        }
        public DBRemito GetRemitoByID(long rc_id) => DBRemito.GetByID(_db_remitos, GetEntidadComercial(), rc_id);

        public bool AddRemito(DBRemito newRemito)
        {
            if (newRemito is null)
            {
                return false;
            }
            if (newRemito.GetCuentaID() != GetCuentaID() || newRemito.GetEntidadComercialID() != GetEntidadComercialID())
            {
                return false; //Cannot add an payament from another account or entity like this...
            }
            if (newRemito.IsEmitido() != IsEmitido())
            {
                return false;
            }
            if (_db_remitos.Contains(newRemito))
            {
                return false;
            }
            if (!newRemito.IsLocal()) //already exists with this ID
            {
                int foundIndex = DBRemito.FindInList(_db_remitos, newRemito);
                if (foundIndex != -1) //Update old data with new in case of match.
                {
                    _db_remitos[foundIndex] = newRemito;
                    return true;
                }
            }
            _db_remitos.Add(newRemito);
            return true;
        }
        public bool AddRemito(MySqlConnection conn, long rc_id) => AddRemito(DBRemito.GetByID(conn, GetEntidadComercial(), rc_id));

        public void RemoveRemito(long rc_id) => _db_remitos.RemoveAll(x => x.GetID() == rc_id);

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

        public List<DBComprobantes> GetAllComprobantesAsociados(MySqlConnection conn)
        {
            List<DBComprobantes> returnList = GetAll(conn, this);
            _db_comprobantes_asociados.Clear();
            foreach (DBComprobantes comprobanteAsociado in returnList)
            {
                _db_comprobantes_asociados.Add(comprobanteAsociado);
            }
            return returnList;
        }
        public bool AddComprobanteAsociado(DBComprobantes comprobanteParaAsociar)
        {
            foreach (DBComprobantes cmpAsociado in _db_comprobantes_asociados)
            {
                if (cmpAsociado.GetCuentaID() == comprobanteParaAsociar.GetCuentaID() && cmpAsociado.GetEntidadComercialID() == comprobanteParaAsociar.GetEntidadComercialID() && cmpAsociado.GetID() == comprobanteParaAsociar.GetID())
                {
                    return false;
                }
            }
            _db_comprobantes_asociados.Add(comprobanteParaAsociar);
            return true;
        }

        public void PushAllComprobantesAsociadosDB(MySqlConnection conn)
        {
            foreach (DBComprobantes cmpAsociado in _db_comprobantes_asociados)
            {
                cmpAsociado.PushToDatabase(conn);
            }
        }
        public List<DBComprobantes> GetAllComprobantesAsociados() => _db_comprobantes_asociados;

        public long GetEntidadComercialID() => _entidadComercial.GetID();

        ///<summary>
        ///Returns a reference to the Bussiness Entity that contains this business receipt.
        ///</summary>
        public DBEntidades GetEntidadComercial() => _entidadComercial;
        public long GetCuentaID() => _entidadComercial.GetCuentaID();

        public DBCuenta GetCuenta() => _entidadComercial.GetCuenta();
        public DBTiposComprobantes GetTipoComprobante() => _tipoComprobante;
        public string GetNumeroComprobante() => _data.cm_numero;
        ///<summary>
        ///Returns the DateTime date when this business receipt was generated.
        ///</summary>
        public DateTime? GetFechaEmitido() => _data.cm_fecha;
        ///<summary>
        ///Returns the DateTime date when this business receipt was payed.
        ///</summary>
        public double GetGravado() => _data.cm_gravado;
        public double GetIVA() => _data.cm_iva;
        public double GetNoGravado() => _data.cm_no_gravado;
        public double GetPercepcion() => _data.cm_percepcion;
        public double GetCambio() => _data.cm_cambio;
        public string GetObservacion() => _data.cm_obs;
        public double GetSubTotal() => _data.cm_subtotal;
        public DBMoneda GetMoneda() => _moneda;
        public double GetOpExtentas() => _data.cm_extentas;
        public double GetOtrosTributos() => _data.cm_otributos;



        ///<summary>
        ///Returns if this business receipt was emitted to get payed or received to be payed.
        ///</summary>
        public bool IsEmitido() => _data.cm_emitido;

        public void SetEntidadComercial(DBEntidades newEntidadComercial)
        {
            _shouldPush = _shouldPush || (_entidadComercial != newEntidadComercial);
            _entidadComercial = newEntidadComercial;
        }
        public void SetEntidadComercial(long ec_id)
        {
            _shouldPush = _shouldPush || (ec_id != GetEntidadComercialID());
            _entidadComercial = GetCuenta().GetEntidadByID(ec_id);
        }
        public void SetEntidadComercial(long ec_id, MySqlConnection conn)
        {
            _shouldPush = _shouldPush || (ec_id != GetEntidadComercialID());
            _entidadComercial = DBEntidades.GetByID(conn, GetCuenta(), ec_id);
        }
        public void SetTipoComprobante(DBTiposComprobantes newType)
        {
            _shouldPush = _shouldPush || (_tipoComprobante != newType);
            _tipoComprobante = newType;
        }
        public void SetTipoComprobante(long tc_id)
        {
            _shouldPush = _shouldPush || (tc_id != _tipoComprobante.GetID());
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
        }
        public void SetTipoComprobante(long tc_id, MySqlConnection conn)
        {
            _shouldPush = _shouldPush || (tc_id != _tipoComprobante.GetID());
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id, conn);
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
        public void SetNumeroComprobante(string numeroCom)
        {
            _shouldPush = _shouldPush || !_data.cm_numero.Equals(numeroCom);
            _data.cm_numero = numeroCom;
        }
        public void SetFechaEmitido(DateTime? newFecha)
        {
            _shouldPush = _shouldPush || (newFecha != _data.cm_fecha);
            _data.cm_fecha = newFecha;
        }
        public void SetGravado(double gravado)
        {
            _shouldPush = _shouldPush || (gravado != _data.cm_gravado);
            _data.cm_gravado = gravado;
        }
        public void SetIVA(double IVA)
        {
            _shouldPush = _shouldPush || (IVA != _data.cm_iva);
            _data.cm_iva = IVA;
        }
        public void SetNoGravado(double no_gravado)
        {
            _shouldPush = _shouldPush || (no_gravado != _data.cm_no_gravado);
            _data.cm_no_gravado = no_gravado;
        }
        public void SetPercepcion(double percepcion)
        {
            _shouldPush = _shouldPush || (percepcion != _data.cm_percepcion);
            _data.cm_percepcion = percepcion;
        }
        public void SetEmitido(bool esEmitido)
        {
            _shouldPush = _shouldPush || (esEmitido != _data.cm_emitido);
            _data.cm_emitido = esEmitido;
        }
        public void SetCambio(double cambio)
        {
            _shouldPush = _shouldPush || (cambio != _data.cm_cambio);
            _data.cm_cambio = cambio;
        }
        public void SetObservacion(string obs)
        {
            _shouldPush = _shouldPush || !_data.cm_obs.Equals(obs);
            _data.cm_obs = obs;
        }
        public void SetSubTotal(double subtotal)
        {
            _shouldPush = _shouldPush || (subtotal != _data.cm_subtotal);
            _data.cm_subtotal = subtotal;
        }

        public void SetOpExtentas(double op_extentas)
        {
            _shouldPush = _shouldPush || (op_extentas != _data.cm_extentas);
            _data.cm_extentas = op_extentas;
        }

        public void SetOtrosTributos(double otros_tributos)
        {
            _shouldPush = _shouldPush || (otros_tributos != _data.cm_otributos);
            _data.cm_otributos = otros_tributos;
        }

        public void SetComprobanteAsociadoID(long newId) => _comprobante_asociado_id = newId;

        public long GetComprobanteAsociadoID()
        {
            if (_comprobanteAsociado is null)
            {
                return _comprobante_asociado_id;
            } else {
                return _comprobanteAsociado.GetID();
            }
        }

        public void AsociarAComprobante(DBComprobantes comprobanteAsociado)
        {
            if (!(comprobanteAsociado is null) && (comprobanteAsociado.GetCuentaID() != GetCuentaID() || comprobanteAsociado.GetEntidadComercialID() != GetEntidadComercialID() || GetID() == comprobanteAsociado.GetID() || IsEmitido() != comprobanteAsociado.IsEmitido() || comprobanteAsociado.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Asociado) || !GetTipoComprobante().HasFlag(TipoComprobanteFlag.Asociado)))
            {

                MessageBox.Show("Error en el método DBComprobantes::AsociarAComprobante.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _shouldPush = true;
            _comprobanteAsociado = comprobanteAsociado;
        }

        public void RefreshComprobanteAsociado(MySqlConnection conn)
        {
            if (GetComprobanteAsociadoID() == GetID() || GetComprobanteAsociadoID() == -1)
            {
                return;
            }
            _comprobanteAsociado = GetByID(conn, GetEntidadComercial(), GetComprobanteAsociadoID());
        }

        public override DBBaseClass GetLocalCopy() => new DBComprobantes(_entidadComercial, -1, _tipoComprobante, _moneda, _data, _comprobanteAsociado);
        public override void Update(DBBaseClass source)
        {
            if (!(source is DBComprobantes)) return;
            DBComprobantes sourceComprobantes = (DBComprobantes)source;
            SetCambio(sourceComprobantes.GetCambio());
            SetComprobanteAsociadoID(sourceComprobantes.GetComprobanteAsociadoID());
            SetEmitido(sourceComprobantes.IsEmitido());
            SetEntidadComercial(sourceComprobantes.GetEntidadComercial());
            SetFechaEmitido(sourceComprobantes.GetFechaEmitido());
            SetGravado(sourceComprobantes.GetGravado());
            SetIVA(sourceComprobantes.GetIVA());
            SetMoneda(sourceComprobantes.GetMoneda());
            SetNoGravado(sourceComprobantes.GetNoGravado());
            SetNumeroComprobante(sourceComprobantes.GetNumeroComprobante());
            SetObservacion(sourceComprobantes.GetObservacion());
            SetPercepcion(sourceComprobantes.GetPercepcion());
            SetSubTotal(sourceComprobantes.GetSubTotal());
            SetTipoComprobante(sourceComprobantes.GetTipoComprobante());
        }

        public override string ToString() => $"ID: {GetID()} - Tipo: {_tipoComprobante.GetName()} - Moneda: {_moneda.GetName()} - {_data}";

        /***********************************************
         * Useful functions for Real world applications
         * ********************************************/

        public double GetTotal() => _data.cm_extentas + _data.cm_otributos + _data.cm_gravado + _data.cm_iva + _data.cm_no_gravado + _data.cm_percepcion + _data.cm_subtotal;

        public double GetTotal_MonedaLocal() => GetTotal() * _data.cm_cambio;
        public double GetGravado_MonedaLocal() => _data.cm_gravado * _data.cm_cambio;
        public double GetIVA_MonedaLocal() => _data.cm_iva * _data.cm_cambio;
        public double GetNoGravado_MonedaLocal() => _data.cm_no_gravado * _data.cm_cambio;
        public double GetPercepcion_MonedaLocal() => _data.cm_percepcion * _data.cm_cambio;
        public double GetOtrosTributos_MonedaLocal() => _data.cm_otributos * _data.cm_cambio;
        public double GetOpExtentas_MonedaLocal() => _data.cm_extentas * _data.cm_cambio;

        public double GetTotalReal_MonedaLocal(MySqlConnection conn)
        {
            double total_asociado = 0.0;
            GetAllComprobantesAsociados(conn);
            foreach (DBComprobantes comprobante in _db_comprobantes_asociados)
            {
                if (comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    total_asociado += comprobante.GetTotal_MonedaLocal();
                } else
                {
                    total_asociado -= comprobante.GetTotal_MonedaLocal();
                }
            }
            return total_asociado + GetTotal_MonedaLocal();
        }

        public static List<DBComprobantes> GetAllEmitidos(List<DBComprobantes> comprobantesList) => comprobantesList.Where(x => x.IsEmitido()).ToList();
        public static List<DBComprobantes> GetAllRecibidos(List<DBComprobantes> comprobantesList) => comprobantesList.Where(x => !x.IsEmitido()).ToList();
        public static List<DBComprobantes> GetAllWithMonedaExtranjera(List<DBComprobantes> comprobantesList) => comprobantesList.Where(x => x.GetMoneda().IsExtranjera()).ToList();

        public static double GetSaldoTotal_MonedaLocal(List<DBComprobantes> comprobantesList)
        {
            double saldoTotal = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                double currentTotal = 0;
                if (comprobante.IsEmitido())
                {
                    currentTotal = comprobante.GetTotal_MonedaLocal();
                }
                else
                {
                    currentTotal = -comprobante.GetTotal_MonedaLocal();
                }
                if (!comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    currentTotal *= -1.0;
                }
                saldoTotal += currentTotal;

            }
            return saldoTotal;
        }
        public static double GetSaldoIVA_MonedaLocal(List<DBComprobantes> comprobantesList) {
            double saldoIVA = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                double currentTotal = 0;
                if (comprobante.IsEmitido())
                {
                    currentTotal = -comprobante.GetIVA_MonedaLocal();
                }
                else
                {
                    currentTotal = comprobante.GetIVA_MonedaLocal();
                }
                if (!comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    currentTotal *= -1.0;
                }

                saldoIVA += currentTotal;
                
            }
            return saldoIVA;
        }
        public static double GetTotal_MonedaLocal(List<DBComprobantes>comprobantesList)
        {
            double total=0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                if (comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    total += comprobante.GetTotal_MonedaLocal();
                } else
                {
                    total -= comprobante.GetTotal_MonedaLocal();
                }

            }
            return total;
        }

        public static double GetTotalReal_MonedaLocal(List<DBComprobantes> comprobantesList, MySqlConnection conn)
        {
            double total = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                total += comprobante.GetTotalReal_MonedaLocal(conn);
            }
            return total;
        }

        public static double GetTotalIVA_MonedaLocal(List<DBComprobantes> comprobantesList)
        {
            double totalIva = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                if (comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    totalIva += comprobante.GetIVA_MonedaLocal();
                }
                else
                {
                    totalIva -= comprobante.GetIVA_MonedaLocal();
                }

            }
            return totalIva;
        }
        public static double GetTotalGravado_MonedaLocal(List<DBComprobantes> comprobantesList)
        {
            double totalGravado = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                if (comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    totalGravado += comprobante.GetGravado_MonedaLocal();
                } else
                {
                    totalGravado -= comprobante.GetGravado_MonedaLocal();
                }
            }
            return totalGravado;
        }
        public static double GetTotalNoGravado_MonedaLocal(List<DBComprobantes> comprobantesList)
        {
            double totalNoGravado = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                if (comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    totalNoGravado += comprobante.GetNoGravado_MonedaLocal();
                } else
                {
                    totalNoGravado -= comprobante.GetNoGravado_MonedaLocal();
                }
            }
            return totalNoGravado;
        }
        public static double GetTotalPercepcion_MonedaLocal(List<DBComprobantes> comprobantesList)
        {
            double totalPercepcion = 0.0;
            foreach (DBComprobantes comprobante in comprobantesList)
            {
                if (comprobante.GetTipoComprobante().HasFlag(TipoComprobanteFlag.Acredita))
                {
                    totalPercepcion += comprobante.GetPercepcion_MonedaLocal();
                } else
                {
                    totalPercepcion -= comprobante.GetPercepcion_MonedaLocal();
                }
            }
            return totalPercepcion;
        }

        //Para saber si un comprobante está pago, buscamos todo los recibos que incluyan a este comprobante
        // y nos fijamos que la suma de todos los pagos de estos recibos sea igual o superior a la suma del importe de todas las facturas relacionadas.

        public bool IsPago(MySqlConnection conn)
        {
            GetAllRecibos(conn);

            double totalPago = 0.0;
            double totalPagoDirecto = 0.0;
            double totalImporte = 0.0;
            double totalPagoLocal = 0.0; //pagos que estoy seguro que fueron para solo esta factura.
            double totalMonedaLocal = GetTotalReal_MonedaLocal(conn);
            List<DBComprobantes> listaComprobantes = new List<DBComprobantes>();
            List<DBRecibo> listaRecibos = GetAllRecibos(conn);

            List<DBComprobantes> listaComprobantesAnalizados = new List<DBComprobantes>();
            List<DBRecibo> listaRecibosAnalizados = new List<DBRecibo>();

            listaComprobantes.Add(this);
            bool searchFinished;
            do
            {
                searchFinished = true;
                foreach (DBComprobantes comprobante in listaComprobantes)
                {
                    if (CheckIfExistsInList(listaComprobantesAnalizados, comprobante))
                    {
                        continue;
                    }
                    List<DBRecibo> recibos = comprobante.GetAllRecibos(conn);
                    foreach (DBRecibo recibo in recibos)
                    {
                        if (!DBRecibo.CheckIfExistsInList(listaRecibos, recibo))
                        {
                            searchFinished = false;
                            listaRecibos.Add(recibo);
                        }
                    }

                    listaComprobantesAnalizados.Add(comprobante);
                }
                foreach (DBRecibo recibo in listaRecibos)
                {
                    if (DBRecibo.CheckIfExistsInList(listaRecibosAnalizados, recibo))
                    {
                        continue;
                    }
                    List<DBComprobantes> comprobantes = recibo.GetAllComprobantes(conn);
                    foreach (DBComprobantes comprobante in comprobantes)
                    {
                        if (comprobantes.Count == 1 && comprobante.GetID() == GetID())
                        {
                            totalPagoLocal += recibo.GetPagosTotal_MonedaLocal(conn);
                        } 
                        if (!CheckIfExistsInList(listaComprobantes, comprobante))
                        {
                            searchFinished = false;
                            listaComprobantes.Add(comprobante);
                        }
                    }

                    listaRecibosAnalizados.Add(recibo);
                }
            } while (!searchFinished);

            foreach (DBComprobantes comprobante in listaComprobantes) {
                totalImporte += comprobante.GetTotalReal_MonedaLocal(conn);
            }

            foreach (DBRecibo recibo in listaRecibos)
            {
                double pagoTotalRecibo = recibo.GetPagosTotal_MonedaLocal(conn);
                totalPago += pagoTotalRecibo;
                List<DBComprobantes> comprobantes = recibo.GetAllComprobantes(conn);
                if (CheckIfExistsInList(comprobantes, this))
                {
                    totalPagoDirecto += pagoTotalRecibo;
                }
            }
            totalPago = Math.Round(totalPago, 2);
            totalImporte = Math.Round(totalImporte, 2);
            totalMonedaLocal = Math.Round(totalMonedaLocal, 2);
            totalPagoLocal = Math.Round(totalPagoLocal, 2);

            return (totalPago >= totalImporte && totalPagoDirecto >= totalMonedaLocal) || (totalPagoLocal >= totalMonedaLocal);
        }

        /**********************
         * DEBUG STUFF ONLY
         * ********************/

        public string PrintAllRecibos()
        {
            string str = "";
            foreach (DBRecibo recibo in _db_recibos)
            {
                str += $"Recibo relacionado> {recibo}\n";
            }
            return str;
        }
        public string PrintAllRemitos()
        {
            string str = "";
            foreach (DBRemito remito in _db_remitos)
            {
                str += $"Remito relacionado> {remito}\n";
            }
            return str;
        }

        private static string[] randomFacturaCodigos =
        {
            "A",
            "B",
            "C"
        };

        public static DBComprobantes GenerateRandom(DBEntidades entidadComercial)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            DateTime fechaEmitido = new DateTime();
            DateTime? fechaFinal = null;
            string randomDateSTR = $"{r.Next(1, 28)}/{r.Next(1, 13)}/{r.Next(2010, 2024)}";
            if (DateTime.TryParse(randomDateSTR, out fechaEmitido))
            {
                fechaFinal = fechaEmitido;
            }
            DBMoneda moneda = DBMoneda.GetRandom();
            double cambio = moneda.IsExtranjera() ? (200.0 + Math.Truncate(10000.0 * r.NextDouble()) / 100.0) : 1.0;
            return new DBComprobantes(entidadComercial,
                DBTiposComprobantes.GetRandom(),
                moneda,
                Convert.ToBoolean(r.Next(0, 2)),
                fechaFinal,
                $"{randomFacturaCodigos[r.Next(0, randomFacturaCodigos.Length)]}{r.Next(1, 10)}-{r.Next(100000, 999999)}",
                Math.Truncate(10000000.0*r.NextDouble()/cambio) /100.0,
                Math.Truncate(2100000.0 *r.NextDouble()/ cambio) /100.0,
                Math.Truncate(5000000.0 *r.NextDouble()/ cambio) /100.0,
                Math.Truncate(500000.0 *r.NextDouble()/ cambio) /100.0,
                cambio,
                "Sin información adicional");
        }
        
    }
}