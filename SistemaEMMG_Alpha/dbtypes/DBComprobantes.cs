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
    public struct ComprobantesData
    {
        public ComprobantesData(long id, DateTime? fecha, DateTime? pago, string numero, double gravado, double iva, double no_gravado, double percepcion, bool emitido)
        {
            cm_id = id;
            cm_fecha = fecha;
            cm_fpago = pago;
            cm_numero = numero;
            cm_gravado = gravado;
            cm_iva = iva;
            cm_no_gravado = no_gravado;
            cm_percepcion = percepcion;
            cm_emitido = emitido;
        }
        public long cm_id { get; set; }
        public DateTime? cm_fecha { get; set; } //sql date format 2004-01-22 yyyy-mm-dd use DateTime.Now.ToString("yyyy-mm-dddd"). To convert it from string to DateTime use DateTime.ParseExact(string, "yyyy-mm-dddd", null)
        public DateTime? cm_fpago { get; set; }
        public string cm_numero { get; set; }
        public double cm_gravado { get; set; }
        public double cm_iva { get; set; }
        public double cm_no_gravado { get; set; }
        public double cm_percepcion { get; set; }
        public bool cm_emitido { get; set; }
    }
    public class DBComprobantes : DBInterface, IDBCuenta<DBEmpresa>, IDBEntidadComercial<DBEntidades>
    {
        private static readonly string db_table = "comprobantes";
        ///<summary>
        ///Commercial entity associated with this business receipt.
        ///</summary>
        private DBEntidades _entidadComercial; //this can change actually...
        private ComprobantesData _data;
        private DBTiposComprobantes _tipoComprobante = null;

        /*
         * SELECT * FROM comprobantes
         * JOIN tipos_comprobantes ON tipos_comprobantes.tc_id = comprobantes.cm_tc_id
         * JOIN ent_comerciales ON ent_comerciales.ec_id = comprobantes.cm_ec_id
         * JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id
         * WHERE cm_em_id = 1;
         * */

        public static bool RemoveFromDB(MySqlConnection conn, DBEmpresa cuenta, long ec_id, long id)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM {db_table}  WHERE cm_em_id = {cuenta.GetID()} AND cm_ec_id = {ec_id} AND cm_id = {id}";
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
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.tc_id = {db_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cm_ec_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cm_em_id = {cuenta.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    DBTipoEntidad newTipoEntidadComercial = new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"));
                    DBEntidades newEntidad = new DBEntidades(cuenta, newTipoEntidadComercial, new EntidadesComercialesData(reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetInt64Safe("ec_dni"), reader.GetStringSafe("ec_rs"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular")));

                    returnList.Add(new DBComprobantes(newEntidad, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetDateTimeSafe("cm_fpago"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido"))))); //Waste of persformance but helps with making the code less propense to error.
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
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.tc_id = {db_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cm_ec_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cm_em_id = {entidadComercial.GetCuentaID()} AND cm_ec_id = {entidadComercial.GetID()}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    returnList.Add(new DBComprobantes(entidadComercial, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetDateTimeSafe("cm_fpago"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido"))))); //Waste of persformance but helps with making the code less propense to error.
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
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.tc_id = {db_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cm_ec_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cm_em_id = {cuenta.GetID()} AND cm_ec_id = {ec_id} AND cm_id = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    DBTipoEntidad newTipoEntidadComercial = new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre"));
                    DBEntidades newEntidad = new DBEntidades(cuenta, newTipoEntidadComercial, new EntidadesComercialesData(reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetInt64Safe("ec_dni"), reader.GetStringSafe("ec_rs"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular")));

                    returnEnt = new DBComprobantes(newEntidad, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetDateTimeSafe("cm_fpago"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido")))); //Waste of persformance but helps with making the code less propense to error.
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
                //SELECT * FROM ent_comerciales JOIN tipos_entidades ON tipos_entidades.te_id = ent_comerciales.ec_te_id WHERE ec_em_id = 1;
                string te_table = DBTipoEntidad.GetDBTableName();
                string ec_table = DBEntidades.GetDBTableName();
                string tc_table = DBTiposComprobantes.GetDBTableName();

                string query = $@"SELECT * FROM {db_table} 
                JOIN {tc_table} ON {tc_table}.tc_id = {db_table}.cm_tc_id 
                JOIN {ec_table} ON {ec_table}.ec_id = {db_table}.cm_ec_id 
                JOIN {te_table} ON {te_table}.te_id = {ec_table}.ec_te_id 
                WHERE cm_em_id = {entidadComercial.GetCuentaID()} AND cm_ec_id = {entidadComercial.GetID()} AND cm_id = {id}";

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DBTiposComprobantes newTipoComprobante = new DBTiposComprobantes(reader.GetInt64Safe("tc_id"), reader.GetStringSafe("tc_nombre"));
                    returnEnt = new DBComprobantes(entidadComercial, newTipoComprobante, new ComprobantesData(reader.GetInt64Safe("cm_id"), reader.GetDateTimeSafe("cm_fecha"), reader.GetDateTimeSafe("cm_fpago"), reader.GetStringSafe("cm_numero"), reader.GetDoubleSafe("cm_gravado"), reader.GetDoubleSafe("cm_iva"), reader.GetDoubleSafe("cm_no_gravado"), reader.GetDoubleSafe("cm_percepcion"), Convert.ToBoolean(reader.GetInt32("cm_emitido")))); //Waste of persformance but helps with making the code less propense to error.
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

        public DBComprobantes(DBEntidades entidadComercial, DBTiposComprobantes newTipo, ComprobantesData newData)
        {
            _entidadComercial = entidadComercial;
            _tipoComprobante = newTipo.Clone(); //is this a good idea at all, to clone?
            _data = newData;
        }

        public DBComprobantes(DBEntidades entidadComercial, long tc_id, ComprobantesData newData)
        {
            _entidadComercial = entidadComercial;
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _data = newData;
        }

        public DBComprobantes(DBEmpresa cuentaSeleccioanda, long ec_id, DBTiposComprobantes newTipo, ComprobantesData newData)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoComprobante = newTipo.Clone();
            _data = newData;
        }

        public DBComprobantes(DBEmpresa cuentaSeleccioanda, long ec_id, long tc_id, ComprobantesData newData)
        {
            _entidadComercial = cuentaSeleccioanda.GetEntidadByID(ec_id);
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _data = newData;
        }
        public DBComprobantes(DBEmpresa cuentaSeleccioanda, MySqlConnection conn, long ec_id, long tc_id, ComprobantesData newData)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoComprobante = DBTiposComprobantes.GetByID(tc_id);
            _data = newData;
        }

        public DBComprobantes(DBEmpresa cuentaSeleccioanda, MySqlConnection conn, long ec_id, DBTiposComprobantes newTipo, ComprobantesData newData)
        {
            _entidadComercial = DBEntidades.GetByID(conn, cuentaSeleccioanda, ec_id);
            _tipoComprobante = newTipo.Clone();
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
            double percepcion=0.0,
            DateTime? pago=null
        ) : this(
            entidadComercial,
            newTipo,
            new ComprobantesData(id, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            entidadComercial,
            newTipo,
            new ComprobantesData(-1, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            entidadComercial,
            tc_id,
            new ComprobantesData(id, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            entidadComercial,
            tc_id,
            new ComprobantesData(-1, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            cuentaSeleccioanda,
            ec_id,
            newTipo,
            new ComprobantesData(id, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            cuentaSeleccioanda,
            ec_id,
            newTipo,
            new ComprobantesData(-1, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            cuentaSeleccioanda,
            conn,
            ec_id,
            newTipo,
            new ComprobantesData(id, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
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
            double percepcion = 0.0,
            DateTime? pago = null
        ) : this(
            cuentaSeleccioanda,
            conn,
            ec_id,
            newTipo,
            new ComprobantesData(-1, fecha, pago, numero, gravado, iva, no_gravado, percepcion, emitido)
        )
        { }

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

        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE cm_em_id = {_entidadComercial.GetCuentaID()} AND cm_ec_id = {_entidadComercial.GetID()} AND cm_id = {_data.cm_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //sql date format 2004-01-22 yyyy-mm-dd use DateTime.Now.ToString("yyyy-mm-dddd"). To convert it from string to DateTime use DateTime.ParseExact(string, "yyyy-mm-dddd", null)
                string fechaEmitido = (_data.cm_fecha.HasValue) ? $"'{((DateTime)_data.cm_fecha).ToString("yyyy-MM-dd")}'" : "NULL";
                string fechaPago = (_data.cm_fpago.HasValue) ? $"'{((DateTime)_data.cm_fpago).ToString("yyyy-MM-dd")}'" : "NULL";

                Console.WriteLine(fechaEmitido);
                //if exists already, just update
                if (recordsCount > 0)
                {
                    query = $"UPDATE {db_table} SET cm_tc_id = {_tipoComprobante.GetID()}, cm_fecha = {fechaEmitido}, cm_fpago = {fechaPago}, cm_numero = '{_data.cm_numero}', cm_gravado={_data.cm_gravado.ToString().Replace(",", ".")}, cm_iva = {_data.cm_iva.ToString().Replace(",", ".")}, cm_no_gravado={_data.cm_no_gravado.ToString().Replace(",", ".")}, cm_percepcion={_data.cm_percepcion.ToString().Replace(",", ".")}, cm_emitido ={Convert.ToInt32(_data.cm_emitido)}  WHERE cm_em_id = {_entidadComercial.GetCuentaID()} AND cm_ec_id = {_entidadComercial.GetID()} AND cm_id = {_data.cm_id}";
                   //query = $"UPDATE {db_table} SET cm_tc_id = {_tipoComprobante.GetID()}, cm_fecha = {fechaEmitido}, cm_fpago = {fechaPago}, cm_numero = '{_data.cm_numero}', cm_gravado={_data.cm_gravado} WHERE cm_em_id = {_entidadComercial.GetCuentaID()} AND cm_ec_id = {_entidadComercial.GetID()} AND cm_id = {_data.cm_id}";

                }
                else //if does not exists, insert into
                {
                    query = $"INSERT INTO {db_table} (cm_em_id, cm_ec_id, cm_tc_id, cm_fecha, cm_fpago, cm_numero, cm_gravado, cm_iva, cm_no_gravado, cm_percepcion, cm_emitido) VALUES ({_entidadComercial.GetCuentaID()}, {_entidadComercial.GetID()}, {_tipoComprobante.GetID()}, {fechaEmitido}, {fechaPago}, '{_data.cm_numero}', {_data.cm_gravado.ToString().Replace(",", ".")}, {_data.cm_iva.ToString().Replace(",", ".")}, {_data.cm_no_gravado.ToString().Replace(",", ".")}, {_data.cm_percepcion.ToString().Replace(",", ".")}, {Convert.ToInt32(_data.cm_emitido)})";
                }
                //if not, add
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (recordsCount <= 0) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new ComprobantesData(cmd.LastInsertedId, _data.cm_fecha, _data.cm_fpago, _data.cm_numero, _data.cm_gravado, _data.cm_iva, _data.cm_no_gravado, _data.cm_percepcion, _data.cm_emitido);
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
                string query = $"DELETE FROM {db_table}  WHERE cm_em_id = {_entidadComercial.GetCuentaID()} AND cm_ec_id = {_entidadComercial.GetID()} AND cm_id = {_data.cm_id}";
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

        public void ResetID()
        {
            _data = new ComprobantesData(-1, _data.cm_fecha, _data.cm_fpago, _data.cm_numero, _data.cm_gravado, _data.cm_iva, _data.cm_no_gravado, _data.cm_percepcion, _data.cm_emitido);
        }
        public long GetID() => _data.cm_id;
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
        public DateTime? GetFechaPago() => _data.cm_fpago;

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
        public void SetFechaPago(DateTime? newFechaPago) => _data.cm_fpago = newFechaPago;
        public void SetGravado(double gravado) => _data.cm_gravado = gravado;
        public void SetIVA(double IVA) => _data.cm_iva = IVA;
        public void SetNoGravado(double no_gravado) => _data.cm_no_gravado = no_gravado;
        public void SetPercepcion(double percepcion) => _data.cm_percepcion = percepcion;
        public void SetEmitido(bool esEmitido) => _data.cm_emitido = esEmitido;
    }
}
