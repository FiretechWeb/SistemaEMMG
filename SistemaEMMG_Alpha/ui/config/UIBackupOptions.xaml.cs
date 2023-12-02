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
    /// Interaction logic for UIBackupOptions.xaml
    /// </summary>
    public partial class UIBackupOptions : UserControl
    {
        public UIBackupOptions()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            cbAutomaticBackups.IsChecked = Config.GetGlobalConfig().AutomaticBackupsEnabled();
            txtAutomaticBackupInterval.Text = Config.GetGlobalConfig().AutomaticBackupsInterval().ToString();
        }

        private void btnGraphicsSave_Click(object sender, RoutedEventArgs e)
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            Config.GetGlobalConfig().SetAutomaticBackups(cbAutomaticBackups.IsChecked == true);
            Config.GetGlobalConfig().SetAutomaticBackupsInterval(SafeConvert.ToInt32(txtAutomaticBackupInterval.Text));
            Config.GetGlobalConfig().ExportToJSONFile(Config.GetDefaultConfigFileName());
        }
    }
}
