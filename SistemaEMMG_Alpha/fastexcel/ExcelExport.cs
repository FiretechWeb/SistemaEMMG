using System;
using System.Collections.Generic;
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

            using (SLDocument sl = new SLDocument())
            {
                SLStyle styleDefaultText = ExcelStyles.defaultTextStyle(sl);
                SLStyle styleTitle = ExcelStyles.titleTextStyle(sl);
                SLStyle styleSaldoARS = ExcelStyles.saldoARSTextStyle(sl);
                SLStyle styleARS = ExcelStyles.ARSTextStyle(sl);
                SLStyle styleUSD = ExcelStyles.USDTextStyle(sl);

                sl.RenameWorksheet(sl.GetCurrentWorksheetName(), "Comprobantes");
                //sl.SelectWorksheet("Comprobantes");

                int row = 1;

                sl.CopyCellDataToRow(row, GetComprobantesHeadersCells(styleTitle));
                row++;

                List<DBRecibo> recibos = new List<DBRecibo>();

                foreach (DBComprobantes comprobante in comprobantes)
                {
                    sl.CopyCellDataToRow(row, GetDBComprobanteAsCells(comprobante, comprobante.IsPago(conn), styleDefaultText, styleARS, styleUSD));

                    //Getting a list of all recibos associated with the comprobantes we are exporting.
                    List<DBRecibo> recibosFromComprobante = comprobante.GetAllRecibos();
                    foreach (DBRecibo recibo in recibosFromComprobante)
                    {
                        if (!DBRecibo.CheckIfExistsInList(recibos, recibo))
                        {
                            recibos.Add(recibo);
                        }
                    }

                    row++;
                }

                StylingComprobantesCells(sl, 1, row, 1, 20);

                row++;

                //Keep refactoring this mess from here.

                List<DBComprobantes> comprobantesRecibidos = DBComprobantes.GetAllRecibidos(comprobantes);
                List<DBComprobantes> comprobantesEmitidos = DBComprobantes.GetAllEmitidos(comprobantes);

                sl.SetCellValue(row, 1, "Total Recibido:");
                sl.SetCellStyle(row, 1, styleTitle);
                sl.SetCellValue(row, 2, DBComprobantes.GetTotal_MonedaLocal(comprobantesRecibidos));
                sl.SetCellStyle(row, 2, styleARS);
                sl.SetCellValue(row + 1, 1, "Total Emitido:");
                sl.SetCellStyle(row + 1, 1, styleTitle);
                sl.SetCellValue(row + 1, 2, DBComprobantes.GetTotal_MonedaLocal(comprobantesEmitidos));
                sl.SetCellStyle(row + 1, 2, styleARS);
                sl.SetCellValue(row + 2, 1, "Saldo:");
                sl.SetCellStyle(row + 2, 1, styleTitle);
                sl.SetCellValue(row + 2, 2, DBComprobantes.GetSaldoTotal_MonedaLocal(comprobantes));
                sl.SetCellStyle(row + 2, 2, styleSaldoARS);

                for (int i = row; i <= row+2; i++)
                {
                    for (int j = 1; j <= 2; j++)
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

                        if (j == 1)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                        }
                        else
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                        }

                        sl.SetCellStyle(i, j, richStyle);
                    }
                }

                sl.SetCellValue(row, 4, "IVA Recibido:");
                sl.SetCellStyle(row, 4, styleTitle);
                sl.SetCellValue(row, 5, DBComprobantes.GetTotalIVA_MonedaLocal(comprobantesRecibidos));
                sl.SetCellStyle(row, 5, styleARS);
                sl.SetCellValue(row + 1, 4, "IVA Emitido:");
                sl.SetCellStyle(row + 1, 4, styleTitle);
                sl.SetCellValue(row + 1, 5, DBComprobantes.GetTotalIVA_MonedaLocal(comprobantesEmitidos));
                sl.SetCellStyle(row + 1, 5, styleARS);
                sl.SetCellValue(row + 2, 4, "Saldo IVA:");
                sl.SetCellStyle(row + 2, 4, styleTitle);
                sl.SetCellValue(row + 2, 5, DBComprobantes.GetSaldoIVA_MonedaLocal(comprobantes));
                sl.SetCellStyle(row + 2, 5, styleSaldoARS);

                for (int i = row; i <= row + 2; i++)
                {
                    for (int j = 4; j <= 5; j++)
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

                        if (j == 4)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                        }
                        else
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                        }

                        sl.SetCellStyle(i, j, richStyle);
                    }
                }

                sl.SetCellValue(row, 7, "Gravado Recibido:");
                sl.SetCellStyle(row, 7, styleTitle);
                sl.SetCellValue(row, 8, DBComprobantes.GetTotalGravado_MonedaLocal(comprobantesRecibidos));
                sl.SetCellStyle(row, 8, styleARS);
                sl.SetCellValue(row + 1, 7, "Gravado Emitido:");
                sl.SetCellStyle(row + 1, 7, styleTitle);
                sl.SetCellValue(row + 1, 8, DBComprobantes.GetTotalGravado_MonedaLocal(comprobantesEmitidos));
                sl.SetCellStyle(row + 1, 8, styleARS);

                for (int i = row; i <= row + 1; i++)
                {
                    for (int j = 7; j <= 8; j++)
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

                        if (j == 7)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                        }
                        else
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                        }

                        sl.SetCellStyle(i, j, richStyle);
                    }
                }

                sl.SetCellValue(row, 10, "No Gravado Recibido:");
                sl.SetCellStyle(row, 10, styleTitle);
                sl.SetCellValue(row, 11, DBComprobantes.GetTotalNoGravado_MonedaLocal(comprobantesRecibidos));
                sl.SetCellStyle(row, 11, styleARS);
                sl.SetCellValue(row + 1, 10, "No Gravado Emitido:");
                sl.SetCellStyle(row + 1, 10, styleTitle);
                sl.SetCellValue(row + 1, 11, DBComprobantes.GetTotalNoGravado_MonedaLocal(comprobantesEmitidos));
                sl.SetCellStyle(row + 1, 11, styleARS);

                for (int i = row; i <= row + 1; i++)
                {
                    for (int j = 10; j <= 11; j++)
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

                        if (j == 10)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                        }
                        else
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                        }

                        sl.SetCellStyle(i, j, richStyle);
                    }
                }

                sl.SetCellValue(row, 13, "Percepción Recibido:");
                sl.SetCellStyle(row, 13, styleTitle);
                sl.SetCellValue(row, 14, DBComprobantes.GetTotalPercepcion_MonedaLocal(comprobantesRecibidos));
                sl.SetCellStyle(row, 14, styleARS);
                sl.SetCellValue(row + 1, 13, "Percepción Emitido:");
                sl.SetCellStyle(row + 1, 13, styleTitle);
                sl.SetCellValue(row + 1, 14, DBComprobantes.GetTotalPercepcion_MonedaLocal(comprobantesEmitidos));
                sl.SetCellStyle(row + 1, 14, styleARS);


                for (int i = row; i <= row + 1; i++)
                {
                    for (int j = 13; j <= 14; j++)
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

                        if (j == 13)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                        }
                        else
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                        }

                        sl.SetCellStyle(i, j, richStyle);
                    }
                }
                for (int i = 1; i < 20; i++) {
                    sl.AutoFitColumn(i);
                }

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
                    sl.SetCellStyle(row, 1, styleTitle);
                    sl.SetCellValue(row, 2, recibo.GetNumero());
                    sl.SetCellStyle(row, 2, styleDefaultText);

                    sl.SetCellValue(row + 1, 1, "Estado:");
                    sl.SetCellStyle(row + 1, 1, styleTitle);
                    sl.SetCellValue(row + 1, 2, recibo.IsEmitido() ? "Emitido" : "Recibido");
                    sl.SetCellStyle(row + 1, 2, styleDefaultText);

                    sl.SetCellValue(row + 2, 1, "CUIT:");
                    sl.SetCellStyle(row + 2, 1, styleTitle);
                    sl.SetCellValue(row + 2, 2, recibo.GetEntidadComercial().GetCUIT());
                    sl.SetCellStyle(row + 2, 2, styleDefaultText);

                    sl.SetCellValue(row + 3, 1, "Razón Social");
                    sl.SetCellStyle(row + 3, 1, styleTitle);
                    sl.SetCellValue(row + 3, 2, recibo.GetEntidadComercial().GetRazonSocial());
                    sl.SetCellStyle(row + 3, 2, styleDefaultText);

                    sl.SetCellValue(row + 4, 1, "Fecha:");
                    sl.SetCellStyle(row + 4, 1, styleTitle);
                    sl.SetCellValue(row + 4, 2, fechaString);
                    sl.SetCellStyle(row + 4, 2, styleDefaultText);

                    sl.SetCellValue(row + 5, 1, "Observación:");
                    sl.SetCellStyle(row + 5, 1, styleTitle);
                    sl.SetCellValue(row + 5, 2, recibo.GetObservacion());
                    sl.SetCellStyle(row + 5, 2, styleDefaultText);

                    maxRow = row + 9;
                    sl.SetCellValue(row, 4, "Detalles");
                    sl.SetCellStyle(row, 4, styleTitle);

                    sl.SetCellValue(row + 1, 4, "Fecha");
                    sl.SetCellStyle(row + 1, 4, styleTitle);

                    sl.SetCellValue(row + 1, 5, "Numero");
                    sl.SetCellStyle(row + 1, 5, styleTitle);

                    sl.SetCellValue(row + 1, 6, "Moneda");
                    sl.SetCellStyle(row + 1, 6, styleTitle);

                    sl.SetCellValue(row + 1, 7, "Cambio");
                    sl.SetCellStyle(row + 1, 7, styleTitle);

                    sl.SetCellValue(row + 1, 8, "Importe (Mon. Loc.)");
                    sl.SetCellStyle(row + 1, 8, styleTitle);

                    sl.SetCellValue(row + 1, 9, "Importe (Mon. Ext.)");
                    sl.SetCellStyle(row + 1, 9, styleTitle);

                    row += 2;
                    foreach (DBComprobantes comprobante in listaComprobantes)
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
                        sl.SetCellStyle(row, 4, styleDefaultText);

                        sl.SetCellValue(row, 5, comprobante.GetNumeroComprobante());
                        sl.SetCellStyle(row, 5, styleDefaultText);

                        sl.SetCellValue(row, 6, comprobante.GetMoneda().GetName());
                        sl.SetCellStyle(row, 6, styleDefaultText);

                        sl.SetCellValue(row, 7, comprobante.GetCambio());
                        sl.SetCellStyle(row, 7, styleARS);

                        sl.SetCellValue(row, 8, comprobante.GetTotalReal_MonedaLocal(conn));
                        sl.SetCellStyle(row, 8, styleARS);

                        sl.SetCellStyle(row, 9, styleUSD);
                        if (comprobante.GetMoneda().IsExtranjera())
                        {
                            sl.SetCellValue(row, 9, comprobante.GetTotal());
                        }
                        else
                        {
                            sl.SetCellValue(row, 9, "");
                        }
                        row++;
                    }
                    if (row > maxRow)
                    {
                        maxRow = row;
                    }
                    row = initRow;

                    sl.SetCellValue(row + 6, 1, "Total comprobantes:");
                    sl.SetCellStyle(row + 6, 1, styleTitle);

                    sl.SetCellValue(row + 6, 2, DBComprobantes.GetTotalReal_MonedaLocal(listaComprobantes, conn));
                    sl.SetCellStyle(row + 6, 2, styleARS);

                    sl.SetCellValue(row, 11, "Pagos");
                    sl.SetCellStyle(row, 11, styleTitle);

                    sl.SetCellValue(row + 1, 11, "Fecha");
                    sl.SetCellStyle(row + 1, 11, styleTitle);

                    sl.SetCellValue(row + 1, 12, "Forma de Pago");
                    sl.SetCellStyle(row + 1, 12, styleTitle);

                    sl.SetCellValue(row + 1, 13, "Moneda");
                    sl.SetCellStyle(row + 1, 13, styleTitle);

                    sl.SetCellValue(row + 1, 14, "Cambio");
                    sl.SetCellStyle(row + 1, 14, styleTitle);

                    sl.SetCellValue(row + 1, 15, "Importe (Mon. Local)");
                    sl.SetCellStyle(row + 1, 15, styleTitle);

                    sl.SetCellValue(row + 1, 16, "Importe (Mon. Ext)");
                    sl.SetCellStyle(row + 1, 16, styleTitle);

                    sl.SetCellValue(row + 1, 17, "Observación");
                    sl.SetCellStyle(row + 1, 17, styleTitle);

                    row += 2;

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

                        sl.SetCellValue(row, 11, fechaEmitido);
                        sl.SetCellStyle(row, 11, styleDefaultText);

                        sl.SetCellValue(row, 12, pago.GetFormaDePago().GetName());
                        sl.SetCellStyle(row, 12, styleDefaultText);

                        sl.SetCellValue(row, 13, pago.GetMoneda().GetName());
                        sl.SetCellStyle(row, 13, styleDefaultText);

                        sl.SetCellValue(row, 14, pago.GetCambio());
                        sl.SetCellStyle(row, 14, styleARS);

                        sl.SetCellValue(row, 15, pago.GetImporte_MonedaLocal());
                        sl.SetCellStyle(row, 15, styleARS);

                        sl.SetCellStyle(row, 16, styleUSD);

                        if (pago.GetMoneda().IsExtranjera())
                        {
                            sl.SetCellValue(row, 16, pago.GetImporte());
                        }
                        else
                        {
                            sl.SetCellValue(row, 16, "");
                        }
                        sl.SetCellValue(row, 17, pago.GetObservacion());
                        sl.SetCellStyle(row, 17, styleUSD);

                        row++;
                    }
                    if (row > maxRow)
                    {
                        maxRow = row;
                    }
                    row = initRow;

                    sl.SetCellValue(row + 7, 1, "Total pagos:");
                    sl.SetCellStyle(row + 7, 1, styleTitle);

                    sl.SetCellValue(row + 7, 2, DBPago.GetTotal_MonedaLocal(listaPagos));
                    sl.SetCellStyle(row + 7, 2, styleARS);

                    int[] startBorder = { initRow, 1 };
                    int[] endBorder = { maxRow-1, 17 };

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

                            if ((j!= startBorder[1]+2) && (j != startBorder[1] + 9)) {
                                if (j == startBorder[1] || ((i < startBorder[0] + 2) && (j > startBorder[1] + 2)))
                                {
                                    richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                                } else
                                {
                                    richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                                }
                            }
                            sl.SetCellStyle(i, j, richStyle);
                        }
                    }

                    row = maxRow+1; //add EVEN more padding
                }
                for (int i = 1; i < 18; i++)
                {
                    sl.AutoFitColumn(i);
                }
                if (!fileName.Trim().ToLower().EndsWith(".xlsx"))
                {
                    fileName += ".xlsx";
                }
                sl.SaveAs($"{fileName}");
            }
            //sl.SelectWorksheet("Sheet1");
        }

        private static void StylingComprobantesCells(SLDocument sl, int startRow, int endRow, int startColumn, int endColumn)
        {
            //Applying borders & background colors.
            for (int i = startRow; i < endRow; i++)
            {
                for (int j = startColumn; j < endColumn; j++)
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

                    if (i == startRow)
                    {
                        richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                    }

                    sl.SetCellStyle(i, j, richStyle);
                }
            }
        }

        private static CellData[] GetComprobantesHeadersCells(SLStyle styleTitle = null)
        {
            CellData[] comprobanteHeader =
            {
                new CellData(1, "Estado", styleTitle),
                new CellData(2, "Fecha", styleTitle),
                new CellData(3, "CUIT", styleTitle),
                new CellData(4, "Tipo", styleTitle),
                new CellData(5, "Numero", styleTitle),
                new CellData(6, "Razón Social", styleTitle),
                new CellData(7, "Moneda", styleTitle),
                new CellData(8, "Cambio", styleTitle),
                new CellData(9, "Gravado (Mon. Loc.)", styleTitle),
                new CellData(10, "IVA (Mon. Loc.)", styleTitle),
                new CellData(11, "No Gravado (Mon. Loc.)", styleTitle),
                new CellData(12, "Percepción (Mon. Loc.)", styleTitle),
                new CellData(13, "Total (Mon. Loc.)", styleTitle),
                new CellData(14, "Gravado (Mon. Ext.)", styleTitle),
                new CellData(15, "IVA (Mon. Ext.)", styleTitle),
                new CellData(16, "No Gravado (Mon. Ext.)" ,styleTitle),
                new CellData(17, "Percepcion (Mon. Ext.)", styleTitle),
                new CellData(18, "Total (Mon. Ext.)", styleTitle),
                new CellData(19, "Observación", styleTitle)
            };

            return comprobanteHeader;
        }

        private static CellData[] GetDBComprobanteAsCells(DBComprobantes comprobante, bool isComprobantePago, SLStyle styleDefaultText = null, SLStyle styleARS = null, SLStyle styleUSD = null)
        {

            //START STYLING: Adding Color if a Comprobante was Pago or not

            if (isComprobantePago)
            {
                if (!(styleDefaultText is null))
                {
                    styleDefaultText.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                }
                if (!(styleARS is null))
                {
                    styleARS.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                }
                if (!(styleUSD is null))
                {
                    styleUSD.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                }
            }
            else
            {
                if (!(styleDefaultText is null))
                {
                    styleDefaultText.SetPatternFill(PatternValues.Solid, System.Drawing.Color.Gainsboro, System.Drawing.Color.Gainsboro);
                }
                if (!(styleARS is null))
                {
                    styleARS.SetPatternFill(PatternValues.Solid, System.Drawing.Color.Gainsboro, System.Drawing.Color.Gainsboro);
                }
                if (!(styleUSD is null))
                {
                    styleUSD.SetPatternFill(PatternValues.Solid, System.Drawing.Color.Gainsboro, System.Drawing.Color.Gainsboro);
                }
            }
            //END STYLING

            string fechaString = "";
            if (comprobante.GetFechaEmitido().HasValue)
            {
                fechaString = ((DateTime)comprobante.GetFechaEmitido()).ToString("dd/MM/yyyy");
            }
            else
            {
                fechaString = "Sin fecha";
            }

            CellData[] listOfCells = {
                new CellData(1, comprobante.IsEmitido() ? "Emitido" : "Recibido", styleDefaultText),
                new CellData(2, fechaString, styleDefaultText),
                new CellData(3, comprobante.GetEntidadComercial().GetCUIT(), styleDefaultText),
                new CellData(4, comprobante.GetTipoComprobante().GetName(), styleDefaultText),
                new CellData(5, comprobante.GetNumeroComprobante(), styleDefaultText),
                new CellData(6, comprobante.GetEntidadComercial().GetRazonSocial(), styleDefaultText),
                new CellData(7, comprobante.GetMoneda().GetName(), styleDefaultText),
                new CellData(8, comprobante.GetCambio(), styleARS),
                new CellData(9, comprobante.GetGravado_MonedaLocal(), styleARS),
                new CellData(10, comprobante.GetIVA_MonedaLocal(), styleARS),
                new CellData(11, comprobante.GetNoGravado_MonedaLocal(),styleARS),
                new CellData(12, comprobante.GetPercepcion_MonedaLocal(), styleARS),
                new CellData(13, comprobante.GetTotal_MonedaLocal(), styleARS),
                new CellData(14, comprobante.GetMoneda().IsExtranjera() ? comprobante.GetGravado() : 0.0, styleUSD),
                new CellData(15, comprobante.GetMoneda().IsExtranjera() ? comprobante.GetIVA() : 0.0, styleUSD),
                new CellData(16, comprobante.GetMoneda().IsExtranjera() ? comprobante.GetNoGravado() : 0.0, styleUSD),
                new CellData(17, comprobante.GetMoneda().IsExtranjera() ? comprobante.GetPercepcion() : 0.0, styleUSD),
                new CellData(18, comprobante.GetMoneda().IsExtranjera() ? comprobante.GetTotal() : 0.0, styleUSD),
                new CellData(19, comprobante.GetObservacion(), styleDefaultText),
            };

            return listOfCells;
        }
    }
}
