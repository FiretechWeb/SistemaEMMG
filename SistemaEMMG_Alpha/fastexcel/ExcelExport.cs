using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    //Format pesos: #.##0,00 [$ARS];-#.##0,00 [$ARS]
    //Format dolares: #.##0,00 [$USD];-#.##0,00 [$USD]
    //Format saldo: #.##0,00 [$ARS];[RED]-#.##0,00 [$ARS]

    //Header background color: #afd095
    // base background color: #dee6ef
    public class ExcelExport
    {
        public static void ExportToFile(List<DBComprobantes> comprobantes, string fileName)
        {
            MySqlConnection conn = DBConnection.Instance().Connection;
            using (SLDocument sl = new SLDocument())
            {
                SLStyle styleDefaultText = sl.CreateStyle();
                styleDefaultText.Font.FontSize = 13;
                styleDefaultText.Alignment.Horizontal = HorizontalAlignmentValues.Left;

                SLStyle styleTitle = sl.CreateStyle();
                styleTitle.Font.FontSize = 14;
                styleTitle.Alignment.Horizontal = HorizontalAlignmentValues.Left;
                styleTitle.Font.Bold = true;

                SLStyle styleSaldoARS = sl.CreateStyle();
                styleSaldoARS.Font.FontSize = 13;
                styleSaldoARS.FormatCode = "#,##0.00 [$ARS];[RED]-#,##0.00 [$ARS]";
                styleSaldoARS.Alignment.Horizontal = HorizontalAlignmentValues.Left;

                SLStyle styleARS = sl.CreateStyle();
                styleARS.Font.FontSize = 13;
                styleARS.FormatCode = "#,##0.00 [$ARS];-#,##0.00 [$ARS]";
                styleARS.Alignment.Horizontal = HorizontalAlignmentValues.Left;

                SLStyle styleUSD = sl.CreateStyle();
                styleUSD.Font.FontSize = 13;
                styleUSD.FormatCode = "#,##0.00 [$USD];-#,##0.00 [$USD]";
                styleUSD.Alignment.Horizontal = HorizontalAlignmentValues.Left;


                sl.RenameWorksheet(sl.GetCurrentWorksheetName(), "Comprobantes");
                //sl.SelectWorksheet("Comprobantes");
                // set a boolean at "A1"
                int row = 1;

                //applying title font style
                for (int i=1; i < 20; i++)
                {
                    sl.SetCellStyle(row, i, styleTitle);
                }
                sl.SetCellValue(row, 1, "Estado");
                sl.SetCellValue(row, 2, "Fecha");
                sl.SetCellValue(row, 3, "CUIT");
                sl.SetCellValue(row, 4, "Tipo");
                sl.SetCellValue(row, 5, "Numero");
                sl.SetCellValue(row, 6, "Razón Social");
                sl.SetCellValue(row, 7, "Moneda");
                sl.SetCellValue(row, 8, "Cambio");
                sl.SetCellValue(row, 9, "Gravado (Mon. Loc.)");
                sl.SetCellValue(row, 10, "IVA (Mon. Loc.)");
                sl.SetCellValue(row, 11, "No Gravado (Mon. Loc.)");
                sl.SetCellValue(row, 12, "Percepción (Mon. Loc.)");
                sl.SetCellValue(row, 13, "Total (Mon. Loc.)");
                sl.SetCellValue(row, 14, "Gravado (Mon. Ext.)");
                sl.SetCellValue(row, 15, "IVA (Mon. Ext.)");
                sl.SetCellValue(row, 16, "No Gravado (Mon. Ext.)");
                sl.SetCellValue(row, 17, "Percepcion (Mon. Ext.)");
                sl.SetCellValue(row, 18, "Total (Mon. Ext.)");
                sl.SetCellValue(row, 19, "Observación");
                row++;

                List<DBRecibo> recibos = new List<DBRecibo>();
                //Creating Worksheet with comprobantes

                foreach (DBComprobantes comprobante in comprobantes)
                {
                    string fechaString = "";
                    if (comprobante.GetFechaEmitido().HasValue)
                    {
                        fechaString = ((DateTime)comprobante.GetFechaEmitido()).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        fechaString = "Sin fecha";
                    }
                    sl.SetCellValue(row, 1, comprobante.IsEmitido() ? "Emitido" : "Recibido");
                    sl.SetCellStyle(row, 1, styleDefaultText);

                    sl.SetCellValue(row, 2, fechaString);
                    sl.SetCellStyle(row, 2, styleDefaultText);

                    sl.SetCellValue(row, 3, comprobante.GetEntidadComercial().GetCUIT());
                    sl.SetCellStyle(row, 3, styleDefaultText);

                    sl.SetCellValue(row, 4, comprobante.GetTipoComprobante().GetName());
                    sl.SetCellStyle(row, 4, styleDefaultText);

                    sl.SetCellValue(row, 5, comprobante.GetNumeroComprobante());
                    sl.SetCellStyle(row, 5, styleDefaultText);

                    sl.SetCellValue(row, 6, comprobante.GetEntidadComercial().GetRazonSocial());
                    sl.SetCellStyle(row, 6, styleDefaultText);

                    sl.SetCellValue(row, 7, comprobante.GetMoneda().GetName());
                    sl.SetCellStyle(row, 7, styleDefaultText);

                    sl.SetCellValue(row, 8, comprobante.GetCambio());
                    sl.SetCellStyle(row, 8, styleARS);

                    sl.SetCellValue(row, 9, comprobante.GetGravado_MonedaLocal());
                    sl.SetCellStyle(row, 9, styleARS);

                    sl.SetCellValue(row, 10, comprobante.GetIVA_MonedaLocal());
                    sl.SetCellStyle(row, 10, styleARS);

                    sl.SetCellValue(row, 11, comprobante.GetNoGravado_MonedaLocal());
                    sl.SetCellStyle(row, 11, styleARS);

                    sl.SetCellValue(row, 12, comprobante.GetPercepcion_MonedaLocal());
                    sl.SetCellStyle(row, 12, styleARS);

                    sl.SetCellValue(row, 13, comprobante.GetTotal_MonedaLocal());
                    sl.SetCellStyle(row, 13, styleARS);

                    sl.SetCellStyle(row, 14, styleUSD);
                    sl.SetCellStyle(row, 15, styleUSD);
                    sl.SetCellStyle(row, 16, styleUSD);
                    sl.SetCellStyle(row, 17, styleUSD);
                    sl.SetCellStyle(row, 18, styleUSD);

                    if (comprobante.GetMoneda().IsExtranjera())
                    {

                        sl.SetCellValue(row, 14, comprobante.GetGravado());
                        sl.SetCellValue(row, 15, comprobante.GetIVA());
                        sl.SetCellValue(row, 16, comprobante.GetNoGravado());
                        sl.SetCellValue(row, 17, comprobante.GetPercepcion());
                        sl.SetCellValue(row, 18, comprobante.GetTotal());
                    }
                    else
                    {
                        sl.SetCellValue(row, 14, "");
                        sl.SetCellValue(row, 15, "");
                        sl.SetCellValue(row, 16, "");
                        sl.SetCellValue(row, 17, "");
                        sl.SetCellValue(row, 18, "");
                    }

                    sl.SetCellValue(row, 19, comprobante.GetObservacion());
                    sl.SetCellStyle(row, 19, styleDefaultText);

                    bool isComprobantePago = comprobante.IsPago(conn);
                    for (int j = 1; j < 20; j++)
                    {
                        SLStyle richStyle = sl.GetCellStyle(row, j);
                        if (isComprobantePago)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                        }
                        else
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.Gainsboro, System.Drawing.Color.Gainsboro);
                        }
                        sl.SetCellStyle(row, j, richStyle);
                    }
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

                //Applying borders & background colors.
                for (int i = 1; i < row; i++)
                {
                    for (int j = 1; j < 20; j++)
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

                        if (i==1)
                        {
                            richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                        }

                        sl.SetCellStyle(i, j, richStyle);
                    }
                }

                row++;

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

                        sl.SetCellValue(row, 8, comprobante.GetTotal_MonedaLocal());
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

                    sl.SetCellValue(row + 6, 2, DBComprobantes.GetTotal_MonedaLocal(listaComprobantes));
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
    }
}
