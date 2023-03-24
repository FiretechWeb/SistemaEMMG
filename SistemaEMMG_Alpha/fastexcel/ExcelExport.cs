using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public class ExcelExport
    {
        public static void ExportToFile(List<DBComprobantes> comprobantes, string fileName)
        {
            MySqlConnection conn = DBConnection.Instance().Connection;
            SLDocument sl = new SLDocument();

            sl.RenameWorksheet(sl.GetCurrentWorksheetName(), "Comprobantes");
            //sl.SelectWorksheet("Comprobantes");
            // set a boolean at "A1"
            int row = 1;

            sl.SetCellValue(row, 1, "Estado");
            sl.SetCellValue(row, 2, "Fecha");
            sl.SetCellValue(row, 3, "CUIT");
            sl.SetCellValue(row, 4, "Tipo");
            sl.SetCellValue(row, 5, "Numero");
            sl.SetCellValue(row, 6, "Razón Social");
            sl.SetCellValue(row, 7, "Gravado");
            sl.SetCellValue(row, 8, "IVA");
            sl.SetCellValue(row, 9, "No Gravado");
            sl.SetCellValue(row, 10, "Percepción");
            sl.SetCellValue(row, 11, "Total");
            row++;

            List<DBRecibo> recibos = new List<DBRecibo>();
            //Creating Worksheet with comprobantes
            double totalEmitido = 0.0;
            double totalRecibido = 0.0;
            double totalIVAEmitido = 0.0;
            double totalIVARecibido = 0.0;
            double totalGravadoRecibido = 0.0;
            double totalGravadoEmitido = 0.0;
            double totalNoGravadoEmitido = 0.0;
            double totalNoGravadoRecibido = 0.0;
            double totalPercepcionRecibido = 0.0;
            double totalPercepcionEmitido = 0.0;
            foreach (DBComprobantes comprobante in comprobantes)
            {
                string fechaString = "";
                if (comprobante.GetFechaEmitido().HasValue)
                {
                    fechaString = ((DateTime)comprobante.GetFechaEmitido()).ToString("dd/MM/yyyy");
                } else
                {
                    fechaString = "Sin fecha";
                }
                sl.SetCellValue(row, 1, comprobante.IsEmitido() ? "Emitido" : "Recibido");
                sl.SetCellValue(row, 2, fechaString);
                sl.SetCellValue(row, 3, comprobante.GetEntidadComercial().GetCUIT());
                sl.SetCellValue(row, 4, comprobante.GetTipoComprobante().GetName());
                sl.SetCellValue(row, 5, comprobante.GetNumeroComprobante());
                sl.SetCellValue(row, 6, comprobante.GetEntidadComercial().GetRazonSocial());
                sl.SetCellValue(row, 7, comprobante.GetGravado());
                sl.SetCellValue(row, 8, comprobante.GetIVA());
                sl.SetCellValue(row, 9, comprobante.GetNoGravado());
                sl.SetCellValue(row, 10, comprobante.GetPercepcion());
                sl.SetCellValue(row, 11, comprobante.GetTotal());
                if (comprobante.IsEmitido())
                {
                    totalEmitido += comprobante.GetTotal();
                    totalIVAEmitido += comprobante.GetIVA();
                    totalGravadoEmitido += comprobante.GetGravado();
                    totalNoGravadoEmitido += comprobante.GetNoGravado();
                    totalPercepcionEmitido += comprobante.GetPercepcion();
                } else
                {
                    totalRecibido += comprobante.GetTotal();
                    totalIVARecibido += comprobante.GetIVA();
                    totalGravadoRecibido += comprobante.GetGravado();
                    totalNoGravadoRecibido += comprobante.GetNoGravado();
                    totalPercepcionRecibido += comprobante.GetPercepcion();
                }
                row++;

                List<DBRecibo> recibosFromComprobante = comprobante.GetAllRecibos(conn);
                foreach (DBRecibo recibo in recibosFromComprobante)
                {
                    if (!DBRecibo.CheckIfExistsInList(recibos, recibo))
                    {
                        recibos.Add(recibo);
                    }
                }
            }
            row++;
            sl.SetCellValue(row, 1, "Total Recibido:");
            sl.SetCellValue(row, 2, totalRecibido);
            sl.SetCellValue(row+1, 1, "Total Emitido:");
            sl.SetCellValue(row + 1, 2, totalEmitido);
            sl.SetCellValue(row, 4, "IVA Recibido:");
            sl.SetCellValue(row, 5, totalIVARecibido);
            sl.SetCellValue(row + 1, 4, "IVA Emitido:");
            sl.SetCellValue(row + 1, 5, totalIVAEmitido);
            sl.SetCellValue(row, 7, "Gravado Recibido:");
            sl.SetCellValue(row, 8, totalGravadoRecibido);
            sl.SetCellValue(row + 1, 7, "Gravado Emitido:");
            sl.SetCellValue(row + 1, 8, totalGravadoEmitido);
            sl.SetCellValue(row, 7, "No Gravado Recibido:");
            sl.SetCellValue(row, 8, totalNoGravadoRecibido);
            sl.SetCellValue(row + 1, 7, "No Gravado Emitido:");
            sl.SetCellValue(row + 1, 8, totalNoGravadoEmitido);
            sl.SetCellValue(row, 10, "Percepción Recibido:");
            sl.SetCellValue(row, 11, totalPercepcionRecibido);
            sl.SetCellValue(row + 1, 10, "Percepción Emitido:");
            sl.SetCellValue(row + 1, 11, totalPercepcionEmitido);
            //Creating Worksheet with recibos
            sl.AddWorksheet("Recibos");
            row = 1;
            foreach (DBRecibo recibo in recibos)
            {
                int maxRow = row;
                int initRow = row;
                List<DBComprobantes> listaComprobantes = recibo.GetAllComprobantes(conn);
                List<DBPago> listaPagos = recibo.GetAllPagos(conn);

                string fechaString = "";
                if (recibo.GetFecha().HasValue)
                {
                    fechaString = ((DateTime)recibo.GetFecha()).ToString("dd/MM/yyyy");
                }
                else
                {
                    fechaString = "Sin fecha";
                }

                sl.SetCellValue(row, 1, "Recibo:");
                sl.SetCellValue(row, 2, recibo.GetNumero());
                sl.SetCellValue(row + 1, 1, "Estado:");
                sl.SetCellValue(row + 1, 2, recibo.IsEmitido() ? "Emitido" : "Recibido");
                sl.SetCellValue(row + 2, 1, "CUIT:");
                sl.SetCellValue(row + 2, 2, recibo.GetEntidadComercial().GetCUIT());
                sl.SetCellValue(row + 3, 1, "Razón Social");
                sl.SetCellValue(row + 3, 2, recibo.GetEntidadComercial().GetRazonSocial());
                sl.SetCellValue(row + 4, 1, "Fecha:");
                sl.SetCellValue(row + 4, 2, fechaString);
                sl.SetCellValue(row + 5, 1, "Observación:");
                sl.SetCellValue(row + 5, 2, recibo.GetObservacion());
                maxRow = row + 9;
                sl.SetCellValue(row, 4, "Detalles");
                sl.SetCellValue(row+1, 4, "Fecha");
                sl.SetCellValue(row+1, 5, "Numero");
                sl.SetCellValue(row+1, 6, "Importe");
                row +=2;
                double totalComprobantes = 0.0;
                foreach(DBComprobantes comprobante in listaComprobantes)
                {
                    string fechaEmitido = "";
                    if (comprobante.GetFechaEmitido().HasValue)
                    {
                        fechaEmitido = ((DateTime)comprobante.GetFechaEmitido()).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        fechaEmitido = "Sin fecha";
                    }

                    sl.SetCellValue(row, 4, fechaEmitido);
                    sl.SetCellValue(row, 5, comprobante.GetNumeroComprobante());
                    sl.SetCellValue(row, 6, comprobante.GetTotal());
                    totalComprobantes += comprobante.GetTotal();
                    row++;
                }
                if (row > maxRow)
                {
                    maxRow = row;
                }
                row = initRow;
                sl.SetCellValue(row + 6, 1, "Total comprobantes:");
                sl.SetCellValue(row + 6, 2, totalComprobantes);

                sl.SetCellValue(row, 8, "Pagos");
                sl.SetCellValue(row + 1, 8, "Fecha");
                sl.SetCellValue(row + 1, 9, "Forma de Pago");
                sl.SetCellValue(row + 1, 10, "Importe");
                sl.SetCellValue(row + 1, 11, "Observación");
                row += 2;

                double totalPagos = 0.0;
                foreach (DBPago pago in listaPagos)
                {
                    string fechaEmitido = "";
                    if (pago.GetFecha().HasValue)
                    {
                        fechaEmitido = ((DateTime)pago.GetFecha()).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        fechaEmitido = "Sin fecha";
                    }

                    sl.SetCellValue(row, 8, fechaEmitido);
                    sl.SetCellValue(row, 9, pago.GetFormaDePago().GetName());
                    sl.SetCellValue(row, 10, pago.GetImporte());
                    sl.SetCellValue(row, 11, pago.GetObservacion());
                    totalPagos += pago.GetImporte();
                    row++;
                }
                if (row > maxRow)
                {
                    maxRow = row;
                }
                row = initRow;

                sl.SetCellValue(row + 7, 1, "Total pagos:");
                sl.SetCellValue(row + 7, 2, totalPagos);

                row = maxRow;
            }
            sl.SaveAs($"{fileName}.xlsx");
            //sl.SelectWorksheet("Sheet1");
        }
    }
}
