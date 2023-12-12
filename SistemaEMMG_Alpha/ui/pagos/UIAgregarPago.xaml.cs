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
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
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

            DBBancos bancoIntroducido = DBBancos.GetByCode(SafeConvert.ToInt32(txtChequeNroBanco.Text.Trim()));
            if (bancoIntroducido is null)
            {
                lblChequeNombreBanco.Content = "Inválido";
            } else
            {
                lblChequeNombreBanco.Content = bancoIntroducido.GetName();
            }
        }

        private void CheckIfAbleToSubmit()
        {
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

            btnAceptar.IsEnabled = true;
        }

        private void RefreshFieldsColorState()
        {
            
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
    }
}
