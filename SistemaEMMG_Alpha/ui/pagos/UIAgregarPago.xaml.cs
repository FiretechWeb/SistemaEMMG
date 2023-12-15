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

    //EDIT IMPORTANTE: SE NECESITA LA ENTIDAD COMERCIAL A USAR PARA SABER EL NRO DE RECIBO!
    //HAY QUE MODIFICAR ESO MAÑANA!!

    /// <summary>
    /// Interaction logic for UIAgregarPago.xaml
    /// </summary>
    public partial class UIAgregarPago : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIPagos _ownerControl = null;
        private DBPago _pagoSeleccionado = null;
        private DBEntidades _entidadSeleccionada = null;
        public void SetUIOwner(UIPagos ownerControl)
        {
            _ownerControl = ownerControl;
        }

        private DBPago GetPagoSeleccionado() => _pagoSeleccionado;

        private void SetPagoSeleccionado(DBPago pago) => _pagoSeleccionado = pago;

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

            SetPagoSeleccionado(selectedPago);
            lblEntidadRazon.Content = "Seleccionar una entidad...";
            _entidadSeleccionada = null;

            cmbFormaDePago.FillWithDBData(DBFormasPago.GetAll());
            cmbMoneda.FillWithDBData(DBMoneda.GetAll());

            if (selectedPago is null)
            {
                txtCambio.Text = "";
                txtChequeCuenta.Text = "";
                txtChequeCUIT.Text = "";
                txtChequeFechaDebito.Text = "";
                txtChequeFechaPago.Text = "";
                txtChequeLocalidad.Text = "";
                txtChequeNroBanco.Text = "";
                txtChequeNroSucursal.Text = "";
                txtChequeNumero.Text = "";
                txtChequePersona.Text = "";
                txtChequeSerie.Text = "";
                txtEntidadRecibo.Text = "";
                txtFecha.Text = "";
                txtImporte.Text = "";
                txtObservacion.Text = "";
                txtNroRecibo.Text = "";
            } else
            {
                DBRecibo recibo = selectedPago.GetRecibo();
                DBEntidades entidadComercial = selectedPago.GetEntidadComercial();

                cmbFormaDePago.SelectedValue = selectedPago.GetFormaDePago().GetID();
                cmbMoneda.SelectedValue = selectedPago.GetMoneda().GetID();
                rdbReciboEmitido.IsChecked = selectedPago.GetRecibo().IsEmitido();
                rdbReciboRecibido.IsChecked = !selectedPago.GetRecibo().IsEmitido();
                txtNroRecibo.Text = selectedPago.GetRecibo().GetNumero();
                txtImporte.Text = SafeConvert.ToString(selectedPago.GetImporte());
                txtCambio.Text = SafeConvert.ToString(selectedPago.GetCambio());
                txtFecha.Text = selectedPago.GetFecha().HasValue ? ((DateTime)selectedPago.GetFecha()).ToString("dd/MM/yyyy") : "";
                txtEntidadRecibo.Text = SafeConvert.ToString(selectedPago.GetEntidadComercial().GetCUIT());
                txtObservacion.Text = selectedPago.GetObservacion();
                if (!selectedPago.GetCheque().HasValue || ((ChequeData)selectedPago.GetCheque()).pg_bnk_id <= -1)
                {
                    txtChequeCuenta.Text = "";
                    txtChequeCUIT.Text = "";
                    txtChequeFechaDebito.Text = "";
                    txtChequeFechaPago.Text = "";
                    txtChequeLocalidad.Text = "";
                    txtChequeNroBanco.Text = "";
                    txtChequeNroSucursal.Text = "";
                    txtChequeNumero.Text = "";
                    txtChequePersona.Text = "";
                    txtChequeSerie.Text = "";
                } else
                {
                    ChequeData dataCheque = (ChequeData)selectedPago.GetCheque();
                    DBBancos chequeBanco = DBBancos.GetByID(dataCheque.pg_bnk_id);
                    txtChequeCuenta.Text = dataCheque.pg_cheque_cta;
                    txtChequeCUIT.Text = dataCheque.pg_cheque_cuit.ToString();
                    txtChequeFechaDebito.Text = dataCheque.pg_cheque_debito.HasValue ? ((DateTime)dataCheque.pg_cheque_debito).ToString("dd/MM/yyyy") : "";
                    txtChequeFechaPago.Text = dataCheque.pg_cheque_pay.HasValue ? ((DateTime)dataCheque.pg_cheque_pay).ToString("dd/MM/yyyy") : ""; ;
                    txtChequeLocalidad.Text = dataCheque.pg_cheque_localidad.ToString();
                    txtChequeNroBanco.Text = (chequeBanco is null) ? "" : chequeBanco.GetCode().ToString();
                    txtChequeNroSucursal.Text = dataCheque.pg_cheque_sucursal.ToString();
                    txtChequeNumero.Text = dataCheque.pg_cheque_num.ToString();
                    txtChequePersona.Text = dataCheque.pg_cheque_persona;
                    txtChequeSerie.Text = dataCheque.pg_cheque_serie;
                }
            }

            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void RefreshFormData()
        {
            if (cmbFormaDePago is null)
            {
                return;
            }
            if (cmbMoneda is null)
            {
                return;
            }
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

            DBBancos bancoIntroducido = DBBancos.GetByCode(SafeConvert.ToInt32(txtChequeNroBanco.Text.Trim()));
            if (bancoIntroducido is null)
            {
                lblChequeNombreBanco.Content = "Inválido";
            } else
            {
                lblChequeNombreBanco.Content = bancoIntroducido.GetName();
            }

            if (rdbReciboEmitido.IsChecked == true)
            {
                gridEntidadInput.Visibility = Visibility.Collapsed;
                gridEntidadRazon.Visibility = Visibility.Collapsed;
            } else
            {
                gridEntidadInput.Visibility = Visibility.Visible;
                gridEntidadRazon.Visibility = Visibility.Visible;
            }

            AutocompletarEntidades();
        }

        private void AutocompletarEntidades()
        {
            if (txtEntidadRecibo.Text.Trim().Length >= 1)
            {
                List<DBEntidades> entidadesEncontradas = DBEntidades.Search(dbCon.Connection, GetCuentaSeleccionada(), txtEntidadRecibo.Text.Trim());
                if (entidadesEncontradas.Count > 0)
                {
                    lblEntidadRazon.Content = entidadesEncontradas[0].GetRazonSocial();
                    txtEntidadRecibo.Text = entidadesEncontradas[0].GetCUIT().ToString();
                } else {
                    lblEntidadRazon.Content = "No se encontró entidad...";
                }
            }
        }

        private void CheckIfAbleToSubmit()
        {
            if (txtNroRecibo is null || txtFecha is null || txtImporte is null) return;

            if (txtNroRecibo.Text.Trim().Length < 1)
            {
                btnAceptar.IsEnabled = false;
                return;
            }

            if (!txtFecha.Text.IsValidDateTime())
            {
                btnAceptar.IsEnabled = false;
                return;
            }

            if (txtImporte.Text.Trim().Length < 1)
            {
                btnAceptar.IsEnabled = false;
                return;
            }

            DBFormasPago selectedFormaPago = cmbFormaDePago.SelectedItemAsFormaPago();
            DBMoneda selectedMoneda = cmbMoneda.SelectedItemAsMoneda();

            if (!(selectedMoneda is null) && selectedMoneda.IsExtranjera())
            {
                if (txtCambio.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
            }

            if (!(selectedFormaPago is null) && selectedFormaPago.GetTipo() == TipoFormaDePago.CHEQUE)
            {
                if(DBBancos.GetByCode(SafeConvert.ToInt32(txtChequeNroBanco.Text.Trim())) is null) {
                    btnAceptar.IsEnabled = false;
                    return;
                }

                if (txtChequeCuenta.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }

                if (txtChequeCUIT.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }

                if (!txtChequeFechaPago.Text.IsValidDateTime())
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
                if (txtChequeLocalidad.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
                if (txtChequeNroSucursal.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
                if (txtChequeNumero.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
                if (txtChequePersona.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
                if (txtChequeSerie.Text.Trim().Length < 1)
                {
                    btnAceptar.IsEnabled = false;
                    return;
                }
            }

            if (rdbReciboEmitido.IsChecked != true && txtEntidadRecibo.Text.Trim().Length < 1)
            {
                btnAceptar.IsEnabled = false;
                return;
            }

            btnAceptar.IsEnabled = true;
        }

        private void RefreshFieldsColorState()
        {
            if (txtNroRecibo is null || txtFecha is null || txtImporte is null) return;
            if (txtNroRecibo.Text.Trim().Length >= 1)
            {
                txtNroRecibo.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtNroRecibo.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtFecha.Text.IsValidDateTime())
            {
                txtFecha.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtFecha.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtImporte.Text.Trim().Length >= 1)
            {
                txtImporte.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtImporte.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            DBFormasPago selectedFormaPago = cmbFormaDePago.SelectedItemAsFormaPago();
            DBMoneda selectedMoneda = cmbMoneda.SelectedItemAsMoneda();

            if (!(selectedMoneda is null) && selectedMoneda.IsExtranjera())
            {
                if (txtCambio.Text.Trim().Length >= 1)
                {
                    txtCambio.ClearValue(TextBox.BorderBrushProperty);
                } else
                {
                    txtCambio.BorderBrush = System.Windows.Media.Brushes.Red;
                }
            }
            if (!(selectedFormaPago is null) && selectedFormaPago.GetTipo() == TipoFormaDePago.CHEQUE)
            {
                if (DBBancos.GetByCode(SafeConvert.ToInt32(txtChequeNroBanco.Text.Trim())) is null)
                {
                    txtChequeNroBanco.BorderBrush = System.Windows.Media.Brushes.Red;
                } else
                {
                    txtChequeNroBanco.ClearValue(TextBox.BorderBrushProperty);
                }

                if (txtChequeNroSucursal.Text.Trim().Length >= 1)
                {
                    txtChequeNroSucursal.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeNroSucursal.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequeCuenta.Text.Trim().Length >= 1)
                {
                    txtChequeCuenta.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeCuenta.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequeCUIT.Text.Trim().Length >= 1)
                {
                    txtChequeCUIT.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeCUIT.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequeFechaPago.Text.IsValidDateTime())
                {
                    txtChequeFechaPago.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeFechaPago.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequeLocalidad.Text.Trim().Length >= 1)
                {
                    txtChequeLocalidad.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeLocalidad.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequeNumero.Text.Trim().Length >= 1)
                {
                    txtChequeNumero.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeNumero.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequePersona.Text.Trim().Length >= 1)
                {
                    txtChequePersona.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequePersona.BorderBrush = System.Windows.Media.Brushes.Red;
                }

                if (txtChequeSerie.Text.Trim().Length >= 1)
                {
                    txtChequeSerie.ClearValue(TextBox.BorderBrushProperty);
                }
                else
                {
                    txtChequeSerie.BorderBrush = System.Windows.Media.Brushes.Red;
                }
            }

            if (rdbReciboEmitido.IsChecked != true && txtEntidadRecibo.Text.Trim().Length < 1)
            {
                txtChequeSerie.BorderBrush = System.Windows.Media.Brushes.Red;
            } else
            {
                txtChequeSerie.ClearValue(TextBox.BorderBrushProperty);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            GetOwnerControl().winAgregarModificarPago.Visibility = Visibility.Collapsed;
        }

        private void cmbFormaDePago_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void cmbMoneda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeNroBanco_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            //Check that Recibo is valid
            DBRecibo reciboSeleccionado = null;

            if (rdbReciboEmitido.IsChecked == true)
            {
                reciboSeleccionado = DBRecibo.GetByNumber(dbCon.Connection, GetCuentaSeleccionada(), txtNroRecibo.Text.Trim(), true);
            } else
            {
                DBEntidades entidadSeleccionada = DBEntidades.GetByCUIT(dbCon.Connection, GetCuentaSeleccionada(), SafeConvert.ToInt64(txtEntidadRecibo.Text.Trim()));
                
                if (entidadSeleccionada is null)
                {
                    MessageBox.Show("El CUIT ingresado no corresponde a ninguna entidad comercial", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                } else
                {
                    reciboSeleccionado = DBRecibo.GetByNumber(dbCon.Connection, entidadSeleccionada, txtNroRecibo.Text.Trim(), false);
                }
            }

            if (reciboSeleccionado is null)
            {
                MessageBox.Show("El numero de recibo no corresponde a ningún recibo válido ingresado en el sistema.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            DBPago pagoSeleccionado = GetPagoSeleccionado();

            DateTime fechaEmitido = new DateTime();
            DateTime.TryParse(txtFecha.Text, out fechaEmitido);

            DBMoneda monedaSelected = cmbMoneda.SelectedItemAsMoneda();
            double cambio = 
                cmbMoneda.SelectedItemAsMoneda().IsExtranjera() ?
                SafeConvert.ToDouble(txtCambio.Text.Trim().Replace(".", ",")) : 
                1.0;


            ChequeData? pagoChequeData = null;

            DBFormasPago formaPagoSeleccionada = cmbFormaDePago.SelectedItemAsFormaPago();

            if (!(formaPagoSeleccionada is null) && formaPagoSeleccionada.GetTipo() == TipoFormaDePago.CHEQUE)
            {
                DBBancos bancoCheque = DBBancos.GetByCode(SafeConvert.ToInt32(txtChequeNroBanco.Text.Trim()));
                
                DateTime fechaChequePago = new DateTime();
                DateTime.TryParse(txtChequeFechaPago.Text, out fechaChequePago);

                DateTime fechaChequeDebitado = new DateTime();
                bool chequeFueDebitado = false;
                chequeFueDebitado = DateTime.TryParse(txtChequeFechaDebito.Text, out fechaChequeDebitado);

                pagoChequeData = new ChequeData(
                    (bancoCheque is null) ? -1 : bancoCheque.GetID(),
                    SafeConvert.ToInt32(txtChequeNroSucursal.Text.Trim()),
                    SafeConvert.ToInt64(txtChequeNumero.Text.Trim()),
                    txtChequeSerie.Text.Trim(),
                    txtChequePersona.Text.Trim(),
                    txtChequeCuenta.Text.Trim(),
                    fechaChequePago,
                    SafeConvert.ToInt32(txtChequeLocalidad.Text.Trim()),
                    chequeFueDebitado ? (DateTime?)fechaChequeDebitado : null,
                    SafeConvert.ToInt64(txtChequeCUIT.Text.Trim())
                    );
            }


            DBPago pagoToPush = new DBPago(
                reciboSeleccionado,
                cmbFormaDePago.SelectedItemAsFormaPago(),
                cmbMoneda.SelectedItemAsMoneda(),
                SafeConvert.ToDouble(txtImporte.Text.Trim()),
                txtObservacion.Text.Trim(),
                fechaEmitido,
                cambio,
                pagoChequeData);

            long oldEntidadId;
            long oldReciboId;

            if (pagoSeleccionado is null)
            {
                pagoSeleccionado = pagoToPush;
                oldEntidadId = pagoToPush.GetEntidadComercialID();
                oldReciboId = pagoToPush.GetReciboID();
            } else
            {
                oldEntidadId = pagoSeleccionado.GetEntidadComercialID();
                oldReciboId = pagoSeleccionado.GetReciboID();
                pagoSeleccionado.Update(pagoToPush);
            }

            if (pagoSeleccionado.PushToDatabase(dbCon.Connection, oldEntidadId, oldReciboId))
            {
                MessageBox.Show("¡Pago agregado a la base de datos correctamente!");
                GetOwnerControl().RefreshData();
                GetOwnerControl().winAgregarModificarPago.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Error al agregar el pago.");

            }

            //DBRecibo.GetByNumber(dbCon.Connection, )
        }

        private void txtNroRecibo_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtCambio_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtImporte_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtFecha_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeNroSucursal_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeLocalidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeSerie_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeNumero_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequePersona_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeCuenta_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeCUIT_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtChequeFechaPago_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtCambio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyMoney(e);
        }

        private void txtImporte_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyMoney(e);
        }

        private void txtFecha_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFecha);
        }

        private void txtChequeNroSucursal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtChequeLocalidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtChequeNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtChequeCUIT_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtChequeFechaPago_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtChequeFechaPago);
        }

        private void txtChequeFechaDebito_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtChequeFechaDebito);
        }

        private void txtChequeNroBanco_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void rdbReciboEmitido_Checked(object sender, RoutedEventArgs e)
        {
            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void rdbReciboRecibido_Checked(object sender, RoutedEventArgs e)
        {
            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void btnAutoCompletarEntidad_Click(object sender, RoutedEventArgs e)
        {
            RefreshFormData();
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtEntidadRecibo_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }
    }
}
