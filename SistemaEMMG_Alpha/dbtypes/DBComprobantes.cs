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
    public class DBComprobantes : DBInterface
    {
        private static readonly string db_table = "comprobantes";
        ///<summary>
        ///Commercial entity associated with this business receipt.
        ///</summary>
        private DBEntidades _entidadComercial; //this can change actually...
        private ComprobantesData _data;
        private DBTiposComprobantes _tipoComprobante = null;

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
            return false;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            return false;
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

        public void SetEntidadComercial(long ec_id, MySqlConnection conn) => DBEntidades.GetByID(conn, GetCuenta(), ec_id);

        public void SetTipoComprobante(DBTiposComprobantes newType) => _tipoComprobante = newType.Clone();

        public void SetTipoComprobante(long tc_id) => DBTiposComprobantes.GetByID(tc_id);

        public void SetTipoComprobante(long tc_id, MySqlConnection conn) => DBTiposComprobantes.GetByID(tc_id, conn);
    }
}
