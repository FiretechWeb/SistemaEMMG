using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

/********************
* To remove after I am done with DBMain, or to change it into something like DBInterface or something like that..
**********************/

namespace SistemaEMMG_Alpha
{
    public class DBOldMain
    {
        private DBOldMain()
        {

        }

        public static DBOldMain Instance()
        {
            if (_instance == null)
            {
                _instance = new DBOldMain();
            }
            return _instance;
        }
        private static DBOldMain _instance = null;
        public long idCuentaSeleccionada = 0;

        public List<DBCuenta> cuentas;
        public List<DBTipoEntidad> tipos_entidades;
        public List<DBTiposComprobantes> tipos_comprobantes;
        public List<DBFormasPago> formas_pago;
        private DBComprobantes _comprobanteSeleccionado = null;
        private DBEntidades _entidadSeleccionada = null;
        private DBPago _pagoSelected = null;

        public void ReadTiposEntidadesFromDB(MySqlConnection conn)
        {
            if (!(tipos_entidades is null))
            {
                tipos_entidades.Clear();
            }
            tipos_entidades = DBTipoEntidad.UpdateAll(conn);
        }

        public void ReadTiposComprobantesFromDB(MySqlConnection conn)
        {
            if (!(tipos_comprobantes is null))
            {
                tipos_comprobantes.Clear();
            }
            tipos_comprobantes = DBTiposComprobantes.UpdateAll(conn);
        }

        public void ReadFormasDePago(MySqlConnection conn)
        {
            if (!(formas_pago is null))
            {
                formas_pago.Clear();
            }
            formas_pago = DBFormasPago.UpdateAll(conn);
        }

        public void ReadCuentasFromDB(MySqlConnection conn)
        {
            if (!(cuentas is null))
            {
                cuentas.Clear();
            }
            cuentas = DBCuenta.UpdateAll(conn);
        }
        public void ReadEntidadesComercialesFromDB(MySqlConnection conn)
        {
            ReadTiposEntidadesFromDB(conn); //We need to make sure we have the tipos_entidades for the Checkbox and stuff...
            GetCurrentAccount().GetAllEntidadesComerciales(conn);
        }
        public void ReadComprobantesFromDB(MySqlConnection conn)
        {
            ReadTiposComprobantesFromDB(conn);
            ReadFormasDePago(conn);
            GetCurrentAccount().GetAllComprobantes(conn);
        }
        public DBCuenta GetCurrentAccount()
        {
            int index = GetCuentaIndexByID(idCuentaSeleccionada);
            if (index == -1)
                return null;
            return cuentas[index];
        }

        public int GetCuentaIndexByID(long cuentaID)
        {
            for (int i = 0; i < cuentas.Count; i++)
            {
                if (cuentas[i].GetID() == cuentaID)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool EliminarCuenta(int index, MySqlConnection conn)
        {
            if (index < 0 || index >= cuentas.Count)
            {
                return false;
            }
            if (cuentas[index].DeleteFromDatabase(conn))
            {
                cuentas.RemoveAt(index);
            }

            return true;

        }

        public bool AgregarNuevaCuenta(string nombreCuenta, long cuitCuenta, MySqlConnection conn)
        {
            if (DBCuenta.CuentaYaExiste(nombreCuenta, cuitCuenta, cuentas))
            {
                MessageBox.Show("¡La cuenta que quiso crear ya existe!, el CUIT y la razón social deben ser únicas.");
                return false;
            }
            DBCuenta newCuenta = new DBCuenta(cuitCuenta, nombreCuenta);

            if (!newCuenta.PushToDatabase(conn))
            {
                return false;
            }
            cuentas.Add(newCuenta);

            return true;
        }

        public void SetPagoSelected(DBPago pagoSelected) => _pagoSelected = pagoSelected;
        public DBPago GetPagoSelected() => _pagoSelected;
        public void DeselectPago() => _pagoSelected = null;
        public void SetComprobanteSelected(DBComprobantes comprobanteSelected) => _comprobanteSeleccionado = comprobanteSelected;

        public DBComprobantes GetComprobanteSelected() => _comprobanteSeleccionado;

        public void DeselectComprobante() => _comprobanteSeleccionado = null;

        public void SetEntidadComercialSelected(DBEntidades entidadComercialSelected) => _entidadSeleccionada = entidadComercialSelected;

        public DBEntidades GetEntidadComercialSelected() => _entidadSeleccionada;

        public void DeselectEntidadComercial() => _entidadSeleccionada = null;
    }
}
