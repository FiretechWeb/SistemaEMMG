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

                ExcelComponents.CreateComprobantesWorksheet(sl, comprobantes, conn);

                List<DBRecibo> recibos = new List<DBRecibo>();

                foreach (DBComprobantes comprobante in comprobantes)
                {
                    List<DBRecibo> recibosFromComprobante = comprobante.GetAllRecibos();
                    foreach (DBRecibo recibo in recibosFromComprobante)
                    {
                        if (!DBRecibo.CheckIfExistsInList(recibos, recibo))
                        {
                            recibos.Add(recibo);
                        }
                    }
                }

                ExcelComponents.CreateRecibosWorksheet(sl, recibos, conn);
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
