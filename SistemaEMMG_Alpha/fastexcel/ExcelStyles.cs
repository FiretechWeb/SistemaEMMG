using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;

namespace SistemaEMMG_Alpha
{
    public static class ExcelStyles
    {
        //Format pesos: #.##0,00 [$ARS];-#.##0,00 [$ARS]
        //Format dolares: #.##0,00 [$USD];-#.##0,00 [$USD]
        //Format saldo: #.##0,00 [$ARS];[RED]-#.##0,00 [$ARS]

        //Header background color: #afd095
        // base background color: #dee6ef
        public static SLStyle defaultTextStyle(SLDocument sl)
        {
            SLStyle styleDefaultText = sl.CreateStyle();
            styleDefaultText.Font.FontSize = 13;
            styleDefaultText.Alignment.Horizontal = HorizontalAlignmentValues.Left;

            return styleDefaultText;
        }

        public static SLStyle titleTextStyle(SLDocument sl)
        {
            SLStyle styleTitle = sl.CreateStyle();
            styleTitle.Font.FontSize = 14;
            styleTitle.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            styleTitle.Font.Bold = true;

            return styleTitle;
        }

        public static SLStyle saldoARSTextStyle(SLDocument sl)
        {
            SLStyle styleSaldoARS = sl.CreateStyle();
            styleSaldoARS.Font.FontSize = 13;
            styleSaldoARS.FormatCode = "#,##0.00 [$ARS];[RED]-#,##0.00 [$ARS]";
            styleSaldoARS.Alignment.Horizontal = HorizontalAlignmentValues.Left;

            return styleSaldoARS;
        }

        public static SLStyle ARSTextStyle(SLDocument sl)
        {
            SLStyle styleARS = sl.CreateStyle();
            styleARS.Font.FontSize = 13;
            styleARS.FormatCode = "#,##0.00 [$ARS];-#,##0.00 [$ARS]";
            styleARS.Alignment.Horizontal = HorizontalAlignmentValues.Left;

            return styleARS;
        }

        public static SLStyle USDTextStyle(SLDocument sl)
        {
            SLStyle styleUSD = sl.CreateStyle();
            styleUSD.Font.FontSize = 13;
            styleUSD.FormatCode = "#,##0.00 [$USD];-#,##0.00 [$USD]";
            styleUSD.Alignment.Horizontal = HorizontalAlignmentValues.Left;

            return styleUSD;
        }
    }
}
