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
        public List<DBTipoEntidad> tipos_entidades;
        public List<DBTiposComprobantes> tipos_comprobantes;
        public List<DBFormasPago> formas_pago;
        private DBComprobantes _comprobanteSeleccionado = null;
        private DBEntidades _entidadSeleccionada = null;
        private DBComprobantePago _pagoSelected = null;

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

        public void ReadEmpresasFromDB(MySqlConnection conn)
        {
            if (!(empresas is null))
            {
                empresas.Clear();
            }
            empresas = DBEmpresa.UpdateAll(conn);
        }
        public void ReadEntidadesComercialesFromDB(MySqlConnection conn)
        {
            ReadTiposEntidadesFromDB(conn); //We need to make sure we have the tipos_entidades for the Checkbox and stuff...
            DBEmpresa empresaSeleccionada = GetCurrentAccount();
            empresaSeleccionada.GetAllEntidadesComerciales(conn);
        }
        public void ReadComprobantesFromDB(MySqlConnection conn)
        {
            ReadTiposComprobantesFromDB(conn);
            ReadFormasDePago(conn);
            DBEmpresa empresaSeleccionada = GetCurrentAccount();
            empresaSeleccionada.GetAllComprobantes(conn);
        }
        public DBEmpresa GetCurrentAccount() {
            int index = GetCuentaIndexByID(idCuentaSeleccionada);
            if (index == -1)
                return null;
            return empresas[index];
        }

        public int GetCuentaIndexByID(long cuentaID)
        {
            for (int i=0; i < empresas.Count; i++)
            {
                if (empresas[i].GetID() == cuentaID)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool EliminarCuentaDeEmpresa(int index, MySqlConnection conn)
        {
            if (index < 0 || index >= empresas.Count)
            {
                return false;
            }
            if (empresas[index].DeleteFromDatabase(conn))
            {
                empresas.RemoveAt(index);
            }

            return true;

        }

        public bool AgregarNuevaCuentaDeEmpresa(string nombreCuenta, long cuitCuenta, MySqlConnection conn)
        {
            if (DBEmpresa.EmpresaYaExiste(nombreCuenta, cuitCuenta, empresas))
            {
                MessageBox.Show("¡La cuenta de empresa que quiso crear ya existe!, el CUIT y la razón social deben ser únicas.");
                return false;
            }
            DBEmpresa newEmpresa = new DBEmpresa(cuitCuenta, nombreCuenta);

            if (!newEmpresa.PushToDatabase(conn))
            {
                return false;
            }
            empresas.Add(newEmpresa);

            return true;
        }

        public void SetPagoSelected(DBComprobantePago pagoSelected) => _pagoSelected = pagoSelected;
        public DBComprobantePago GetPagoSelected() => _pagoSelected;
        public void DeselectPago() => _pagoSelected = null;
        public void SetComprobanteSelected(DBComprobantes comprobanteSelected) => _comprobanteSeleccionado = comprobanteSelected;

        public DBComprobantes GetComprobanteSelected() => _comprobanteSeleccionado;

        public void DeselectComprobante() => _comprobanteSeleccionado = null;

        public void SetEntidadComercialSelected(DBEntidades entidadComercialSelected) => _entidadSeleccionada = entidadComercialSelected;

        public DBEntidades GetEntidadComercialSelected() => _entidadSeleccionada;

        public void DeselectEntidadComercial() => _entidadSeleccionada = null;
    }
}
