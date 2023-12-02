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
    /// Interaction logic for UIGraphicOptions.xaml
    /// </summary>

    public partial class UIGraphicOptions : UserControl
    {
        public UIGraphicOptions()
        {
            InitializeComponent();
        }

        public void RefreshData()
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            cmbOptionFontSize.SelectedIndex = (int)Config.GetGlobalConfig().GetFontSize();
            cmbUITheme.SelectedIndex = (int)Config.GetGlobalConfig().GetVisualTheme();
        }

        private void btnGraphicsSave_Click(object sender, RoutedEventArgs e)
        {
            if (Config.GetGlobalConfig() is null)
            {
                return;
            }

            Config.GetGlobalConfig().SetFontSize((TypeFontSize)cmbOptionFontSize.SelectedIndex);
            Config.GetGlobalConfig().SetVisualTheme((TypeVisualTheme)cmbUITheme.SelectedIndex);

            Config.GetGlobalConfig().ExportToJSONFile(Config.GetDefaultConfigFileName());
        }
    }
}
