using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public static partial class ExcelComponents
    {
        public static void CreateComprobantesWorksheet(SLDocument sl, List<DBComprobantes> comprobantes, MySqlConnection conn, int row = 1)
        {
            SLStyle styleDefaultText = ExcelStyles.defaultTextStyle(sl);
            SLStyle styleTitle = ExcelStyles.titleTextStyle(sl);
            SLStyle styleSaldoARS = ExcelStyles.saldoARSTextStyle(sl);
            SLStyle styleARS = ExcelStyles.ARSTextStyle(sl);
            SLStyle styleUSD = ExcelStyles.USDTextStyle(sl);

            sl.RenameWorksheet(sl.GetCurrentWorksheetName(), "Comprobantes");

            sl.CopyCellDataToRow(row, GetComprobantesHeadersCells(styleTitle));
            row++;

            foreach (DBComprobantes comprobante in comprobantes)
            {
                sl.CopyCellDataToRow(row, GetDBComprobanteAsCells(comprobante, comprobante.IsPago(conn), styleDefaultText, styleARS, styleUSD));
                row++;
            }

            StylingComprobantesCells(sl, 1, row, 1, 20);

            row++;

            sl.CopyCellDataToRow(row, GetTotalesDeComprobantesCells(comprobantes, styleTitle, styleARS, styleSaldoARS));

            for (int i = 1; i < 20; i++)
            {
                sl.AutoFitColumn(i);
            }

        }

        private static CellData[] GetTotalesDeComprobantesCells(List<DBComprobantes> comprobantes, SLStyle styleTitle = null, SLStyle styleARS = null, SLStyle styleSaldoARS = null)
        {
            //Keep refactoring this mess from here.

            List<DBComprobantes> comprobantesRecibidos = DBComprobantes.GetAllRecibidos(comprobantes);
            List<DBComprobantes> comprobantesEmitidos = DBComprobantes.GetAllEmitidos(comprobantes);

            CellData[] listOfCells = {
                new CellData(0, 1, "Total Recibido:", styleTitle),
                new CellData(0, 2, DBComprobantes.GetTotal_MonedaLocal(comprobantesRecibidos), styleARS),
                new CellData(1, 1, "Total Emitido:", styleTitle),
                new CellData(1, 2, DBComprobantes.GetTotal_MonedaLocal(comprobantesEmitidos), styleARS),
                new CellData(2, 1, "Saldo:", styleTitle),
                new CellData(2, 2, DBComprobantes.GetSaldoTotal_MonedaLocal(comprobantes), styleSaldoARS),
                new CellData(0, 4, "IVA Recibido:", styleTitle),
                new CellData(0, 5, DBComprobantes.GetTotalIVA_MonedaLocal(comprobantesRecibidos), styleARS),
                new CellData(1, 4, "IVA Emitido:", styleTitle),
                new CellData(1, 5, DBComprobantes.GetTotalIVA_MonedaLocal(comprobantesEmitidos), styleARS),
                new CellData(2, 4, "Saldo IVA:", styleTitle),
                new CellData(2, 5, DBComprobantes.GetSaldoIVA_MonedaLocal(comprobantes), styleSaldoARS),
                new CellData(0, 7, "Gravado Recibido:", styleTitle),
                new CellData(0, 8, DBComprobantes.GetTotalGravado_MonedaLocal(comprobantesRecibidos), styleARS),
                new CellData(1, 7, "Gravado Emitido:", styleTitle),
                new CellData(1, 8, DBComprobantes.GetTotalGravado_MonedaLocal(comprobantesEmitidos), styleARS),
                new CellData(0, 10, "No Gravado Recibido:", styleTitle),
                new CellData(0, 11, DBComprobantes.GetTotalNoGravado_MonedaLocal(comprobantesRecibidos), styleARS),
                new CellData(1, 10, "No Gravado Emitido:", styleTitle),
                new CellData(1, 11, DBComprobantes.GetTotalNoGravado_MonedaLocal(comprobantesEmitidos), styleARS),
                new CellData(0, 13, "Percepción Recibido:", styleTitle),
                new CellData(0, 14, DBComprobantes.GetTotalPercepcion_MonedaLocal(comprobantesRecibidos), styleARS),
                new CellData(1, 13, "Percepción Emitido:", styleTitle),
                new CellData(1, 14, DBComprobantes.GetTotalPercepcion_MonedaLocal(comprobantesEmitidos), styleARS),
            };

            for (int i = 0; i < listOfCells.Length; i++)
            {
                SLStyle richStyle = listOfCells[i].style;

                richStyle.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;
                richStyle.Border.BottomBorder.Color = System.Drawing.Color.Black;
                richStyle.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                richStyle.Border.LeftBorder.Color = System.Drawing.Color.Black;
                richStyle.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                richStyle.Border.RightBorder.Color = System.Drawing.Color.Black;
                richStyle.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                richStyle.Border.TopBorder.Color = System.Drawing.Color.Black;

                if ((listOfCells[i].column - 1) % 3 == 0)
                {
                    richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightSeaGreen, System.Drawing.Color.LightSeaGreen);
                }
                else
                {
                    richStyle.SetPatternFill(PatternValues.Solid, System.Drawing.Color.LightBlue, System.Drawing.Color.LightBlue);
                }

                listOfCells[i].style = richStyle;
            }

            return listOfCells;
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
