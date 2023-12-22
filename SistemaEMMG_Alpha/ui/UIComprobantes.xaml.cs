using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIComprobantes.xaml
    /// </summary>
    public partial class UIComprobantes : BaseUCClass
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private List<DBComprobantes> _listaComprobantes = null;
        private DBComprobantes _comprobanteSeleccionado = null;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (GetMainWindow() is null)
            {
                return null;
            }
            return GetMainWindow().GetCuentaSeleccionada();
        }

        public DBComprobantes GetComprobanteSeleccionado() => _comprobanteSeleccionado;

        public void SetComprobanteSeleccionado(DBComprobantes newComprobante)
        {
            _comprobanteSeleccionado = newComprobante;
            if (newComprobante is null)
            {
                lblComprobanteSeleccionado.Content = "No hay comprobante seleccionado";
                btnModificar.IsEnabled = false;
                btnEliminar.IsEnabled = false;
            }
            else
            {
                lblComprobanteSeleccionado.Content = _comprobanteSeleccionado.GetNumeroComprobante();
                btnModificar.IsEnabled = true;
                btnEliminar.IsEnabled = true;
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
            List<DBTiposComprobantes> listaTipoComprobantes = DBTiposComprobantes.GetAll();

            cmbFiltroTipoEnt.Items.Clear();
            cmbFiltroTipoEnt.SelectedValuePath = "Key";
            cmbFiltroTipoEnt.DisplayMemberPath = "Value";
            cmbFiltroTipoEnt.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach (DBTipoEntidad tipoEntidad in listaTipoEntidades)
            {
                cmbFiltroTipoEnt.Items.Add(new KeyValuePair<long, string>(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }

            cmbFiltroTipoComp.Items.Clear();
            cmbFiltroTipoComp.SelectedValuePath = "Key";
            cmbFiltroTipoComp.DisplayMemberPath = "Value";
            cmbFiltroTipoComp.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach (DBTiposComprobantes tipoComprobante in listaTipoComprobantes)
            {
                cmbFiltroTipoComp.Items.Add(new KeyValuePair<long, string>(tipoComprobante.GetID(), tipoComprobante.GetName()));
            }

            cmbFiltroTipoEnt.SelectedValue = -1;
            cmbFiltroTipoComp.SelectedValue = -1;

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

            _listaComprobantes = DBComprobantes.Search(
                dbCon.Connection,
                GetCuentaSeleccionada(),
                cmbFiltroEstado.SelectedIndex,
                fechaInicial,
                fechaFinal,
                SafeConvert.ToInt64(textFiltroCUIT.Text.Trim()),
                cmbFiltroEstadoPago.SelectedIndex,
                ((KeyValuePair<long, string>)cmbFiltroTipoComp.SelectedItem).Key,
                ((KeyValuePair<long, string>)cmbFiltroTipoEnt.SelectedItem).Key, 
                txtNumeroFiltro.Text.Trim());

            dgComprobantes.Items.Clear();

            foreach (DBComprobantes comprobante in _listaComprobantes)
            {
                dgComprobantes.Items.Add(new
                {
                    cm_id = comprobante.GetID(),
                    cm_ec_id = comprobante.GetEntidadComercialID(),
                    emitido = comprobante.IsEmitido() ? "Emitido" : "Recibido",
                    fecha = comprobante.GetFechaEmitido().HasValue ? ((DateTime)comprobante.GetFechaEmitido()).ToString("dd-MM-yyyy") : "Sin fecha",
                    cuit = comprobante.GetEntidadComercial().GetCUIT(),
                    tipo = comprobante.GetTipoComprobante().GetName(),
                    numero = comprobante.GetNumeroComprobante(),
                    razon = comprobante.GetEntidadComercial().GetRazonSocial(),
                    gravado = $"{comprobante.GetGravado().ToString("0.00")} {comprobante.GetMoneda().GetName()}",
                    iva = $"{comprobante.GetIVA().ToString("0.00")} {comprobante.GetMoneda().GetName()}",
                    no_gravado = $"{comprobante.GetNoGravado().ToString("0.00")} {comprobante.GetMoneda().GetName()}",
                    percepcion = $"{comprobante.GetPercepcion().ToString("0.00")} {comprobante.GetMoneda().GetName()}",
                    op_extentas = $"{comprobante.GetOpExtentas().ToString("0.00")} {comprobante.GetMoneda().GetName()}",
                    otros_tributos = $"{comprobante.GetOtrosTributos().ToString("0.00")} {comprobante.GetMoneda().GetName()}",
                    total = $"{comprobante.GetTotal_MonedaLocal().ToString("0.00")} ARS"
                });
            }
            SetComprobanteSeleccionado(null);
        }
        public UIComprobantes()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiAgregarModificarPanel.SetUIOwner(this);
            uiFixMissing.SetUIOwner(this);
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            refreshListWithFilter();
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel file (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                ExcelExport.ExportToFile(_listaComprobantes, saveFileDialog.FileName);
            }
        }

        private void dgComprobantes_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    int cm_id = Convert.ToInt32(items.cm_id); //IMPORTANT NOTE: dynamic values does not work at all with the self-made SafeConvert static class!!
                    int cm_ec_id = Convert.ToInt32(items.cm_ec_id); //IMPORTANT NOTE: dynamic values does not work at all with the self-made SafeConvert static class!!

                    SetComprobanteSeleccionado(DBComprobantes.GetByID(dbCon.Connection, GetCuentaSeleccionada(), cm_ec_id, cm_id));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error with Dynamic...");
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
            if (GetComprobanteSeleccionado() is null)
            {
                return;
            }
            uiAgregarModificarPanel.RefreshData(GetComprobanteSeleccionado());
            uiAgregarModificarPanel.Visibility = Visibility.Visible;
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            DBComprobantes comprobanteAEliminar = GetComprobanteSeleccionado();

            Trace.Assert(!(comprobanteAEliminar is null));

            MessageBoxResult msgBoxConfirmationResult = System.Windows.MessageBox.Show("¿Está seguro que desea eliminar este comprobante?, todos los recibos y remitos que contengan a este comprobante van a eliminarlo de sus listas.", "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo);
            if (msgBoxConfirmationResult == MessageBoxResult.Yes)
            {
                if (comprobanteAEliminar.DeleteFromDatabase(dbCon.Connection))
                {
                    MessageBox.Show("Comprobante eliminado exitosamente");
                    RefreshData();
                }
                else
                {
                    MessageBox.Show("Error tratando de eliminar el comprobante.");
                }
            }
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

        private void btnImportar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == true)
            {
                List<DBTiposComprobantes> missingTiposComprobantes = new List<DBTiposComprobantes>();
                List<DBEntidades> missingEntidades = new List<DBEntidades>();
                List<DBMoneda> missingMonedas = new List<DBMoneda>();
                AFIPComprobantes.GetMissingTypesFromFile(GetCuentaSeleccionada(), openDialog.FileName, missingEntidades, missingTiposComprobantes, missingMonedas);

                List<DBBaseClass> missingList = new List<DBBaseClass>();
                missingList.AddRange(missingTiposComprobantes);
                missingList.AddRange(missingEntidades);

                if (missingList.Count > 0)
                {
                    uiFixMissing.RefreshData(missingList);
                    uiFixMissing.Visibility = Visibility.Visible;
                } else
                {
                    List<DBComprobantes> comprobantesAFIP = AFIPComprobantes.ImportFromFile(GetCuentaSeleccionada(), openDialog.FileName);

                    foreach (DBComprobantes comprobante in comprobantesAFIP)
                    {
                        comprobante.PushToDatabase(dbCon.Connection);
                    }

                    if (comprobantesAFIP.Count > 0)
                    {
                        MessageBox.Show("Comprobantes importados exitosamente");
                    } else
                    {
                        MessageBox.Show("¡Todos los comprobantes ya estaban en el sistema!");
                    }
                    RefreshData();
                }

            }
        }
    }
}
