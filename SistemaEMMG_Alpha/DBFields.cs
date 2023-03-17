using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public struct TiposEntidadesData
    {
        public TiposEntidadesData(long id, string nom)
        {
            te_id = id;
            te_nombre = nom; //Event handler to check that nom is not longer than 32 characters
        }
        public long te_id { get; set; }
        public string te_nombre { get; set; }
    }

    public struct BancosData
    {
        public BancosData (long id, string nom)
        {
            bc_id = id;
            bc_nombre = nom;
        }
        public long bc_id { get; set; }
        public string bc_nombre { get; set; }
    }
    public struct TiposComprobantesData
    {
        public TiposComprobantesData(long id, string nom)
        {
            tc_id = id;
            tc_nombre = nom;
        }
        public long tc_id { get; set; }
        public string tc_nombre { get; set; }
    }
    public struct FormasPagoData
    {
        public FormasPagoData(long id, string nom)
        {
            fp_id = id;
            fp_nombre = nom;
        }
        public long fp_id { get; set; }
        public string fp_nombre { get; set; }
    }
    public struct EntidadesComercialesData
    {
        public EntidadesComercialesData(long id, long cuit, long dni, string rs, string email, string tel, string cel)
        {
            ec_id = id;
            ec_cuit = cuit;
            ec_dni = dni;
            ec_rs = rs;
            ec_email = email;
            ec_telefono = tel;
            ec_celular = cel;
        }
        //Class handles ec_em_id y ec_te_id since it is better to hold references to those dataTypes instead of values
        public long ec_id { get; set; }
        public long ec_cuit { get; set; }
        public long ec_dni { get; set; }
        public string ec_rs { get; set; }
        public string ec_email { get; set; }
        public string ec_telefono {get; set; }
        public string ec_celular { get; set; }
    }

    public struct ComprobantesData
    {
        public ComprobantesData(long id, DateTime fecha, DateTime pago, string numero, double gravado, double iva, double no_gravado, double percepcion, bool emitido)
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
        public DateTime cm_fecha { get; set; }
        public DateTime cm_fpago { get; set; }
        public string cm_numero { get; set; }
        public double cm_gravado { get; set; }
        public double cm_iva { get; set; }
        public double cm_no_gravado { get; set; }
        public double cm_percepcion { get; set; }
        public bool cm_emitido { get; set; }
    }
    public class DBFields
    {
        private DBFields()
        {

        }

        public static DBFields Instance()
        {
            if (_instance == null)
            {
                _instance = new DBFields();
            }
            return _instance;
        }
        private static DBFields _instance = null;
        public long idCuentaSeleccionada = 0;
        public List<DBEmpresa> empresas;

        public void ReadEmpresasFromDB(MySqlConnection conn)
        {
            if (!(empresas is null))
            {
                empresas.Clear();
            }
            empresas = DBEmpresa.GetEmpresasFromDataBase(conn);
        }
    }
}
