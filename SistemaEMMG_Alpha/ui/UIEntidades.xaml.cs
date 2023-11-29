using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;


namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIEntidades.xaml
    /// </summary>
    public partial class UIEntidades : BaseUCClass
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;

        private DBEntidades _entidadSeleccionada =null;
        private List<DBEntidades> _listaEntidades = new List<DBEntidades>();


        public DBCuenta GetCuentaSeleccionada()
        {
            if (GetMainWindow() is null)
            {
                return null;
            }
            return GetMainWindow().GetCuentaSeleccionada();
        }

        public DBEntidades GetEntidadSeleccionada() => _entidadSeleccionada;

        public void SetEntidadSeleccionada(DBEntidades newEntidad)
        {
            _entidadSeleccionada = newEntidad;
            if (newEntidad is null)
            {
                lblEntidadSeleccionada.Content = "No hay entidad seleccionada";
                btnEliminarEntidad.IsEnabled = false;
                btnModificarEntidad.IsEnabled = false;
                btnComprobantes.IsEnabled = false;
                btnRemitos.IsEnabled = false;
                btnRecibos.IsEnabled = false;
            }
            else
            {
                lblEntidadSeleccionada.Content = _entidadSeleccionada.GetRazonSocial();
                btnEliminarEntidad.IsEnabled = true;
                btnModificarEntidad.IsEnabled = true;
                btnComprobantes.IsEnabled = true;
                btnRemitos.IsEnabled = true;
                btnRecibos.IsEnabled = true;
            }
        }

        public void RefreshData()
        {
            if (GetMainWindow() is null)
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

            if (!GetMainWindow().CheckDBConnection())
            {
                return;
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);
            _listaEntidades = DBEntidades.GetAll(dbCon.Connection, GetCuentaSeleccionada());
            listEntidadesComerciales.Items.Clear();
            listEntidadesComerciales.SelectedValuePath = "Key";
            listEntidadesComerciales.DisplayMemberPath = "Value";

            foreach (DBEntidades entidadComercial in _listaEntidades)
            {
                listEntidadesComerciales.Items.Add(new KeyValuePair<long, string>(entidadComercial.GetID(), $"{entidadComercial.GetCUIT()}: {entidadComercial.GetRazonSocial()}"));
            }

            if (_listaEntidades.Count > 1)
            {
                listEntidadesComerciales.SelectedIndex = 0;
                SetEntidadSeleccionada(DBEntidades.GetByID(_listaEntidades, GetCuentaSeleccionada(), ((KeyValuePair<long, string>)listEntidadesComerciales.SelectedItem).Key));
            } else
            {
                listEntidadesComerciales.SelectedIndex = -1;
                SetEntidadSeleccionada(null);
            }

            GetMainWindow().refreshTabItems();
        }

        public UIEntidades()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiAgregarModificarEntidad.SetUIOwner(this);
        }

        private void btnBuscarEntidad_Click(object sender, RoutedEventArgs e)
        {
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            List<DBEntidades> entidadesComercialesList = DBEntidades.Search(dbCon.Connection, GetCuentaSeleccionada(), txtFiltroBusqueda.Text);

            listEntidadesComerciales.Items.Clear();
            listEntidadesComerciales.SelectedValuePath = "Key";
            listEntidadesComerciales.DisplayMemberPath = "Value";

            foreach (DBEntidades entidad in entidadesComercialesList)
            {
                listEntidadesComerciales.Items.Add(new KeyValuePair<long, string>(entidad.GetID(), $"{entidad.GetCUIT()}: {entidad.GetRazonSocial()}"));
            }
        }

        private void listEntidadesComerciales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox cmbSender = sender as ListBox;
            if (cmbSender.SelectedItem is null)
            {
                return;
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            DBEntidades newEntidadSelected = DBEntidades.GetByID(_listaEntidades, GetCuentaSeleccionada(), ((KeyValuePair<long, string>)listEntidadesComerciales.SelectedItem).Key);
            if (newEntidadSelected is null)
            {
                return;
            }
            SetEntidadSeleccionada(newEntidadSelected);
        }

        private void btnAgregarEntidad_Click(object sender, RoutedEventArgs e)
        {
            uiAgregarModificarEntidad.RefreshData();
            uiAgregarModificarEntidad.Visibility = Visibility.Visible;
        }

        private void btnModificarEntidad_Click(object sender, RoutedEventArgs e)
        {
            if (_entidadSeleccionada is null)
            {
                return;
            }
            uiAgregarModificarEntidad.RefreshData(_entidadSeleccionada);
            uiAgregarModificarEntidad.Visibility = Visibility.Visible;
        }

        private void btnEliminarEntidad_Click(object sender, RoutedEventArgs e)
        {
            DBEntidades entidadAEliminar = GetEntidadSeleccionada();

            Trace.Assert(!(entidadAEliminar is null));

            MessageBoxResult msgBoxConfirmationResult = System.Windows.MessageBox.Show("¿Está seguro que desea eliminar está entidad comercial?, se eliminaran todos los comprobantes, recibos y remitos relacionados a esta entidad comercial.", "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo);
            if (msgBoxConfirmationResult == MessageBoxResult.Yes)
            {
                if (entidadAEliminar.DeleteFromDatabase(dbCon.Connection))
                {
                    MessageBox.Show("Entidad comercial eliminada exitosamente");
                    RefreshData();
                }
                else
                {
                    MessageBox.Show("Error tratando de eliminar la entidad comercial.");
                }
            }
        }

        private void btnComprobantes_Click(object sender, RoutedEventArgs e)
        {
            if (_entidadSeleccionada is null)
            {
                return;
            }
            GetMainWindow().uiComprobantespanel.textFiltroCUIT.Text = _entidadSeleccionada.GetCUIT().ToString();
            GetMainWindow().uiComprobantespanel.RefreshData();
            Dispatcher.BeginInvoke((Action)(() => GetMainWindow().tabControlMain.SelectedIndex = 2));
        }

        private void btnRecibos_Click(object sender, RoutedEventArgs e)
        {
            if (_entidadSeleccionada is null)
            {
                return;
            }
            GetMainWindow().uiRecibosPanel.textFiltroCUIT.Text = _entidadSeleccionada.GetCUIT().ToString();
            GetMainWindow().uiRecibosPanel.RefreshData();
            Dispatcher.BeginInvoke((Action)(() => GetMainWindow().tabControlMain.SelectedIndex = 3));
        }

        private void btnRemitos_Click(object sender, RoutedEventArgs e)
        {
            if (_entidadSeleccionada is null)
            {
                return;
            }
            GetMainWindow().uiRemitosPanel.textFiltroCUIT.Text = _entidadSeleccionada.GetCUIT().ToString();
            GetMainWindow().uiRemitosPanel.RefreshData();
            Dispatcher.BeginInvoke((Action)(() => GetMainWindow().tabControlMain.SelectedIndex = 4));
        }
    }
}
