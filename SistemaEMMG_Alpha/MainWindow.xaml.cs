﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SistemaEMMG_Alpha
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        enum TabItemsSelections
        {
            TI_CUENTAS,
            TI_ENTIDADES,
            TI_COMPROBANTES,
            TI_RECIBOS,
            TI_REMITOS,
            TI_PAGOS
        };
        public short oldTabItemSelection = -1; //To avoid bug with Tab Items

        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private DBCuenta _cuentaSeleccionada = null;
        public DBCuenta GetCuentaSeleccionada() => _cuentaSeleccionada;

        public void SetCuentaSeleccionada(DBCuenta newCuenta)
        {
            _cuentaSeleccionada = newCuenta;
            if (newCuenta is null)
            {
                disableTabItems();
                lblCuentaSeleccionada.Content = "No hay ninguna cuenta seleccionada.";
            }
            else
            {
                refreshTabItems();
                lblCuentaSeleccionada.Content = _cuentaSeleccionada.GetRazonSocial();
            }
        }

        private void enableTabItems()
        {
            tabComprobantes.IsEnabled = true;
            tabEntidades.IsEnabled = true;
            tabRecibos.IsEnabled = true;
            tabRemitos.IsEnabled = true;
            tabPagos.IsEnabled = true;
        }
        private void disableTabItems()
        {
            tabComprobantes.IsEnabled = false;
            tabEntidades.IsEnabled = false;
            tabRecibos.IsEnabled = false;
            tabRemitos.IsEnabled = false;
            tabPagos.IsEnabled = false;
        }

        public void refreshTabItems()
        {
            if (GetCuentaSeleccionada() is null)
            {
                disableTabItems();
                return;
            }
            enableTabItems();
            //_cuentaSeleccionada
            List<DBEntidades> entidadesComerciales = DBEntidades.GetAll(dbCon.Connection, GetCuentaSeleccionada());

            if (entidadesComerciales.Count <= 0)
            {
                tabComprobantes.IsEnabled = false;
                tabRecibos.IsEnabled = false;
                tabRemitos.IsEnabled = false;
                tabPagos.IsEnabled = false;
            }
        }

        public bool ConnectWithDatabase()
        {
            if (!(dbCon is null))
            {
                return true;
            }
            bool sucessfulConnected = false;
            try
            {
                dbCon = DBConnection.Instance();
                dbCon.Server = Config.GetGlobalConfig().GetDatabaseData().hostName;
                dbCon.DatabaseName = Config.GetGlobalConfig().GetDatabaseData().databaseName;
                dbCon.UserName = Config.GetGlobalConfig().GetDatabaseData().userName;
                dbCon.Password = Config.GetGlobalConfig().GetDatabaseData().userPassword;
                if (dbCon.StartConnection())
                {
                    sucessfulConnected = true;
                } else
                {
                    sucessfulConnected = false;
                    dbCon.Connection = null;
                    dbCon = null;
                }
            } catch (Exception ex)
            {
                sucessfulConnected = false;
                dbCon.Connection = null;
                dbCon = null;
                Console.WriteLine($"Error connecting with database: {ex}");
            }
            return sucessfulConnected;
        }


        public void SoftwareMain()
        {
            Console.WriteLine("Able to connect Database!");

            //Intialize data
            dbData = DBMain.Instance();
            dbData.RefreshBasicDataDB(dbCon.Connection);
            List<DBCuenta> cuentasDisponibles = DBCuenta.GetAll();
            if (cuentasDisponibles.Count > 0)
            {
                SetCuentaSeleccionada(cuentasDisponibles[0]);
            } else
            {
                SetCuentaSeleccionada(null);
            }
            uiCuentasPanel.RefreshData();
        }
        public void InitUserControls()
        {
            errorScreen.SetMainWindow(this);
            uiCuentasPanel.SetMainWindow(this);
            uiEntidadesPanel.SetMainWindow(this);
            uiComprobantespanel.SetMainWindow(this);
            uiRecibosPanel.SetMainWindow(this);
            uiRemitosPanel.SetMainWindow(this);
            uiPagosPanel.SetMainWindow(this);
            configWin.SetMainWindow(this);
            accessWin.SetMainWindow(this);
            developerWin.SetMainWindow(this);
        }
        
        public MainWindow()
        {
            InitializeComponent();
            InitUserControls();

            //Load CFG file
            Config.SetGlobalConfig(new Config(Config.GetDefaultConfigFileName()));

           if (!ConnectWithDatabase())
            {
                errorScreen.Visibility = Visibility.Visible;
                Console.WriteLine("Connecting with database failed!");
            } else
            {
                errorScreen.Visibility = Visibility.Collapsed;
                SoftwareMain();
            }
           
        }

        public bool CheckDBConnection()
        {
            if (dbCon.IsConnected())
            {
                return true;
            }
            errorScreen.Visibility = Visibility.Visible;
            return false;
        }

        private void btnBackupDB_Click(object sender, RoutedEventArgs e)
        {
            System.IO.Directory.CreateDirectory("backups");
            DateTime dt = DateTime.Now;
            dbCon.Backup($"backups/dbbackup_{dt.ToString("dd_MM_yyyy")}.sql");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.F11) && !(User.GetCurrentUser() is null) && User.GetCurrentUser().IsAdmin())
            {
                developerWin.Visibility = developerWin.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                short newTabItemSelection = oldTabItemSelection;
                if (tabComprobantes.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_COMPROBANTES;
                }
                else if (tabCuentas.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_CUENTAS;
                }
                else if (tabEntidades.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_ENTIDADES;
                } else if (tabRecibos.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_RECIBOS;
                }
                else if (tabRemitos.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_REMITOS;
                } else if (tabPagos.IsSelected)
                {
                    newTabItemSelection = (short)TabItemsSelections.TI_PAGOS;
                }

                if (oldTabItemSelection == newTabItemSelection || oldTabItemSelection == -1)
                {
                    oldTabItemSelection = newTabItemSelection;
                    return;
                }

                oldTabItemSelection = newTabItemSelection;

                switch ((TabItemsSelections)newTabItemSelection)
                {
                    case TabItemsSelections.TI_CUENTAS:
                        uiCuentasPanel.RefreshData();
                        Console.WriteLine("TI_CUENTAS");
                        break;
                    case TabItemsSelections.TI_ENTIDADES:
                        uiEntidadesPanel.RefreshData();
                        Console.WriteLine("TI_ENTIDADES");
                        break;
                    case TabItemsSelections.TI_COMPROBANTES:
                        uiComprobantespanel.RefreshData();
                        Console.WriteLine("TI_COMPROBANTES");
                        break;
                    case TabItemsSelections.TI_REMITOS:
                        uiRemitosPanel.RefreshData();
                        Console.WriteLine("TI_REMITOS");
                        break;
                    case TabItemsSelections.TI_RECIBOS:
                        uiRecibosPanel.RefreshData();
                        Console.WriteLine("TI_RECIBOS");
                        break;
                    case TabItemsSelections.TI_PAGOS:
                        uiPagosPanel.RefreshData();
                        Console.WriteLine("TI_PAGOS");
                        break;
                }
            }
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            configWin.RefreshData();
            configWinWrapper.Visibility = Visibility.Visible;
        }
    }
}