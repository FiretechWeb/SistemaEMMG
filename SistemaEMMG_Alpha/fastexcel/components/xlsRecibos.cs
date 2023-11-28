using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public static partial class ExcelComponents
    {
        public static void CreateRecibosWorksheet(SLDocument sl, List<DBRecibo> recibos, MySqlConnection conn, int row = 1)
        {
            SLStyle styleDefaultText = ExcelStyles.defaultTextStyle(sl);
            SLStyle styleTitle = ExcelStyles.titleTextStyle(sl);
            SLStyle styleARS = ExcelStyles.ARSTextStyle(sl);
            SLStyle styleUSD = ExcelStyles.USDTextStyle(sl);

            sl.AddWorksheet("Recibos");

            foreach (DBRecibo recibo in recibos)
            {
                int maxRow = row;
                int initRow = row;

                List<DBComprobantes> listaComprobantes = recibo.GetAllComprobantes(conn);
                List<DBPago> listaPagos = recibo.GetAllPagos(conn);

                sl.CopyCellDataToRow(row, GetRecibosHeadersCellsData(listaComprobantes, listaPagos, recibo, conn, styleDefaultText, styleTitle, styleARS));

                maxRow = row + 9;
                row += 2;

                foreach (DBComprobantes comprobante in listaComprobantes)
                {
                    sl.CopyCellDataToRow(row, GetRecibosComprobanteCellData(comprobante, conn, styleDefaultText, styleARS, styleUSD));
                    row++;
                }

                if (row > maxRow)
                {
                    maxRow = row;
                }
                row = initRow;
                row += 2;

                foreach (DBPago pago in listaPagos)
                {
                    sl.CopyCellDataToRow(row, GetRecibosPagoCellData(pago, styleDefaultText, styleARS, styleUSD));
                    row++;
                }

                if (row > maxRow)
                {
                    maxRow = row;
                }
                row = initRow;

                SetRecibosWorksheetBorders(sl, initRow, maxRow);

                row = maxRow + 1; //add EVEN more padding
            }

            for (int i = 1; i < 18; i++)
            {
                sl.AutoFitColumn(i);
            }
        }

        private static CellData[] GetRecibosHeadersCellsData(List<DBComprobantes> comprobantes, List<DBPago> pagos, DBRecibo recibo, MySqlConnection conn, SLStyle styleDefaultText = null, SLStyle styleTitle = null, SLStyle styleARS = null)
        {
            string fechaString = "";
            if (recibo.GetFecha().HasValue)
            {
                fechaString = ((DateTime)recibo.GetFecha()).ToString("dd/MM/yyyy");
            }
            else
            {
                fechaString = "Sin fecha";
            }

            CellData[] listOfCells =
            {
                new CellData(0, 1, "Recibo:", styleTitle),
                new CellData(0, 2, recibo.GetNumero(), styleDefaultText),
                new CellData(1, 1, "Estado:", styleTitle),
                new CellData(1, 2, recibo.IsEmitido() ? "Emitido" : "Recibido", styleDefaultText),
                new CellData(2, 1, "CUIT:", styleTitle),
                new CellData(2, 2, recibo.GetEntidadComercial().GetCUIT(), styleDefaultText),
                new CellData(3, 1, "Razón Social", styleTitle),
                new CellData(3, 2, recibo.GetEntidadComercial().GetRazonSocial(), styleDefaultText),
                new CellData(4, 1, "Fecha:", styleTitle),
                new CellData(4, 1, fechaString, styleDefaultText),
                new CellData(5, 1, "Observación:", styleTitle),
                new CellData(5, 2, recibo.GetObservacion(), styleDefaultText),
                new CellData(6, 1, "Total comprobantes:", styleTitle),
                new CellData(6, 2, DBComprobantes.GetTotalReal_MonedaLocal(comprobantes, conn), styleARS),
                new CellData(7, 1, "Total pagos:", styleTitle),
                new CellData(7, 2, DBPago.GetTotal_MonedaLocal(pagos), styleARS),

                new CellData(0, 4, "Detalles", styleTitle),
                new CellData(1, 4, "Fecha", styleTitle),
                new CellData(1, 5, "Numero", styleTitle),
                new CellData(1, 6, "Moneda", styleTitle),
                new CellData(1, 7, "Cambio", styleTitle),
                new CellData(1, 8, "Importe (Mon. Loc.)", styleTitle),
                new CellData(1, 9, "Importe (Mon. Ext.)", styleTitle),

                new CellData(0, 11, "Pagos", styleTitle),
                new CellData(1, 11, "Fecha", styleTitle),
                new CellData(1, 12, "Forma de Pago", styleTitle),
                new CellData(1, 13, "Moneda", styleTitle),
                new CellData(1, 14, "Cambio", styleTitle),
                new CellData(1, 15, "Importe (Mon. Local)", styleTitle),
                new CellData(1, 16, "Importe (Mon. Ext)", styleTitle),
                new CellData(1, 17, "Observación", styleTitle),
            };

            return listOfCells;
        }

        private static CellData[] GetRecibosPagoCellData(DBPago pago, SLStyle styleDefaultText = null, SLStyle styleARS = null, SLStyle styleUSD = null)
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

            CellData[] listOfCells =
            {
                new CellData(11, fechaEmitido, styleDefaultText),
                new CellData(12, pago.GetFormaDePago().GetName(), styleDefaultText),
                new CellData(13, pago.GetMoneda().GetName(), styleDefaultText),
                new CellData(14, pago.GetCambio(), styleARS),
                new CellData(15, pago.GetImporte_MonedaLocal(), styleARS),
                new CellData(16, pago.GetMoneda().IsExtranjera() ? pago.GetImporte() : 0.0, styleUSD),
                new CellData(17, pago.GetObservacion(), styleUSD)
            };

            return listOfCells;
        }

        private static CellData[] GetRecibosComprobanteCellData(DBComprobantes comprobante, MySqlConnection conn, SLStyle styleDefaultText = null, SLStyle styleARS = null, SLStyle styleUSD = null)
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

            CellData[] listOfCells =
            {
                new CellData(4, fechaEmitido, styleDefaultText),
                new CellData(5, comprobante.GetNumeroComprobante(), styleDefaultText),
                new CellData(6, comprobante.GetMoneda().GetName(), styleDefaultText),
                new CellData(7, comprobante.GetCambio(), styleARS),
                new CellData(8, comprobante.GetTotalReal_MonedaLocal(conn), styleARS),
                new CellData(9, comprobante.GetMoneda().IsExtranjera() ? comprobante.GetTotal() : 0, styleUSD),
            };

            return listOfCells;
        }

        private static void SetRecibosWorksheetBorders(SLDocument sl, int initRow, int maxRow)
        {
                int[] startBorder = { initRow, 1 };
                int[] endBorder = { maxRow - 1, 17 };

                for (int i = startBorder[0]; i < endBorder[0]; i++)
                {
                    for (int j = startBorder[1]; j <= endBorder[1]; j++)
                    {
                        SLStyle richStyle = sl.GetCellStyle(i, j);
                        richStyle.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
                        richStyle.Border.BottomBorder.Color = System.Drawing.Color.Black;
                        richStyle.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                        richStyle.Border.LeftBorder.Color = System.Drawing.Color.Black;
                        richStyle.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                        richStyle.Border.RightBorder.Color = System.Drawing.Color.Black;
                        richStyle.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                        richStyle.Border.TopBorder.Color = System.Drawing.Color.Black;

                        if ((j != startBorder[1] + 2) && (j != startBorder[1] + 9))
                        {
                            if (j == startBorder[1] || ((i < startBorder[0] + 2) && (j > startBorder[1] + 2)))
                            {
                                richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                            }
                            else
                            {
                                richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                            }
                        }
                        sl.SetCellStyle(i, j, richStyle);
                    }
                }
        }
    }
}
