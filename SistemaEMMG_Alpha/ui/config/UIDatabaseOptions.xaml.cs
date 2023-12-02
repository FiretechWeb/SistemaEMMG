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

namespace SistemaEMMG_Alpha.ui.config
{
    /// <summary>
    /// Interaction logic for UIDatabaseOptions.xaml
    /// </summary>
    public partial class UIDatabaseOptions : UserControl
    {
        public UIDatabaseOptions()
        {
            InitializeComponent();
        }
        public void RefreshData()
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            txtServerName.Text = Config.GetGlobalConfig().GetDatabaseData().hostName;
            txtDatabaseName.Text = Config.GetGlobalConfig().GetDatabaseData().databaseName;
            txtUserName.Text = Config.GetGlobalConfig().GetDatabaseData().userName;
            txtServerPassword.Password = Config.GetGlobalConfig().GetDatabaseData().userPassword;
        }

        private void btnGraphicsSave_Click(object sender, RoutedEventArgs e)
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            Config.GetGlobalConfig().SetDatabaseData(new DatabaseAccessData(
                txtServerName.Text.Trim(),
                txtDatabaseName.Text.Trim(),
                txtUserName.Text.Trim(),
                txtServerPassword.Password.Trim()
                ));

            Config.GetGlobalConfig().ExportToJSONFile(Config.GetDefaultConfigFileName());
        }
    }
}
