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
    /// Interaction logic for UIPrinterOptions.xaml
    /// </summary>
    public partial class UIPrinterOptions : UserControl
    {
        public UIPrinterOptions()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            cmbPrintersAvailable.Items.Clear();
            cmbPrintersAvailable.SelectedValuePath = "Key";
            cmbPrintersAvailable.DisplayMemberPath = "Value";

            List<string> printersInstalled = PrinterManagment.GetInstalledPrinters();
            int printerSelected = -1;
            for (int i=0; i < printersInstalled.Count; i++)
            {
                cmbPrintersAvailable.Items.Add(new KeyValuePair<long, string>(i, printersInstalled[i]));

                if (printersInstalled[i].ToLower().Equals(Config.GetGlobalConfig().GetDefaultPrinter().ToLower()))
                {
                    printerSelected = i;
                }
            }
            if (printerSelected != -1)
            {
                cmbPrintersAvailable.SelectedIndex = printerSelected;
            }
            //TODO
        }

        private void btnGraphicsSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPrintersAvailable.SelectedItem is null)
            {
                return;
            }

            Config.GetGlobalConfig().SetDefaultPrinter(((KeyValuePair<long, string>)cmbPrintersAvailable.SelectedItem).Value);
            Config.GetGlobalConfig().ExportToJSONFile(Config.GetDefaultConfigFileName());

        }
    }
}
