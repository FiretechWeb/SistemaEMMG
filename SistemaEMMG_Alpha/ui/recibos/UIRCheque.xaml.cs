using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha.ui.recibos
{

    /// <summary>
    /// Interaction logic for UIRCheque.xaml
    /// </summary>
    public partial class UIRCheque : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIPagos _ownerControl = null;

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

        

        public void CheckIfAbleToSubmit()
        {
            if (!txtFechaPago.Text.IsValidDateTime())
            {
                btnGuardar.IsEnabled = false;
                return;
            }
            if (DBBancos.GetByCode(SafeConvert.ToInt32(txtBankCode.Text.Trim())) is null)
            {
                btnGuardar.IsEnabled = false;
                return;
            } 

            if (txtCuenta.Text.Trim().Length < 2)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            if (txtCUIT.Text.Trim().Length < 3)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            if (txtEntidadPersona.Text.Trim().Length < 3)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            if (txtLocalidad.Text.Trim().Length < 1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            if (txtSucursal.Text.Trim().Length < 1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            if (txtSerie.Text.Trim().Length < 1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            if (txtNumCheque.Text.Trim().Length < 1)
            {
                btnGuardar.IsEnabled = false;
                return;
            }

            btnGuardar.IsEnabled = true;
        }

        private void RefreshFieldsColorState()
        {
            if (txtFechaPago.Text.IsValidDateTime())
            {
                txtFechaPago.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtFechaPago.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (!(DBBancos.GetByCode(SafeConvert.ToInt32(txtBankCode.Text.Trim())) is null))
            {
                txtBankCode.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtBankCode.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtCuenta.Text.Trim().Length >= 2)
            {
                txtCuenta.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtCuenta.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtCUIT.Text.Trim().Length >= 3)
            {
                txtCUIT.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtCUIT.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtEntidadPersona.Text.Trim().Length >= 3)
            {
                txtEntidadPersona.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtEntidadPersona.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtLocalidad.Text.Trim().Length >= 1)
            {
                txtLocalidad.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtLocalidad.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtSucursal.Text.Trim().Length >= 1)
            {
                txtSucursal.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtSucursal.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtSerie.Text.Trim().Length >= 1)
            {
                txtSerie.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtSerie.BorderBrush = System.Windows.Media.Brushes.Red;
            }

            if (txtNumCheque.Text.Trim().Length >= 1)
            {
                txtNumCheque.ClearValue(TextBox.BorderBrushProperty);
            }
            else
            {
                txtNumCheque.BorderBrush = System.Windows.Media.Brushes.Red;
            }
        }

        public UIRCheque()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }
        public void RefreshData(ChequeData? cheque)
        {
            if (cheque.HasValue)
            {
                ChequeData currentCheque = (ChequeData)cheque;
                DBBancos banco = DBBancos.GetByID(currentCheque.pg_bnk_id);
                if (banco is null)
                {
                    txtBankCode.Text = "00000";
                    lblBankName.Content = "Código de banco inválido.";

                } else {
                    txtBankCode.Text = banco.GetCodeWithFormat();
                    lblBankName.Content = banco.GetName();
                }
                txtNumCheque.Text = currentCheque.pg_cheque_num.ToString("00000000");
                txtCuenta.Text = currentCheque.pg_cheque_cta;
                txtCUIT.Text = currentCheque.pg_cheque_cuit.ToString(); //.ToString("dd/MM/yyyy")
                txtFechaDebitado.Text = currentCheque.pg_cheque_debito.HasValue ? ((DateTime)currentCheque.pg_cheque_debito).ToString("dd/MM/yyyy") : "";
                txtFechaPago.Text = currentCheque.pg_cheque_pay.HasValue ? ((DateTime)currentCheque.pg_cheque_pay).ToString("dd/MM/yyyy") : "";
                txtLocalidad.Text = currentCheque.pg_cheque_localidad.ToString();
                txtSerie.Text = currentCheque.pg_cheque_serie;
                txtEntidadPersona.Text = currentCheque.pg_cheque_persona;
                txtSucursal.Text = currentCheque.pg_cheque_sucursal.ToString();
            } else
            {
                txtNumCheque.Text = "";
                txtCuenta.Text = "";
                txtCUIT.Text = "";
                txtFechaDebitado.Text = "";
                txtFechaPago.Text = "";
                txtLocalidad.Text = "";
                txtSerie.Text = "";
                txtEntidadPersona.Text = "";
                txtSucursal.Text = "";
                lblBankName.Content = "Introduzca un código.";
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (GetOwnerControl() is null)
            {
                return;
            }

            DateTime fechaChequePago = new DateTime();
            DateTime.TryParse(txtFechaPago.Text, out fechaChequePago);

            DateTime fechaChequeDebitado = new DateTime();
            bool chequeFueDebitado = false;
            chequeFueDebitado = DateTime.TryParse(txtFechaDebitado.Text, out fechaChequeDebitado);

            DBBancos banco = DBBancos.GetByCode(SafeConvert.ToInt32(txtBankCode.Text.Trim()));

            GetOwnerControl().SetChequeData(new ChequeData(
                  (banco is null) ? -1 : banco.GetID(),
                  SafeConvert.ToInt32(txtSucursal.Text.Trim()),
                  SafeConvert.ToInt64(txtNumCheque.Text.Trim()),
                  txtSerie.Text.Trim(),
                  txtEntidadPersona.Text.Trim(),
                  txtCuenta.Text.Trim(),
                  fechaChequePago,
                  SafeConvert.ToInt32(txtLocalidad.Text.Trim()),
                  chequeFueDebitado ? (DateTime?)fechaChequeDebitado : null,
                  SafeConvert.ToInt64(txtCUIT.Text.Trim())
                ));

            GetOwnerControl().uiChequeInfo.Visibility = Visibility.Collapsed;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (GetOwnerControl() is null)
            {
                return;
            }

            GetOwnerControl().uiChequeInfo.Visibility = Visibility.Collapsed;
        }

        private void txtBankCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            DBBancos banco = DBBancos.GetByCode(SafeConvert.ToInt32(txtBankCode.Text));
            if (banco is null)
            {
                lblBankName.Content = "Código de banco inválido.";
            }
            else
            {
                lblBankName.Content = banco.GetName();
            }

            CheckIfAbleToSubmit();
        }

        private void txtFechaPago_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFechaPago);
        }

        private void txtFechaDebitado_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFechaDebitado);
        }

        private void txtSucursal_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtBankCode_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtLocalidad_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtCUIT_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }

        private void txtSucursal_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtLocalidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtSerie_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtNumCheque_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtEntidadPersona_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtCuenta_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtCUIT_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtFechaPago_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }

        private void txtFechaDebitado_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfAbleToSubmit();
            RefreshFieldsColorState();
        }
    }
}
