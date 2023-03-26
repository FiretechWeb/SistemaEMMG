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

namespace SistemaEMMG_Alpha.ui.comprobantes
{
    /// <summary>
    /// Interaction logic for UIRecibo.xaml
    /// </summary>
    public partial class UIRecibo : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIAgregarModificarComprobante _ownerControl = null;
        private DBComprobantes _comprobanteSeleccionado = null;

        public void SetUIOwner(UIAgregarModificarComprobante ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIAgregarModificarComprobante GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }


        public UIRecibo()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        public void RefreshData(DBComprobantes selectedComprobante)
        {
            _comprobanteSeleccionado = selectedComprobante;
        }
    }
}
