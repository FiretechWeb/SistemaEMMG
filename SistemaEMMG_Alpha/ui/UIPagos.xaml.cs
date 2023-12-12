using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace SistemaEMMG_Alpha.ui
{
    /// <summary>
    /// Interaction logic for UIPagos.xaml
    /// </summary>
    public partial class UIPagos : BaseUCClass
    {
        public DBConnection dbCon = null;
        public DBMain dbData = null;
        private List<DBPago> _listaPagos = null;
        private DBPago _pagoSeleccionado = null;

        public DBCuenta GetCuentaSeleccionada()
        {
            if (GetMainWindow() is null)
            {
                return null;
            }
            return GetMainWindow().GetCuentaSeleccionada();
        }

        public DBPago GetPagoSeleccionado() => _pagoSeleccionado;

        private void SetListaPagos(List<DBPago> newListaPagos) => _listaPagos = newListaPagos;

        private List<DBPago> GetListaPagos() => _listaPagos;


        public void SetPagoSeleccionado(DBPago newPago)
        {
            _pagoSeleccionado = newPago;
            if (newPago is null)
            {
                lblPagoSeleccionado.Content = "No hay pago seleccionado";
                btnModificar.IsEnabled = false;
                btnEliminar.IsEnabled = false;
            }
            else
            {
                lblPagoSeleccionado.Content = _pagoSeleccionado.GetRecibo().GetNumero() + " - " + _pagoSeleccionado.GetImporte().ToString() + "$";
                btnModificar.IsEnabled = true;
                btnEliminar.IsEnabled = true;
            }
        }

        public UIPagos()
        {
            InitializeComponent();
            dbCon = DBConnection.Instance();
            dbData = DBMain.Instance();

            winAgregarModificarPago.SetUIOwner(this);
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
            if (GetCuentaSeleccionada() is null)
            {
                return;
            }
            if (!GetMainWindow().CheckDBConnection())
            {
                return;
            }

            dbData.RefreshBasicDataDB(dbCon.Connection);

            List<DBFormasPago> formasDePagos = DBFormasPago.GetAll();

            cmbFiltroFormaPago.Items.Clear();
            cmbFiltroFormaPago.SelectedValuePath = "Key";
            cmbFiltroFormaPago.DisplayMemberPath = "Value";
            cmbFiltroFormaPago.Items.Add(new KeyValuePair<long, string>(-1, "Todos"));

            foreach(DBFormasPago formaPago in formasDePagos)
            {
                cmbFiltroFormaPago.Items.Add(new KeyValuePair<long, string>(formaPago.GetID(), formaPago.GetName()));
            }

            cmbFiltroFormaPago.SelectedIndex = 0;

            RefreshWithFilter();
        }


        private void RefreshWithFilter()
        {
            if (dbCon is null)
            {
                dbCon = DBConnection.Instance();
            }

            DateTime _fechaInicial = new DateTime();
            DateTime? fechaInicial = null;

            if (DateTime.TryParse(txtFiltroFechaInicial.Text, out _fechaInicial))
            {
                fechaInicial = _fechaInicial;
            }
            DateTime _fechaFinal = new DateTime();
            DateTime? fechaFinal = null;

            if (DateTime.TryParse(txtFiltroFechaFinal.Text, out _fechaFinal))
            {
                fechaFinal = _fechaFinal;
            }

            SetListaPagos(DBPago.Search(
                dbCon.Connection,
                GetCuentaSeleccionada(),
                (cmbFiltroFormaPago.SelectedItem is null) ? null : DBFormasPago.GetByID(((KeyValuePair<long, string>)cmbFiltroFormaPago.SelectedItem).Key),
                fechaInicial,
                fechaFinal,
                txtFiltroEmisor.Text.Trim(),
                txtFiltroReceptor.Text.Trim(),
                txtFiltroNroCheque.Text.Trim(),
                txtFiltroNroRecibo.Text.Trim()
                ));

            dgPagos.Items.Clear();

            foreach (DBPago pago in GetListaPagos())
            {
                dgPagos.Items.Add(new
                {
                    pg_id = pago.GetID(),
                    pg_ec_id = pago.GetEntidadComercialID(),
                    pg_rc_id = pago.GetReciboID(),
                    tipo = pago.GetFormaDePago().GetName(),
                    fecha = pago.GetFecha().HasValue ? ((DateTime)pago.GetFecha()).ToString("dd-MM-yyyy") : "Sin fecha",
                    emisor = pago.GetEmisorRazonSocial(),
                    receptor = pago.GetRecibidoRazonSocial(),
                    recibo = pago.GetRecibo().GetNumero(),
                    cheque = pago.GetCheque().HasValue ? ((ChequeData)pago.GetCheque()).pg_cheque_num.ToString() : "-",
                    importe = $"{pago.GetImporte()} {pago.GetMoneda().GetName()}",
                    importe_recibo = $"{pago.GetRecibo().GetComprobantesTotal_MonedaLocal(dbCon.Connection)} ARS"
                });
                SetPagoSeleccionado(null);
            }

        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            RefreshWithFilter();
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            winAgregarModificarPago.RefreshData();
            winAgregarModificarPago.Visibility = Visibility.Visible;
        }
    }
}
