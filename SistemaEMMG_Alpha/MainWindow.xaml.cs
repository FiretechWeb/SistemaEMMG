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


namespace SistemaEMMG_Alpha
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
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
        
        private void guiCuentasRefresh()
        {
            cmbCuentasEmpresas.Items.Clear();
            cmbCuentasEmpresas.SelectedValuePath = "Key";
            cmbCuentasEmpresas.DisplayMemberPath = "Value";
            foreach (DBEmpresa empresa in dbData.empresas)
            {
                cmbCuentasEmpresas.Items.Add(new KeyValuePair<long, string>(empresa.GetID(), empresa.GetRazonSocial()));
            }
            if (dbData.empresas.Count > 0)
            {
                cmbCuentasEmpresas.SelectedIndex = (int)dbData.idCuentaSeleccionada;
                lblCuentaSeleccionada.Content = $"Cuenta seleccionada: {dbData.empresas[(int)dbData.idCuentaSeleccionada].GetRazonSocial()}";
            }
            else
            {
                lblCuentaSeleccionada.Content = "No hay cuentas. Por favor cree una para usar el sistema.";
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
            MessageBox.Show("Cuenta seleccionada changed...");
            ComboBox cmbSender = sender as ComboBox;
            if (dbData.idCuentaSeleccionada != cmbSender.SelectedIndex)
            {
                dbData.idCuentaSeleccionada = cmbSender.SelectedIndex;
                guiCuentasRefresh();
            }
        }
    }
}
