using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha
{
    public struct RemitoData
    {
        public RemitoData(DateTime? fecha, string numero, string obs, bool emitido)
        {
            rm_fecha = fecha;
            rm_nro = numero;
            rm_obs = obs;
            rm_emitido = emitido;
        }
        public DateTime? rm_fecha { get; set; }
        public string rm_nro { get; set; }
        public string rm_obs { get; set; }

        public bool rm_emitido { get; set; }

        public static readonly string NameOf_rm_fecha = nameof(rm_fecha);
        public static readonly string NameOf_rm_nro = nameof(rm_nro);
        public static readonly string NameOf_rm_obs = nameof(rm_obs);
        public static readonly string NameOf_rm_emitido = nameof(rm_emitido);

        public static RemitoData CreateFromReader(MySqlDataReader reader)
        {
            return new RemitoData(reader.GetDateTimeSafe(NameOf_rm_fecha),
                                        reader.GetStringSafe(NameOf_rm_nro),
                                        reader.GetStringSafe(NameOf_rm_obs),
                                        Convert.ToBoolean(reader.GetInt32Safe(NameOf_rm_emitido)));
        }
        public override string ToString() => $"Emitido: {rm_emitido} - Fecha: {rm_fecha} - Número: {rm_nro} - Observación: {rm_obs}";
    }
    public class DBRemito : DBBaseClass, IDBase<DBRemito>, IDBCuenta<DBCuenta>, IDBEntidadComercial<DBEntidades>
    {
        public const string db_table = "remitos";
        public const string db_relation_table = "remitos_comprobantes";
        public const string NameOf_rm_em_id = "rm_em_id";
        public const string NameOf_rm_ec_id = "rm_ec_id";
        public const string NameOf_rm_ts_id = "rm_ts_id";
        public const string NameOf_id = "rm_id";
        ///<summary>
        ///Commercial entity associated with this business receipt.
        ///</summary>
        private DBEntidades _entidadComercial; //this can change actually... (Maybe the DB's design it's flawed... it is what it is LOL)
        private RemitoData _data;
        private DBTipoRemito _tipoRemito = null;
        private readonly List<DBComprobantes> _db_comprobantes = new List<DBComprobantes>();

        public static string GetSQL_SelectQueryWithRelations(string fieldsToGet)
        {
            string te_table = DBTipoEntidad.db_table;
            string ec_table = DBEntidades.db_table;
            string tc_table = DBTipoRemito.db_table;

            return $@"SELECT {fieldsToGet} FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.{DBTipoRemito.NameOf_id} = {db_table}.{NameOf_rm_ts_id} 
                JOIN {ec_table} ON {ec_table}.{DBEntidades.NameOf_id} = {db_table}.{NameOf_rm_ec_id} AND {ec_table}.{DBEntidades.NameOf_ec_em_id} = {db_table}.{NameOf_rm_em_id} 
                JOIN {te_table} ON {te_table}.{DBTipoEntidad.NameOf_id} = {ec_table}.{DBEntidades.NameOf_ec_te_id}";
        }
        string IDBase<DBRemito>.GetSQL_SelectQueryWithRelations(string fieldsToGet) => GetSQL_SelectQueryWithRelations(fieldsToGet);

        public static List<DBRemito> GetAll(MySqlConnection conn, DBCuenta cuenta)
        {
            List<DBRemito> returnList = new List<DBRemito>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rm_em_id} = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRemito(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTipoRemito(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los remitos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBRemito> GetAll(MySqlConnection conn, DBEntidades entidadComercial)
        {
            List<DBRemito> returnList = new List<DBRemito>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRemito(entidadComercial, new DBTipoRemito(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los remitos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBRemito> GetAll(MySqlConnection conn, DBComprobantes comprobante)
        {
            List<DBRemito> returnList = new List<DBRemito>();
            try
            {
                string tr_table = DBTipoRemito.db_table;
                string cm_table = DBComprobantes.db_table;
                string query = $@"SELECT * FROM {db_relation_table} 
                JOIN {db_table} ON {db_relation_table}.rt_em_id = {db_table}.{NameOf_rm_em_id} AND {db_relation_table}.rt_ec_id = {db_table}.{NameOf_rm_ec_id} AND {db_relation_table}.rt_rm_id = {db_table}.{NameOf_id} 
                JOIN {cm_table} ON {db_relation_table}.rt_em_id = {cm_table}.{DBComprobantes.NameOf_cm_em_id} AND {db_relation_table}.rt_ec_id = {cm_table}.{DBComprobantes.NameOf_cm_ec_id} AND {db_relation_table}.rt_cm_id = {cm_table}.{DBComprobantes.NameOf_id} 
                JOIN {tr_table} ON {tr_table}.{DBTipoRemito.NameOf_id} = {db_table}.{NameOf_rm_ts_id} 
                WHERE rt_em_id = {comprobante.GetCuentaID()} AND rt_ec_id = {comprobante.GetEntidadComercialID()} AND rt_cm_id = {comprobante.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRemito(comprobante.GetEntidadComercial(), new DBTipoRemito(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los remitos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBRemito GetByID(MySqlConnection conn, DBEntidades entidadComercial, long id)
        {
            DBRemito returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {entidadComercial.GetID()} AND {NameOf_id} = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBRemito(entidadComercial, new DBTipoRemito(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todos los remitos de una cuenta, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }
        public static DBRemito GetByID(MySqlConnection conn, DBCuenta cuenta, long ec_id, long id) => GetByID(conn, DBEntidades.GetByID(conn, cuenta, ec_id), id);

        public static DBRemito GetByID(List<DBRemito> listaRemitos, DBCuenta cuenta, long ec_id, long id)
        {
            foreach (DBRemito remito in listaRemitos)
            {
                if (remito.GetID() == id && remito.GetEntidadComercialID() == ec_id && remito.GetCuentaID() == cuenta.GetID())
                {
                    return remito;
                }
            }
            return null;
        }

        public static DBRemito GetByID(List<DBRemito> listaRemitos, DBEntidades entidadComercial, long id)
        {
            foreach (DBRemito remito in listaRemitos)
            {
                if (remito.GetID() == id && remito.GetEntidadComercialID() == entidadComercial.GetID() && remito.GetCuentaID() == entidadComercial.GetCuentaID())
                {
                    return remito;
                }
            }

            return null;
        }

        public static int FindInList(List<DBRemito> listaRemitos, DBRemito ent)
        {
            for (int i=0; i < listaRemitos.Count; i++)
            {
                if (listaRemitos[i].GetCuentaID() == ent.GetCuentaID() && listaRemitos[i].GetEntidadComercialID() == ent.GetEntidadComercialID() && listaRemitos[i].GetID() == ent.GetID())
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool CheckIfExistsInList(List<DBRemito> listaRemitos, DBRemito ent) => FindInList(listaRemitos, ent) != -1;

        /************************
         * Filter/Search methods
         * **********************/

        /************
         *  estadoEmision:
         *      -1: Todos
         *      0: Recibido
         *      1: Emitido
         *************/
        public static List<DBRemito> Search(MySqlConnection conn, DBCuenta cuenta, int estadoEmision, DateTime? fechaComienzo, DateTime? fechaFinal, long CUIT, long rm_ts_id, long ec_te_id, string numero)
        {
            List<DBRemito> returnList = new List<DBRemito>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rm_em_id} = {cuenta.GetID()} ";
                estadoEmision -= 1;
                if (rm_ts_id > -1)
                {
                    query += $"AND {NameOf_rm_ts_id} = {rm_ts_id} ";
                }
                if (ec_te_id > -1)
                {
                    query += $"AND {DBEntidades.NameOf_ec_te_id} = {ec_te_id} ";
                }
                if (estadoEmision >= 0)
                {
                    query += $"AND {RemitoData.NameOf_rm_emitido} = {estadoEmision} ";
                }
                if (!(fechaComienzo is null))
                {
                    string fechaComienzoStr = ((DateTime)fechaComienzo).ToString("yyyy/MM/dd");
                    query += $"AND {RemitoData.NameOf_rm_fecha} >= '{fechaComienzoStr}' ";
                }
                if (!(fechaFinal is null))
                {
                    string fechaFinalStr = ((DateTime)fechaFinal).ToString("yyyy/MM/dd");
                    query += $"AND {RemitoData.NameOf_rm_fecha} <= '{fechaFinalStr}' ";
                }
                if (!string.IsNullOrEmpty(numero.Trim()))
                {
                    query += $"AND UPPER({RemitoData.NameOf_rm_nro}) LIKE '%{numero.Trim().ToUpper()}%'";
                }
                if (CUIT > 0)
                {
                    query += $"AND {EntidadesComercialesData.NameOf_ec_cuit} LIKE '%{CUIT}%'";
                }
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRemito(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader), new DBTipoRemito(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBRemito::Search. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return returnList;
        }

        public static List<DBRemito> SearchByNumber(MySqlConnection conn, DBEntidades entidadComercial, string numeroRemito, bool isEmitido)
        {
            List<DBRemito> returnList = new List<DBRemito>();
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {entidadComercial.GetID()} AND {RemitoData.NameOf_rm_emitido} = {Convert.ToInt32(isEmitido)} AND UPPER({RemitoData.NameOf_rm_nro}) LIKE '%{numeroRemito.Trim().ToUpper()}%'";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBRemito(entidadComercial, new DBTipoRemito(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBRemito::SearchByNumber. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static DBRemito GetByNumber(MySqlConnection conn, DBEntidades entidadComercial, string numeroRemito, bool isEmitido)
        {
            DBRemito returnEnt = null;
            try
            {
                string query = $"{GetSQL_SelectQueryWithRelations("*")} WHERE {NameOf_rm_em_id} = {entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {entidadComercial.GetID()} AND {RemitoData.NameOf_rm_emitido} = {Convert.ToInt32(isEmitido)} AND UPPER({RemitoData.NameOf_rm_nro}) = '{numeroRemito.Trim().ToUpper()}'";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnEnt = new DBRemito(entidadComercial, new DBTipoRemito(reader), reader);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en DBRemito::GetByNumber. Problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnEnt;
        }

        public DBRemito(DBEntidades entidadComercial, long id, DBTipoRemito newTipo, RemitoData newData) : base(id)
        {
            _entidadComercial = entidadComercial;
            _tipoRemito = newTipo;
            _data = newData;
        }

        public DBRemito(DBEntidades entidadComercial, long id, long ts_id, RemitoData newData) : base(id)
        {
            _entidadComercial = entidadComercial;
            _tipoRemito = DBTipoRemito.GetByID(ts_id);
            _data = newData;
        }

        public DBRemito(DBCuenta cuentaSeleccioanda, long id, long ec_id, DBTipoRemito newTipo, RemitoData newData) : base(id)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoRemito = newTipo;
            _data = newData;
        }

        public DBRemito(DBCuenta cuentaSeleccioanda, long id, long ec_id, long ts_id, RemitoData newData) : base(id)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoRemito = DBTipoRemito.GetByID(ts_id);
            _data = newData;
        }
        public DBRemito(DBCuenta cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, long ts_id, RemitoData newData) : base(id)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoRemito = DBTipoRemito.GetByID(ts_id);
            _data = newData;
        }

        public DBRemito(DBCuenta cuentaSeleccioanda, MySqlConnection conn, long id, long ec_id, DBTipoRemito newTipo, RemitoData newData) : base(id)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoRemito = newTipo;
            _data = newData;
        }

        public DBRemito(
            DBEntidades entidadComercial,
            DBTipoRemito newTipo,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs
        ) : this(
            entidadComercial,
            id,
            newTipo,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }

        public DBRemito(
            DBEntidades entidadComercial,
            DBTipoRemito newTipo,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            entidadComercial,
            -1,
            newTipo,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }

        public DBRemito(
            DBEntidades entidadComercial,
            long ts_id,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            entidadComercial,
            id,
            ts_id,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }
        public DBRemito(
            DBEntidades entidadComercial,
            long ts_id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            entidadComercial,
            -1,
            ts_id,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }
        public DBRemito(
            DBCuenta cuentaSeleccioanda,
            long ec_id,
            DBTipoRemito newTipo,
            long id,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            cuentaSeleccioanda,
            id,
            ec_id,
            newTipo,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }
        public DBRemito(
            DBCuenta cuentaSeleccioanda,
            long ec_id,
            DBTipoRemito newTipo,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            cuentaSeleccioanda,
            -1,
            ec_id,
            newTipo,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }
        public DBRemito(
            DBCuenta cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            bool emitido,
            DBTipoRemito newTipo,
            long id,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            cuentaSeleccioanda,
            conn,
            id,
            ec_id,
            newTipo,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }
        public DBRemito(
            DBCuenta cuentaSeleccioanda,
            MySqlConnection conn,
            long ec_id,
            DBTipoRemito newTipo,
            bool emitido,
            DateTime? fecha,
            string numero,
            string obs = ""
        ) : this(
            cuentaSeleccioanda,
            conn,
            -1,
            ec_id,
            newTipo,
            new RemitoData(fecha, numero, obs, emitido)
        )
        { }
        public DBRemito(DBEntidades entidadComercial, DBTipoRemito newTipo, MySqlDataReader reader) : this(
            entidadComercial,
            reader.GetInt64Safe(NameOf_id),
            newTipo,
            RemitoData.CreateFromReader(reader))
        { }

        public bool PushToDatabase(MySqlConnection conn, long old_rm_ec_id)
        {
            long old_cm_id = GetID();
            bool wasAbleToPushAndDelete = false;
            if (!IsLocal() && old_rm_ec_id != GetEntidadComercialID())
            {
                MakeLocal();
            }
            else
            {
                return PushToDatabase(conn);
            }
            if (InsertIntoToDatabase(conn, old_rm_ec_id, old_cm_id))
            {
                DBRemito oldRemito = GetByID(conn, GetCuenta(), old_rm_ec_id, old_cm_id);
                if (!(oldRemito is null))
                {
                    wasAbleToPushAndDelete = oldRemito.DeleteFromDatabase(conn);
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
                string query = $"SELECT * FROM {db_table} WHERE {NameOf_rm_em_id} = {GetCuentaID()} AND {NameOf_rm_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                long new_tipo_remito_id = -1;
                long new_entidad_comercial_id = -1;
                while (reader.Read())
                {
                    _data = RemitoData.CreateFromReader(reader);
                    new_tipo_remito_id = reader.GetInt64Safe(NameOf_rm_ts_id);
                    new_entidad_comercial_id = reader.GetInt64Safe(NameOf_rm_ec_id);
                    _shouldPush = false;
                    wasAbleToPull = true;
                }
                reader.Close();

                if (new_tipo_remito_id != -1)
                {
                    _tipoRemito = DBTipoRemito.GetByID(new_tipo_remito_id, conn);
                }
                if (new_entidad_comercial_id != -1)
                {
                    _entidadComercial = DBEntidades.GetByID(conn, GetCuenta(), new_entidad_comercial_id);
                }
            }
            catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en DBRemito::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string fechaEmitido = (_data.rm_fecha.HasValue) ? $"'{((DateTime)_data.rm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string query = $@"UPDATE {db_table} SET 
                                {NameOf_rm_ts_id} = {_tipoRemito.GetID()}, 
                                {RemitoData.NameOf_rm_fecha} = {fechaEmitido}, 
                                {RemitoData.NameOf_rm_nro} = '{Regex.Replace(_data.rm_nro.Trim(), @"\s+", " ")}', 
                                {RemitoData.NameOf_rm_obs} = '{_data.rm_obs}', 
                                {RemitoData.NameOf_rm_emitido} = {Convert.ToInt32(_data.rm_emitido)} 
                                WHERE {NameOf_rm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
                _shouldPush = _shouldPush && !wasAbleToUpdate;
            }
            catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en DBRemito::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToUpdate;
        }

        public bool InsertIntoToDatabase_Raw(MySqlConnection conn)
        {
            bool wasAbleToInsert = false;
            try
            {
                string fechaEmitido = (_data.rm_fecha.HasValue) ? $"'{((DateTime)_data.rm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";

                string query = $@"INSERT INTO {db_table} (
                                {NameOf_rm_em_id},
                                {NameOf_rm_ec_id},
                                {NameOf_rm_ts_id},
                                {RemitoData.NameOf_rm_fecha},
                                {RemitoData.NameOf_rm_nro}, 
                                {RemitoData.NameOf_rm_obs}, 
                                {RemitoData.NameOf_rm_emitido} ) 
                                VALUES (
                                {_entidadComercial.GetCuentaID()},
                                {_entidadComercial.GetID()},
                                {_tipoRemito.GetID()},
                                {fechaEmitido},
                                '{Regex.Replace(_data.rm_nro.Trim(), @"\s+", " ")}',
                                '{_data.rm_obs}', 
                                {Convert.ToInt32(_data.rm_emitido)} )";

                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    ChangeID(cmd.LastInsertedId);
                    _entidadComercial.AddRemito(this);
                }
                _shouldPush = _shouldPush && !wasAbleToInsert;
            }
            catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error DBRemito::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_relation_table} WHERE rt_em_id = {GetCuentaID()} AND rt_ec_id = {GetEntidadComercialID()} AND rt_rm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                deletedCorrectly = false;
                MessageBox.Show("Error tratando de eliminar información relacionada a un remito en DBRemito: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_table} WHERE {NameOf_rm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    MakeLocal();
                    _entidadComercial.RemoveRemito(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRemito: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }
        public bool? DuplicatedExistsInDatabase(MySqlConnection conn, long ignore_ec_id, long ignore_rm_id)
        {
            bool? duplicatedExistsInDB = null;
            try
            {
                string query = "";
                if (_data.rm_emitido) //IF emitido, then the number should be unique across ALL DBEntidades belonging to this account
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rm_em_id} = {GetCuentaID()} AND (({NameOf_rm_ec_id} <> {ignore_ec_id}) OR ({NameOf_id} <> {ignore_rm_id})) AND {NameOf_id} <> {GetID()} AND UPPER({RemitoData.NameOf_rm_nro}) = '{Regex.Replace(_data.rm_nro.Trim().ToUpper(), @"\s+", " ")}'";
                }
                else //If not emitido (recibido) then the number should be unique only for this specific DBEntidades
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rm_em_id} = {GetCuentaID()} AND {NameOf_rm_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} <> {GetID()} AND (({NameOf_rm_ec_id} <> {ignore_ec_id}) OR ({NameOf_id} <> {ignore_rm_id})) AND UPPER({RemitoData.NameOf_rm_nro}) = '{Regex.Replace(_data.rm_nro.Trim().ToUpper(), @"\s+", " ")}'";
                }
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRemito::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (_data.rm_emitido) //IF emitido, then the number should be unique across ALL DBEntidades belonging to this account
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rm_em_id} = {GetCuentaID()} AND {NameOf_id} <> {GetID()} AND UPPER({RemitoData.NameOf_rm_nro}) = '{Regex.Replace(_data.rm_nro.Trim().ToUpper(), @"\s+", " ")}'";
                }
                else //If not emitido (recibido) then the number should be unique only for this specific DBEntidades
                {
                    query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rm_em_id} = {GetCuentaID()} AND {NameOf_rm_ec_id} = {GetEntidadComercialID()} AND {NameOf_id} <> {GetID()} AND UPPER({RemitoData.NameOf_rm_nro}) = '{_data.rm_nro.Trim().ToUpper()}'";
                }
                var cmd = new MySqlCommand(query, conn);
                duplicatedExistsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRemito::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE {NameOf_rm_em_id} = {_entidadComercial.GetCuentaID()} AND {NameOf_rm_ec_id} = {_entidadComercial.GetID()} AND {NameOf_id} = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRemito::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"SELECT COUNT(*) FROM {db_relation_table} WHERE rt_em_id = {_entidadComercial.GetCuentaID()} AND rt_ec_id = {_entidadComercial.GetID()} AND rt_cm_id = {comprobante.GetID()} AND rt_rm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                existsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en el método DBRemito::CheckIfRelatioshipWithComprobanteExistsDB: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBRemito::PushRelationshipComprobanteDB.\nImposible relacionar un comprobante de otra entidad comercial a la del remito.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (comprobante.IsEmitido() != _data.rm_emitido)
            {
                return false;
            }
            if ((comprobante.ExistsInDatabase(conn) != true) || (ExistsInDatabase(conn) != true))
            {
                MessageBox.Show("Error en el método DBComprobantes::PushRelationshipremitoDB.\n Parece que uno de los datos a relacionar no existe en la base de datos.\nRecuerde llamar esta función solo si los datos están en la base de datos.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                                rt_em_id,
                                rt_ec_id,
                                rt_rm_id,
                                rt_cm_id) 
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
                MessageBox.Show("Error DBRemito::PushRelationshipComprobanteDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_relation_table} WHERE rt_em_id = {_entidadComercial.GetCuentaID()} AND rt_ec_id = {_entidadComercial.GetID()} AND rt_cm_id = {cm_id} AND rt_rm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRemito::RemoveRelationshipComprobanteDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error en el método DBRemito::RemoveRelationshipComprobanteDB.\nImposible relacionar un comprobante de otra entidad comercial a la del remito.", "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                string query = $"DELETE FROM {db_relation_table} WHERE rt_em_id = {_entidadComercial.GetCuentaID()} AND rt_ec_id = {_entidadComercial.GetID()} AND rt_rm_id = {GetID()}";
                var cmd = new MySqlCommand(query, conn);
                deletedCorrectly = cmd.ExecuteNonQuery() > 0;
                if (deletedCorrectly)
                {
                    _db_comprobantes.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBRemito::RemoveAllRelationshipsWithComprobantesDB " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (_data.rm_emitido != newComprobante.IsEmitido())
            {
                return false;
            }
            if (_db_comprobantes.Contains(newComprobante))
            {
                return false;
            }
            if (!newComprobante.IsLocal())
            {
                int foundIndex = DBComprobantes.FindInList(_db_comprobantes, newComprobante);
                if (foundIndex != -1) //Update old data with new in case of match.
                {
                    _db_comprobantes[foundIndex] = newComprobante;
                    return true;
                }
            }
            _db_comprobantes.Add(newComprobante);
            return true;
        }

        public bool AddComprobante(MySqlConnection conn, long cm_id) => AddComprobante(DBComprobantes.GetByID(conn, GetEntidadComercial(), cm_id));

        public void RemoveComprobante(long cm_id) => _db_comprobantes.RemoveAll(x => x.GetID() == cm_id);

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
        public long GetEntidadComercialID() => _entidadComercial.GetID();

        ///<summary>
        ///Returns a reference to the Bussiness Entity that contains this business receipt.
        ///</summary>
        public DBEntidades GetEntidadComercial() => _entidadComercial;
        public long GetCuentaID() => _entidadComercial.GetCuentaID();

        public DBCuenta GetCuenta() => _entidadComercial.GetCuenta();

        public DBTipoRemito GetTipoRemito() => _tipoRemito;

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
        public void SetTipoRemito(DBTipoRemito newType)
        {
            _shouldPush = _shouldPush || (_tipoRemito != newType);
            _tipoRemito = newType;
        }
        public void SetTipoRemito(long tc_id)
        {
            _shouldPush = _shouldPush || (tc_id != _tipoRemito.GetID());
            _tipoRemito = DBTipoRemito.GetByID(tc_id);
        }
        public void SetTipoRemito(long tc_id, MySqlConnection conn)
        {
            _shouldPush = _shouldPush || (tc_id != _tipoRemito.GetID());
            _tipoRemito = DBTipoRemito.GetByID(tc_id, conn);
        }

        public DateTime? GetFecha() => _data.rm_fecha;

        public string GetNumero() => _data.rm_nro;

        public string GetObservacion() => _data.rm_obs;

        ///<summary>
        ///Returns if this business recibo was emitted to get payed or received to be payed.
        ///</summary>
        public bool IsEmitido() => _data.rm_emitido;

        public void SetEmitido(bool esEmitido)
        {
            _shouldPush = _shouldPush || (esEmitido != _data.rm_emitido);
            _data.rm_emitido = esEmitido;
        }

        public void SetFecha(DateTime? fecha)
        {
            _shouldPush = _shouldPush || (fecha != _data.rm_fecha);
            _data.rm_fecha = fecha;
        }
        public void SetNumero(string numero)
        {
            _shouldPush = _shouldPush || !_data.rm_nro.Equals(numero);
            _data.rm_nro = numero;
        }
        public void SetObservacion(string obs)
        {
            _shouldPush = _shouldPush || !_data.rm_obs.Equals(obs);
            _data.rm_obs = obs;
        }

        public override DBBaseClass GetLocalCopy() => new DBRemito(_entidadComercial, -1, _tipoRemito, _data);

        public override string ToString() => $"ID: {GetID()} - Tipo: {_tipoRemito.GetName()} - {_data}";

        /**********************
         * DEBUG STUFF ONLY
         * ********************/

        public string PrintAllComprobantes()
        {
            string str = "";
            foreach (DBComprobantes comprobante in _db_comprobantes)
            {
                str += $"Comprobante relacionado> {comprobante}\n";
            }
            return str;
        }

        public static DBRemito GenerateRandom(DBEntidades entidadComercial)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            DateTime fechaEmitido = new DateTime();
            DateTime? fechaFinal = null;
            string randomDateSTR = $"{r.Next(1, 28)}/{r.Next(1, 13)}/{r.Next(2010, 2024)}";
            if (DateTime.TryParse(randomDateSTR, out fechaEmitido))
            {
                fechaFinal = fechaEmitido;
            }

            return new DBRemito(entidadComercial, DBTipoRemito.GetRandom(), Convert.ToBoolean(r.Next(0, 2)), fechaFinal, $"{r.Next(10000, 99999)}", "Sin información");
        }
    }
}