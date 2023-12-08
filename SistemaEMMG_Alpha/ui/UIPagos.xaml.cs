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

            List<DBPago> listaPagos = DBPago.GetAll(dbCon.Connection, GetCuentaSeleccionada());

            dgPagos.Items.Clear();

            foreach (DBPago pago in listaPagos)
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
    }
}
