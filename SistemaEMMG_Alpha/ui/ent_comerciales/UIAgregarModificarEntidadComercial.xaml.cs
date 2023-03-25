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
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui.ent_comerciales
{
    /// <summary>
    /// Interaction logic for UIAgregarModificarEntidadComercial.xaml
    /// </summary>
    public partial class UIAgregarModificarEntidadComercial : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIEntidades _ownerControl = null;
        private DBEntidades _selectedEntidadComercial = null;

        public void SetUIOwner(UIEntidades ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIEntidades GetOwnerControl() => _ownerControl;

        private void CheckIfAbleToSubmit()
        {
            btnGuardar.IsEnabled = !(string.IsNullOrEmpty(txtCuitEntidad.Text.Trim()) || string.IsNullOrEmpty(txtRazonEntidad.Text.Trim()));
        }

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }


        public UIAgregarModificarEntidadComercial()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        public void RefreshData(DBEntidades selectedEntidadComercial = null)
        {
            _selectedEntidadComercial = selectedEntidadComercial;
            cmbTipoEntidad.Items.Clear();
            cmbTipoEntidad.SelectedValuePath = "Key";
            cmbTipoEntidad.DisplayMemberPath = "Value";
            List<DBTipoEntidad> tiposEntidades = DBTipoEntidad.GetAll();
            foreach(DBTipoEntidad tipoEntidad in tiposEntidades)
            {
                cmbTipoEntidad.Items.Add(new KeyValuePair<long, string>(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }

            if (_selectedEntidadComercial is null)
            {
                txtCelEntidad.Text = "";
                txtCuitEntidad.Text = "";
                txtEmailEntidad.Text = "";
                txtRazonEntidad.Text = "";
                txtTelEntidad.Text = "";
                if (tiposEntidades.Count > 0)
                {
                    cmbTipoEntidad.SelectedIndex = 0;
                }
            } else
            {
                txtCelEntidad.Text = _selectedEntidadComercial.GetCelular();
                txtEmailEntidad.Text = _selectedEntidadComercial.GetEmail();
                txtCuitEntidad.Text = _selectedEntidadComercial.GetCUIT().ToString();
                txtRazonEntidad.Text = _selectedEntidadComercial.GetRazonSocial();
                txtTelEntidad.Text = _selectedEntidadComercial.GetTelefono();
                cmbTipoEntidad.SelectedValue = _selectedEntidadComercial.GetTipoEntidad().GetID();
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(!(_ownerControl is null || GetCuentaSeleccionada() is null));

            if (_selectedEntidadComercial is null)
            {
                _selectedEntidadComercial = new DBEntidades(
                    GetCuentaSeleccionada(),
                    DBTipoEntidad.GetByID(((KeyValuePair<long, string>)cmbTipoEntidad.SelectedItem).Key),
                    SafeConvert.ToInt64(txtCuitEntidad.Text),
                    txtRazonEntidad.Text,
                    txtEmailEntidad.Text,
                    txtTelEntidad.Text,
                    txtCelEntidad.Text
                    );
            } else
            {
                _selectedEntidadComercial.SetTipoEntidad(DBTipoEntidad.GetByID(((KeyValuePair<long, string>)cmbTipoEntidad.SelectedItem).Key));
                _selectedEntidadComercial.SetCuit(SafeConvert.ToInt64(txtCuitEntidad.Text));
                _selectedEntidadComercial.SetRazonSocial(txtRazonEntidad.Text);
                _selectedEntidadComercial.SetEmail(txtEmailEntidad.Text);
                _selectedEntidadComercial.SetTelefono(txtTelEntidad.Text);
                _selectedEntidadComercial.SetCelular(txtCelEntidad.Text);
            }

            if (_selectedEntidadComercial.PushToDatabase(dbCon.Connection))
            {
                MessageBox.Show("Entidad comercial agregada / modificada a la base de datos correctamente!");
                _ownerControl.RefreshData();
                Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Error al tratar de agregar / modificar esta entidad comercial. ¿Ya existe una entidad comercial con el mismo CUIT y Razón Social?.");
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void txtCuitEntidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void txtRazonEntidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void txtCuitEntidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
