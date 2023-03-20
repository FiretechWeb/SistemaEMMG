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
    public struct ComprobantesData
    {
        public ComprobantesData(DateTime? fecha, string numero, double gravado, double iva, double no_gravado, double percepcion, bool emitido)
        {
            cm_fecha = fecha;
            cm_numero = numero;
            cm_gravado = gravado;
            cm_iva = iva;
            cm_no_gravado = no_gravado;
            cm_percepcion = percepcion;
            cm_emitido = emitido;
        }
        public DateTime? cm_fecha { get; set; }
        public string cm_numero { get; set; }
        public double cm_gravado { get; set; }
        public double cm_iva { get; set; }
        public double cm_no_gravado { get; set; }
        public double cm_percepcion { get; set; }
        public bool cm_emitido { get; set; }

        public static readonly string NameOf_cm_fecha = nameof(cm_fecha);
        public static readonly string NameOf_cm_numero = nameof(cm_numero);
        public static readonly string NameOf_cm_gravado = nameof(cm_gravado);
        public static readonly string NameOf_cm_iva = nameof(cm_iva);
        public static readonly string NameOf_cm_no_gravado = nameof(cm_no_gravado);
        public static readonly string NameOf_cm_percepcion = nameof(cm_percepcion);
        public static readonly string NameOf_cm_emitido = nameof(cm_emitido);

        public override string ToString()
        {
            return $"Fecha: {cm_fecha} - Número: {cm_numero} - Gravado: {cm_gravado} - IVA: {cm_iva} - No Gravado: {cm_no_gravado} - Percepción: {cm_percepcion} - Emitido: {cm_emitido}";
        }
    }
    public class DBComprobantes : DBBaseClass, IDBCuenta<DBEmpresa>, IDBEntidadComercial<DBEntidades>
    {
        public const string db_table = "comprobantes";
        public const string NameOf_cm_em_id = "cm_em_id";
        public const string NameOf_cm_ec_id = "cm_ec_id";
        public const string NameOf_cm_tc_id = "cm_tc_id";
        public const string NameOf_id = "cm_id";
        ///<summary>
        ///Commercial entity associated with this business receipt.
        ///</summary>
        private DBEntidades _entidadComercial; //this can change actually... (Maybe the DB's design it's flawed... it is what it is LOL)
        private long _id;
        private ComprobantesData _data;
        private DBTiposComprobantes _tipoComprobante = null;
        private readonly List<DBComprobantePago> _db_pagos = new List<DBComprobantePago>();


        public static bool RemoveFromDB(MySqlConnection conn, DBEmpresa cuenta, long ec_id, long id)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table}  WHERE {NameOf_cm_em_id} = {cuenta.GetID()} AND {NameOf_cm_ec_id} = {ec_id} AND {NameOf_id} = {id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<static> Error tratando de eliminar una fila de la base de datos en DBComprobantes: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return deletedCorrectly;
        }

        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBEmpresa cuenta)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string te_table = DBTipoEntidad.db_table;
                string ec_table = DBEntidades.db_table;
                string tc_table = DBTiposComprobantes.db_table;

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_cm_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_cm_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} 
                WHERE {NameOf_cm_em_id} = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe(DBTiposComprobantes.NameOf_id), reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre));
                    DBTipoEntidad newTipoEntidadComercial = new DBTipoEntidad(reader.GetInt64Safe(DBTipoEntidad.NameOf_id), reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre));
                    DBEntidades newEntidad = new DBEntidades(cuenta,
                        reader.GetInt64Safe(DBEntidades.NameOf_id),
                        newTipoEntidadComercial,
                        new EntidadesComercialesData(
                            reader.GetInt64Safe(EntidadesComercialesData.NameOf_ec_cuit),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_rs),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_email),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_telefono),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_celular)));

                    returnList.Add(new DBComprobantes(newEntidad,
                        reader.GetInt64Safe(NameOf_id),
                        newTipoComprobante,
                        new ComprobantesData(
                            reader.GetDateTimeSafe(ComprobantesData.NameOf_cm_fecha),
                            reader.GetStringSafe(ComprobantesData.NameOf_cm_numero),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_iva),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_no_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_percepcion),
                            Convert.ToBoolean(reader.GetInt32(ComprobantesData.NameOf_cm_emitido))))); 
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBComprobantes> GetAll(MySqlConnection conn, DBEntidades entidadComercial)
        {
            List<DBComprobantes> returnList = new List<DBComprobantes>();
            try
            {
                string te_table = DBTipoEntidad.db_table;
                string ec_table = DBEntidades.db_table;
                string tc_table = DBTiposComprobantes.db_table;

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_cm_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_cm_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} 
                WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe(DBTiposComprobantes.NameOf_id), reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre));
                    returnList.Add(new DBComprobantes(entidadComercial,
                        reader.GetInt64Safe(NameOf_id),
                        newTipoComprobante,
                        new ComprobantesData(
                            reader.GetDateTimeSafe(ComprobantesData.NameOf_cm_fecha),
                            reader.GetStringSafe(ComprobantesData.NameOf_cm_numero),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_iva),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_no_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_percepcion),
                            Convert.ToBoolean(reader.GetInt32(ComprobantesData.NameOf_cm_emitido))))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBComprobantes GetByID(MySqlConnection conn, DBEmpresa cuenta, long ec_id, long id)
        {
            DBComprobantes returnEnt = null;
            try
            {
                string te_table = DBTipoEntidad.db_table;
                string ec_table = DBEntidades.db_table;
                string tc_table = DBTiposComprobantes.db_table;

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_cm_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_cm_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} 
                WHERE {NameOf_cm_em_id} = {cuenta.GetID()} AND {NameOf_cm_ec_id} = {ec_id} AND {NameOf_id} = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe(DBTiposComprobantes.NameOf_id), reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre));
                    DBTipoEntidad newTipoEntidadComercial = new DBTipoEntidad(reader.GetInt64Safe(DBTipoEntidad.NameOf_id), reader.GetStringSafe(TiposEntidadesData.NameOf_te_nombre));
                    DBEntidades newEntidad = new DBEntidades(cuenta,
                        reader.GetInt64Safe(DBEntidades.NameOf_id),
                        newTipoEntidadComercial,
                        new EntidadesComercialesData(
                            reader.GetInt64Safe(EntidadesComercialesData.NameOf_ec_cuit),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_rs),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_email),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_telefono),
                            reader.GetStringSafe(EntidadesComercialesData.NameOf_ec_celular)));

                    returnEnt = new DBComprobantes(newEntidad,
                        reader.GetInt64Safe(NameOf_id),
                        newTipoComprobante,
                        new ComprobantesData(
                            reader.GetDateTimeSafe(ComprobantesData.NameOf_cm_fecha),
                            reader.GetStringSafe(ComprobantesData.NameOf_cm_numero),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_iva),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_no_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_percepcion),
                            Convert.ToBoolean(reader.GetInt32(ComprobantesData.NameOf_cm_emitido))));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public static DBComprobantes GetByID(MySqlConnection conn, DBEntidades entidadComercial, long id)
        {
            DBComprobantes returnEnt = null;
            try
            {
                string te_table = DBTipoEntidad.db_table;
                string ec_table = DBEntidades.db_table;
                string tc_table = DBTiposComprobantes.db_table;

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTiposComprobantes.NameOf_id} = {db_table}.{NameOf_cm_tc_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_cm_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_cm_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id} 
                WHERE {NameOf_cm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {entidadComercial.GetID()} AND {NameOf_id} = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(
                        reader.GetInt64Safe(DBTiposComprobantes.NameOf_id),
                        reader.GetStringSafe(TiposComprobantesData.NameOf_tc_nombre));
                    returnEnt = new DBComprobantes(entidadComercial,
                        reader.GetInt64Safe(NameOf_id),
                        newTipoComprobante,
                        new ComprobantesData(
                            reader.GetDateTimeSafe(ComprobantesData.NameOf_cm_fecha),
                            reader.GetStringSafe(ComprobantesData.NameOf_cm_numero),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_iva),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_no_gravado),
                            reader.GetDoubleSafe(ComprobantesData.NameOf_cm_percepcion),
                            Convert.ToBoolean(reader.GetInt32(ComprobantesData.NameOf_cm_emitido)))); //Waste of persformance but helps with making the code less propense to error.
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los comprobantes de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public static DBComprobantes GetByID(List<DBComprobantes> listaComprobantes, DBEmpresa cuenta, long ec_id, long id)
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

        public static bool CheckIfExistsInList(List<DBComprobantes> listaComprobantes, DBComprobantes ent)
        {
            foreach (DBComprobantes comprobante in listaComprobantes)
            {
                if (comprobante.GetCuentaID() == ent.GetCuentaID() && comprobante.GetEntidadComercialID() == ent.GetEntidadComercialID() && comprobante.GetID() == ent.GetID())
                {
                    return true;
                }
            }
            return false;
        }

        public DBComprobantes(DBEntidades entidadComercial, long id, DBTiposComprobantes newTipo, ComprobantesData newData)
        {
            _id = id;
            _entidadComercial = entidadComercial;
            _tipoComprobante = newTipo; //is this a good idea at all, to clone?
            _data = newData;
        }

        public DBComprobantes(DBEntidades entidadComercial, long id, long tc_id, ComprobantesData newData)
        {
            _id = id;
            _entidadComercial = entidadComercial;
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _data = newData;
        }

        public DBComprobantes(DBEmpresa cuentaSeleccioanda, long id, long ec_id, DBTiposComprobantes newTipo, ComprobantesData newData)
        {
            _id = id;
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoComprobante = newTipo;
            _data = newData;
        }

        public DBComprobantes(DBEmpresa cuentaSeleccioanda, long id, long ec_id, long tc_id, ComprobantesData newData)
        {
            _id = id;
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _data = newData;
        }
        public DBComprobantes(DBEmpresa cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, long tc_id, ComprobantesData newData)
        {
            _id = id;
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _data = newData;
        }

        public DBComprobantes(DBEmpresa cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, DBTiposComprobantes newTipo, ComprobantesData newData)
        {
            _id = id;
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoComprobante = newTipo;
            _data = newData;
        }

        public DBComprobantes(
            DBEntidades entidadComercial,
            DBTiposComprobantes newTipo,
            long id,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado=0.0,
            double percepcion=0.0
        ) : this(
            entidadComercial,
            id,
            newTipo,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        ) { }

        public DBComprobantes(
            DBEntidades entidadComercial,
            DBTiposComprobantes newTipo,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            entidadComercial,
            -1,
            newTipo,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }

        public DBComprobantes(
            DBEntidades entidadComercial,
            long tc_id,
            long id,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            entidadComercial,
            id,
            tc_id,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }
        public DBComprobantes(
            DBEntidades entidadComercial,
            long tc_id,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            entidadComercial,
            -1,
            tc_id,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }
        public DBComprobantes(
            DBEmpresa cuentaSeleccioanda,
            long ec_id,
            DBTiposComprobantes newTipo,
            long id,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            cuentaSeleccioanda,
            id,
            ec_id,
            newTipo,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }
        public DBComprobantes(
            DBEmpresa cuentaSeleccioanda,
            long ec_id,
            DBTiposComprobantes newTipo,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            cuentaSeleccioanda,
            -1,
            ec_id,
            newTipo,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }
        public DBComprobantes(
            DBEmpresa cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            DBTiposComprobantes newTipo,
            long id,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            cuentaSeleccioanda,
            conn,
            id,
            ec_id,
            newTipo,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }
        public DBComprobantes(
            DBEmpresa cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            DBTiposComprobantes newTipo,
            bool emitido,
            DateTime fecha,
            string numero,
            double gravado,
            double iva,
            double no_gravado = 0.0,
            double percepcion = 0.0
        ) : this(
            cuentaSeleccioanda,
            conn,
            -1,
            ec_id,
            newTipo,
            new ComprobantesData(fecha, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }
        public DBComprobantes(DBEntidades entidadComercial, DBTiposComprobantes newTipo, MySqlDataReader reader) : this (
            entidadComercial,
            reader.GetInt64Safe(NameOf_id),
            newTipo,
             new ComprobantesData(
                reader.GetDateTimeSafe(ComprobantesData.NameOf_cm_fecha),
                reader.GetStringSafe(ComprobantesData.NameOf_cm_numero),
                reader.GetDoubleSafe(ComprobantesData.NameOf_cm_gravado),
                reader.GetDoubleSafe(ComprobantesData.NameOf_cm_iva),
                reader.GetDoubleSafe(ComprobantesData.NameOf_cm_no_gravado),
                reader.GetDoubleSafe(ComprobantesData.NameOf_cm_percepcion),
                Convert.ToBoolean(reader.GetInt32(ComprobantesData.NameOf_cm_emitido)))) { }

        public ComprobantesData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public DBTiposComprobantes TipoComprobante
        {
            get => _tipoComprobante;
            set
            {
                _tipoComprobante = value;
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
                string fechaEmitido = (_data.cm_fecha.HasValue) ? $"'{((DateTime)_data.cm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_cm_tc_id} = {_tipoComprobante.GetID()}, 
                                {ComprobantesData.NameOf_cm_fecha} = {fechaEmitido}, 
                                {ComprobantesData.NameOf_cm_numero} = '{_data.cm_numero}', 
                                {ComprobantesData.NameOf_cm_gravado} = {_data.cm_gravado.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_iva} = {_data.cm_iva.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_no_gravado} = {_data.cm_no_gravado.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_percepcion} = {_data.cm_percepcion.ToString().Replace(",", ".")}, 
                                {ComprobantesData.NameOf_cm_emitido } = {Convert.ToInt32(_data.cm_emitido)} 
                                WHERE {NameOf_cm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
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
                MessageBox.Show("Error en DBComprobantes::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public override bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string fechaEmitido = (_data.cm_fecha.HasValue) ? $"'{((DateTime)_data.cm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";

                string query = $@"INSERT INTO {db_table} (
                                {NameOf_cm_em_id},
                                {NameOf_cm_ec_id},
                                {NameOf_cm_tc_id},
                                {ComprobantesData.NameOf_cm_fecha},
                                {ComprobantesData.NameOf_cm_numero},
                                {ComprobantesData.NameOf_cm_gravado},
                                {ComprobantesData.NameOf_cm_iva},
                                {ComprobantesData.NameOf_cm_no_gravado},
                                {ComprobantesData.NameOf_cm_percepcion},
                                {ComprobantesData.NameOf_cm_emitido})
                                VALUES (
                                {_entidadComercial.GetCuentaID()},
                                {_entidadComercial.GetID()},
                                {_tipoComprobante.GetID()},
                                {fechaEmitido},
                                '{_data.cm_numero}',
                                {_data.cm_gravado.ToString().Replace(",", ".")},
                                {_data.cm_iva.ToString().Replace(",", ".")},
                                {_data.cm_no_gravado.ToString().Replace(",", ".")},
                                {_data.cm_percepcion.ToString().Replace(",", ".")},
                                {Convert.ToInt32(_data.cm_emitido)})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBComprobantes::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public override bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table} WHERE {NameOf_cm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_cm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBComprobantes: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (deletedCorrectly)
            {
                _entidadComercial.RemoveComprobante(this);
            }
            return deletedCorrectly;
        }
        public override bool? ExistsInDatabase(MySqlConnection conn)
        {
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

        public List<DBComprobantePago> GetAllPagos(MySqlConnection conn) //Get directly from database
        {
            List<DBComprobantePago> returnList = DBComprobantePago.GetAll(conn, this);
            _db_pagos.Clear();
            foreach (DBComprobantePago pago in returnList)
            {
                _db_pagos.Add(pago);
            }
            return returnList;
        }
        public List<DBComprobantePago> GetAllPagos() //Get CACHE
        {
            List<DBComprobantePago> returnList = new List<DBComprobantePago>();
            foreach (DBComprobantePago pago in _db_pagos)
            {
                returnList.Add(pago);
            }
            return returnList;
        }
        public DBComprobantePago GetPagoByID(long cp_id)
        {
            return DBComprobantePago.GetByID(_db_pagos, this, cp_id);
        }

        public bool AddPago(DBComprobantePago newPago)
        {
            if (newPago.GetCuentaID() != GetCuentaID() || newPago.GetEntidadComercialID() != GetEntidadComercialID() || newPago.GetComprobanteID() != GetID())
            {
                return false; //Cannot add an payament from another account, entity or receipt like this...
            }
            if (DBComprobantePago.CheckIfExistsInList(_db_pagos, newPago))
            {
                return false;
            }
            _db_pagos.Add(newPago);
            return true;
        }
        private long GetLocalPagoAvailableID()
        {
            long availableID = -1;
            foreach (DBComprobantePago pago in _db_pagos)
            {
                if (pago.GetID() <= availableID)
                {
                    availableID = pago.GetID() - 1;
                }
            }
            return availableID;
        }
        public bool AddPagoLocally(DBComprobantePago newPago) //used to handle local data that is not stored at the DB yet...
        {
            if (newPago.GetCuentaID() != GetCuentaID() || newPago.GetEntidadComercialID() != GetEntidadComercialID() || newPago.GetComprobanteID() != GetID())
            {
                Console.WriteLine("cannot add new pago...");
                return false; //Cannot add an payament from another account, entity or receipt like this...
            }
            if (DBComprobantePago.CheckIfExistsInList(_db_pagos, newPago))
            {
                return false;
            }
            _db_pagos.Add(newPago);
            return true;
        }

        public void RemovePago(DBComprobantePago entRemove)
        {
            _db_pagos.Remove(entRemove);
        }

        protected override void ChangeID(long id) => _id = id;
        public override long GetID() => _id;
        public long GetEntidadComercialID() => _entidadComercial.GetID();

        ///<summary>
        ///Returns a reference to the Bussiness Entity that contains this business receipt.
        ///</summary>
        public DBEntidades GetEntidadComercial() => _entidadComercial;
        public long GetCuentaID() => _entidadComercial.GetCuentaID();

        public DBEmpresa GetCuenta() => _entidadComercial.GetCuenta();
        public DBTiposComprobantes GetTipoComprobante() => _tipoComprobante.Clone();
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

        ///<summary>
        ///Returns if this business receipt was emitted to get payed or received to be payed.
        ///</summary>
        public bool IsEmitido() => _data.cm_emitido;

        public void SetEntidadComercial(DBEntidades newEntidadComercial) => _entidadComercial = newEntidadComercial;
        public void SetEntidadComercial(long ec_id) => _entidadComercial = GetCuenta().GetEntidadByID(ec_id);
        public void SetEntidadComercial(long ec_id, MySqlConnection conn) => _entidadComercial = DBEntidades.GetByID(conn, GetCuenta(), ec_id);
        public void SetTipoComprobante(DBTiposComprobantes newType) => _tipoComprobante = newType.Clone();
        public void SetTipoComprobante(long tc_id) => _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
        public void SetTipoComprobante(long tc_id, MySqlConnection conn) => _tipoComprobante = DBTiposComprobantes.GetByID(tc_id, conn);
        public void SetNumeroComprobante(string numeroCom) => _data.cm_numero = numeroCom;
        public void SetFechaEmitido(DateTime? newFecha) => _data.cm_fecha = newFecha;
        public void SetGravado(double gravado) => _data.cm_gravado = gravado;
        public void SetIVA(double IVA) => _data.cm_iva = IVA;
        public void SetNoGravado(double no_gravado) => _data.cm_no_gravado = no_gravado;
        public void SetPercepcion(double percepcion) => _data.cm_percepcion = percepcion;
        public void SetEmitido(bool esEmitido) => _data.cm_emitido = esEmitido;
    }
}
