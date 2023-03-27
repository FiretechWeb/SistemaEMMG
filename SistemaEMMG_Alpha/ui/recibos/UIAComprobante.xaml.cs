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

namespace SistemaEMMG_Alpha.ui.recibos
{
    /// <summary>
    /// Interaction logic for UIAComprobante.xaml
    /// </summary>
    public partial class UIAComprobante : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIAgregarModificarRecibo _ownerControl = null;
        private DBComprobantes _comprobanteSeleccionado = null;
        private DBRecibo _reciboSelected = null;

        public void SetUIOwner(UIAgregarModificarRecibo ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIAgregarModificarRecibo GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        private void SetComprobanteSeleccionado(DBComprobantes newSeleccion)
        {
            _comprobanteSeleccionado = newSeleccion;
            if (_comprobanteSeleccionado is null)
            {
                lblComprobanteSeleccionado.Content = "Ninguno.";
            }
            else
            {
                lblComprobanteSeleccionado.Content = _comprobanteSeleccionado.GetNumeroComprobante();
            }
        }

        public UIAComprobante()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }


        public void RefreshData(DBRecibo selectedRecibo)
        {
            _reciboSelected = selectedRecibo;
            if (_reciboSelected is null)
            {
                return;
            }

            listComprobantesAsociados.Items.Clear();
            listComprobantesAsociados.SelectedValuePath = "Key";
            listComprobantesAsociados.DisplayMemberPath = "Value";

            listComprobantesSimilares.Items.Clear();
            listComprobantesSimilares.SelectedValuePath = "Key";
            listComprobantesSimilares.DisplayMemberPath = "Value";

            List<DBComprobantes> comprobantesAsociados;

            if (_reciboSelected.IsLocal())
            {
                comprobantesAsociados = _reciboSelected.GetAllComprobantes();
            }
            else
            {
                comprobantesAsociados = _reciboSelected.GetAllComprobantes(dbCon.Connection);
            }
            double importeTotal = 0.0;
            foreach (DBComprobantes comprobante in comprobantesAsociados)
            {
                listComprobantesAsociados.Items.Add(new KeyValuePair<long, string>(comprobante.GetID(), $"{comprobante.GetNumeroComprobante()}: {comprobante.GetTotal_MonedaLocal()} ARS"));
                importeTotal += comprobante.GetTotal_MonedaLocal();
            }
            listComprobantesSimilares.SelectedIndex = -1;
            listComprobantesAsociados.SelectedIndex = -1;
            lblComprobantesImporte.Content = $"{importeTotal.ToString("0.00")} ARS";
            SetComprobanteSeleccionado(null);
        }

        private void listComprobantesAsociados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            if (listComprobantesAsociados.SelectedIndex == -1)
            {
                return;
            }
            if (listComprobantesAsociados.SelectedItem is null)
            {
                return;
            }
            _comprobanteSeleccionado = DBComprobantes.GetByID(dbCon.Connection, _reciboSelected.GetEntidadComercial(), ((KeyValuePair<long, string>)listComprobantesAsociados.SelectedItem).Key);
        }

        private void btnAsociar_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            DBComprobantes newComprobante = DBComprobantes.GetByNumber(dbCon.Connection, _reciboSelected.GetEntidadComercial(), txtNumeroComprobante.Text.Trim(), _reciboSelected.IsEmitido());

            if (newComprobante is null)
            {
                List<DBComprobantes> comprobantesSimilares = DBComprobantes.SearchByNumber(dbCon.Connection, _reciboSelected.GetEntidadComercial(), txtNumeroComprobante.Text.Trim(), _reciboSelected.IsEmitido());
                listComprobantesSimilares.Items.Clear();
                listComprobantesSimilares.SelectedValuePath = "Key";
                listComprobantesSimilares.DisplayMemberPath = "Value";
                foreach (DBComprobantes comprobante in comprobantesSimilares)
                {
                    listComprobantesSimilares.Items.Add(new KeyValuePair<long, string>(comprobante.GetID(), $"{comprobante.GetNumeroComprobante()}: {comprobante.GetTotal_MonedaLocal()} ARS"));
                }
            }
            else
            {
                _reciboSelected.AddComprobante(newComprobante);
                if (!_reciboSelected.IsLocal())
                {
                    _reciboSelected.PushRelationshipComprobanteDB(dbCon.Connection, newComprobante);
                }
                RefreshData(_reciboSelected);
            }
        }

        private void btnQuitar_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            _reciboSelected.RemoveComprobante(_comprobanteSeleccionado);

            if (!_reciboSelected.IsLocal())
            {
                _reciboSelected.RemoveRelationshipComprobanteDB(dbCon.Connection, _comprobanteSeleccionado);
            }
            RefreshData(_reciboSelected);
        }

        private void listComprobantesSimilares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            if (listComprobantesSimilares.SelectedIndex == -1)
            {
                return;
            }
            if (listComprobantesSimilares.SelectedItem is null)
            {
                return;
            }

            DBComprobantes comprobanteSimilarSelected = DBComprobantes.GetByID(dbCon.Connection, _reciboSelected.GetEntidadComercial(), ((KeyValuePair<long, string>)listComprobantesSimilares.SelectedItem).Key);

            if (comprobanteSimilarSelected is null)
            {
                return;
            }
            txtNumeroComprobante.Text = comprobanteSimilarSelected.GetNumeroComprobante();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
