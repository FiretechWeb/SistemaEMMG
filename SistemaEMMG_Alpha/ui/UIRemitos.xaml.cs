using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIRemitos.xaml
    /// </summary>
    public partial class UIRemitos : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private MainWindow _mainWin = null;
        private List<DBRemito> _listaRemitos = null;
        private DBRemito _remitoSeleccionado = null;
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

        public MainWindow GetMainWindow() => _mainWin;


        public DBRemito GetRemitoSeleccionado() => _remitoSeleccionado;

        public void SetRemitoSeleccionado(DBRemito newRemito)
        {
            _remitoSeleccionado = newRemito;
            if (newRemito is null)
            {
                lblRemitoSeleccionado.Content = "No hay remito seleccionado";
                btnModificar.IsEnabled = false;
                btnEliminar.IsEnabled = false;
            }
            else
            {
                lblRemitoSeleccionado.Content = _remitoSeleccionado.GetNumero();
                btnModificar.IsEnabled = true;
                btnEliminar.IsEnabled = true;
            }
        }


        public UIRemitos()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiAgregarModificarPanel.SetUIOwner(this);
        }

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
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            if (!_mainWin.CheckDBConnection())
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);

            List<DBTipoEntidad> listaTipoEntidades = DBTipoEntidad.GetAll();
            List<DBTipoRemito> listaTipoRemitos = DBTipoRemito.GetAll();

            cmbFiltroTipoEnt.Items.Clear();
            cmbFiltroTipoEnt.SelectedValuePath = "Key";
            cmbFiltroTipoEnt.DisplayMemberPath = "Value";
            cmbFiltroTipoEnt.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach (DBTipoEntidad tipoEntidad in listaTipoEntidades)
            {
                cmbFiltroTipoEnt.Items.Add(new KeyValuePair<long, string>(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }

            cmbFiltroTipoRemito.Items.Clear();
            cmbFiltroTipoRemito.SelectedValuePath = "Key";
            cmbFiltroTipoRemito.DisplayMemberPath = "Value";
            cmbFiltroTipoRemito.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach (DBTipoRemito tipoRemito in listaTipoRemitos)
            {
                cmbFiltroTipoRemito.Items.Add(new KeyValuePair<long, string>(tipoRemito.GetID(), tipoRemito.GetName()));
            }

            cmbFiltroTipoEnt.SelectedValue = -1;
            cmbFiltroTipoRemito.SelectedValue = -1;

            refreshListWithFilter();

        }

        private void refreshListWithFilter()
        {
            DateTime _fechaInicial = new DateTime();
            DateTime? fechaInicial = null;

            if (DateTime.TryParse(txtFiltroFechaInicial.Text, out _fechaInicial))
            {
                fechaInicial = _fechaInicial;
            }
            DateTime _fechaFinal = new DateTime();
            DateTime? fechaFinal = null;

            if (DateTime.TryParse(txtFiltroFechaFinal.Text, out _fechaFinal))
            {
                fechaFinal = _fechaFinal;
            }

            _listaRemitos = DBRemito.Search(
                dbCon.Connection,
                GetCuentaSeleccionada(),
                cmbFiltroEstado.SelectedIndex,
                fechaInicial,
                fechaFinal,
                SafeConvert.ToInt64(textFiltroCUIT.Text.Trim()),
                ((KeyValuePair<long, string>)cmbFiltroTipoRemito.SelectedItem).Key,
                ((KeyValuePair<long, string>)cmbFiltroTipoEnt.SelectedItem).Key,
                txtNumeroFiltro.Text.Trim());

            dgRemitos.Items.Clear();

            foreach (DBRemito remito in _listaRemitos)
            {
                dgRemitos.Items.Add(new
                {
                    rm_id = remito.GetID(),
                    rm_ec_id = remito.GetEntidadComercialID(),
                    emitido = remito.IsEmitido() ? "Emitido" : "Recibido",
                    fecha = remito.GetFecha().HasValue ? ((DateTime)remito.GetFecha()).ToString("dd-MM-yyyy") : "Sin fecha",
                    cuit = remito.GetEntidadComercial().GetCUIT(),
                    tipo = remito.GetTipoRemito().GetName(),
                    numero = remito.GetNumero(),
                    razon = remito.GetEntidadComercial().GetRazonSocial()
                });
            }
            SetRemitoSeleccionado(null);
        }


        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            refreshListWithFilter();
        }

        private void dgRemitos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_mainWin is null)
            {
                return;
            }
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            if (e.Source is DataGrid)
            {
                DataGrid senderDG = sender as DataGrid;
                //Todo: dynamic it's a terrible idea, find a way to make this work without that at all.
                dynamic items = senderDG.SelectedItem;
                if (senderDG.SelectedItem is null)
                {
                    return;
                }
                try
                {
                    int rm_id = Convert.ToInt32(items.rm_id); //IMPORTANT NOTE: dynamic values does not work at all with the self-made SafeConvert static class!!
                    int rm_ec_id = Convert.ToInt32(items.rm_ec_id); //IMPORTANT NOTE: dynamic values does not work at all with the self-made SafeConvert static class!!

                    SetRemitoSeleccionado(DBRemito.GetByID(dbCon.Connection, GetCuentaSeleccionada(), rm_ec_id, rm_id));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error with Dynamic...");
                }
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            DBRemito remitoAEliminar = GetRemitoSeleccionado();

            Trace.Assert(!(remitoAEliminar is null));

            MessageBoxResult msgBoxConfirmationResult = System.Windows.MessageBox.Show("¿Está seguro que desea eliminar este remito?, todos los comprobantes contengan a este remito van a eliminarlo de sus listas.", "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo);
            if (msgBoxConfirmationResult == MessageBoxResult.Yes)
            {
                if (remitoAEliminar.DeleteFromDatabase(dbCon.Connection))
                {
                    MessageBox.Show("Remito eliminado exitosamente");
                    RefreshData();
                }
                else
                {
                    MessageBox.Show("Error tratando de eliminar el remito.");
                }
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            uiAgregarModificarPanel.RefreshData();
            uiAgregarModificarPanel.Visibility = Visibility.Visible;
        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (GetRemitoSeleccionado() is null)
            {
                return;
            }
            uiAgregarModificarPanel.RefreshData(GetRemitoSeleccionado());
            uiAgregarModificarPanel.Visibility = Visibility.Visible;
        }
    }
}
