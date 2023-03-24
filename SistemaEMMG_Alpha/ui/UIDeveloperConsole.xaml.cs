﻿using System;
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
    /// Interaction logic for UIDeveloperConsole.xaml
    /// </summary>
    public partial class UIDeveloperConsole : UserControl
    {
        public UIDeveloperConsole()
        {
            InitializeComponent();
        }

        private void txtDeveloperInputConsole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (txtDeveloperInputConsole.Text.Trim().ToLower().Equals("clear"))
                {
                    txtDeveloperDisplayConsole.Text = "";
                }
                else
                {
                    txtDeveloperDisplayConsole.Text += DeveloperConsole.Instance().ProcessInput(txtDeveloperInputConsole.Text);
                }
                txtDeveloperDisplayConsole.ScrollToEnd();
                txtDeveloperInputConsole.Text = "";
            }
        }

        private void txtDeveloperInputConsole_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                txtDeveloperInputConsole.Text = DeveloperConsole.Instance().RetrieveConsoleHistory(txtDeveloperInputConsole.Text, true);
            }
            else if (e.Key == Key.Down)
            {
                txtDeveloperInputConsole.Text = DeveloperConsole.Instance().RetrieveConsoleHistory(txtDeveloperInputConsole.Text, false);
            }
        }
    }
}
