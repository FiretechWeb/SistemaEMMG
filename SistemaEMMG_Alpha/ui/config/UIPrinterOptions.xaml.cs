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

            //TODO
        }
    }
}
