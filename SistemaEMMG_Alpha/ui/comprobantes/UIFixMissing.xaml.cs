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
    /// Interaction logic for UIFixMissing.xaml
    /// </summary>
    public partial class UIFixMissing : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private List<DBBaseClass> _missingElements = null;
        private UIComprobantes _ownerControl = null;

        public void SetUIOwner(UIComprobantes ownerControl)
        {
            _ownerControl = ownerControl;
        }

        public UIComprobantes GetOwnerControl() => _ownerControl;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_ownerControl is null)
            {
                return null;
            }
            return _ownerControl.GetCuentaSeleccionada();
        }
        public UIFixMissing()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiMissingEntidad.SetUIOwner(this);
            uiMissingTipoComprobante.SetUIOwner(this);
        }

        public void SetMissingElements(List<DBBaseClass> missing)
        {
            _missingElements = missing;
        }

        public List<DBBaseClass> GetMissingElements() => _missingElements;

        public void RefreshData(List<DBBaseClass> missing)
        {
            if (missing.Count == 0) return;

            SetMissingElements(missing);

            lblPendientes.Content = $"Problemas pendientes: {missing.Count}";
            if (missing[0] is DBEntidades)
            {
                uiMissingEntidad.RefreshData((DBEntidades)missing[0]);
                uiMissingEntidad.Visibility = Visibility.Visible;
                uiMissingTipoComprobante.Visibility = Visibility.Collapsed;
            } else if (missing[0] is DBTiposComprobantes) {
                uiMissingTipoComprobante.RefreshData((DBTiposComprobantes)missing[0]);
                uiMissingEntidad.Visibility = Visibility.Collapsed;
                uiMissingTipoComprobante.Visibility = Visibility.Visible;
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            GetOwnerControl().uiFixMissing.Visibility = Visibility.Collapsed;
        }

        private bool CheckIfAbleToSubmit()
        {
            if (GetMissingElements().Count == 0) return false;

            if (GetMissingElements()[0] is DBEntidades)
            {
                return uiMissingEntidad.IsAbleToSubmit();
            }

            return true;
        }

        private void btnContinuar_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIfAbleToSubmit()) return;

            if (GetMissingElements()[0] is DBTiposComprobantes)
            {
                GetMissingElements()[0] = uiMissingTipoComprobante.GetTipoComprobante();
            }
            else if (GetMissingElements()[0] is DBEntidades)
            {
                GetMissingElements()[0] = uiMissingEntidad.GetEntidad();
            }

            GetMissingElements()[0].PushToDatabase(dbCon.Connection);

            if (GetMissingElements()[0] is DBTiposComprobantes)
            {
                ((DBTiposComprobantes)GetMissingElements()[0]).PushAliases(dbCon.Connection);
            }

            List<DBBaseClass> nextList = new List<DBBaseClass>();
            for (int i=1; i < GetMissingElements().Count; i++)
            {
                nextList.Add(GetMissingElements()[i]);
            }
            if (nextList.Count == 0)
            {
                GetOwnerControl().uiFixMissing.Visibility = Visibility.Collapsed;
                return;
            }
            RefreshData(nextList);
        }
    }
}
