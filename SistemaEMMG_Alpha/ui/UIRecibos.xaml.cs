using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIRecibos.xaml
    /// </summary>
    public partial class UIRecibos : BaseUCClass
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private List<DBRecibo> _listaRecibos = null;
        private DBRecibo _reciboSeleccionado = null;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (GetMainWindow() is null)
            {
                return null;
            }
            return GetMainWindow().GetCuentaSeleccionada();
        }


        public DBRecibo GetReciboSeleccionado() => _reciboSeleccionado;

        public void SetReciboSeleccionado(DBRecibo newRecibo)
        {
            _reciboSeleccionado = newRecibo;
            if (newRecibo is null)
            {
                lblReciboSeleccionado.Content = "No hay recibo seleccionado";
                btnModificar.IsEnabled = false;
                btnEliminar.IsEnabled = false;
            }
            else
            {
                lblReciboSeleccionado.Content = _reciboSeleccionado.GetNumero();
                btnModificar.IsEnabled = true;
                btnEliminar.IsEnabled = true;
            }
        }

        public UIRecibos()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiAgregarModificarReciboPanel.SetUIOwner(this);
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
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            if (!GetMainWindow().CheckDBConnection())
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);

            List<DBTipoEntidad> listaTipoEntidades = DBTipoEntidad.GetAll();
            List<DBTipoRecibo> listaTipoRecibos = DBTipoRecibo.GetAll();

            cmbFiltroTipoEnt.Items.Clear();
            cmbFiltroTipoEnt.SelectedValuePath = "Key";
            cmbFiltroTipoEnt.DisplayMemberPath = "Value";
            cmbFiltroTipoEnt.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach (DBTipoEntidad tipoEntidad in listaTipoEntidades)
            {
                cmbFiltroTipoEnt.Items.Add(new KeyValuePair<long, string>(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }

            cmbFiltroTipoRecibo.Items.Clear();
            cmbFiltroTipoRecibo.SelectedValuePath = "Key";
            cmbFiltroTipoRecibo.DisplayMemberPath = "Value";
            cmbFiltroTipoRecibo.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach (DBTipoRecibo tipoRecibo in listaTipoRecibos)
            {
                cmbFiltroTipoRecibo.Items.Add(new KeyValuePair<long, string>(tipoRecibo.GetID(), tipoRecibo.GetName()));
            }

            cmbFiltroTipoEnt.SelectedValue = -1;
            cmbFiltroTipoRecibo.SelectedValue = -1;

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

            Console.WriteLine($"Fecha parsed: {fechaFinal}");

            _listaRecibos = DBRecibo.Search(
                dbCon.Connection,
                GetCuentaSeleccionada(),
                cmbFiltroEstado.SelectedIndex,
                fechaInicial,
                fechaFinal,
                SafeConvert.ToInt64(textFiltroCUIT.Text.Trim()),
                ((KeyValuePair<long, string>)cmbFiltroTipoRecibo.SelectedItem).Key,
                ((KeyValuePair<long, string>)cmbFiltroTipoEnt.SelectedItem).Key,
                txtNumeroFiltro.Text.Trim());

            dgRecibos.Items.Clear();

            foreach (DBRecibo recibo in _listaRecibos)
            {
                dgRecibos.Items.Add(new
                {
                    rc_id = recibo.GetID(),
                    rc_ec_id = recibo.GetEntidadComercialID(),
                    emitido = recibo.IsEmitido() ? "Emitido" : "Recibido",
                    fecha = recibo.GetFecha().HasValue ? ((DateTime)recibo.GetFecha()).ToString("dd-MM-yyyy") : "Sin fecha",
                    cuit = recibo.GetEntidadComercial().GetCUIT(),
                    tipo = recibo.GetTipoRecibo().GetName(),
                    numero = recibo.GetNumero(),
                    razon = recibo.GetEntidadComercial().GetRazonSocial(),
                    importe = $"{recibo.GetComprobantesTotal_MonedaLocal(dbCon.Connection).ToString("0.00")} ARS",
                    pagado = $"{recibo.GetPagosTotal_MonedaLocal(dbCon.Connection).ToString("0.00")} ARS"
                });
            }
            SetReciboSeleccionado(null);
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            refreshListWithFilter();
        }

        private void dgRecibos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GetMainWindow() is null)
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
                    int rc_id = Convert.ToInt32(items.rc_id); //IMPORTANT NOTE: dynamic values does not work at all with the self-made SafeConvert static class!!
                    int rc_ec_id = Convert.ToInt32(items.rc_ec_id); //IMPORTANT NOTE: dynamic values does not work at all with the self-made SafeConvert static class!!

                    SetReciboSeleccionado(DBRecibo.GetByID(dbCon.Connection, GetCuentaSeleccionada(), rc_ec_id, rc_id));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error with Dynamic...");
                }
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            DBRecibo reciboAEliminar = GetReciboSeleccionado();

            Trace.Assert(!(reciboAEliminar is null));

            MessageBoxResult msgBoxConfirmationResult = System.Windows.MessageBox.Show("¿Está seguro que desea eliminar este recibo?, todos los comprobantes contengan a este recibo van a eliminarlo de sus listas.", "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo);
            if (msgBoxConfirmationResult == MessageBoxResult.Yes)
            {
                if (reciboAEliminar.DeleteFromDatabase(dbCon.Connection))
                {
                    MessageBox.Show("Recibo eliminado exitosamente");
                    RefreshData();
                }
                else
                {
                    MessageBox.Show("Error tratando de eliminar el recibo.");
                }
            }
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            uiAgregarModificarReciboPanel.PrepararParaAgregar();
            uiAgregarModificarReciboPanel.RefreshData();
            uiAgregarModificarReciboPanel.Visibility = Visibility.Visible;
        }

        private void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (GetReciboSeleccionado() is null)
            {
                return;
            }
            uiAgregarModificarReciboPanel.PrepararParaModificar();
            uiAgregarModificarReciboPanel.RefreshData(GetReciboSeleccionado());
            uiAgregarModificarReciboPanel.Visibility = Visibility.Visible;
        }

        private void txtFiltroFechaInicial_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFiltroFechaInicial);
        }

        private void txtFiltroFechaFinal_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyDateTimeText(e, txtFiltroFechaFinal);
        }

        private void textFiltroCUIT_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = InputHandler.OnlyNumbers(e);
        }
    }
}
