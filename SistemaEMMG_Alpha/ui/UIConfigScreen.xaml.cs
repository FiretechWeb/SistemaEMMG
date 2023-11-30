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

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIConfigScreen.xaml
    /// </summary>
    public partial class UIConfigScreen : BaseUCClass
    {
        public UIConfigScreen()
        {
            InitializeComponent();
        }

        private void btnExitConfig_Click(object sender, RoutedEventArgs e)
        {
            if (GetMainWindow() is null)
            {
                return;
            }
            GetMainWindow().configWinWrapper.Visibility = Visibility.Collapsed;
        }

        private void btnUIOptions_Click(object sender, RoutedEventArgs e)
        {
            cfgBackupOptionWrapper.Visibility = Visibility.Collapsed;
            cfgPrintersOptionWrapper.Visibility = Visibility.Collapsed;
            cfgGraphicOptionWrapper.Visibility = Visibility.Visible;
        }

        private void btnBackupOptions_Click(object sender, RoutedEventArgs e)
        {
            cfgPrintersOptionWrapper.Visibility = Visibility.Collapsed;
            cfgGraphicOptionWrapper.Visibility = Visibility.Collapsed;
            cfgBackupOptionWrapper.Visibility = Visibility.Visible;
        }

        private void btnPrinterOptions_Click(object sender, RoutedEventArgs e)
        {
            cfgGraphicOptionWrapper.Visibility = Visibility.Collapsed;
            cfgBackupOptionWrapper.Visibility = Visibility.Collapsed;
            cfgPrintersOptionWrapper.Visibility = Visibility.Visible;
        }
    }
}
