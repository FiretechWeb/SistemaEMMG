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
    /// Interaction logic for UILoginScreen.xaml
    /// </summary>
    public partial class UILoginScreen : BaseUCClass
    {
        public DBConnection dbCon = null;
        public UILoginScreen()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (dbCon is null)
            {
                dbCon = DBConnection.Instance();
            }
            string username = txtUserName.Text.Trim();
            string password = txtUserPass.Password;

            User tmpUser = new User(username, password);

            if (tmpUser.isValid(dbCon.Connection) && tmpUser.PullFromDatabase(dbCon.Connection))
            {
                User.SetCurrentUser(tmpUser);
                GetMainWindow().accessWin.Visibility = Visibility.Collapsed;
            } else
            {
                MessageBox.Show("Datos de acceso incorrectos", "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
