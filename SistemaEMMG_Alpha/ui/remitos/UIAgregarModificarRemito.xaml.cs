using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui.remitos
{
    /// <summary>
    /// Interaction logic for UIAgregarModificarRemito.xaml
    /// </summary>
    public partial class UIAgregarModificarRemito : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIRemitos _ownerControl = null;
        private DBRemito _remitoSeleccionado = null;

        public void SetUIOwner(UIRemitos ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIRemitos GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        public UIAgregarModificarRemito()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiComprobantePanel.SetUIOwner(this);
        }

        private void CheckIfAbleToSubmit()
        {
            DateTime fechaEmitido = new DateTime();
            if (!DateTime.TryParse(txtFecha.Text, out fechaEmitido))
            {
                btnGuardar.IsEnabled = false;
                return;
            }
            if (txtNumero.Text.Trim().Length < 1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }
            btnGuardar.IsEnabled = true;
        }

        public void RefreshData(DBRemito selectedRemito = null)
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
            _remitoSeleccionado = selectedRemito;

            List<DBEntidades> entidadesComerciales = DBEntidades.GetAll(dbCon.Connection, GetCuentaSeleccionada());
            List<DBTipoRemito> listTiposRemitos = DBTipoRemito.GetAll();

            if (entidadesComerciales.Count <= 0 || listTiposRemitos.Count <= 0)
            {
                return;
            }

            cmbTipo.Items.Clear();
            cmbTipo.SelectedValuePath = "Key";
            cmbTipo.DisplayMemberPath = "Value";

            foreach (DBTipoRemito tipoRemito in listTiposRemitos)
            {
                cmbTipo.Items.Add(new KeyValuePair<long, string>(tipoRemito.GetID(), tipoRemito.GetName()));
            }

            listEntidadesEncontradas.Items.Clear();
            listSelectedEntidadComercial.Items.Clear();
            listSelectedEntidadComercial.SelectedValuePath = "Key";
            listSelectedEntidadComercial.DisplayMemberPath = "Value";

            if (_remitoSeleccionado is null)
            {
                _remitoSeleccionado = new DBRemito(entidadesComerciales[0], listTiposRemitos[0], true, null, "00", "nothing"); //Created to handle pagos/remitos relations.
                listSelectedEntidadComercial.Items.Add(new KeyValuePair<long, string>(-1, "Seleccione una entidad comercial..."));
                rdbRecibido.IsChecked = false;
                rdbEmitido.IsChecked = true;
                txtNumero.Text = "";
                listEntidadesEncontradas.Items.Clear();
                txtFiltroEntidad.Text = "";
                txtFecha.Text = "";
                txtObservacion.Text = "";
                cmbTipo.SelectedIndex = 0;
            }
            else
            {
                listSelectedEntidadComercial.Items.Add(
                    new KeyValuePair<long, string>(_remitoSeleccionado.GetEntidadComercialID(),
                    $"{_remitoSeleccionado.GetEntidadComercial().GetCUIT()}: {_remitoSeleccionado.GetEntidadComercial().GetRazonSocial()}")
                    );

                rdbRecibido.IsChecked = !_remitoSeleccionado.IsEmitido();
                rdbEmitido.IsChecked = _remitoSeleccionado.IsEmitido();
                txtNumero.Text = _remitoSeleccionado.GetNumero();
                listEntidadesEncontradas.Items.Clear();
                txtFiltroEntidad.Text = "";
                txtFecha.Text = _remitoSeleccionado.GetFecha().HasValue ? ((DateTime)_remitoSeleccionado.GetFecha()).ToString("dd/MM/yyyy") : "";
                txtObservacion.Text = _remitoSeleccionado.GetObservacion();
                cmbTipo.SelectedValue = _remitoSeleccionado.GetTipoRemito().GetID();
            }

            listSelectedEntidadComercial.SelectedIndex = 0;
            CheckIfAbleToSubmit();
        }

        private void btnBuscarEntidad_Click(object sender, RoutedEventArgs e)
        {
            List<DBEntidades> entidadesComercialesList = DBEntidades.Search(dbCon.Connection, GetCuentaSeleccionada(), txtFiltroEntidad.Text);

            listEntidadesEncontradas.Items.Clear();
            listEntidadesEncontradas.SelectedValuePath = "Key";
            listEntidadesEncontradas.DisplayMemberPath = "Value";

            foreach (DBEntidades entidad in entidadesComercialesList)
            {
                listEntidadesEncontradas.Items.Add(new KeyValuePair<long, string>(entidad.GetID(), $"{entidad.GetCUIT()}: {entidad.GetRazonSocial()}"));
            }
        }

        private void listEntidadesEncontradas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listEntidadesEncontradas.SelectedItem is null)
            {
                return;
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            long listECSelectedID = ((KeyValuePair<long, string>)listEntidadesEncontradas.SelectedItem).Key;
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

            if (_remitoSeleccionado.IsLocal())
            {
                _remitoSeleccionado.SetEntidadComercial(selectedEntidad);
            }
            listSelectedEntidadComercial.SelectedIndex = 0;
        }

        private void txtNumero_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void txtFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(!(_ownerControl is null));

            long old_cm_ec_id = _remitoSeleccionado.GetEntidadComercialID();

            DateTime fechaEmitido = new DateTime();
            DateTime.TryParse(txtFecha.Text, out fechaEmitido);

            _remitoSeleccionado.SetFecha(fechaEmitido);
            _remitoSeleccionado.SetEmitido(SafeConvert.ToBoolean(rdbEmitido.IsChecked));
            _remitoSeleccionado.SetEntidadComercial(((KeyValuePair<long, string>)listSelectedEntidadComercial.SelectedItem).Key, dbCon.Connection);
            _remitoSeleccionado.SetTipoRemito(((KeyValuePair<long, string>)cmbTipo.SelectedItem).Key);
            _remitoSeleccionado.SetNumero(txtNumero.Text);
            _remitoSeleccionado.SetObservacion(txtObservacion.Text);
            bool remitoWasLocal = _remitoSeleccionado.IsLocal();

            if (_remitoSeleccionado.PushToDatabase(dbCon.Connection, old_cm_ec_id))
            {
                if (remitoWasLocal)
                {
                    _remitoSeleccionado.PushAllRelationshipsWithComprobantesDB(dbCon.Connection);
                }
                MessageBox.Show("Remito agregado / modificado a la base de datos correctamente!");
                _ownerControl.RefreshData();
                Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Error al tratar de agregar / modificar este recibo. ¿Ya existe un recibo con el mismo número?.");
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void rdbRecibido_Checked(object sender, RoutedEventArgs e)
        {
            if (_remitoSeleccionado is null)
            {
                return;
            }
            if (_remitoSeleccionado.IsLocal())
            {
                _remitoSeleccionado.SetEmitido(false);
            }
        }

        private void rdbEmitido_Checked(object sender, RoutedEventArgs e)
        {
            if (_remitoSeleccionado is null)
            {
                return;
            }
            if (_remitoSeleccionado.IsLocal())
            {
                _remitoSeleccionado.SetEmitido(true);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_remitoSeleccionado is null)
            {
                return;
            }
            uiComprobantePanel.RefreshData(_remitoSeleccionado);
            uiComprobantePanel.Visibility = Visibility.Visible;
        }

        private void txtFecha_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFecha);
        }
    }
}
