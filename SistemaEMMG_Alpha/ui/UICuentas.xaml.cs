using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UICuentas.xaml
    /// </summary>
    public partial class UICuentas : BaseUCClass
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;


        public DBCuenta GetCuentaSeleccionada()
        {
            
            if (GetMainWindow() is null)
            {
                return null;
            }
            return GetMainWindow().GetCuentaSeleccionada();
        }

        public void RefreshData()
        {
            if (GetMainWindow() is null)
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

            if (!GetMainWindow().CheckDBConnection())
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

            DBCuenta cuentaSeleccionada = GetMainWindow().GetCuentaSeleccionada();

            if (GetCuentaSeleccionada() is null)
            {
                if (cuentas.Count > 0)
                {
                    listCuentas.SelectedIndex = 0;
                    cmbCuentas.SelectedIndex = 0;
                    GetMainWindow().SetCuentaSeleccionada(DBCuenta.GetByID(((KeyValuePair<long, string>)listCuentas.SelectedItem).Key));
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

            MessageBoxResult msgBoxConfirmationResult = System.Windows.MessageBox.Show("¿Está seguro que desea eliminar está cuenta?, se eliminaran todas las entidades comerciales, los comprobantes, recibos y remitos relacionados a esta cuenta.", "Confirmar eliminación", System.Windows.MessageBoxButton.YesNo);
            if (msgBoxConfirmationResult == MessageBoxResult.Yes)
            {
                bool isSelectedCuenta = (cuentaAEliminar.GetID() == GetCuentaSeleccionada().GetID());
                if (cuentaAEliminar.DeleteFromDatabase(dbCon.Connection))
                {
                    MessageBox.Show("Cuenta eliminada exitosamente");
                    if (isSelectedCuenta)
                    {
                        GetMainWindow().SetCuentaSeleccionada(null);
                    }
                    RefreshData();
                }  else
                {
                    MessageBox.Show("Error tratando de eliminar la cuenta.");
                }
            }
        }

        private void cmbCuentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbSender = sender as ComboBox;
            if (cmbSender.SelectedItem is null)
            {
                return;
            }
            DBCuenta newCuentaSelected = DBCuenta.GetByID(((KeyValuePair<long, string>)cmbSender.SelectedItem).Key);
            if (newCuentaSelected is null)
            {
                return;
            }
            GetMainWindow().SetCuentaSeleccionada(newCuentaSelected);
            //TODO: Refresh add Comprobantes, add Remitos, add Recibos and all UI to avoid issues.
        }
    }
}
