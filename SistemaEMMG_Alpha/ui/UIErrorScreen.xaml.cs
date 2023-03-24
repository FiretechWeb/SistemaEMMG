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
    /// Interaction logic for UIErrorScreen.xaml
    /// </summary>
    public partial class UIErrorScreen : UserControl
    {
        private MainWindow _mainWin = null;
        public UIErrorScreen()
        {
            InitializeComponent();
        }

        public void SetMainWindow(MainWindow mainWin)
        {
            _mainWin = mainWin;
        }

        private void btnDbReconnect_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWin is null)
            {
                return;
            }
            if (_mainWin.ConnectWithDatabase())
            {
                _mainWin.errorScreen.Visibility = Visibility.Collapsed;
                _mainWin.SoftwareMain();
            }
            else
            {
                MessageBox.Show("Error al tratar de reconectarse con la base de datos.");
            }
        }
    }
}
