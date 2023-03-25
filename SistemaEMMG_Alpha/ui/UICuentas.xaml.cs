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
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UICuentas.xaml
    /// </summary>
    public partial class UICuentas : UserControl
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;

        private MainWindow _mainWin = null;
        public void SetMainWindow(MainWindow mainWin)
        {
            _mainWin = mainWin;
        }

        public DBCuenta GetCuentaSeleccionada()
        {
            if (_mainWin is null)
            {
                return null;
            }
            return _mainWin.GetCuentaSeleccionada();
        }

        public MainWindow GetMainWindow() => _mainWin;

        public void RefreshData()
        {
            if (_mainWin is null)
            {
                return;
            }
            if (dbData is null)
            {
                dbData = DBMain.Instance();
            }
            if (dbCon is null)
            {
                dbCon = DBConnection.Instance();
            }

            if (!_mainWin.CheckDBConnection())
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);
            List<DBCuenta> cuentas = DBCuenta.GetAll();
            listCuentas.Items.Clear();
            cmbCuentas.Items.Clear();
            listCuentas.SelectedValuePath = "Key";
            listCuentas.DisplayMemberPath = "Value";
            cmbCuentas.SelectedValuePath = "Key";
            cmbCuentas.DisplayMemberPath = "Value";
            foreach (DBCuenta cuenta in cuentas)
            {
                cmbCuentas.Items.Add(new KeyValuePair<long, string>(cuenta.GetID(), cuenta.GetRazonSocial()));
                listCuentas.Items.Add(new KeyValuePair<long, string>(cuenta.GetID(), $"{cuenta.GetCUIT()}: {cuenta.GetRazonSocial()}"));
            }

            DBCuenta cuentaSeleccionada = _mainWin.GetCuentaSeleccionada();

            if (GetCuentaSeleccionada() is null)
            {
                if (cuentas.Count > 0)
                {
                    listCuentas.SelectedIndex = 0;
                    cmbCuentas.SelectedIndex = 0;
                    _mainWin.SetCuentaSeleccionada(DBCuenta.GetByID(((KeyValuePair<long, string>)listCuentas.SelectedItem).Key));
                    btnEliminarCuenta.IsEnabled = true;
                    btnModificarCuenta.IsEnabled = true;
                } else
                {
                    listCuentas.SelectedIndex = -1;
                    cmbCuentas.SelectedIndex = -1;
                    btnEliminarCuenta.IsEnabled = false;
                    btnModificarCuenta.IsEnabled = false;
                }
            } else
            {
                listCuentas.SelectedValue = GetCuentaSeleccionada().GetID();
                cmbCuentas.SelectedValue = GetCuentaSeleccionada().GetID();
                btnEliminarCuenta.IsEnabled = true;
                btnModificarCuenta.IsEnabled = true;
            }
        }

        public UICuentas()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();
            uiAgregarModificarCuenta.SetUIOwner(this);

        }

        private void btnCrearNuevaCuenta_Click(object sender, RoutedEventArgs e)
        {
            uiAgregarModificarCuenta.RefreshData();
            uiAgregarModificarCuenta.Visibility = Visibility.Visible;
        }

        private void btnModificarCuenta_Click(object sender, RoutedEventArgs e)
        {
            DBCuenta cuentaAModificar = DBCuenta.GetByID(((KeyValuePair<long, string>)listCuentas.SelectedItem).Key);

            Trace.Assert(!(cuentaAModificar is null));

            uiAgregarModificarCuenta.RefreshData(cuentaAModificar);
            uiAgregarModificarCuenta.Visibility = Visibility.Visible;
        }

        private void btnEliminarCuenta_Click(object sender, RoutedEventArgs e)
        {
            DBCuenta cuentaAEliminar = DBCuenta.GetByID(((KeyValuePair<long, string>)listCuentas.SelectedItem).Key);

            Trace.Assert(!(cuentaAEliminar is null));

            MessageBoxResult msgBoxConfirmationResult = System.Windows.MessageBox.Show("¿Está seguro que desea eliminar está cuenta?, se eliminaran todos los comprobantes, recibos y remitos relacionados a esta cuenta.", "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo);
            if (msgBoxConfirmationResult == MessageBoxResult.Yes)
            {
                bool isSelectedCuenta = (cuentaAEliminar.GetID() == GetCuentaSeleccionada().GetID());
                if (cuentaAEliminar.DeleteFromDatabase(dbCon.Connection))
                {
                    MessageBox.Show("Cuenta eliminada exitosamente");
                    if (isSelectedCuenta)
                    {
                        _mainWin.SetCuentaSeleccionada(null);
                    }
                    RefreshData();
                }  else
                {
                    MessageBox.Show("Error tratando de eliminar la cuenta.");
                }
            }
        }
    }
}
