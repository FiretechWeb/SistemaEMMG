using System.Windows;
using System.Windows.Controls;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIErrorScreen.xaml
    /// </summary>
    public partial class UIErrorScreen : BaseUCClass
    {
        public UIErrorScreen()
        {
            InitializeComponent();
        }

        private void btnDbReconnect_Click(object sender, RoutedEventArgs e)
        {
            if (GetMainWindow() is null)
            {
                return;
            }
            if (GetMainWindow().ConnectWithDatabase())
            {
                Visibility = Visibility.Collapsed;
                GetMainWindow().SoftwareMain();
            }
            else
            {
                MessageBox.Show("Error al tratar de reconectarse con la base de datos.");
            }
        }
    }
}
