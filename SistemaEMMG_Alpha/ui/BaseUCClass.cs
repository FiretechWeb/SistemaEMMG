using System.Windows.Controls;

namespace SistemaEMMG_Alpha.ui
{
    public class BaseUCClass : UserControl
    {
        private MainWindow _mainWin = null;
        public void SetMainWindow(MainWindow mainWin)
        {
            _mainWin = mainWin;
        }
        public MainWindow GetMainWindow() => _mainWin;
    }
}
