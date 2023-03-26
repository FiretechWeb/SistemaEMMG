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
using System.Diagnostics;

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
            _comprobanteSeleccionado = selectedComprobante;

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

            listEntidadesComerciales.Items.Clear();
            listSelectedEntidadComercial.Items.Clear();
            listSelectedEntidadComercial.SelectedValuePath = "Key";
            listSelectedEntidadComercial.DisplayMemberPath = "Value";

            if (_comprobanteSeleccionado is null)
            {
                _comprobanteSeleccionado = new DBComprobantes(entidadesComerciales[0], listTiposComprobantes[0], listMonedas[0], true, null, "-1", 0.0, 0.0); //Created to handle pagos/remitos relations.
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
                cmbMoneda.SelectedIndex = 0;
                RefreshMonedaSelected();
                cmbTipoComprobante.SelectedIndex = 0;
            } else
            {
                listSelectedEntidadComercial.Items.Add(
                    new KeyValuePair<long, string>(_comprobanteSeleccionado.GetEntidadComercialID(),
                    $"{_comprobanteSeleccionado.GetEntidadComercial().GetCUIT()}: {_comprobanteSeleccionado.GetEntidadComercial().GetRazonSocial()}")
                    );

                rdbRecibido.IsChecked = !_comprobanteSeleccionado.IsEmitido();
                rdbEmitido.IsChecked = _comprobanteSeleccionado.IsEmitido();
                txNumeroComprobante.Text = _comprobanteSeleccionado.GetNumeroComprobante();
                listEntidadesComerciales.Items.Clear();
                txtFiltroBusquedaEntidad.Text = "";
                txtFechaEmitido.Text = _comprobanteSeleccionado.GetFechaEmitido().HasValue ? ((DateTime)_comprobanteSeleccionado.GetFechaEmitido()).ToString("dd/MM/yyyy") : "";
                txtGravado.Text = SafeConvert.ToString(_comprobanteSeleccionado.GetGravado());
                txtNoGravado.Text = SafeConvert.ToString(_comprobanteSeleccionado.GetNoGravado());
                txtIVA.Text = SafeConvert.ToString(_comprobanteSeleccionado.GetIVA());
                txtPercepcion.Text = SafeConvert.ToString(_comprobanteSeleccionado.GetPercepcion());
                txtObservacion.Text = _comprobanteSeleccionado.GetObservacion();
                cmbMoneda.SelectedValue = _comprobanteSeleccionado.GetMoneda().GetID();
                RefreshMonedaSelected();
                cmbTipoComprobante.SelectedValue = _comprobanteSeleccionado.GetTipoComprobante().GetID();
                txtCambio.Text = SafeConvert.ToString(_comprobanteSeleccionado.GetCambio());
            }

            listSelectedEntidadComercial.SelectedIndex = 0;
        }


        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(!(_ownerControl is null));

            long old_cm_ec_id = _comprobanteSeleccionado.GetEntidadComercialID();

            DateTime fechaEmitido = new DateTime();
            DateTime.TryParse(txtFechaEmitido.Text, out fechaEmitido);

            _comprobanteSeleccionado.SetFechaEmitido(fechaEmitido);
            _comprobanteSeleccionado.SetEmitido(SafeConvert.ToBoolean(rdbEmitido.IsChecked));
            _comprobanteSeleccionado.SetEntidadComercial(((KeyValuePair<long, string>)listSelectedEntidadComercial.SelectedItem).Key, dbCon.Connection);
            _comprobanteSeleccionado.SetTipoComprobante(((KeyValuePair<long, string>)cmbTipoComprobante.SelectedItem).Key);
            _comprobanteSeleccionado.SetMoneda(DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key));
            _comprobanteSeleccionado.SetNumeroComprobante(txNumeroComprobante.Text);
            _comprobanteSeleccionado.SetGravado(SafeConvert.ToDouble(txtGravado.Text.Replace(".", ",")));
            _comprobanteSeleccionado.SetIVA(SafeConvert.ToDouble(txtIVA.Text.Replace(".", ",")));
            _comprobanteSeleccionado.SetNoGravado(SafeConvert.ToDouble(txtNoGravado.Text.Replace(".", ",")));
            _comprobanteSeleccionado.SetPercepcion(SafeConvert.ToDouble(txtPercepcion.Text.Replace(".", ",")));
            _comprobanteSeleccionado.SetCambio(SafeConvert.ToDouble(txtCambio.Text.Replace(".", ",")));
            _comprobanteSeleccionado.SetObservacion(txtObservacion.Text);

            if (_comprobanteSeleccionado.PushToDatabase(dbCon.Connection, old_cm_ec_id))
            {
                MessageBox.Show("Comprobante agregado / modificado a la base de datos correctamente!");
                _ownerControl.RefreshData();
                Visibility = Visibility.Collapsed;
            } else
            {
                MessageBox.Show("Error al tratar de agregar / modificar este comprobante. ¿Ya existe un comprobante con el mismo número?.");
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void btnBuscarEntidad_Click(object sender, RoutedEventArgs e)
        {
            List<DBEntidades> entidadesComercialesList = DBEntidades.Search(dbCon.Connection, GetCuentaSeleccionada(), txtFiltroBusquedaEntidad.Text);

            listEntidadesComerciales.Items.Clear();
            listEntidadesComerciales.SelectedValuePath = "Key";
            listEntidadesComerciales.DisplayMemberPath = "Value";

            foreach (DBEntidades entidad in entidadesComercialesList)
            {
                listEntidadesComerciales.Items.Add(new KeyValuePair<long, string>(entidad.GetID(), $"{entidad.GetCUIT()}: {entidad.GetRazonSocial()}"));
            }
        }

        private void listEntidadesComerciales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listEntidadesComerciales.SelectedItem is null)
            {
                return;
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            long listECSelectedID = ((KeyValuePair<long, string>)listEntidadesComerciales.SelectedItem).Key;
            if (listECSelectedID < 0)
            {
                return;
            }
            DBEntidades selectedEntidad = DBEntidades.GetByID(dbCon.Connection, GetCuentaSeleccionada(), (int)listECSelectedID);
            if (selectedEntidad is null)
            {
                return;
            }
            listSelectedEntidadComercial.Items.Clear();
            listSelectedEntidadComercial.SelectedValuePath = "Key";
            listSelectedEntidadComercial.DisplayMemberPath = "Value";
            listSelectedEntidadComercial.Items.Add(new KeyValuePair<long, string>(selectedEntidad.GetID(),
                $"{selectedEntidad.GetCUIT()}: {selectedEntidad.GetRazonSocial()}"));
            listSelectedEntidadComercial.SelectedIndex = 0;
        }
    }
}
