using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;

namespace SistemaEMMG_Alpha
{
    public enum CellDataType
    {
        STRING = 0,
        LONG,
        DOUBLE,
        BOOL

    }
    public struct CellData
    {
        private CellData(int _row, int _column, SLStyle _style = null)
        {
            row = _row;
            column = _column;
            style = _style;
            value = "";
            dataType = -1;
        }
        private CellData(int _column, SLStyle _style = null)
        {
            row = 0;
            column = _column;
            style = _style;
            value = "";
            dataType = -1;
        }
        private CellData(SLStyle _style = null)
        {
            row = 0;
            column = 0;
            style = _style;
            value = "";
            dataType = -1;
        }

        public CellData(int _row, int _column, string _value, SLStyle _style = null) : this(_row, _column, _style)
        {
            value = _value;
            dataType = (int)CellDataType.STRING;
        }
        public CellData(int _column, string _value, SLStyle _style = null) : this (_column, _style)
        {
            value = _value;
            dataType = (int)CellDataType.STRING;
        }
        public CellData(string _value, SLStyle _style = null) : this (_style)
        {
            value = _value;
            dataType = (int)CellDataType.STRING;
        }

        public CellData(int _row, int _column, long _value, SLStyle _style = null) : this(_row, _column, _style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.LONG;
        }
        public CellData(int _column, long _value, SLStyle _style = null) : this(_column, _style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.LONG;
        }
        public CellData(long _value, SLStyle _style = null) : this(_style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.LONG;
        }

        public CellData(int _row, int _column, double _value, SLStyle _style = null) : this(_row, _column, _style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.DOUBLE;
        }
        public CellData(int _column, double _value, SLStyle _style = null) : this(_column, _style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.DOUBLE;
        }
        public CellData(double _value, SLStyle _style = null) : this(_style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.DOUBLE;
        }

        public CellData(int _row, int _column, bool _value, SLStyle _style = null) : this(_row, _column, _style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.BOOL;
        }
        public CellData(int _column, bool _value, SLStyle _style = null) : this(_column, _style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.BOOL;
        }
        public CellData(bool _value, SLStyle _style = null) : this(_style)
        {
            value = _value.ToString();
            dataType = (int)CellDataType.BOOL;
        }

        public int row { get; set; }
        public int column { get; set; }
        public string value { get; set; }

        public int getDataType()
        {
            return dataType;
        }
 
        private int dataType { get; }

        public SLStyle style { get; set; }
    }
    public static class ExcelExtensions
    {
        public static bool SetCellValue(this SLDocument doc, int RowIndex, int ColumnIndex, CellData data)
        {
            switch (data.getDataType())
            {
                case (int)CellDataType.LONG:
                    return doc.SetCellValue(RowIndex, ColumnIndex, SafeConvert.ToInt64(data.value));
                case (int)CellDataType.DOUBLE:
                    return doc.SetCellValue(RowIndex, ColumnIndex, SafeConvert.ToDouble(data.value));
                case (int)CellDataType.BOOL:
                    return doc.SetCellValue(RowIndex, ColumnIndex, SafeConvert.ToBoolean(data.value));
            }
            return doc.SetCellValue(RowIndex, ColumnIndex, data.value);
        }
        public static int CopyCellDataToRow(this SLDocument doc, int row, CellData[] listOfCells, SLStyle globalStyle = null, bool forceGlobalStyle = false)
        {
            int biggestRow = row;
            foreach (CellData cell in listOfCells)
            {
                int currentRow = row + cell.row;
                doc.SetCellValue(currentRow, cell.column, cell);
                if (!(cell.style is null) && !forceGlobalStyle)
                {
                    doc.SetCellStyle(currentRow, cell.column, cell.style);
                } else if (!(globalStyle is null))
                {
                    doc.SetCellStyle(currentRow, cell.column, globalStyle);
                }

                if (currentRow > biggestRow)
                {
                    biggestRow = currentRow;
                }
            }

            return biggestRow;
        }
    }
}
