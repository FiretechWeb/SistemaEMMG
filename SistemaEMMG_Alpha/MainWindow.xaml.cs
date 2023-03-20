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
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

/*
rdbCMDRecibido
rdbCMDEmitido
cbxCMDTipoComprobante
txtCMDNumero
lsbCMDEntidades
txtCMDEntidadesFilter
txtCMDFechaEmitido
txtCMDGravado
txtCMDIVA
txtCMDNoGravado
txtCMDPercepcion

cbxCMTiposPagos
txtCMPagoObservacion
lbxCMPagos
btnCMAgregarPago
btnCMGuardarPago
btnCMEliminarPago
*/

namespace SistemaEMMG_Alpha
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        enum TabItemsSelections
        {
            TI_CUENTAS,
            TI_ENTIDADES,
            TI_BANCOS,
            TI_COMPROBANTES
        };
        public short oldTabItemSelection = -1; //To avoid bug with Tab Items
        public DBConnection dbCon = null;
        public DBFields dbData = null; // Aca está la papa
        //winCMDetalles
        private void guiSetComprobantesMainVisible()
        {
            winCMDetalles.Visibility = Visibility.Collapsed;
            winCMMain.Visibility = Visibility.Visible;

            guiComprobantesRefresh(true);
        }
        private void guiSetComprobantesDetallesVisibile()
        {
            winCMMain.Visibility = Visibility.Collapsed;
            winCMDetalles.Visibility = Visibility.Visible;
            
            guiRefreshComprobantesDetalles();
        }
        private bool ConnectWithDatabase()
        {
            if (!(dbCon is null))
            {
                return true;
            }
            bool sucessfulConnected = false;
            try
            {
                dbCon = DBConnection.Instance();
                dbCon.Server = "localhost";
                dbCon.DatabaseName = "sistemacomprobantes";
                dbCon.UserName = "root";
                dbCon.Password = "root";
                if (dbCon.IsConnected())
                {
                    sucessfulConnected = true;
                } else
                {
                    sucessfulConnected = false;
                    dbCon.Connection = null;
                    dbCon = null;
                }
            } catch (Exception ex)
            {
                sucessfulConnected = false;
                dbCon.Connection = null;
                dbCon = null;
                Console.WriteLine($"Error connecting with database: {ex}");
            }
            return sucessfulConnected;
        }

        private DBComprobantes GenerateNewComprobanteFromDetallesForm()
        {
            DateTime fechaEmitido = new DateTime();
            DateTime? feFinal = null;

            if (DateTime.TryParse(txtCMDFechaEmitido.Text, out fechaEmitido))
            {
                feFinal = fechaEmitido;
            }

            long entidadComercial_id = ((KeyValuePair<long, string>)lbxCMDEntidadSelected.SelectedItem).Key;

             return new DBComprobantes(
                    dbData.GetCurrentAccount(),
                    -1,
                    entidadComercial_id,
                    ((KeyValuePair<long, string>)cbxCMDTipoComprobante.SelectedItem).Key,
                    new ComprobantesData(feFinal,
                                        txtCMDNumero.Text,
                                        SafeConvert.ToDouble(txtCMDGravado.Text.Replace(".", ",")),
                                        SafeConvert.ToDouble(txtCMDIVA.Text.Replace(".", ",")),
                                        SafeConvert.ToDouble(txtCMDNoGravado.Text.Replace(".", ",")),
                                        SafeConvert.ToDouble(txtCMDPercepcion.Text.Replace(".", ",")),
                                        SafeConvert.ToBoolean(rdbCMDEmitido.IsChecked))
            );
        }

        private void UpdateSelectedComprobanteFromDetallesForm()
        {
            DateTime fechaEmitido = new DateTime();
            DateTime? feFinal = null;

            if (DateTime.TryParse(txtCMDFechaEmitido.Text, out fechaEmitido))
            {
                feFinal = fechaEmitido;
            }

            long entidadComercial_id = ((KeyValuePair<long, string>)lbxCMDEntidadSelected.SelectedItem).Key;

            dbData.GetComprobanteSelected().SetFechaEmitido(feFinal);
            dbData.GetComprobanteSelected().SetGravado(SafeConvert.ToDouble(txtCMDGravado.Text.Replace(".", ",")));
            dbData.GetComprobanteSelected().SetIVA(SafeConvert.ToDouble(txtCMDIVA.Text.Replace(".", ",")));
            dbData.GetComprobanteSelected().SetNoGravado(SafeConvert.ToDouble(txtCMDNoGravado.Text.Replace(".", ",")));
            dbData.GetComprobanteSelected().SetNumeroComprobante(txtCMDNumero.Text);
            dbData.GetComprobanteSelected().SetPercepcion(SafeConvert.ToDouble(txtCMDPercepcion.Text.Replace(".", ",")));
            dbData.GetComprobanteSelected().SetEmitido(SafeConvert.ToBoolean(rdbCMDEmitido.IsChecked));
            dbData.GetComprobanteSelected().SetEntidadComercial(entidadComercial_id);
            dbData.GetComprobanteSelected().SetTipoComprobante(((KeyValuePair<long, string>)cbxCMDTipoComprobante.SelectedItem).Key);
        }

        private void SoftwareMain()
        {
            Console.WriteLine("Able to connect Database!");

            //Intialize accounts
            dbData = DBFields.Instance();
            dbData.ReadEmpresasFromDB(dbCon.Connection);
            if (dbData.empresas.Count > 0)
            {
                dbData.idCuentaSeleccionada = dbData.empresas[0].GetID();
            } else
            {
                dbData.idCuentaSeleccionada = -1;
            }
            guiCuentasRefresh();
        }
        
        private void guiRefreshComprobantesDetalles(bool refreshDatabase=false)
        {
            cbxCMTiposPagos.Items.Clear();
            if (dbData.GetCurrentAccount() is null)
            {
                return; //No accounts available
            }
            if (refreshDatabase)
            {
                dbData.ReadEntidadesComercialesFromDB(dbCon.Connection); //Necessary for checkbox D:
                dbData.ReadComprobantesFromDB(dbCon.Connection);
            }
            cbxCMTiposPagos.SelectedValuePath = "Key";
            cbxCMTiposPagos.DisplayMemberPath = "Value";
            foreach (DBFormasPago formaPago in dbData.formas_pago)
            {
                cbxCMTiposPagos.Items.Add(new KeyValuePair<long, string>(formaPago.GetID(), formaPago.GetName()));
            }
            if (dbData.formas_pago.Count > 0)
            {
                cbxCMTiposPagos.SelectedIndex = 0;
            }

            cbxCMDTipoComprobante.Items.Clear();
            cbxCMDTipoComprobante.SelectedValuePath = "Key";
            cbxCMDTipoComprobante.DisplayMemberPath = "Value";
            foreach (DBTiposComprobantes tipoComprobante in dbData.tipos_comprobantes)
            {
                cbxCMDTipoComprobante.Items.Add(new KeyValuePair<long, string>(tipoComprobante.GetID(), tipoComprobante.GetName()));
            }

            List<DBEntidades> entidadesComercialesList = dbData.GetCurrentAccount().GetAllEntidadesComerciales();

            lsbCMDEntidades.Items.Clear();
            lsbCMDEntidades.SelectedValuePath = "Key";
            lsbCMDEntidades.DisplayMemberPath = "Value";

            foreach(DBEntidades entidad in entidadesComercialesList)
            {
                lsbCMDEntidades.Items.Add(new KeyValuePair<long, string>(entidad.GetID(), $"{entidad.GetCUIT()}: {entidad.GetRazonSocial()}"));
            }

            if (dbData.GetComprobanteSelected() is null)
            {
                lsbCMDEntidades.SelectedIndex = 0;
                cbxCMDTipoComprobante.SelectedIndex = 0;
                lbxCMDEntidadSelected.Items.Clear();
                lbxCMDEntidadSelected.SelectedValuePath = "Key";
                lbxCMDEntidadSelected.DisplayMemberPath = "Value";
                if (entidadesComercialesList.Count > 0)
                {
                    lbxCMDEntidadSelected.Items.Add(new KeyValuePair<long, string>(entidadesComercialesList[0].GetID(), $"{entidadesComercialesList[0].GetCUIT()}: {entidadesComercialesList[0].GetRazonSocial()}"));
                }
                lbxCMDEntidadSelected.SelectedIndex = 0;
            } else
            {
                guiRefreshComprobantesForms();
            }
            
            guiRefreshComprobantesDetallesPagos(true);
        }

        private void guiRefreshComprobantesForms()
        {
            cbxCMDTipoComprobante.SelectedValue = dbData.GetComprobanteSelected().GetTipoComprobante().GetID();
            lbxCMDEntidadSelected.Items.Clear();
            lbxCMDEntidadSelected.SelectedValuePath = "Key";
            lbxCMDEntidadSelected.DisplayMemberPath = "Value";
            lbxCMDEntidadSelected.Items.Add(new KeyValuePair<long, string>(dbData.GetComprobanteSelected().GetEntidadComercialID(), $"{dbData.GetComprobanteSelected().GetEntidadComercial().GetCUIT()}: {dbData.GetComprobanteSelected().GetEntidadComercial().GetRazonSocial()}"));
            lbxCMDEntidadSelected.SelectedIndex = 0;
            txtCMDFechaEmitido.Text = dbData.GetComprobanteSelected().GetFechaEmitido().HasValue ? ((DateTime)dbData.GetComprobanteSelected().GetFechaEmitido()).ToString("dd/MM/yyyy") : "";
            rdbCMDRecibido.IsChecked = !dbData.GetComprobanteSelected().IsEmitido();
            rdbCMDEmitido.IsChecked = dbData.GetComprobanteSelected().IsEmitido();
            txtCMDNumero.Text = dbData.GetComprobanteSelected().GetNumeroComprobante();
            txtCMDGravado.Text = SafeConvert.ToString(dbData.GetComprobanteSelected().GetGravado());
            txtCMDIVA.Text = SafeConvert.ToString(dbData.GetComprobanteSelected().GetIVA());
            txtCMDNoGravado.Text = SafeConvert.ToString(dbData.GetComprobanteSelected().GetNoGravado());
            txtCMDPercepcion.Text = SafeConvert.ToString(dbData.GetComprobanteSelected().GetPercepcion());

        }
        private void guiRefreshComprobantesDetallesPagos(bool refreshDatabase = false)
        {
            lbxCMPagos.Items.Clear();
            dbData.DeselectPago();
            btnCMGuardarPago.IsEnabled = false;
            btnCMEliminarPago.IsEnabled = false;
            if (dbData.GetComprobanteSelected() is null)
            {
                return;
            }

            List<DBComprobantePago> pagos;
            if (refreshDatabase)
            {
                pagos = dbData.GetComprobanteSelected().GetAllPagos(dbCon.Connection);
            } else
            {
                pagos = dbData.GetComprobanteSelected().GetAllPagos();
            }

            lbxCMPagos.SelectedValuePath = "Key";
            lbxCMPagos.DisplayMemberPath = "Value";
            foreach (DBComprobantePago pago in pagos)
            {
                lbxCMPagos.Items.Add(new KeyValuePair<long, string>(pago.GetID(), pago.GetObservacion()));
            }
            lbxCMPagos.SelectedIndex = -1;
        }
        private void guiCuentasRefresh(bool refreshDatabase=false)
        {
            if (refreshDatabase)
            {
                dbData.ReadEmpresasFromDB(dbCon.Connection);
            }
            cmbCuentasEmpresas.Items.Clear();
            cmbCuentasEmpresas.SelectedValuePath = "Key";
            cmbCuentasEmpresas.DisplayMemberPath = "Value";
            foreach (DBEmpresa empresa in dbData.empresas)
            {
                cmbCuentasEmpresas.Items.Add(new KeyValuePair<long, string>(empresa.GetID(), empresa.GetRazonSocial()));
            }
            if (dbData.empresas.Count > 0)
            {
                if (dbData.idCuentaSeleccionada < 0)
                {
                    dbData.idCuentaSeleccionada = dbData.empresas[0].GetID();
                } else if (dbData.idCuentaSeleccionada >= dbData.empresas.Count)
                {
                    dbData.idCuentaSeleccionada = dbData.empresas[dbData.empresas.Count - 1].GetID();
                }
                cmbCuentasEmpresas.SelectedValue = dbData.idCuentaSeleccionada;
            }
            guiRefreshCuentaSeleccionadaLabel();
        }

        private void guiComprobantesRefresh(bool refreshDataBase = false)
        {
            if (dbData.GetCurrentAccount() is null)
            {
                return; //No accounts available
            }
            if (refreshDataBase)
            {
                dbData.ReadEntidadesComercialesFromDB(dbCon.Connection); //Necessary for checkbox D:
                dbData.ReadComprobantesFromDB(dbCon.Connection);
            }
            List<DBComprobantes> comprobantes = dbData.GetCurrentAccount().GetAllComprobantes();

            dgComprobantes.Items.Clear();

            foreach (DBComprobantes comprobante in comprobantes)
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
                    gravado = comprobante.GetGravado(),
                    iva = comprobante.GetIVA(),
                    no_gravado = comprobante.GetNoGravado(),
                    percepcion = comprobante.GetPercepcion()
                });
            }

            guiRefreshComprobantesForm(dbData.GetCurrentAccount().GetComprobanteByIndex(0));
        }

        private void guiRefreshComprobantesForm(DBComprobantes comprobanteSelected)
        {
            if (dbData.GetCurrentAccount() is null)
            {
                return; //No accounts available
            }
            List<DBEntidades> entidadesComercialesList = dbData.GetCurrentAccount().GetAllEntidadesComerciales();

            cbxCMEntidadComercial.Items.Clear();
            cbxCMEntidadComercial.SelectedValuePath = "Key";
            cbxCMEntidadComercial.DisplayMemberPath = "Value";
            foreach (DBEntidades entidadComercial in entidadesComercialesList)
            {
                cbxCMEntidadComercial.Items.Add(new KeyValuePair<long, string>(entidadComercial.GetID(), $"{entidadComercial.GetCUIT()}: {entidadComercial.GetRazonSocial()}"));
            }
            cbxCMTipoComprobante.Items.Clear();
            cbxCMTipoComprobante.SelectedValuePath = "Key";
            cbxCMTipoComprobante.DisplayMemberPath = "Value";
            foreach (DBTiposComprobantes tipoComprobante in dbData.tipos_comprobantes)
            {
                cbxCMTipoComprobante.Items.Add(new KeyValuePair<long, string>(tipoComprobante.GetID(), tipoComprobante.GetName()));
            }

            if (comprobanteSelected is null)
            {
                return;
            }
            chbxCMEsEmitido.IsChecked = comprobanteSelected.IsEmitido();
            txtCMFechaEmitido.Text = comprobanteSelected.GetFechaEmitido().HasValue ? ((DateTime)comprobanteSelected.GetFechaEmitido()).ToString("dd/MM/yyyy") : "";
            cbxCMEntidadComercial.SelectedValue = comprobanteSelected.GetEntidadComercialID();
            cbxCMTipoComprobante.SelectedValue = comprobanteSelected.GetTipoComprobante().GetID();
            txtCMNumeroFactura.Text = comprobanteSelected.GetNumeroComprobante();
            txtCMGravado.Text = SafeConvert.ToString(comprobanteSelected.GetGravado());
            txtCMIVA.Text = SafeConvert.ToString(comprobanteSelected.GetIVA());
            txtCMNoGravado.Text = SafeConvert.ToString(comprobanteSelected.GetNoGravado());
            txtCMPercepcion.Text = SafeConvert.ToString(comprobanteSelected.GetPercepcion());

        }

        private void guiEntidadesRefresh(bool refreshDataBase=false)
        {
            if (dbData.GetCurrentAccount() is null)
            {
                return; //No accounts available
            }

            if (refreshDataBase)
            {
                dbData.ReadEntidadesComercialesFromDB(dbCon.Connection);
            }
            List<DBEntidades> entidadesComercialesLista = dbData.GetCurrentAccount().GetAllEntidadesComerciales();
            guiRefreshTipoEntidadesComerciales();

            listEntidadesComerciales.Items.Clear();
            listEntidadesComerciales.SelectedValuePath = "Key";
            listEntidadesComerciales.DisplayMemberPath = "Value";
            foreach (DBEntidades entidadComercial in entidadesComercialesLista)
            {
                listEntidadesComerciales.Items.Add(new KeyValuePair<long, string>(entidadComercial.GetID(), $"{entidadComercial.GetTipoEntidad().GetName()}: {entidadComercial.GetRazonSocial()}"));
            }
            if (listEntidadesComerciales.Items.Count > 0)
            {
                listEntidadesComerciales.SelectedIndex = 0;
            }

            guiRefreshEntidadesForm();
        }

        private void guiRefreshEntidadesForm()
        {
            if (dbData.GetCurrentAccount() is null)
            {
                return; //No accounts available
            }

            if (listEntidadesComerciales.SelectedItem is null)
            {
                return;
            }
            long listECSelectedID = ((KeyValuePair<long, string>)listEntidadesComerciales.SelectedItem).Key;

            if (listECSelectedID < 0)
            {
                return;
            }
            DBEntidades selectedEntidad = dbData.GetCurrentAccount().GetEntidadByID((int)listECSelectedID);
            foreach (KeyValuePair<long, string> cbItem in cmbTipoEC.Items)
            {
                if (cbItem.Key == selectedEntidad.GetTipoEntidad().GetID())
                {
                    cmbTipoEC.SelectedValue = cbItem.Key;
                    break;
                }
            }

            txbCUITEC.Text = selectedEntidad.GetCUIT().ToString();
            tbxRazonSocialEC.Text = selectedEntidad.GetRazonSocial();
            tbxEmailEC.Text = selectedEntidad.GetEmail();
            tbxTelEC.Text = selectedEntidad.GetTelefono();
            tbxCelEC.Text = selectedEntidad.GetCelular();
        }
        private void guiRefreshTipoEntidadesComerciales()
        {
            List<DBTipoEntidad> tiposEntidadesComerciales = DBTipoEntidad.GetAll();
            cmbTipoEC.Items.Clear();
            cmbTipoEC.SelectedValuePath = "Key";
            cmbTipoEC.DisplayMemberPath = "Value";

            foreach (DBTipoEntidad tipoEntidad in tiposEntidadesComerciales)
            {
                cmbTipoEC.Items.Add(new KeyValuePair<long, string>(tipoEntidad.GetID(), tipoEntidad.GetName()));
            }
        }

        private void guiRefreshCuentaSeleccionadaLabel()
        {
            if (dbData.idCuentaSeleccionada < 0)
            {
                lblCuentaSeleccionada.Content = "No hay cuentas. Por favor cree una para usar el sistema.";
             }
            else
            {
                lblCuentaSeleccionada.Content = $"{dbData.empresas[dbData.GetCuentaIndexByID(dbData.idCuentaSeleccionada)].GetRazonSocial()}";
            }
        }
        public MainWindow()
        {
            InitializeComponent();

           if (!ConnectWithDatabase())
            {
                errorScreen.Visibility = Visibility.Visible;
                Console.WriteLine("Connecting with database failed!");
            } else
            {
                errorScreen.Visibility = Visibility.Collapsed;
                SoftwareMain();
            }
           
        }

        private void btnDbReconnect_Click(object sender, RoutedEventArgs e)
        {
            if (ConnectWithDatabase())
            {
                errorScreen.Visibility = Visibility.Collapsed;
                SoftwareMain();
            } else
            {
                MessageBox.Show("Error al tratar de reconectarse con la base de datos.");
            }
        }

        private void cmbCuentasEmpresas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbSender = sender as ComboBox;
            if (cmbSender.SelectedItem is null)
            {
                return;
            }
            if (dbData.idCuentaSeleccionada != ((KeyValuePair<long, string>)cmbSender.SelectedItem).Key)
            {
                dbData.idCuentaSeleccionada = ((KeyValuePair<long, string>)cmbSender.SelectedItem).Key;
                guiRefreshCuentaSeleccionadaLabel();
            }
        }

        private void btnBackupDB_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory("backups");
            DateTime dt = DateTime.Now;
            dbCon.Backup($"backups/dbbackup_{dt.ToString("dd_MM_yyyy")}.sql");
        }

        private void btnAddNewAccount_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txbNuevaCuentaNombre.Text.Trim()) || string.IsNullOrEmpty(txbNuevaCuentaCUIT.Text.Trim()))
            {
                return;
            }
            dbData.AgregarNuevaCuentaDeEmpresa(txbNuevaCuentaNombre.Text.Trim(), SafeConvert.ToInt64(txbNuevaCuentaCUIT.Text.Trim()), dbCon.Connection);
            txbNuevaCuentaNombre.Text = "";
            txbNuevaCuentaCUIT.Text = "";
            guiCuentasRefresh();
        }

        private void txbNuevaCuentaNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txbNuevaCuentaNombre.Text.Trim()) || string.IsNullOrEmpty(txbNuevaCuentaCUIT.Text.Trim()))
            {
                btnAddNewAccount.IsEnabled = false;
            } else
            {
                btnAddNewAccount.IsEnabled = true;
            }
        }

        private void txbNuevaCuentaCUIT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txbNuevaCuentaNombre.Text.Trim()) || string.IsNullOrEmpty(txbNuevaCuentaCUIT.Text.Trim()))
            {
                btnAddNewAccount.IsEnabled = false;
            }
            else
            {
                btnAddNewAccount.IsEnabled = true;
            }
        }

        private void txbNuevaCuentaCUIT_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnEliminarCuenta(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Seguro que queres eliminar una cuenta?, vas a perder todos los datos asociados a esta cuenta: Clientes, Proovedores, comprobantes, etc... Recomiendo hacer un backup primero.", "Confirmación", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                dbData.EliminarCuentaDeEmpresa(dbData.GetCuentaIndexByID(dbData.idCuentaSeleccionada), dbCon.Connection);
                guiCuentasRefresh();
            }
            else
            {
                return;
            }

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl)
            {
                short newTabItemSelection = oldTabItemSelection;
                if(tabBancos.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_BANCOS;
                } else if (tabComprobantes.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_COMPROBANTES;
                } else if (tabCuentas.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_CUENTAS;
                } else if (tabEntidades.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_ENTIDADES;
                }

                if (oldTabItemSelection == newTabItemSelection || oldTabItemSelection == -1)
                {
                    oldTabItemSelection = newTabItemSelection;
                    return;
                }

                oldTabItemSelection = newTabItemSelection;

                switch ((TabItemsSelections)newTabItemSelection)
                {
                    case TabItemsSelections.TI_CUENTAS:
                        guiCuentasRefresh(true);
                        Console.WriteLine("TI_CUENTAS");
                        break;
                    case TabItemsSelections.TI_BANCOS:
                        Console.WriteLine("TI_BANCOS");
                        break;
                    case TabItemsSelections.TI_ENTIDADES:
                        Console.WriteLine("TI_ENTIDADES");
                        guiEntidadesRefresh(true);
                        break;
                    case TabItemsSelections.TI_COMPROBANTES:
                        Console.WriteLine("TI_COMPROBANTES");
                        guiSetComprobantesMainVisible();
                        guiComprobantesRefresh(true);
                        break;
                }
            }
        }

        private void listEntidadesComerciales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            guiRefreshEntidadesForm();
        }

        private void btnModificarEC_Click(object sender, RoutedEventArgs e)
        {
            if (listEntidadesComerciales.SelectedItem is null)
            {
                return;
            }

            long listECSelectedID = ((KeyValuePair<long, string>)listEntidadesComerciales.SelectedItem).Key;

            if (listECSelectedID < 0)
            {
                return;
            }

            DBEntidades selectedEntidad = dbData.GetCurrentAccount().GetEntidadByID((int)listECSelectedID);
            if (selectedEntidad is null)
            {
                MessageBox.Show("Error al tratar de modificar esta entidad comercial. Valor devuelto: NULL");
                return;
            }
            selectedEntidad.SetRazonSocial(tbxRazonSocialEC.Text);
            selectedEntidad.SetCuit(SafeConvert.ToInt64(txbCUITEC.Text));
            selectedEntidad.SetEmail(tbxEmailEC.Text);
            selectedEntidad.SetTelefono(tbxTelEC.Text);
            selectedEntidad.SetCelular(tbxCelEC.Text);
            selectedEntidad.TipoEntidad = DBTipoEntidad.GetByID(((KeyValuePair<long, string>)cmbTipoEC.SelectedItem).Key);

            selectedEntidad.PushToDatabase(dbCon.Connection);

            guiEntidadesRefresh();
        }

        private void btnAgregarEC_Click(object sender, RoutedEventArgs e)
        {
            //AddNewEntidad
            DBEntidades nuevaEntidad = new DBEntidades(dbData.GetCurrentAccount(), ((KeyValuePair<long, string>)cmbTipoEC.SelectedItem).Key, SafeConvert.ToInt64(txbCUITEC.Text), tbxRazonSocialEC.Text);
            nuevaEntidad.SetEmail(tbxEmailEC.Text);
            nuevaEntidad.SetTelefono(tbxTelEC.Text);
            nuevaEntidad.SetCelular(tbxCelEC.Text);

            if (dbData.GetCurrentAccount().AddNewEntidad(nuevaEntidad))
            {
                nuevaEntidad.PushToDatabase(dbCon.Connection);
                guiEntidadesRefresh();
            } else
            {
                MessageBox.Show("¡Ya existe una entidad comercial con la misma Razón Social y CUIT!");
            }


        }

        private void btnEliminarEC_Click(object sender, RoutedEventArgs e)
        {
            if (listEntidadesComerciales.SelectedItem is null)
            {
                return;
            }

            long listECSelectedID = ((KeyValuePair<long, string>)listEntidadesComerciales.SelectedItem).Key;

            if (listECSelectedID < 0)
            {
                return;
            }

            DBEntidades selectedEntidad = dbData.GetCurrentAccount().GetEntidadByID((int)listECSelectedID);

            selectedEntidad.DeleteFromDatabase(dbCon.Connection);
            guiEntidadesRefresh();
        }

        private void txbCUITEC_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void dgComprobantes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

                    dbData.SetComprobanteSelected(dbData.GetCurrentAccount().GetComprobanteByID(cm_ec_id, cm_id));
                    guiRefreshComprobantesForm(dbData.GetComprobanteSelected());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error with Dynamic...");
                }
            }
        }

        private void btnCMAgregar_Click(object sender, RoutedEventArgs e)
        {
            DateTime fechaEmitido = new DateTime();
            DateTime? feFinal = null;

            if (DateTime.TryParse(txtCMFechaEmitido.Text, out fechaEmitido))
            {
                feFinal = fechaEmitido;
            }

            long entidadComercial_id = ((KeyValuePair<long, string>)cbxCMEntidadComercial.SelectedItem).Key;

            DBComprobantes newComprobante = new DBComprobantes(
                    dbData.GetCurrentAccount(),
                    -1,
                    entidadComercial_id,
                    ((KeyValuePair<long, string>)cbxCMTipoComprobante.SelectedItem).Key,
                    new ComprobantesData(feFinal,
                                        txtCMNumeroFactura.Text,
                                        SafeConvert.ToDouble(txtCMGravado.Text.Replace(".", ",")),
                                        SafeConvert.ToDouble(txtCMIVA.Text.Replace(".", ",")),
                                        SafeConvert.ToDouble(txtCMNoGravado.Text.Replace(".", ",")),
                                        SafeConvert.ToDouble(txtCMPercepcion.Text.Replace(".", ",")),
                                        SafeConvert.ToBoolean(chbxCMEsEmitido.IsChecked))
            );
            if (newComprobante.PushToDatabase(dbCon.Connection))
            {
                dbData.GetCurrentAccount().GetEntidadByID(entidadComercial_id).AddNewComprobante(newComprobante);
                dbData.SetComprobanteSelected(newComprobante);
                guiComprobantesRefresh();
            }
        }

        private void btnCMModificar_Click(object sender, RoutedEventArgs e)
        {
            if (dbData.GetComprobanteSelected() is null)
            {
                return;
            }
            long cm_ec_id = dbData.GetComprobanteSelected().GetEntidadComercialID();
            long cm_id = dbData.GetComprobanteSelected().GetID();

            DBComprobantes selectedComprobante = dbData.GetComprobanteSelected();
            if (selectedComprobante is null)
            {
                MessageBox.Show("Error al tratar de modificar este comprobante. Valor devuelto: NULL");
                return;
            }

            DateTime fechaEmitido = new DateTime();
            if (DateTime.TryParse(txtCMFechaEmitido.Text, out fechaEmitido)) {
                selectedComprobante.SetFechaEmitido(fechaEmitido);
            } else
            {
                selectedComprobante.SetFechaEmitido(null);
            }

            selectedComprobante.SetEmitido(SafeConvert.ToBoolean(chbxCMEsEmitido.IsChecked));
            selectedComprobante.SetEntidadComercial(((KeyValuePair<long, string>)cbxCMEntidadComercial.SelectedItem).Key);
            selectedComprobante.SetTipoComprobante(((KeyValuePair<long, string>)cbxCMTipoComprobante.SelectedItem).Key);
            selectedComprobante.SetNumeroComprobante(txtCMNumeroFactura.Text);
            selectedComprobante.SetGravado(SafeConvert.ToDouble(txtCMGravado.Text.Replace(".", ",")));
            selectedComprobante.SetIVA(SafeConvert.ToDouble(txtCMIVA.Text.Replace(".", ",")));
            selectedComprobante.SetNoGravado(SafeConvert.ToDouble(txtCMNoGravado.Text.Replace(".", ",")));
            selectedComprobante.SetPercepcion(SafeConvert.ToDouble(txtCMPercepcion.Text.Replace(".", ",")));

            if (cm_ec_id != selectedComprobante.GetEntidadComercialID())
            {
                if (selectedComprobante.InsertIntoToDatabase(dbCon.Connection))
                {
                    DBComprobantes.RemoveFromDB(dbCon.Connection, dbData.GetCurrentAccount(), cm_ec_id, cm_id);
                    selectedComprobante.GetEntidadComercial().AddNewComprobante(selectedComprobante);
                }
            } else
            {
                selectedComprobante.PushToDatabase(dbCon.Connection);
            }

            guiComprobantesRefresh();
        }

        private void btnCMEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dbData.GetComprobanteSelected() is null)
            {
                return;
            }
            DBComprobantes selectedComprobante = dbData.GetComprobanteSelected();
            if (selectedComprobante is null)
            {
                MessageBox.Show("Error al tratar de modificar este comprobante. Valor devuelto: NULL");
                return;
            }

            selectedComprobante.DeleteFromDatabase(dbCon.Connection);
            dbData.DeselectComprobante();
            guiComprobantesRefresh();

        }

        private void btnCMVerDetalles_Click(object sender, RoutedEventArgs e)
        {
            guiSetComprobantesDetallesVisibile();
        }

        private void btnCMDAtras_Click(object sender, RoutedEventArgs e)
        {
            guiSetComprobantesMainVisible();
        }

        private void lbxCMPagos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dbData.GetComprobanteSelected() is null)
            {
                MessageBox.Show("Contactar al programador: GetComprobanteSelected() es NULL en lbxCMPagos_SelectionChanged, eso no debería pasar.", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ListBox lbxSender = sender as ListBox;
            if (lbxSender.SelectedItem is null)
            {
                btnCMGuardarPago.IsEnabled = false;
                btnCMEliminarPago.IsEnabled = false;
                dbData.DeselectPago();
                return;
            }
            dbData.SetPagoSelected(dbData.GetComprobanteSelected().GetPagoByID(((KeyValuePair<long, string>)lbxSender.SelectedItem).Key));

            if (dbData.GetPagoSelected() is null)
            {
                btnCMGuardarPago.IsEnabled = false;
                btnCMEliminarPago.IsEnabled = false;
                return;
            }
            btnCMGuardarPago.IsEnabled = true;
            btnCMEliminarPago.IsEnabled = true;
            txtCMPagoObservacion.Text = dbData.GetPagoSelected().GetObservacion();
            cbxCMTiposPagos.SelectedValue = dbData.GetPagoSelected().GetFormaDePago().GetID();
        }

        private void btnCMAgregarPago_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCMDEntidadSelected.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: lbxCMDEntidadSelected.SelectedItem NULL en btnCMAgregarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbxCMDTipoComprobante.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: cbxCMDTipoComprobante.SelectedItem NULL en btnCMAgregarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (dbData.GetComprobanteSelected() is null) //If there is no comprobante selected, then let's try to create a temporary one to work with...
            {
                dbData.SetComprobanteSelected(GenerateNewComprobanteFromDetallesForm());
                if (dbData.GetComprobanteSelected() is null)
                {
                    MessageBox.Show("Contactar al programador: GetComprobanteSelected() es NULL en btnCMAgregarPago_Click, eso no debería pasar.", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DBComprobantePago newPago = new DBComprobantePago(dbData.GetComprobanteSelected(), -1, ((KeyValuePair<long, string>)cbxCMTiposPagos.SelectedItem).Key, 0.0, txtCMPagoObservacion.Text);
            if (dbData.GetComprobanteSelected().ExistsInDatabase(dbCon.Connection) == true)
            {
                if (newPago.PushToDatabase(dbCon.Connection))
                {
                    dbData.GetComprobanteSelected().AddPago(newPago);
                    guiRefreshComprobantesDetallesPagos();
                }
            } else //Add locally...
            {
                dbData.GetComprobanteSelected().AddPagoLocally(newPago);
                guiRefreshComprobantesDetallesPagos();
            }
        }

        private void btnCMGuardarPago_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCMDEntidadSelected.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: lbxCMDEntidadSelected.SelectedItem NULL en btnCMGuardarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbxCMDTipoComprobante.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: cbxCMDTipoComprobante.SelectedItem NULL en btnCMGuardarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if ((dbData.GetComprobanteSelected() is null) || (dbData.GetPagoSelected() is null))
            {
                MessageBox.Show("Contactar al programador: GetComprobanteSelected() o GetPagoSelected()  es NULL en btnCMGuardarPago_Click, eso no debería pasar.", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dbData.GetPagoSelected().GetEntidadComercialID() != dbData.GetComprobanteSelected().GetEntidadComercialID() || dbData.GetPagoSelected().GetComprobanteID() != dbData.GetComprobanteSelected().GetID())
            {
                MessageBox.Show("Contactar al programador: El pago seleccionado no pertenece al comprobante seleccionado en btnCMGuardarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DBComprobantePago pagoModificado = dbData.GetPagoSelected();
            pagoModificado.SetFormaDePago(((KeyValuePair<long, string>)cbxCMTiposPagos.SelectedItem).Key);
            pagoModificado.SetObservacion(txtCMPagoObservacion.Text);

             if (dbData.GetComprobanteSelected().ExistsInDatabase(dbCon.Connection) == false || pagoModificado.PushToDatabase(dbCon.Connection))
             {
                guiRefreshComprobantesDetallesPagos();
             }

        }

        private void btnCMEliminarPago_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCMDEntidadSelected.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: lbxCMDEntidadSelected.SelectedItem NULL en btnCMEliminarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbxCMDTipoComprobante.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: cbxCMDTipoComprobante.SelectedItem NULL en btnCMEliminarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if ((dbData.GetComprobanteSelected() is null) || (dbData.GetPagoSelected() is null))
            {
                MessageBox.Show("Contactar al programador: GetComprobanteSelected() o GetPagoSelected()  es NULL en btnCMEliminarPago_Click, eso no debería pasar.", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dbData.GetPagoSelected().GetEntidadComercialID() != dbData.GetComprobanteSelected().GetEntidadComercialID() || dbData.GetPagoSelected().GetComprobanteID() != dbData.GetComprobanteSelected().GetID())
            {
                MessageBox.Show("Contactar al programador: El pago seleccionado no pertenece al comprobante seleccionado en btnCMEliminarPago_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DBComprobantePago pagoEliminar = dbData.GetPagoSelected();

            if (dbData.GetComprobanteSelected().ExistsInDatabase(dbCon.Connection) == false || pagoEliminar.DeleteFromDatabase(dbCon.Connection))
            {
                dbData.GetComprobanteSelected().RemovePago(pagoEliminar);
            }
            guiRefreshComprobantesDetallesPagos();
        }

        private void lsbCMDEntidades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lbxSender = sender as ListBox;
            if (lbxSender.SelectedItem is null)
            {
                return;
            }
            lbxCMDEntidadSelected.Items.Clear();
            lbxCMDEntidadSelected.SelectedValuePath = "Key";
            lbxCMDEntidadSelected.DisplayMemberPath = "Value";
            lbxCMDEntidadSelected.Items.Add((KeyValuePair<long, string>)lsbCMDEntidades.SelectedItem);
            lbxCMDEntidadSelected.SelectedIndex = 0;
        }

        private void btnCMDBuscarEntidad_Click(object sender, RoutedEventArgs e)
        {
            List<DBEntidades> entidadesComercialesList = DBConsultas.DBEntidadesWith(dbCon.Connection, dbData.GetCurrentAccount(), txtCMDEntidadesFilter.Text);

            lsbCMDEntidades.Items.Clear();
            lsbCMDEntidades.SelectedValuePath = "Key";
            lsbCMDEntidades.DisplayMemberPath = "Value";

            foreach (DBEntidades entidad in entidadesComercialesList)
            {
                lsbCMDEntidades.Items.Add(new KeyValuePair<long, string>(entidad.GetID(), $"{entidad.GetCUIT()}: {entidad.GetRazonSocial()}"));
            }
        }

        private void btnCMDGuardarEntidad_Click(object sender, RoutedEventArgs e)
        {
            if (dbData.GetComprobanteSelected() is null)
            {
                MessageBox.Show("Contactar al programador: has tratado de actualizar un comprobante que no existe en la base de datos en btnCMDGuardarEntidad_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (lbxCMDEntidadSelected.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: lbxCMDEntidadSelected.SelectedItem NULL en btnCMDGuardarEntidad_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbxCMDTipoComprobante.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: cbxCMDTipoComprobante.SelectedItem NULL en btnCMDGuardarEntidad_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            DBComprobantes selectedComprobante = dbData.GetComprobanteSelected();
            if (selectedComprobante is null)
            {
                MessageBox.Show("Error al tratar de modificar este comprobante. Valor devuelto: NULL");
                return;
            }

            long cm_ec_id = dbData.GetComprobanteSelected().GetEntidadComercialID();
            long cm_id = dbData.GetComprobanteSelected().GetID();


            DateTime fechaEmitido = new DateTime();
            if (DateTime.TryParse(txtCMDFechaEmitido.Text, out fechaEmitido))
            {
                selectedComprobante.SetFechaEmitido(fechaEmitido);
            }
            else
            {
                selectedComprobante.SetFechaEmitido(null);
            }

            selectedComprobante.SetEmitido(SafeConvert.ToBoolean(rdbCMDEmitido.IsChecked));
            selectedComprobante.SetEntidadComercial(((KeyValuePair<long, string>)lbxCMDEntidadSelected.SelectedItem).Key);
            selectedComprobante.SetTipoComprobante(((KeyValuePair<long, string>)cbxCMDTipoComprobante.SelectedItem).Key);
            selectedComprobante.SetNumeroComprobante(txtCMDNumero.Text);
            selectedComprobante.SetGravado(SafeConvert.ToDouble(txtCMDGravado.Text.Replace(".", ",")));
            selectedComprobante.SetIVA(SafeConvert.ToDouble(txtCMDIVA.Text.Replace(".", ",")));
            selectedComprobante.SetNoGravado(SafeConvert.ToDouble(txtCMDNoGravado.Text.Replace(".", ",")));
            selectedComprobante.SetPercepcion(SafeConvert.ToDouble(txtCMDPercepcion.Text.Replace(".", ",")));

            bool dataUpdateDBSuccess = false;

            if (cm_ec_id != selectedComprobante.GetEntidadComercialID()) //entidad comercial cambio, esto va a ser un pushtodatabase
            {
                if (selectedComprobante.InsertIntoToDatabase(dbCon.Connection))
                {
                    dataUpdateDBSuccess = true;
                    DBComprobantes.RemoveFromDB(dbCon.Connection, dbData.GetCurrentAccount(), cm_ec_id, cm_id);
                    selectedComprobante.GetEntidadComercial().AddNewComprobante(selectedComprobante);
                }
            }
            else
            {
                dataUpdateDBSuccess = selectedComprobante.PushToDatabase(dbCon.Connection);
            }

            if (dataUpdateDBSuccess)
            {
                MessageBox.Show("¡Información agregada a la base de datos correctamente!");
            }
        }

        private void btnCMDInsertarEntidad_Click(object sender, RoutedEventArgs e)
        {
            if (lbxCMDEntidadSelected.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: lbxCMDEntidadSelected.SelectedItem NULL en bbtnCMDInsertarEntidad_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbxCMDTipoComprobante.SelectedItem is null)
            {
                MessageBox.Show("Contactar al programador: cbxCMDTipoComprobante.SelectedItem NULL en btnCMDInsertarEntidad_Click. ", "Exception sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            long entidadComercial_id = ((KeyValuePair<long, string>)lbxCMDEntidadSelected.SelectedItem).Key;

            DBComprobantes newComprobante;
            if (!(dbData.GetComprobanteSelected() is null) && dbData.GetComprobanteSelected().ExistsInDatabase(dbCon.Connection) == false)
            {
                UpdateSelectedComprobanteFromDetallesForm();
                newComprobante = dbData.GetComprobanteSelected();
            } else {
                newComprobante = GenerateNewComprobanteFromDetallesForm();
            }
            if (newComprobante.PushToDatabase(dbCon.Connection))
            {
                dbData.GetCurrentAccount().GetEntidadByID(entidadComercial_id).AddNewComprobante(newComprobante);
                dbData.SetComprobanteSelected(newComprobante);

                //Now let's add the "pagos" to the database...

                List<DBComprobantePago> listaPagosToAdd = dbData.GetComprobanteSelected().GetAllPagos();

                foreach (DBComprobantePago pago in listaPagosToAdd)
                {
                    Console.WriteLine("Agregando pago...");
                    if (pago.PushToDatabase(dbCon.Connection))
                    {
                        Console.WriteLine("Pago agregado...");
                    }
                }
                guiComprobantesRefresh();

                MessageBox.Show("¡Información agregada a la base de datos correctamente!");
            }

        }
    }
}