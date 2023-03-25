using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha.ui.cuentas
{
    /// <summary>
    /// Interaction logic for UIAgregarModificarCuenta.xaml
    /// </summary>
    public partial class UIAgregarModificarCuenta : UserControl
    {

        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UICuentas _ownerControl = null;
        private DBCuenta _selectedCuenta = null;

        public void SetUIOwner(UICuentas ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UICuentas GetOwnerControl() => _ownerControl;

        private void CheckIfAbleToSubmit()
        {
            btnCuentaGuardar.IsEnabled = !(string.IsNullOrEmpty(txtCuentaCUIT.Text.Trim()) || string.IsNullOrEmpty(txtCuentaRazonSocial.Text.Trim()));
        }

        public void RefreshData(DBCuenta selectedCuenta=null)
        {
            _selectedCuenta = selectedCuenta;

            if (_selectedCuenta is null)
            {
                txtCuentaCUIT.Text = "";
                txtCuentaRazonSocial.Text = "";
            } else
            {
                txtCuentaCUIT.Text = _selectedCuenta.GetCUIT().ToString();
                txtCuentaRazonSocial.Text = _selectedCuenta.GetRazonSocial();
            }
        }

        public UIAgregarModificarCuenta()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        private void btnCuentaGuardar_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(!(_ownerControl is null));

            if (_selectedCuenta is null)
            {
                _selectedCuenta = new DBCuenta(
                    SafeConvert.ToInt64(txtCuentaCUIT.Text),
                    txtCuentaRazonSocial.Text);
            } else
            {
                _selectedCuenta.SetCUIT(SafeConvert.ToInt64(txtCuentaCUIT.Text));
                _selectedCuenta.SetRazonSocial(txtCuentaRazonSocial.Text);
            }

            if (_selectedCuenta.PushToDatabase(dbCon.Connection))
            {
                MessageBox.Show("Cuenta agregada / modificada a la base de datos correctamente!");
                _ownerControl.RefreshData();
                Visibility = Visibility.Collapsed;
            } else
            {
                MessageBox.Show("Error al tratar de agregar / modificar esta cuenta. ¿Ya existe una cuenta con el mismo CUIT y Razón Social?.");
            }
        }

        private void btnCuentaCancelar_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void txtCuentaCUIT_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtCuentaCUIT_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void txtCuentaRazonSocial_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }
    }
}
