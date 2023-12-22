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
    /// Interaction logic for UIMissTipoComprobante.xaml
    /// </summary>
    public partial class UIMissTipoComprobante : UserControl
    {

        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIFixMissing _ownerControl = null;
        private DBTiposComprobantes _tipoComprobante = null;

        public void SetUIOwner(UIFixMissing ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIFixMissing GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }

        private void SetTipoComprobanteSeleccionado(DBTiposComprobantes tipoComprobante)
        {
            _tipoComprobante = tipoComprobante;
        }

        private DBTiposComprobantes GetTipoComprobanteSeleccionado() => _tipoComprobante;

        public UIMissTipoComprobante()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }
        public void RefreshData(DBTiposComprobantes tipoComprobante)
        {
            if (_ownerControl is null)
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

            SetTipoComprobanteSeleccionado(tipoComprobante);

            dbData.RefreshBasicDataDB(dbCon.Connection);
            List<DBTiposComprobantes> tipoComprobantes = DBTiposComprobantes.GetAll();

            cmbTipoComprobante.Items.Clear();
            cmbTipoComprobante.SelectedValuePath = "Key";
            cmbTipoComprobante.DisplayMemberPath = "Value";
            cmbTipoComprobante.Items.Add(new KeyValuePair<long, string>(-1, "Nuevo Tipo Comprobante"));

            foreach (DBTiposComprobantes tc in tipoComprobantes)
            {
                cmbTipoComprobante.Items.Add(new KeyValuePair<long, string>(tc.GetID(), tc.GetName()));
            }

            cmbTipoComprobante.SelectedIndex = 0;
            txtMissingTipoComprobante.Text = tipoComprobante.GetName();
        }

        public DBTiposComprobantes GetTipoComprobante()
        {
            DBTiposComprobantes tmpComprobante = cmbTipoComprobante.SelectedItemAsTipoComprobante();
            if (tmpComprobante is null)
            {
                tmpComprobante = GetTipoComprobanteSeleccionado();
                tmpComprobante.SetName(txtMissingTipoComprobante.Text.Trim());
                tmpComprobante.SetFlags((int)TipoComprobanteFlag.Gravado | (int)TipoComprobanteFlag.IVA | (int)TipoComprobanteFlag.NoGravado | (int)TipoComprobanteFlag.Percepcion | (int)TipoComprobanteFlag.Acredita);
            }
            else
            {
                foreach (DBTipoComprobanteAlias alias in GetTipoComprobanteSeleccionado().GetAliases())
                {
                    tmpComprobante.AddAlias(alias.GetAlias());
                }
            }
            return tmpComprobante;

        }

        private void cmbTipoComprobante_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetOwnerControl() is null)
            {
                return;
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            txtMissingTipoComprobante.IsEnabled = (cmbTipoComprobante.SelectedItemAsTipoComprobante() is null);
        }
    }
}
