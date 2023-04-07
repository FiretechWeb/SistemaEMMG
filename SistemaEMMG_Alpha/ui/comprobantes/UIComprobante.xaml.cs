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
    /// Interaction logic for UIComprobante.xaml
    /// </summary>
    public partial class UIComprobante : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIAgregarModificarComprobante _ownerControl = null;
        private DBComprobantes _comprobanteSeleccionado = null;
        private DBComprobantes _fatherComprobanteSelected = null;

        public void SetUIOwner(UIAgregarModificarComprobante ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIAgregarModificarComprobante GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        private void SetComprobanteAsociadoSeleccionado(DBComprobantes newSeleccion)
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

        public UIComprobante()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        public void RefreshData(DBComprobantes parentComprobante)
        {
            _fatherComprobanteSelected = parentComprobante;
            if (_fatherComprobanteSelected is null)
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

            if (_fatherComprobanteSelected.IsLocal())
            {
                comprobantesAsociados = _fatherComprobanteSelected.GetAllComprobantesAsociados();
            }
            else
            {
                comprobantesAsociados = _fatherComprobanteSelected.GetAllComprobantesAsociados(dbCon.Connection);
            }
            double importeTotal = 0.0;
            foreach (DBComprobantes comprobante in comprobantesAsociados)
            {
                listComprobantesAsociados.Items.Add(new KeyValuePair<long, string>(comprobante.GetID(), $"{comprobante.GetNumeroComprobante()}: {comprobante.GetTotal_MonedaLocal()} ARS"));
                importeTotal += comprobante.GetTotal_MonedaLocal();
            }
            listComprobantesSimilares.SelectedIndex = -1;
            listComprobantesAsociados.SelectedIndex = -1;
            SetComprobanteAsociadoSeleccionado(null);
        }

        private void listComprobantesAsociados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_fatherComprobanteSelected is null)
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
            _comprobanteSeleccionado = DBComprobantes.GetByID(dbCon.Connection, _fatherComprobanteSelected.GetEntidadComercial(), ((KeyValuePair<long, string>)listComprobantesAsociados.SelectedItem).Key);
            SetComprobanteAsociadoSeleccionado(_comprobanteSeleccionado);
        }

        private void btnAsociar_Click(object sender, RoutedEventArgs e)
        {
            if (_fatherComprobanteSelected is null)
            {
                return;
            }
            DBComprobantes newComprobante = DBComprobantes.GetByNumber(dbCon.Connection, _fatherComprobanteSelected.GetEntidadComercial(), txtNumeroComprobante.Text.Trim(), _fatherComprobanteSelected.IsEmitido(), 1);

            if (newComprobante is null)
            {
                List<DBComprobantes> comprobantesSimilares = DBComprobantes.SearchByNumber(dbCon.Connection, _fatherComprobanteSelected.GetEntidadComercial(), txtNumeroComprobante.Text.Trim(), _fatherComprobanteSelected.IsEmitido(), 1);
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
                newComprobante.AsociarAComprobante(_fatherComprobanteSelected);
                if (!_fatherComprobanteSelected.IsLocal())
                {
                    newComprobante.PushToDatabase(dbCon.Connection);
                }
                RefreshData(_fatherComprobanteSelected);
            }
        }

        private void btnQuitar_Click(object sender, RoutedEventArgs e)
        {
            if (_fatherComprobanteSelected is null)
            {
                return;
            }
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            _comprobanteSeleccionado.AsociarAComprobante(null);
            if (!_comprobanteSeleccionado.IsLocal())
            {
                _comprobanteSeleccionado.PushToDatabase(dbCon.Connection);
            }
            RefreshData(_fatherComprobanteSelected);
        }

        private void listComprobantesSimilares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_fatherComprobanteSelected is null)
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

            DBComprobantes comprobanteSimilarSelected = DBComprobantes.GetByID(dbCon.Connection, _fatherComprobanteSelected.GetEntidadComercial(), ((KeyValuePair<long, string>)listComprobantesSimilares.SelectedItem).Key);

            if (comprobanteSimilarSelected is null)
            {
                return;
            }
            txtNumeroComprobante.Text = comprobanteSimilarSelected.GetNumeroComprobante();
        }
    }
}
