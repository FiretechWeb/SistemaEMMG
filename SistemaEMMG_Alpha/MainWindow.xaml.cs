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

         private void SoftwareMain()
        {
            //Here everything that happens after database is correctly loaded
            Console.WriteLine("Able to connect Database!");

            //Intialize accounts
            dbData = DBFields.Instance();
            dbData.ReadEmpresasFromDB(dbCon.Connection);
            if (dbData.empresas.Count > 0)
            {
                dbData.idCuentaSeleccionada = 0;
            } else
            {
                dbData.idCuentaSeleccionada = -1;
            }
            guiCuentasRefresh();
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
                    dbData.idCuentaSeleccionada = 0;
                } else if (dbData.idCuentaSeleccionada >= dbData.empresas.Count)
                {
                    dbData.idCuentaSeleccionada = dbData.empresas.Count-1;
                }
                cmbCuentasEmpresas.SelectedIndex = (int)dbData.idCuentaSeleccionada;
            }
            guiRefreshCuentaSeleccionadaLabel();
        }

        private void guiEntidadesRefresh(bool refreshDataBase=false)
        {
            if (refreshDataBase)
            {
                dbData.ReadEntidadesComercialesFromDB(dbCon.Connection);
            }
            List<DBEntidades> entidadesComercialesLista = dbData.GetCurrentAccount().GetAllEntidadesComerciales();

            listEntidadesComerciales.Items.Clear();
            listEntidadesComerciales.SelectedValuePath = "Key";
            listEntidadesComerciales.DisplayMemberPath = "Value";
            foreach (DBEntidades entidadComercial in entidadesComercialesLista)
            {
                listEntidadesComerciales.Items.Add(new KeyValuePair<long, string>(entidadComercial.GetID(), entidadComercial.GetRazonSocial()));
            }
        }

        private void guiRefreshCuentaSeleccionadaLabel()
        {
            if (dbData.idCuentaSeleccionada < 0 || dbData.idCuentaSeleccionada >= dbData.empresas.Count)
            {
                lblCuentaSeleccionada.Content = "No hay cuentas. Por favor cree una para usar el sistema.";
             }
            else
            {
                lblCuentaSeleccionada.Content = $"{dbData.empresas[(int)dbData.idCuentaSeleccionada].GetRazonSocial()}";
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
            if (dbData.idCuentaSeleccionada != cmbSender.SelectedIndex)
            {
                dbData.idCuentaSeleccionada = cmbSender.SelectedIndex;
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
            dbData.AgregarNuevaCuentaDeEmpresa(txbNuevaCuentaNombre.Text.Trim(), Convert.ToInt64(txbNuevaCuentaCUIT.Text.Trim()), dbCon.Connection);
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
                dbData.EliminarCuentaDeEmpresa((int)dbData.idCuentaSeleccionada, dbCon.Connection);
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
                        break;
                }
            }
        }

    }
}