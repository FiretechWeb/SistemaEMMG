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
        private DBPago _pagoSelected = null;
        private DBRecibo _reciboSelected = null;
        private ChequeData? _dataCheque = null;

        public void SetUIOwner(UIPagos ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public ChequeData? GetChequeData() => _dataCheque;

        public void SetChequeData(ChequeData? cheque) => _dataCheque = cheque;

        public UIPagos GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        public UIRCheque()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }
        public void RefreshData()
        {
            //TODO: Set fields using GetChequeData()
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (GetOwnerControl() is null)
            {
                return;
            }
            SetChequeData(new ChequeData());  //TODO: Create new ChequeData from FORM

            GetOwnerControl().SetChequeData(GetChequeData());
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
    }
}
