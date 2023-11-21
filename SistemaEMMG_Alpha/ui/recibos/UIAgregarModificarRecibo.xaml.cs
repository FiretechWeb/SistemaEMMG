using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui.recibos
{
    /// <summary>
    /// Interaction logic for UIAgregarModificarRecibo.xaml
    /// </summary>
    public partial class UIAgregarModificarRecibo : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIRecibos _ownerControl = null;
        private DBRecibo _reciboSeleccionado = null;
        private bool modificandoRecibo = false;

        public void SetUIOwner(UIRecibos ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIRecibos GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        public UIAgregarModificarRecibo()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiComprobantesPanel.SetUIOwner(this);
            uiPagosPanel.SetUIOwner(this);
        }

        private long getListSelectedEntidadComercialID()
        {
            if (listSelectedEntidadComercial.SelectedItem is null) return -1;

            return ((KeyValuePair<long, string>)listSelectedEntidadComercial.SelectedItem).Key;
        }
        private bool inputFechaEmitidoIsValid()
        {
            DateTime fechaEmitido = new DateTime();
            return DateTime.TryParse(txtFecha.Text, out fechaEmitido);
        }

        private void CheckIfAbleToSubmit()
        {
            if (!inputFechaEmitidoIsValid())
            {
                btnGuardar.IsEnabled = false;
                return;
            }
            if (txtNumero.Text.Trim().Length < 1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }
            if (getListSelectedEntidadComercialID() <= -1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }
            btnGuardar.IsEnabled = true;
        }

        private void RefreshFieldsColorState()
        {
            if (inputFechaEmitidoIsValid())
            {
                txtFecha.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtFecha.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtNumero.Text.Trim().Length >= 1)
            {
                txtNumero.ClearValue(TextBox.BorderBrushProperty);
            } else
            {
                txtNumero.BorderBrush = System.Windows.Media.Brushes.Red;
            }
        }

        public void PrepararParaAgregar()
        {
            modificandoRecibo = false;
        }

        public void PrepararParaModificar()
        {
            modificandoRecibo = true;
        }
        public void RefreshData(DBRecibo selectedRecibo = null)
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
            _reciboSeleccionado = selectedRecibo;

            List<DBEntidades> entidadesComerciales = DBEntidades.GetAll(dbCon.Connection, GetCuentaSeleccionada());
            List<DBTipoRecibo> listTiposRecibos = DBTipoRecibo.GetAll();

            if (entidadesComerciales.Count <= 0 || listTiposRecibos.Count <= 0)
            {
                return;
            }
            
            cmbTipo.Items.Clear();
            cmbTipo.SelectedValuePath = "Key";
            cmbTipo.DisplayMemberPath = "Value";

            foreach (DBTipoRecibo tipoRecibo in listTiposRecibos)
            {
                cmbTipo.Items.Add(new KeyValuePair<long, string>(tipoRecibo.GetID(), tipoRecibo.GetName()));
            }

            listEntidadesEncontradas.Items.Clear();
            listSelectedEntidadComercial.Items.Clear();
            listSelectedEntidadComercial.SelectedValuePath = "Key";
            listSelectedEntidadComercial.DisplayMemberPath = "Value";

            if (_reciboSeleccionado is null)
            {
                _reciboSeleccionado = new DBRecibo(entidadesComerciales[0], listTiposRecibos[0], true, null, "00", "nothing"); //Created to handle pagos/remitos relations.
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
                    new KeyValuePair<long, string>(_reciboSeleccionado.GetEntidadComercialID(),
                    $"{_reciboSeleccionado.GetEntidadComercial().GetCUIT()}: {_reciboSeleccionado.GetEntidadComercial().GetRazonSocial()}")
                    );

                rdbRecibido.IsChecked = !_reciboSeleccionado.IsEmitido();
                rdbEmitido.IsChecked = _reciboSeleccionado.IsEmitido();
                txtNumero.Text = _reciboSeleccionado.GetNumero();
                listEntidadesEncontradas.Items.Clear();
                txtFiltroEntidad.Text = "";
                txtFecha.Text = _reciboSeleccionado.GetFecha().HasValue ? ((DateTime)_reciboSeleccionado.GetFecha()).ToString("dd/MM/yyyy") : "";
                txtObservacion.Text = _reciboSeleccionado.GetObservacion();
                cmbTipo.SelectedValue = _reciboSeleccionado.GetTipoRecibo().GetID();
            }

            listSelectedEntidadComercial.SelectedIndex = 0;
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
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

            if (_reciboSeleccionado.IsLocal())
            {
                _reciboSeleccionado.SetEntidadComercial(selectedEntidad);
            }
            listSelectedEntidadComercial.SelectedIndex = 0;

            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtNumero_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Trace.Assert(!(_ownerControl is null));

            if (_reciboSeleccionado.GetEntidadComercial() is null) return;

            long old_cm_ec_id = _reciboSeleccionado.GetEntidadComercialID();

            DateTime fechaEmitido = new DateTime();
            DateTime.TryParse(txtFecha.Text, out fechaEmitido);

            _reciboSeleccionado.SetFecha(fechaEmitido);
            _reciboSeleccionado.SetEmitido(SafeConvert.ToBoolean(rdbEmitido.IsChecked));
            _reciboSeleccionado.SetEntidadComercial(((KeyValuePair<long, string>)listSelectedEntidadComercial.SelectedItem).Key, dbCon.Connection);
            _reciboSeleccionado.SetTipoRecibo(((KeyValuePair<long, string>)cmbTipo.SelectedItem).Key);
            _reciboSeleccionado.SetNumero(txtNumero.Text);
            _reciboSeleccionado.SetObservacion(txtObservacion.Text);
            bool reciboWasLocal = _reciboSeleccionado.IsLocal();
            bool pushDataSuccess = false;
            if (_reciboSeleccionado.PushToDatabase(dbCon.Connection, old_cm_ec_id))
            {
                if (reciboWasLocal)
                {
                    _reciboSeleccionado.PushAllRelationshipsWithComprobantesDB(dbCon.Connection);
                    List<DBPago> pagos = _reciboSeleccionado.GetAllPagos();
                    foreach (DBPago pago in pagos)
                    {
                        pago.PushToDatabase(dbCon.Connection);
                    }
                }

                pushDataSuccess = true;
            }
            if (modificandoRecibo || pushDataSuccess)
            {
                MessageBox.Show("Recibo agregado / modificado a la base de datos correctamente!");
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
            if (_reciboSeleccionado is null)
            {
                return;
            }
            if (_reciboSeleccionado.IsLocal())
            {
                _reciboSeleccionado.SetEmitido(false);
            }
        }

        private void rdbEmitido_Checked(object sender, RoutedEventArgs e)
        {
            if (_reciboSeleccionado is null)
            {
                return;
            }
            if (_reciboSeleccionado.IsLocal())
            {
                _reciboSeleccionado.SetEmitido(true);
            }
        }

        private void btnComprobantes_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSeleccionado is null)
            {
                return;
            }
            uiComprobantesPanel.RefreshData(_reciboSeleccionado);
            uiComprobantesPanel.Visibility = Visibility.Visible;
        }

        private void btnPagos_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSeleccionado is null)
            {
                return;
            }
            uiPagosPanel.RefreshData(_reciboSeleccionado);
            uiPagosPanel.Visibility = Visibility.Visible;
        }

        private void txtFecha_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFecha); 
        }
    }
}
