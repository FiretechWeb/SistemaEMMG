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
    /// Interaction logic for UIRecibo.xaml
    /// </summary>
    public partial class UIRecibo : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIAgregarModificarComprobante _ownerControl = null;
        private DBComprobantes _comprobanteSeleccionado = null;
        private DBRecibo _reciboSelected = null;

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

        private void SetReciboSeleccionado(DBRecibo newSeleccion)
        {
            _reciboSelected = newSeleccion;
            if (_reciboSelected is null)
            {
                lblReciboSeleccionado.Content = "Ninguno.";
            } else
            {
                lblReciboSeleccionado.Content = _reciboSelected.GetNumero();
            }
        }

        public UIRecibo()
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

            listRecibosAsociados.Items.Clear();
            listRecibosAsociados.SelectedValuePath = "Key";
            listRecibosAsociados.DisplayMemberPath = "Value";

            listRecibosSimilares.Items.Clear();
            listRecibosSimilares.SelectedValuePath = "Key";
            listRecibosSimilares.DisplayMemberPath = "Value";


            List<DBRecibo> recibosAsociados;
            if (_comprobanteSeleccionado.IsLocal())
            {
                recibosAsociados = selectedComprobante.GetAllRecibos();
            } else
            {
                recibosAsociados = selectedComprobante.GetAllRecibos(dbCon.Connection);
            }
            

            foreach (DBRecibo recibo in recibosAsociados)
            {
                listRecibosAsociados.Items.Add(new KeyValuePair<long, string>(recibo.GetID(), recibo.GetNumero()));
            }
            listRecibosSimilares.SelectedIndex = -1;
            listRecibosAsociados.SelectedIndex = -1;
            SetReciboSeleccionado(null);
        }

        private void listRecibosAsociados_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            if (listRecibosAsociados.SelectedIndex == -1)
            {
                return;
            }
            if (listRecibosAsociados.SelectedItem is null)
            {
                return;
            }
            _reciboSelected = DBRecibo.GetByID(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), ((KeyValuePair<long, string>)listRecibosAsociados.SelectedItem).Key);
        }

        private void btnAsociar_Click(object sender, RoutedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            DBRecibo newRecibo = DBRecibo.GetByNumber(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), txtNumeroRecibo.Text.Trim(), _comprobanteSeleccionado.IsEmitido());

            if (newRecibo is null)
            {
                List<DBRecibo> recibosSimilares = DBRecibo.SearchByNumber(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), txtNumeroRecibo.Text.Trim(), _comprobanteSeleccionado.IsEmitido());
                listRecibosSimilares.Items.Clear();
                listRecibosSimilares.SelectedValuePath = "Key";
                listRecibosSimilares.DisplayMemberPath = "Value";
                foreach (DBRecibo recibo in recibosSimilares)
                {
                    listRecibosSimilares.Items.Add(new KeyValuePair<long, string>(recibo.GetID(), recibo.GetNumero()));
                }
            } else
            {
                _comprobanteSeleccionado.AddRecibo(newRecibo);
                if (!_comprobanteSeleccionado.IsLocal())
                {
                    _comprobanteSeleccionado.PushRelationshipReciboDB(dbCon.Connection, newRecibo);
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
            if (_reciboSelected is null)
            {
                return;
            }
            _comprobanteSeleccionado.RemoveRecibo(_reciboSelected);
            if (!_comprobanteSeleccionado.IsLocal())
            {
                _comprobanteSeleccionado.RemoveRelationshipReciboDB(dbCon.Connection, _reciboSelected);
            }
            RefreshData(_comprobanteSeleccionado);
        }

        private void listRecibosSimilares_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_comprobanteSeleccionado is null)
            {
                return;
            }
            if (listRecibosSimilares.SelectedIndex == -1)
            {
                return;
            }
            if (listRecibosSimilares.SelectedItem is null)
            {
                return;
            }

            DBRecibo similarReciboSelected = DBRecibo.GetByID(dbCon.Connection, _comprobanteSeleccionado.GetEntidadComercial(), ((KeyValuePair<long, string>)listRecibosSimilares.SelectedItem).Key);

            if (similarReciboSelected is null)
            {
                return;
            }
            txtNumeroRecibo.Text = similarReciboSelected.GetNumero();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
