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
    /// Interaction logic for UICRemito.xaml
    /// </summary>
    public partial class UICRemito : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIAgregarModificarComprobante _ownerControl = null;
        private DBComprobantes _comprobanteSeleccionado = null;
        private DBRemito _remitoSelected = null;

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

        private void SetRemitoSeleccionado(DBRemito newSeleccion)
        {
            _remitoSelected = newSeleccion;
            if (_remitoSelected is null)
            {
                lblRemitoSeleccionado.Content = "Ninguno.";
            }
            else
            {
                lblRemitoSeleccionado.Content = _remitoSelected.GetNumero();
            }
        }

        public UICRemito()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        public void RefreshData(DBComprobantes selectedComprobante)
        {
            _comprobanteSeleccionado = selectedComprobante;
            if (_comprobanteSeleccionado is null)
            {
                return;
            }

            listRemitosAsociados.Items.Clear();
            listRemitosAsociados.SelectedValuePath = "Key";
            listRemitosAsociados.DisplayMemberPath = "Value";

            listRemitosSimilares.Items.Clear();
            listRemitosSimilares.SelectedValuePath = "Key";
            listRemitosSimilares.DisplayMemberPath = "Value";

            List<DBRemito> remitosAsociados = selectedComprobante.GetAllRemitos(dbCon.Connection);

            foreach (DBRemito remito in remitosAsociados)
            {
                listRemitosAsociados.Items.Add(new KeyValuePair<long, string>(remito.GetID(), remito.GetNumero()));
            }
            listRemitosSimilares.SelectedIndex = -1;
            listRemitosSimilares.SelectedIndex = -1;
            SetRemitoSeleccionado(null);
        }

        private void btnAsociar_Click(object sender, RoutedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            DBRemito newRemito = DBRemito.GetByNumber(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), txtNumeroRemito.Text.Trim(), _comprobanteSeleccionado.IsEmitido());

            if (newRemito is null)
            {
                List<DBRemito> remitosSimilares = DBRemito.SearchByNumber(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), txtNumeroRemito.Text.Trim(), _comprobanteSeleccionado.IsEmitido());
                listRemitosSimilares.Items.Clear();
                listRemitosSimilares.SelectedValuePath = "Key";
                listRemitosSimilares.DisplayMemberPath = "Value";
                foreach (DBRemito remito in remitosSimilares)
                {
                    listRemitosSimilares.Items.Add(new KeyValuePair<long, string>(remito.GetID(), remito.GetNumero()));
                }
            }
            else
            {
                _comprobanteSeleccionado.AddRemito(newRemito);
                if (!_comprobanteSeleccionado.IsLocal())
                {
                    _comprobanteSeleccionado.PushRelationshipRemitoDB(dbCon.Connection, newRemito);
                }
                RefreshData(_comprobanteSeleccionado);
            }
        }

        private void btnQuitar_Click(object sender, RoutedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            if (_remitoSelected is null)
            {
                return;
            }
            _comprobanteSeleccionado.RemoveRemito(_remitoSelected);
            if (!_comprobanteSeleccionado.IsLocal())
            {
                _comprobanteSeleccionado.RemoveRelationshipRemitoDB(dbCon.Connection, _remitoSelected);
            }
            RefreshData(_comprobanteSeleccionado);
        }

        private void listRemitosAsociados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            if (listRemitosAsociados.SelectedIndex == -1)
            {
                return;
            }
            if (listRemitosAsociados.SelectedItem is null)
            {
                return;
            }
            _remitoSelected = DBRemito.GetByID(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), ((KeyValuePair<long, string>)listRemitosAsociados.SelectedItem).Key);
        }

        private void listRemitosSimilares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            if (listRemitosSimilares.SelectedIndex == -1)
            {
                return;
            }
            if (listRemitosSimilares.SelectedItem is null)
            {
                return;
            }

            DBRemito similarRemitoSelected = DBRemito.GetByID(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), ((KeyValuePair<long, string>)listRemitosSimilares.SelectedItem).Key);

            if (similarRemitoSelected is null)
            {
                return;
            }
            txtNumeroRemito.Text = similarRemitoSelected.GetNumero();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
