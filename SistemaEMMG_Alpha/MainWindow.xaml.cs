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

        public short oldTabItemSelection = -1; //To avoid bug with Tab Items
        public DBConnection dbCon = null;
        public DBOldMain dbData = null;

        public bool ConnectWithDatabase()
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


        public void SoftwareMain()
        {
            Console.WriteLine("Able to connect Database!");

            //Intialize accounts
            dbData = DBOldMain.Instance();
            dbData.ReadCuentasFromDB(dbCon.Connection);
            if (dbData.cuentas.Count > 0)
            {
                dbData.idCuentaSeleccionada = dbData.cuentas[0].GetID();
            } else
            {
                dbData.idCuentaSeleccionada = -1;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();

            errorScreen.SetMainWindow(this);

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

        private void btnBackupDB_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory("backups");
            DateTime dt = DateTime.Now;
            dbCon.Backup($"backups/dbbackup_{dt.ToString("dd_MM_yyyy")}.sql");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                developerWin.Visibility = developerWin.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}