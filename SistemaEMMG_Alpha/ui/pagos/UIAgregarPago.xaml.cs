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

namespace SistemaEMMG_Alpha.ui.pagos
{
    /// <summary>
    /// Interaction logic for UIAgregarPago.xaml
    /// </summary>
    public partial class UIAgregarPago : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIPagos _ownerControl = null;
        private DBPago _pagoSeleccionado = null;
        public void SetUIOwner(UIPagos ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIPagos GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        public UIAgregarPago()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        public void RefreshData(DBPago selectedPago = null)
        {
            if (GetOwnerControl() is null) return;

            if (dbCon is null)
            {
                dbCon = DBConnection.Instance();
            }

            if (dbData is null)
            {
                dbData = DBMain.Instance();
            }

            if (!GetOwnerControl().GetMainWindow().CheckDBConnection())
            {
                return;
            }

            if (GetCuentaSeleccionada() is null) return;

            dbData.RefreshBasicDataDB(dbCon.Connection);

            _pagoSeleccionado = selectedPago;

            cmbFormaDePago.FillWithDBData(DBFormasPago.GetAll());
            cmbMoneda.FillWithDBData(DBMoneda.GetAll());

            RefreshFormData();
        }

        private void RefreshFormData()
        {
            DBFormasPago selectedFormaPago = cmbFormaDePago.SelectedItemAsFormaPago();
            DBMoneda selectedMoneda = cmbMoneda.SelectedItemAsMoneda();

            if (!(selectedFormaPago is null) && selectedFormaPago.GetTipo() == TipoFormaDePago.CHEQUE)
            {
                gridCheques.Visibility = Visibility.Visible;
            } else
            {
                gridCheques.Visibility = Visibility.Collapsed;
            }

            if (!(selectedMoneda is null) && selectedMoneda.IsExtranjera())
            {
                lblCambio.Visibility = Visibility.Visible;
                txtCambio.Visibility = Visibility.Visible;
            } else
            {
                lblCambio.Visibility = Visibility.Collapsed;
                txtCambio.Visibility = Visibility.Collapsed;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            GetOwnerControl().winAgregarModificarPago.Visibility = Visibility.Collapsed;
        }

        private void cmbFormaDePago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFormData();
        }

        private void cmbMoneda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFormData();
        }
    }
}
