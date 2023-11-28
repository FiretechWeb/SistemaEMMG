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

        private static SLStyle _defaultTextStyle = null;
        public static SLStyle defaultTextStyle(SLDocument sl)
        {
            if (_defaultTextStyle is null)
            {
                _defaultTextStyle = sl.CreateStyle();
                _defaultTextStyle.Font.FontSize = 13;
                _defaultTextStyle.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            }

            return _defaultTextStyle;
        }

        private static SLStyle _titleTextStyle = null;
        public static SLStyle titleTextStyle(SLDocument sl)
        {
            if (_titleTextStyle is null)
            {
                _titleTextStyle = sl.CreateStyle();
                _titleTextStyle.Font.FontSize = 14;
                _titleTextStyle.Alignment.Horizontal = HorizontalAlignmentValues.Left;
                _titleTextStyle.Font.Bold = true;
            }

            return _titleTextStyle;
        }

        private static SLStyle _styleSaldoARS = null;
        public static SLStyle saldoARSTextStyle(SLDocument sl)
        {
            if (_styleSaldoARS is null)
            {
                _styleSaldoARS = sl.CreateStyle();
                _styleSaldoARS.Font.FontSize = 13;
                _styleSaldoARS.FormatCode = "#,##0.00 [$ARS];[RED]-#,##0.00 [$ARS]";
                _styleSaldoARS.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            }
            return _styleSaldoARS;
        }

        private static SLStyle _styleARS = null;
        public static SLStyle ARSTextStyle(SLDocument sl)
        {
            if (_styleARS is null)
            {
                _styleARS = sl.CreateStyle();
                _styleARS.Font.FontSize = 13;
                _styleARS.FormatCode = "#,##0.00 [$ARS];-#,##0.00 [$ARS]";
                _styleARS.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            }
            return _styleARS;
        }

        private static SLStyle _styleUSD = null;
        public static SLStyle USDTextStyle(SLDocument sl)
        {
            if (_styleUSD is null)
            {
                _styleUSD = sl.CreateStyle();
                _styleUSD.Font.FontSize = 13;
                _styleUSD.FormatCode = "#,##0.00 [$USD];-#,##0.00 [$USD]";
                _styleUSD.Alignment.Horizontal = HorizontalAlignmentValues.Left;
            }
            return _styleUSD;
        }
    }
}
