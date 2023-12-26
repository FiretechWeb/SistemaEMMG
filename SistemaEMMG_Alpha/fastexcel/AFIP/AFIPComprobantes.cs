using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public static class AFIPComprobantes
    {
        private static readonly int CP_CLMN_FECHA = 1;
        private static readonly int CP_CLMN_TIPO = 2;
        private static readonly int CP_CLMN_PVENTA = 3;
        private static readonly int CP_CLMN_NUMEROD = 4;
        private static readonly int CP_CLMN_TIPODOC = 7;
        private static readonly int CP_CLMN_NRORECEP = 8;
        private static readonly int CP_CLMN_DENRECEP = 9;
        private static readonly int CP_CLMN_CAMBIO = 10;
        private static readonly int CP_CLMN_MONEDA = 11;
        private static readonly int CP_CLMN_GRAVADO = 12;
        private static readonly int CP_CLMN_NOGRAVADO = 13;
        private static readonly int CP_CLMN_OPEXT = 14;
        private static readonly int CP_CLMN_OTRIBUTOS = 15;
        private static readonly int CP_CLMN_IVA = 16;
        private static readonly int CP_CLMN_TOTAL = 17;

        private static DBMoneda GetTipoMonedaFromCell(string cellText)
        {
            MySqlConnection conn = DBConnection.Instance().Connection;

            cellText = cellText.DeepNormalize();
            if (cellText.Contains("$"))
            {
                return DBMoneda.GetByName("ars", conn);
            } else if (cellText.Contains("usd"))
            {
                return DBMoneda.GetByName("usd", conn);
            }

            return null;
        }

        private static DBTiposComprobantes GetTipoComprobanteFromCell(string cellText)
        {
            MySqlConnection conn = DBConnection.Instance().Connection;

            return DBTiposComprobantes.GetByAlias(cellText, conn, true);
        }

        public static void GetMissingTypesFromFile(DBCuenta cuenta, string excelFileName, List<DBEntidades> entidades, List<DBTiposComprobantes> tiposComprobantes, List<DBMoneda> monedas)
        {
            MySqlConnection conn = DBConnection.Instance().Connection;

            using (SLDocument sl = new SLDocument(excelFileName))
            {
                //Try to loop through comprobantes data
                for (int row=3; sl.HasCellValue($"A{row}"); row++)
                {
                    //Check if tipo de comprobante does exists.
                    DBTiposComprobantes tipoComprobante = GetTipoComprobanteFromCell(sl.GetCellValueAsString(row, CP_CLMN_TIPO));

                    if (tipoComprobante is null)
                    {
                        tipoComprobante = new DBTiposComprobantes(sl.GetCellValueAsString(row, CP_CLMN_TIPO), 0);
                        tipoComprobante.AddAlias(sl.GetCellValueAsString(row, CP_CLMN_TIPO));
                        tiposComprobantes.Add(tipoComprobante);
                    }

                    //Check if Moneda exists

                    DBMoneda moneda = GetTipoMonedaFromCell(sl.GetCellValueAsString(row, CP_CLMN_MONEDA));
                    if (moneda is null)
                    {
                        moneda = new DBMoneda(sl.GetCellValueAsString(row, CP_CLMN_MONEDA), true);
                        monedas.Add(moneda);
                    }

                    //Check if commercial entity does exists
                    string tipoDOC = sl.GetCellValueAsString(row, CP_CLMN_TIPODOC).Trim().ToLower();

                    if (!tipoDOC.Equals("cuit")) continue;

                    long receptorCUIT = sl.GetCellValueAsInt64(row, CP_CLMN_NRORECEP);
                    string receptorRazon = sl.GetCellValueAsString(row, CP_CLMN_DENRECEP);
                    DBEntidades entidadComercial = DBEntidades.GetByCUIT(conn, cuenta, receptorCUIT);

                    if (entidadComercial is null)
                    {
                        entidadComercial = new DBEntidades(cuenta, DBTipoEntidad.GetByName("cliente", conn), receptorCUIT, receptorRazon);
                        entidades.Add(entidadComercial);
                    }
                }
                sl.CloseWithoutSaving();
            }
        }

        public static List<DBComprobantes> ImportFromFile(DBCuenta cuenta, string excelFileName)
        {
            List<DBComprobantes> comprobantes = new List<DBComprobantes>();

            MySqlConnection conn = DBConnection.Instance().Connection;

            using (SLDocument sl = new SLDocument(excelFileName))
            {
                //Try to loop through comprobantes data
                bool isEmitido = sl.HasCellValue($"A{1}") && sl.GetCellValueAsString(1, 1).Trim().ToLower().Contains("emitidos");

                for (int row=3; sl.HasCellValue($"A{row}"); row++)
                {
                    DBTiposComprobantes tipoComprobante = GetTipoComprobanteFromCell(sl.GetCellValueAsString(row, CP_CLMN_TIPO));

                    if (tipoComprobante is null) continue;

                    DBMoneda moneda = GetTipoMonedaFromCell(sl.GetCellValueAsString(row, CP_CLMN_MONEDA));
                    if (moneda is null) continue;

                    string tipoDOC = sl.GetCellValueAsString(row, CP_CLMN_TIPODOC).Trim().ToLower();
                    if (!tipoDOC.Equals("cuit")) continue;

                    long receptorCUIT = sl.GetCellValueAsInt64(row, CP_CLMN_NRORECEP);
                    DBEntidades entidadComercial = DBEntidades.GetByCUIT(conn, cuenta, receptorCUIT);

                    if (entidadComercial is null) continue;

                    string numeroFactura = $"{sl.GetCellValueAsString(row, CP_CLMN_PVENTA)}-{sl.GetCellValueAsString(row, CP_CLMN_NUMEROD)}";
                    DBComprobantes comprobante = DBComprobantes.GetByNumberNormalized(conn, entidadComercial, numeroFactura, isEmitido);

                    if (!(comprobante is null)) continue;
                    
                    DateTime fechaEmitido = new DateTime();
                    DateTime.TryParse(sl.GetCellValueAsString(row, CP_CLMN_FECHA), out fechaEmitido);

                    comprobante = new DBComprobantes(
                        entidadComercial,
                        tipoComprobante,
                        moneda,
                        isEmitido,
                        fechaEmitido,
                        numeroFactura,
                        sl.GetCellValueAsDouble(row, CP_CLMN_GRAVADO),
                        sl.GetCellValueAsDouble(row, CP_CLMN_IVA),
                        sl.GetCellValueAsDouble(row, CP_CLMN_NOGRAVADO),
                        0.0,
                        sl.GetCellValueAsDouble(row, CP_CLMN_CAMBIO),
                        "",
                        tipoComprobante.HasFlag(TipoComprobanteFlag.Total) ? sl.GetCellValueAsDouble(row, CP_CLMN_TOTAL) : 0.0,
                        sl.GetCellValueAsDouble(row, CP_CLMN_OPEXT),
                        sl.GetCellValueAsDouble(row, CP_CLMN_OTRIBUTOS)
                        );
                    comprobantes.Add(comprobante);
                }
                sl.CloseWithoutSaving();
            }

            return comprobantes;
        }
    }
}
