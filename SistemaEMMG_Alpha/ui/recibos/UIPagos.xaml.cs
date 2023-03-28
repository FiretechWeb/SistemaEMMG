using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha.ui.recibos
{
    /// <summary>
    /// Interaction logic for UIPagos.xaml
    /// </summary>
    public partial class UIPagos : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIAgregarModificarRecibo _ownerControl = null;
        private DBPago _pagoSelected = null;
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

        private void SetPagoSeleccionado(DBPago newSeleccion)
        {
            _pagoSelected = newSeleccion;
            if (_pagoSelected is null)
            {
                lblPagoSeleccionado.Content = "Ninguno.";
                btnEliminar.IsEnabled = false;
            }
            else
            {
                lblPagoSeleccionado.Content = $"{_pagoSelected.GetFormaDePago().GetName()}: {_pagoSelected.GetImporte()} {_pagoSelected.GetMoneda().GetName()}";
                btnEliminar.IsEnabled = true;
            }
            RefreshForm();
            CheckIfAbleToSubmit();
        }
        private void RefreshForm()
        {
            if (_pagoSelected is null)
            {
                txtCambio.Text = "1.0";
                txtFecha.Text = "";
                txtImporte.Text = "";
                txtObservacion.Text = "";
                cmbFormaPago.SelectedIndex = 0;
                cmbMoneda.SelectedIndex = 0;
            } else {
                txtCambio.Text = _pagoSelected.GetCambio().ToString();
                txtFecha.Text = _pagoSelected.GetFecha().HasValue ? ((DateTime)_pagoSelected.GetFecha()).ToString("dd/MM/yyyy") : "";
                txtImporte.Text = _pagoSelected.GetImporte().ToString();
                txtObservacion.Text = _pagoSelected.GetObservacion();
                cmbFormaPago.SelectedValue = _pagoSelected.GetFormaDePago().GetID();
                cmbMoneda.SelectedValue = _pagoSelected.GetMoneda().GetID();
            }
        }

        private void CheckIfAbleToSubmit()
        {
            DateTime fechaEmitido = new DateTime();
            if (!DateTime.TryParse(txtFecha.Text, out fechaEmitido))
            {
                btnAgregar.IsEnabled = false;
                btnModificar.IsEnabled = false;
                return;
            }
            if (txtImporte.Text.Trim().Length < 1)
            {
                btnAgregar.IsEnabled = false;
                btnModificar.IsEnabled = false;
                return;
            }
            btnAgregar.IsEnabled = true;
            if (!(_pagoSelected is null))
            {
                btnModificar.IsEnabled = true;
            }
        }

        public UIPagos()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }


        public void RefreshData(DBRecibo selectedRecibo)
        {
            if (_ownerControl is null)
            {
                return;
            }
            if (_ownerControl.GetOwnerControl().GetMainWindow() is null)
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
            if (!_ownerControl.GetOwnerControl().GetMainWindow().CheckDBConnection())
            {
                return;
            }

            _reciboSelected = selectedRecibo;

            if (_reciboSelected is null)
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);

            List<DBMoneda> listMonedas = DBMoneda.GetAll();
            List<DBFormasPago> listFormasPago = DBFormasPago.GetAll();

            if (listMonedas.Count <= 0 || listFormasPago.Count <= 0)
            {
                return;
            }

            listPagos.Items.Clear();
            listPagos.SelectedValuePath = "Key";
            listPagos.DisplayMemberPath = "Value";

            cmbFormaPago.Items.Clear();
            cmbFormaPago.SelectedValuePath = "Key";
            cmbFormaPago.DisplayMemberPath = "Value";

            cmbMoneda.Items.Clear();
            cmbMoneda.SelectedValuePath = "Key";
            cmbMoneda.DisplayMemberPath = "Value";

            foreach (DBMoneda moneda in listMonedas)
            {
                cmbMoneda.Items.Add(new KeyValuePair<long, string>(moneda.GetID(), moneda.GetName()));
            }

            foreach (DBFormasPago formaDePago in listFormasPago)
            {
                cmbFormaPago.Items.Add(new KeyValuePair<long, string>(formaDePago.GetID(), formaDePago.GetName()));
            }

            List<DBPago> pagos;

            if (_reciboSelected.IsLocal())
            {
                pagos = _reciboSelected.GetAllPagos();
            }
            else
            {
                pagos = _reciboSelected.GetAllPagos(dbCon.Connection);
            }
            double importeTotal = 0.0;
            foreach (DBPago pago in pagos)
            {
                listPagos.Items.Add(new KeyValuePair<long, string>(pago.GetID(), $"{pago.GetFormaDePago().GetName()}: {pago.GetImporte()} {pago.GetMoneda().GetName()}"));
                importeTotal += pago.GetImporte_MonedaLocal();
            }
            listPagos.SelectedIndex = -1;
            lblPagosImporte.Content = $"{importeTotal.ToString("0.00")} ARS";
            SetPagoSeleccionado(null);
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }

            double cambio;
            DBMoneda monedaSelected = DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key);
            if (monedaSelected.IsExtranjera())
            {
                cambio = SafeConvert.ToDouble(txtCambio.Text.Trim().Replace(".", ","));
            } else
            {
                cambio = 1.0;
            }


            DateTime fechaEmitido = new DateTime();
            DateTime.TryParse(txtFecha.Text, out fechaEmitido);

            DBPago newPago = new DBPago(
                _reciboSelected,
                DBFormasPago.GetByID(((KeyValuePair<long, string>)cmbFormaPago.SelectedItem).Key),
                DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key),
                SafeConvert.ToDouble(txtImporte.Text.Trim().Replace(".", ",")),
                txtObservacion.Text.Trim(),
                fechaEmitido,
                cambio);

            _reciboSelected.AddPago(newPago);

            if (!_reciboSelected.IsLocal())
            {
                if (newPago.PushToDatabase(dbCon.Connection))
                {
                    MessageBox.Show("¡Pago agregado a la base de datos correctamente!");
                }
                else
                {
                    MessageBox.Show("Error, no se pudo agregar el pago a la base de datos.");
                }
            }
            RefreshData(_reciboSelected);
        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            if (_pagoSelected is null)
            {
                return;
            }

            double cambio;
            DBMoneda monedaSelected = DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key);
            if (monedaSelected.IsExtranjera())
            {
                cambio = SafeConvert.ToDouble(txtCambio.Text.Trim().Replace(".", ","));
            }
            else
            {
                cambio = 1.0;
            }

            DateTime fechaEmitido = new DateTime();
            DateTime.TryParse(txtFecha.Text, out fechaEmitido);

            _pagoSelected.SetCambio(cambio);
            _pagoSelected.SetFecha(fechaEmitido);
            _pagoSelected.SetFormaDePago(DBFormasPago.GetByID(((KeyValuePair<long, string>)cmbFormaPago.SelectedItem).Key));
            _pagoSelected.SetMoneda(DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key));
            _pagoSelected.SetObservacion(txtObservacion.Text.Trim());
            _pagoSelected.SetImporte(SafeConvert.ToDouble(txtImporte.Text.Trim().Replace(".", ",")));

            if (!_reciboSelected.IsLocal())
            {
                if (_pagoSelected.PushToDatabase(dbCon.Connection))
                {
                    MessageBox.Show("¡Pago actualizado en la base de datos correctamente!");
                }
                else
                {
                    MessageBox.Show("Error, no se pudo actualizar el pago a la base de datos.");
                }
            }
            RefreshData(_reciboSelected);
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            if (_pagoSelected is null)
            {
                return;
            }
            _reciboSelected.RemovePago(_pagoSelected);

            if (!_reciboSelected.IsLocal())
            {
                _pagoSelected.DeleteFromDatabase(dbCon.Connection);
            }
            RefreshData(_reciboSelected);
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void cmbMoneda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMoneda.SelectedIndex == -1)
            {
                return;
            }
            if (cmbMoneda.SelectedItem is null)
            {
                return;
            }

            DBMoneda monedaSelected = DBMoneda.GetByID(((KeyValuePair<long, string>)cmbMoneda.SelectedItem).Key);

            if (monedaSelected.IsExtranjera())
            {
                txtCambio.Visibility = Visibility.Visible;
                lblCambio.Visibility = Visibility.Visible;
            } else
            {
                txtCambio.Visibility = Visibility.Collapsed;
                lblCambio.Visibility = Visibility.Collapsed;
            }
        }

        private void listPagos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_reciboSelected is null)
            {
                return;
            }
            if (listPagos.SelectedIndex == -1)
            {
                return;
            }
            if (listPagos.SelectedItem is null)
            {
                return;
            }
            if (!_reciboSelected.IsLocal())
            {
                SetPagoSeleccionado(DBPago.GetByID(dbCon.Connection, _reciboSelected, ((KeyValuePair<long, string>)listPagos.SelectedItem).Key));

            } else
            {
                SetPagoSeleccionado(_reciboSelected.GetPagoByID(((KeyValuePair<long, string>)listPagos.SelectedItem).Key));
            }
            RefreshForm();
        }

        private void txtImporte_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void txtFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
        }

        private void txtCambio_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void txtImporte_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,.]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
