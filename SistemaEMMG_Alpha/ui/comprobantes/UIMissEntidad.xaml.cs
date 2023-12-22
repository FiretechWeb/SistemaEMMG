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
    /// Interaction logic for UIMissEntidad.xaml
    /// </summary>
    public partial class UIMissEntidad : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private UIFixMissing _ownerControl = null;
        private DBEntidades _entidadSeleccionada = null;

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

        private void SetEntidadSeleccionada(DBEntidades entidad)
        {
            _entidadSeleccionada = entidad;
        }

        private DBEntidades GetEntidadSeleccionada() => _entidadSeleccionada;

        public UIMissEntidad()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
        }

        public void RefreshData(DBEntidades entidadComercial)
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

            SetEntidadSeleccionada(entidadComercial);
            dbData.RefreshBasicDataDB(dbCon.Connection);

            List<DBTipoEntidad> tipoEntidades = DBTipoEntidad.GetAll();


            cmbTipoEntidad.Items.Clear();
            cmbTipoEntidad.SelectedValuePath = "Key";
            cmbTipoEntidad.DisplayMemberPath = "Value";

            foreach (DBTipoEntidad tipoEntidad in tipoEntidades)
            {
                cmbTipoEntidad.Items.Add(new KeyValuePair<long, string>(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }

            cmbTipoEntidad.SelectedValue = GetEntidadSeleccionada().GetTipoEntidad().GetID();
            txtMissingCel.Text = GetEntidadSeleccionada().GetCelular();
            txtMissingRazon.Text = GetEntidadSeleccionada().GetRazonSocial();
            lblMissingCUIT.Content = GetEntidadSeleccionada().GetCUIT().ToString();
            txtMissingTelefono.Text = GetEntidadSeleccionada().GetTelefono();
        }

        public bool IsAbleToSubmit()
        {
            if (txtMissingRazon.Text.Trim().Length == 0) return false;

            return true;
        }

        public DBEntidades GetEntidad()
        {
            GetEntidadSeleccionada().SetCelular(txtMissingCel.Text.Trim());
            GetEntidadSeleccionada().SetEmail(txtMissingEmail.Text.Trim());
            GetEntidadSeleccionada().SetRazonSocial(txtMissingRazon.Text.Trim());
            GetEntidadSeleccionada().SetTelefono(txtMissingTelefono.Text.Trim());
            GetEntidadSeleccionada().SetTipoEntidad(cmbTipoEntidad.SelectedItemAsTipoEntidad());

            return GetEntidadSeleccionada();

        }
    }
}
