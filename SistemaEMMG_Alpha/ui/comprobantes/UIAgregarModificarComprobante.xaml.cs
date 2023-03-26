using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SistemaEMMG_Alpha.ui.comprobantes
{
    /// <summary>
    /// Interaction logic for UIAgregarModificarComprobante.xaml
    /// </summary>
    public partial class UIAgregarModificarComprobante : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIComprobantes _ownerControl = null;
        private DBComprobantes _comprobanteSeleccionado = null;

        public void SetUIOwner(UIComprobantes ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIComprobantes GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        public UIAgregarModificarComprobante()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }
        private void RefreshMonedaSelected()
        {
            if (cmbMoneda.SelectedIndex == -1 || (cmbMoneda.SelectedItem is null))
            {
                gridCambio.Visibility = Visibility.Collapsed;
                txtCambio.Text = "";
                return;
            }
            DBMoneda monedaSeleccionada = DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key);
            if ((monedaSeleccionada is null) || !monedaSeleccionada.IsExtranjera())
            {
                gridCambio.Visibility = Visibility.Collapsed;
                txtCambio.Text = "";
            }
            else
            {
                gridCambio.Visibility = Visibility.Visible;
                txtCambio.Text = "";
            }
        }
        public void RefreshData(DBComprobantes selectedComprobante = null)
        {
            if (_ownerControl is null)
            {
                return;
            }
            if (_ownerControl.GetMainWindow() is null)
            {
                return;
            }
            if (dbData is null)
            {
                dbData = DBMain.Instance();
            }
            if (dbCon is null)
            {
                dbCon = DBConnection.Instance();
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            if (!_ownerControl.GetMainWindow().CheckDBConnection())
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);

            List<DBEntidades> entidadesComerciales = DBEntidades.GetAll(dbCon.Connection, GetCuentaSeleccionada());
            List<DBTiposComprobantes> listTiposComprobantes = DBTiposComprobantes.GetAll();
            List<DBMoneda> listMonedas = DBMoneda.GetAll();

            if (entidadesComerciales.Count <= 0 || listTiposComprobantes.Count <= 0 || listMonedas.Count <= 0)
            {
                return;
            }

            cmbTipoComprobante.Items.Clear();
            cmbTipoComprobante.SelectedValuePath = "Key";
            cmbTipoComprobante.DisplayMemberPath = "Value";

            foreach (DBTiposComprobantes tipoComprobante in listTiposComprobantes)
            {
                cmbTipoComprobante.Items.Add(new KeyValuePair<long, string>(tipoComprobante.GetID(), tipoComprobante.GetName()));
            }
            cmbMoneda.Items.Clear();
            cmbMoneda.SelectedValuePath = "Key";
            cmbMoneda.DisplayMemberPath = "Value";

            foreach (DBMoneda moneda in listMonedas)
            {
                cmbMoneda.Items.Add(new KeyValuePair<long, string>(moneda.GetID(), moneda.GetName()));
            }




            _comprobanteSeleccionado = selectedComprobante;
            listEntidadesComerciales.Items.Clear();

            if (_comprobanteSeleccionado is null)
            {
                _comprobanteSeleccionado = new DBComprobantes(entidadesComerciales[0], listTiposComprobantes[0], listMonedas[0], true, null, "-1", 0.0, 0.0); //Created to handle pagos/remitos relations.

                listSelectedEntidadComercial.Items.Clear();
                listSelectedEntidadComercial.SelectedValuePath = "Key";
                listSelectedEntidadComercial.DisplayMemberPath = "Value";
                listSelectedEntidadComercial.Items.Add(new KeyValuePair<long, string>(-1, "Seleccione una entidad comercial..."));
                rdbRecibido.IsChecked = false;
                rdbEmitido.IsChecked = true;
                txNumeroComprobante.Text = "";
                listEntidadesComerciales.Items.Clear();
                txtFiltroBusquedaEntidad.Text = "";
                txtFechaEmitido.Text = "";
                txtGravado.Text = "";
                txtNoGravado.Text = "";
                txtIVA.Text = "";
                txtPercepcion.Text = "";
                txtObservacion.Text = "";
                if (listMonedas.Count > 0)
                {
                    cmbMoneda.SelectedIndex = 0;
                }
                RefreshMonedaSelected();

                if (listTiposComprobantes.Count > 0)
                {
                    cmbTipoComprobante.SelectedIndex = 0;
                }

            } else
            {

            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
