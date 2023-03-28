using System.Windows;
using System.Windows.Controls;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIErrorScreen.xaml
    /// </summary>
    public partial class UIErrorScreen : UserControl
    {
        private MainWindow _mainWin = null;
        public void SetMainWindow(MainWindow mainWin)
        {
            _mainWin = mainWin;
        }
        public MainWindow GetMainWindow() => _mainWin;

        public UIErrorScreen()
        {
            InitializeComponent();
        }

        private void btnDbReconnect_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWin is null)
            {
                return;
            }
            if (_mainWin.ConnectWithDatabase())
            {
                Visibility = Visibility.Collapsed;
                _mainWin.SoftwareMain();
            }
            else
            {
                MessageBox.Show("Error al tratar de reconectarse con la base de datos.");
            }
        }
    }
}
