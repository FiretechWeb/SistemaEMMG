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
using System.Diagnostics;


namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIEntidades.xaml
    /// </summary>
    public partial class UIEntidades : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;

        private DBEntidades _entidadSeleccionada =null;
        private List<DBEntidades> _listaEntidades = new List<DBEntidades>();
        private MainWindow _mainWin = null;

        public void SetMainWindow(MainWindow mainWin)
        {
            _mainWin = mainWin;
        }

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_mainWin is null)
            {
                return null;
            }
            return _mainWin.GetCuentaSeleccionada();
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
            }
            else
            {
                lblEntidadSeleccionada.Content = _entidadSeleccionada.GetRazonSocial();
                btnEliminarEntidad.IsEnabled = true;
                btnModificarEntidad.IsEnabled = true;
            }
        }

        public MainWindow GetMainWindow() => _mainWin;

        public void RefreshData()
        {
            if (_mainWin is null)
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

            if (!_mainWin.CheckDBConnection())
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
    }
}
