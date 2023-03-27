using System;
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
    public struct ReciboData
    {
        public ReciboData(DateTime? fecha, string numero, string obs, bool emitido)
        {
            rc_fecha = fecha;
            rc_nro = numero;
            rc_obs = obs;
            rc_emitido = emitido;
        }
        public DateTime? rc_fecha { get; set; }
        public string rc_nro { get; set; }
        public string rc_obs { get; set; }

        public bool rc_emitido { get; set; }

        public static readonly string NameOf_rc_fecha = nameof(rc_fecha);
        public static readonly string NameOf_rc_nro = nameof(rc_nro);
        public static readonly string NameOf_rc_obs = nameof(rc_obs);
        public static readonly string NameOf_rc_emitido = nameof(rc_emitido);

        public static ReciboData CreateFromReader(MySqlDataReader reader)
        {
            return new ReciboData(reader.GetDateTimeSafe(NameOf_rc_fecha),
                                        reader.GetStringSafe(NameOf_rc_nro),
                                        reader.GetStringSafe(NameOf_rc_obs),
                                        Convert.ToBoolean(reader.GetInt32Safe(NameOf_rc_emitido)));
        }
        public override string ToString() => $"Emitido: {rc_emitido} - Fecha: {rc_fecha} - Número: {rc_nro} - Observación: {rc_obs}";
    }
    public class DBRecibo : DBBaseClass, IDBase<DBRecibo>, IDBCuenta<DBCuenta>, IDBEntidadComercial<DBEntidades>
    {
        public const string db_table = "recibos";
        public const string db_relation_table = "recibos_comprobantes";
        public const string NameOf_rc_em_id = "rc_em_id";
        public const string NameOf_rc_ec_id = "rc_ec_id";
        public const string NameOf_rc_tr_id = "rc_tr_id";
        public const string NameOf_id = "rc_id";
        ///<summary>
        ///Commercial entity associated with this business receipt.
        ///</summary>
        private DBEntidades _entidadComercial; //this can change actually... (Maybe the DB's design it's flawed... it is what it is LOL)
        private ReciboData _data;
        private DBTipoRecibo _tipoRecibo = null;
        private readonly List<DBPago> _db_pagos = new List<DBPago>();
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            string te_table = DBTipoEntidad.db_table;
            string ec_table = DBEntidades.db_table;
            string tc_table = DBTipoRecibo.db_table;

            return $@"SELECT {fieldsToGet} FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTipoRecibo.NameOf_id} = {db_table}.{NameOf_rc_tr_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_rc_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_rc_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id}";
        }
        string IDBase<DBRecibo>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBRecibo> GetAll(MySqlConnection conn, DBCuenta cuenta)
        {
            List<DBRecibo> returnList = new List<DBRecibo>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rc_em_id} = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRecibo(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTipoRecibo(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los Recibos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBRecibo> GetAll(MySqlConnection conn, DBEntidades entidadComercial)
        {
            List<DBRecibo> returnList = new List<DBRecibo>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rc_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRecibo(entidadComercial, new DBTipoRecibo(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los Recibos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBRecibo> GetAll(MySqlConnection conn, DBComprobantes comprobante)
        {
            List<DBRecibo> returnList = new List<DBRecibo>();
            try
            {
                string tr_table = DBTipoRecibo.db_table;
                string cm_table = DBComprobantes.db_table;
                string query = $@"SELECT * FROM {db_relation_table} 
                JOIN {db_table} ON {db_relation_table}.rp_em_id = {db_table}.{NameOf_rc_em_id} AND {db_relation_table}.rp_ec_id = {db_table}.{NameOf_rc_ec_id} AND {db_relation_table}.rp_rc_id = {db_table}.{NameOf_id} 
                JOIN {cm_table} ON {db_relation_table}.rp_em_id = {cm_table}.{DBComprobantes.NameOf_cm_em_id} AND {db_relation_table}.rp_ec_id = {cm_table}.{DBComprobantes.NameOf_cm_ec_id} AND {db_relation_table}.rp_cm_id = {cm_table}.{DBComprobantes.NameOf_id} 
                JOIN {tr_table} ON {tr_table}.{DBTipoRecibo.NameOf_id} = {db_table}.{NameOf_rc_tr_id} 
                WHERE rp_em_id = {comprobante.GetCuentaID()} AND rp_ec_id = {comprobante.GetEntidadComercialID()} AND rp_cm_id = {comprobante.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRecibo(comprobante.GetEntidadComercial(), new DBTipoRecibo(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los recibos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBRecibo GetByID(MySqlConnection conn, DBEntidades entidadComercial, long id)
        {
            DBRecibo returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rc_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {entidadComercial.GetID()} AND {NameOf_id} = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBRecibo(entidadComercial, new DBTipoRecibo(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los Recibos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }
        public static DBRecibo GetByID(MySqlConnection conn, DBCuenta cuenta, long ec_id, long id) => GetByID(conn, DBEntidades.GetByID(conn, cuenta, ec_id), id);

        public static DBRecibo GetByID(List<DBRecibo> listaRecibos, DBCuenta cuenta, long ec_id, long id)
        {
            foreach (DBRecibo Recibo in listaRecibos)
            {
                if (Recibo.GetID() == id && Recibo.GetEntidadComercialID() == ec_id && Recibo.GetCuentaID() == cuenta.GetID())
                {
                    return Recibo;
                }
            }
            return null;
        }

        public static DBRecibo GetByID(List<DBRecibo> listaRecibos, DBEntidades entidadComercial, long id)
        {
            foreach (DBRecibo Recibo in listaRecibos)
            {
                if (Recibo.GetID() == id && Recibo.GetEntidadComercialID() == entidadComercial.GetID() && Recibo.GetCuentaID() == entidadComercial.GetCuentaID())
                {
                    return Recibo;
                }
            }

            return null;
        }

        public static bool CheckIfExistsInList(List<DBRecibo> listaRecibos, DBRecibo ent)
        {
            foreach (DBRecibo Recibo in listaRecibos)
            {
                if (Recibo.GetCuentaID() == ent.GetCuentaID() && Recibo.GetEntidadComercialID() == ent.GetEntidadComercialID() && Recibo.GetID() == ent.GetID())
                {
                    return true;
                }
            }
            return false;
        }

        /***********************
         * Filter/Search methods
         * *********************/

        /************
         *  estadoEmision:
         *      -1: Todos
         *      0: Recibido
         *      1: Emitido
         *************/
        public static List<DBRecibo> Search(MySqlConnection conn, DBCuenta cuenta, int estadoEmision, DateTime? fechaComienzo, DateTime? fechaFinal, long CUIT, long rc_tr_id, long ec_te_id, string numero)
        {
            List<DBRecibo> returnList = new List<DBRecibo>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rc_em_id} = {cuenta.GetID()} ";
                estadoEmision -= 1;
                if (rc_tr_id > -1)
                {
                    query += $"AND {NameOf_rc_tr_id} = {rc_tr_id} ";
                }
                if (ec_te_id > -1)
                {
                    query += $"AND {DBEntidades.NameOf_ec_te_id} = {ec_te_id} ";
                }
                if (estadoEmision >= 0)
                {
                    query += $"AND {ReciboData.NameOf_rc_emitido} = {estadoEmision} ";
                }
                if (!(fechaComienzo is null))
                {
                    string fechaComienzoStr = ((DateTime)fechaComienzo).ToString("yyyy/MM/dd");
                    query += $"AND {ReciboData.NameOf_rc_fecha} >= '{fechaComienzoStr}' ";
                }
                if (!(fechaFinal is null))
                {
                    string fechaFinalStr = ((DateTime)fechaFinal).ToString("yyyy/MM/dd");
                    query += $"AND {ReciboData.NameOf_rc_fecha} <= '{fechaFinalStr}' ";
                }
                if (!string.IsNullOrEmpty(numero.Trim()))
                {
                    query += $"AND UPPER({ReciboData.NameOf_rc_nro}) LIKE '%{numero.Trim().ToUpper()}%'";
                }
                if (CUIT > 0)
                {
                    query += $"AND {EntidadesComercialesData.NameOf_ec_cuit} LIKE '%{CUIT}%'";
                }
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRecibo(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTipoRecibo(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBRecibo::Search. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return returnList;
        }

        public static List<DBRecibo> SearchByNumber(MySqlConnection conn, DBEntidades entidadComercial, string numeroRecibo, bool isEmitido)
        {
            List<DBRecibo> returnList = new List<DBRecibo>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rc_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {entidadComercial.GetID()} AND {ReciboData.NameOf_rc_emitido} = {Convert.ToInt32(isEmitido)} AND UPPER({ReciboData.NameOf_rc_nro}) LIKE '%{numeroRecibo.Trim().ToUpper()}%'";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRecibo(entidadComercial, new DBTipoRecibo(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBRecibo::SearchByNumber. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBRecibo GetByNumber(MySqlConnection conn, DBEntidades entidadComercial, string numeroRecibo, bool isEmitido)
        {
            DBRecibo returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rc_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {entidadComercial.GetID()} AND {ReciboData.NameOf_rc_emitido} = {Convert.ToInt32(isEmitido)} AND UPPER({ReciboData.NameOf_rc_nro}) = '{numeroRecibo.Trim().ToUpper()}'";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBRecibo(entidadComercial, new DBTipoRecibo(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBRecibo::GetByNumber. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBRecibo(DBEntidades entidadComercial, long id, DBTipoRecibo newTipo, ReciboData newData) : base (id)
        {
            _entidadComercial = entidadComercial;
            _tipoRecibo = newTipo;
            _data = newData;
        }

        public DBRecibo(DBEntidades entidadComercial, long id, long tr_id, ReciboData newData) : base (id)
        {
            _entidadComercial = entidadComercial;
            _tipoRecibo = DBTipoRecibo.GetByID(tr_id);
            _data = newData;
        }

        public DBRecibo(DBCuenta cuentaSeleccioanda, long id, long ec_id, DBTipoRecibo newTipo, ReciboData newData) : base (id)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoRecibo = newTipo;
            _data = newData;
        }

        public DBRecibo(DBCuenta cuentaSeleccioanda, long id, long ec_id, long tr_id, ReciboData newData) : base (id)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoRecibo = DBTipoRecibo.GetByID(tr_id);
            _data = newData;
        }
        public DBRecibo(DBCuenta cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, long tr_id, ReciboData newData) : base(id)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoRecibo = DBTipoRecibo.GetByID(tr_id);
            _data = newData;
        }

        public DBRecibo(DBCuenta cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, DBTipoRecibo newTipo, ReciboData newData) : base (id)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoRecibo = newTipo;
            _data = newData;
        }

        public DBRecibo(
            DBEntidades entidadComercial,
            DBTipoRecibo newTipo,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs
        ) : this(
            entidadComercial,
            id,
            newTipo,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }

        public DBRecibo(
            DBEntidades entidadComercial,
            DBTipoRecibo newTipo,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            entidadComercial,
            -1,
            newTipo,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }

        public DBRecibo(
            DBEntidades entidadComercial,
            long tr_id,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            entidadComercial,
            id,
            tr_id,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }
        public DBRecibo(
            DBEntidades entidadComercial,
            long tr_id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            entidadComercial,
            -1,
            tr_id,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }
        public DBRecibo(
            DBCuenta cuentaSeleccioanda,
            long ec_id,
            DBTipoRecibo newTipo,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            cuentaSeleccioanda,
            id,
            ec_id,
            newTipo,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }
        public DBRecibo(
            DBCuenta cuentaSeleccioanda,
            long ec_id,
            DBTipoRecibo newTipo,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            cuentaSeleccioanda,
            -1,
            ec_id,
            newTipo,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }
        public DBRecibo(
            DBCuenta cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            bool emitido,
            DBTipoRecibo newTipo,
            long id,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            cuentaSeleccioanda,
            conn,
            id,
            ec_id,
            newTipo,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }
        public DBRecibo(
            DBCuenta cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            DBTipoRecibo newTipo,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs=""
        ) : this(
            cuentaSeleccioanda,
            conn,
            -1,
            ec_id,
            newTipo,
            new ReciboData(fecha, numero, obs, emitido)
        )
        { }
        public DBRecibo(DBEntidades entidadComercial, DBTipoRecibo newTipo, MySqlDataReader reader) : this(
            entidadComercial,
            reader.GetInt64Safe(NameOf_id),
            newTipo,
            ReciboData.CreateFromReader(reader))
        { }

        public bool PushToDatabase(MySqlConnection conn, long old_rc_ec_id)
        {
            long old_cm_id = GetID();
            bool wasAbleToPushAndDelete = false;
            if (!IsLocal() && old_rc_ec_id != GetEntidadComercialID())
            {
                MakeLocal();
            }
            else
            {
                return PushToDatabase(conn);
            }
            if (InsertIntoToDatabase(conn, old_rc_ec_id, old_cm_id))
            {
                DBRecibo oldRecibo = GetByID(conn, GetCuenta(), old_rc_ec_id, old_cm_id);
                if (!(oldRecibo is null))
                {
                    wasAbleToPushAndDelete = oldRecibo.DeleteFromDatabase(conn);
                }
                else
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_rc_em_id} = {GetCuentaID()} AND {NameOf_rc_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_recibo_id = -1;
                long new_entidad_comercial_id = -1;
                while (reader.Read())
                {
                    _data = ReciboData.CreateFromReader(reader);
                    new_tipo_recibo_id = reader.GetInt64Safe(NameOf_rc_tr_id);
                    new_entidad_comercial_id = reader.GetInt64Safe(NameOf_rc_ec_id);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();

                if (new_tipo_recibo_id != -1)
                {
                    _tipoRecibo = DBTipoRecibo.GetByID(new_tipo_recibo_id, conn);
                }
                if (new_entidad_comercial_id != -1)
                {
                    _entidadComercial = DBEntidades.GetByID(conn, GetCuenta(), new_entidad_comercial_id);
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
                string fechaEmitido = (_data.rc_fecha.HasValue) ? $"'{((DateTime)_data.rc_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_rc_tr_id} = {_tipoRecibo.GetID()}, 
                                {ReciboData.NameOf_rc_fecha} = {fechaEmitido}, 
                                {ReciboData.NameOf_rc_nro} = '{Regex.Replace(_data.rc_nro.Trim(), @"\s+", " ")}', 
                                {ReciboData.NameOf_rc_obs} = '{_data.rc_obs}', 
                                {ReciboData.NameOf_rc_emitido} = {Convert.ToInt32(_data.rc_emitido)} 
                                WHERE {NameOf_rc_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBRecibo::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public bool InsertIntoToDatabase_Raw(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string fechaEmitido = (_data.rc_fecha.HasValue) ? $"'{((DateTime)_data.rc_fecha).ToString("yyyy-MM-dd")}'" : "NULL";

                string query = $@"INSERT INTO {db_table} (
                                {NameOf_rc_em_id},
                                {NameOf_rc_ec_id},
                                {NameOf_rc_tr_id},
                                {ReciboData.NameOf_rc_fecha},
                                {ReciboData.NameOf_rc_nro}, 
                                {ReciboData.NameOf_rc_obs}, 
                                {ReciboData.NameOf_rc_emitido} ) 
                                VALUES (
                                {_entidadComercial.GetCuentaID()},
                                {_entidadComercial.GetID()},
                                {_tipoRecibo.GetID()},
                                {fechaEmitido},
                                '{Regex.Replace(_data.rc_nro.Trim(), @"\s+", " ")}',
                                '{_data.rc_obs}', 
                                {Convert.ToInt32(_data.rc_emitido)} )";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    _entidadComercial.AddNewRecibo(this);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBRecibo::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_relation_table} WHERE rp_em_id = {GetCuentaID()} AND rp_ec_id = {GetEntidadComercialID()} AND rp_rc_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                query = $"DELETE FROM {DBPago.db_table} WHERE {DBPago.NameOf_pg_em_id} = {GetCuentaID()} AND {DBPago.NameOf_pg_ec_id} = {GetEntidadComercialID()} AND {DBPago.NameOf_pg_rc_id} = {GetID()}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error tratando de eliminar información relacionada a un recibo en DBRecibo: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_table} WHERE {NameOf_rc_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _entidadComercial.RemoveRecibo(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRecibo: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public bool? DuplicatedExistsInDatabase(MySqlConnection conn, long ignore_ec_id, long ignore_rc_id)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = "";
                if (_data.rc_emitido) //IF emitido, then the number should be unique across ALL DBEntidades belonging to this account
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rc_em_id} = {GetCuentaID()} AND (({NameOf_rc_ec_id} <> {ignore_ec_id}) OR ({NameOf_id} <> {ignore_rc_id})) AND {NameOf_id} <> {GetID()} AND UPPER({ReciboData.NameOf_rc_nro}) = '{Regex.Replace(_data.rc_nro.Trim().ToUpper(), @"\s+", " ")}'";
                }
                else //If not emitido (recibido) then the number should be unique only for this specific DBEntidades
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rc_em_id} = {GetCuentaID()} AND {NameOf_rc_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} <> {GetID()} AND (({NameOf_rc_ec_id} <> {ignore_ec_id}) OR ({NameOf_id} <> {ignore_rc_id})) AND UPPER({ReciboData.NameOf_rc_nro}) = '{Regex.Replace(_data.rc_nro.Trim().ToUpper(), @"\s+", " ")}'";
                }
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRecibo::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                duplicatedExistsInDB = null;
            }
            return duplicatedExistsInDB;
        }

        public override bool? DuplicatedExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = "";
                if (_data.rc_emitido) //IF emitido, then the number should be unique across ALL DBEntidades belonging to this account
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rc_em_id} = {GetCuentaID()} AND {NameOf_id} <> {GetID()} AND UPPER({ReciboData.NameOf_rc_nro}) = '{Regex.Replace(_data.rc_nro.Trim().ToUpper(), @"\s+", " ")}'";
                } else //If not emitido (recibido) then the number should be unique only for this specific DBEntidades
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rc_em_id} = {GetCuentaID()} AND {NameOf_rc_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} <> {GetID()} AND UPPER({ReciboData.NameOf_rc_nro}) = '{_data.rc_nro.Trim().ToUpper()}'";
                }
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRecibo::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rc_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_rc_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRecibo::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }

        private bool? CheckIfRelatioshipWithComprobanteExistsDB(MySqlConnection conn, DBComprobantes comprobante)
        {
            if (IsLocal() || comprobante.IsLocal())
            {
                return false;
            }
            bool? existsInDB = null;
            try
            {
                string query = $"SELECT COUNT(*) FROM {db_relation_table} WHERE rp_em_id = {_entidadComercial.GetCuentaID()} AND rp_ec_id = {_entidadComercial.GetID()} AND rp_cm_id = {comprobante.GetID()} AND rp_rc_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRecibo::CheckIfRelatioshipWithComprobanteExistsDB: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                existsInDB = null;
            }
            return existsInDB;
        }
        public bool PushRelationshipComprobanteDB(MySqlConnection conn, DBComprobantes comprobante)
        {
            if (IsLocal() || comprobante.IsLocal())
            {
                return false;
            }
            if (comprobante.GetEntidadComercialID() != GetEntidadComercialID() || comprobante.GetCuentaID() != GetCuentaID())
            {
                MessageBox.Show("Error en el método DBRecibo::PushRelationshipComprobanteDB.\nImposible relacionar un comprobante de otra entidad comercial a la del recibo.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (comprobante.IsEmitido() != _data.rc_emitido)
            {
                return false;
            }
            if ((comprobante.ExistsInDatabase(conn) != true) || (ExistsInDatabase(conn) != true))
            {
                MessageBox.Show("Error en el método DBComprobantes::PushRelationshipReciboDB.\n Parece que uno de los datos a relacionar no existe en la base de datos.\nRecuerde llamar esta función solo si los datos están en la base de datos.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CheckIfRelatioshipWithComprobanteExistsDB(conn, comprobante) != false)
            {
                return false;
            }

            bool wasAbleToInsertRelation = false;
            try
            {
                string query = $@"INSERT INTO {db_relation_table} (
                                rp_em_id,
                                rp_ec_id,
                                rp_rc_id,
                                rp_cm_id) 
                                VALUES (
                                {_entidadComercial.GetCuentaID()},
                                {_entidadComercial.GetID()},
                                {GetID()},
                                {comprobante.GetID()})";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsertRelation = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                wasAbleToInsertRelation = false;
                MessageBox.Show("Error DBRecibo::PushRelationshipComprobanteDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return wasAbleToInsertRelation;
        }

        public bool RemoveRelationshipComprobanteDB(MySqlConnection conn, long cm_id)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_relation_table} WHERE rp_em_id = {_entidadComercial.GetCuentaID()} AND rp_ec_id = {_entidadComercial.GetID()} AND rp_cm_id = {cm_id} AND rp_rc_id = {GetID()}";
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

        public bool RemoveRelationshipComprobanteDB(MySqlConnection conn, DBComprobantes comprobante)
        {
            if (IsLocal())
            {
                return false;
            }
            if (comprobante.GetEntidadComercialID() != GetEntidadComercialID() || comprobante.GetCuentaID() != GetCuentaID())
            {
                MessageBox.Show("Error en el método DBRecibo::RemoveRelationshipComprobanteDB.\nImposible relacionar un comprobante de otra entidad comercial a la del recibo.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            bool deletedCorrectly = RemoveRelationshipComprobanteDB(conn, comprobante.GetID());
            if (deletedCorrectly)
            {
                RemoveComprobante(comprobante);
            }
            return deletedCorrectly;
        }
        public bool RemoveAllRelationshipsWithComprobantesDB(MySqlConnection conn)
        {
            if (IsLocal())
            {
                return false;
            }
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_relation_table} WHERE rp_em_id = {_entidadComercial.GetCuentaID()} AND rp_ec_id = {_entidadComercial.GetID()} AND rp_rc_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    _db_comprobantes.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRecibo::RemoveAllRelationshipsWithComprobantesDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                deletedCorrectly = false;
            }
            return deletedCorrectly;
        }
        public void PushAllRelationshipsWithComprobantesDB(MySqlConnection conn)
        {
            foreach (DBComprobantes comprobante in _db_comprobantes)
            {
                PushRelationshipComprobanteDB(conn, comprobante);
            }
        }

        public List<DBComprobantes> GetAllComprobantes(MySqlConnection conn)
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
            return DBComprobantes.GetByID(_db_comprobantes, GetEntidadComercial(), cm_id);
        }

        public bool AddComprobante(DBComprobantes newComprobante)
        {
            if (newComprobante is null)
            {
                return false;
            }
            if (newComprobante.GetCuentaID() != GetCuentaID() || newComprobante.GetEntidadComercialID() != GetEntidadComercialID())
            {
                return false; //Cannot add an payament from another account or entity like this...
            }
            if (newComprobante.IsEmitido() != _data.rc_emitido)
            {
                return false;
            }
            if (_db_comprobantes.Contains(newComprobante))
            {
                return false;
            }
            _db_comprobantes.Add(newComprobante);
            return true;
        }

        public bool AddComprobante(MySqlConnection conn, long cm_id)
        {
            DBComprobantes comprobante = DBComprobantes.GetByID(conn, GetEntidadComercial(), cm_id);
            if (comprobante is null)
            {
                return false;
            }
            if (comprobante.IsEmitido() != _data.rc_emitido)
            {
                return false;
            }
            for (int i=0; i < _db_comprobantes.Count; i++)
            {
                if (_db_comprobantes[i].GetCuentaID() == comprobante.GetCuentaID() && _db_comprobantes[i].GetEntidadComercialID() == comprobante.GetEntidadComercialID() &&  _db_comprobantes[i].GetID() == comprobante.GetID())
                {
                    _db_comprobantes[i] = comprobante;
                    return false;
                }
            }
            return AddComprobante(comprobante);
        }
        public void RemoveComprobante(long cm_id)
        {
            List<DBComprobantes> filteredList = _db_comprobantes.Where(x => x.GetID() != cm_id).ToList();
            _db_comprobantes.Clear();
            foreach(DBComprobantes comprobante in filteredList)
            {
                _db_comprobantes.Add(comprobante);
            }
        }
        public void RemoveComprobante(DBComprobantes entRemove) => _db_comprobantes.Remove(entRemove);

        public List<DBPago> GetAllPagos(MySqlConnection conn) //Get directly from database
        {
            List<DBPago> returnList = DBPago.GetAll(conn, this);
            _db_pagos.Clear();
            foreach (DBPago pago in returnList)
            {
                _db_pagos.Add(pago);
            }
            return returnList;
        }
        public List<DBPago> GetAllPagos() //Get CACHE
        {
            List<DBPago> returnList = new List<DBPago>();
            foreach (DBPago pago in _db_pagos)
            {
                returnList.Add(pago);
            }
            return returnList;
        }
        public DBPago GetPagoByID(long pg_id) => DBPago.GetByID(_db_pagos, this, pg_id);

        public bool AddPago(DBPago newPago)
        {
            if (newPago is null)
            {
                return false;
            }
            if (newPago.GetCuentaID() != GetCuentaID() || newPago.GetEntidadComercialID() != GetEntidadComercialID() || GetID() != newPago.GetReciboID())
            {
                return false; //Cannot add an payament from another account or entity like this...
            }
            if (_db_pagos.Contains(newPago))
            {
                return false;
            }
            _db_pagos.Add(newPago);
            return true;
        }
        public void RemovePago(DBPago entRemove) => _db_pagos.Remove(entRemove);

        public long GetEntidadComercialID() => _entidadComercial.GetID();

        ///<summary>
        ///Returns a reference to the Bussiness Entity that contains this business receipt.
        ///</summary>
        public DBEntidades GetEntidadComercial() => _entidadComercial;
        public long GetCuentaID() => _entidadComercial.GetCuentaID();

        public DBCuenta GetCuenta() => _entidadComercial.GetCuenta();

        public DBTipoRecibo GetTipoRecibo() => _tipoRecibo;

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
        public void SetTipoRecibo(DBTipoRecibo newType)
        {
            _shouldPush = _shouldPush || (_tipoRecibo != newType);
            _tipoRecibo = newType;
        }
        public void SetTipoRecibo(long tc_id)
        {
            _shouldPush = _shouldPush || (tc_id != _tipoRecibo.GetID());
            _tipoRecibo = DBTipoRecibo.GetByID(tc_id);
        }
        public void SetTipoRecibo(long tc_id, MySqlConnection conn)
        {
            _shouldPush = _shouldPush || (tc_id != _tipoRecibo.GetID());
            _tipoRecibo = DBTipoRecibo.GetByID(tc_id, conn);
        }

        public DateTime? GetFecha() => _data.rc_fecha;

        public string GetNumero() => _data.rc_nro;

        public string GetObservacion() => _data.rc_obs;

        ///<summary>
        ///Returns if this business recibo was emitted to get payed or received to be payed.
        ///</summary>
        public bool IsEmitido() => _data.rc_emitido;

        public void SetEmitido(bool esEmitido)
        {
            _shouldPush = _shouldPush || (esEmitido != _data.rc_emitido);
            _data.rc_emitido = esEmitido;
        }

        public void SetFecha(DateTime? fecha)
        {
            _shouldPush = _shouldPush || (fecha != _data.rc_fecha);
            _data.rc_fecha = fecha;
        }
        public void SetNumero(string numero)
        {
            _shouldPush = _shouldPush || !_data.rc_nro.Equals(numero);
            _data.rc_nro = numero;
        }
        public void SetObservacion(string obs)
        {
            _shouldPush = _shouldPush || !_data.rc_obs.Equals(obs);
            _data.rc_obs = obs;
        }

        public override DBBaseClass GetLocalCopy() => new DBRecibo(_entidadComercial, -1, _tipoRecibo, _data);

        public override string ToString() => $"ID: {GetID()} - Tipo: {_tipoRecibo.GetName()} - {_data}";

        /***********************************************
         * Useful functions for Real world applications
         * ********************************************/

        public double GetPagosTotal_MonedaLocal(MySqlConnection conn)
        {
            GetAllPagos(conn);
            return GetPagosTotal_MonedaLocal();
        }
        public double GetPagosTotal_MonedaLocal()
        {
            double totalPagos = 0.0;
            foreach (DBPago pago in _db_pagos)
            {
                totalPagos += pago.GetImporte_MonedaLocal();
            }
            return totalPagos;
        }
        public double GetComprobantesTotal_MonedaLocal(MySqlConnection conn)
        {
            GetAllComprobantes(conn);
            return GetComprobantesTotal_MonedaLocal();
        }
        public double GetComprobantesTotal_MonedaLocal()
        {
            double totalComprobantes = 0.0;
            foreach (DBComprobantes comprobante in _db_comprobantes)
            {
                totalComprobantes += comprobante.GetTotal_MonedaLocal();
            }
            return totalComprobantes;
        }

        /**********************
         * DEBUG STUFF ONLY
         * ********************/

        public string PrintAllPagos()
        {
            string str = "";
            foreach (DBPago pago in _db_pagos)
            {
                str += $"Pago> {pago}\n";
            }
            return str;
        }
        public string PrintAllComprobantes()
        {
            string str = "";
            foreach (DBComprobantes comprobante in _db_comprobantes)
            {
                str += $"Comprobante relacionado> {comprobante}\n";
            }
            return str;
        }

        public static DBRecibo GenerateRandom(DBEntidades entidadComercial)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            DateTime fechaEmitido = new DateTime();
            DateTime? fechaFinal = null;
            string randomDateSTR = $"{r.Next(1, 28)}/{r.Next(1, 13)}/{r.Next(2010, 2024)}";
            if (DateTime.TryParse(randomDateSTR, out fechaEmitido))
            {
                fechaFinal = fechaEmitido;
            }

            return new DBRecibo(entidadComercial, DBTipoRecibo.GetRandom(), Convert.ToBoolean(r.Next(0, 2)), fechaFinal, $"{r.Next(10000, 99999)}", "Sin información");
        }

    }
}