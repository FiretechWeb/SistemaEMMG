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


namespace SistemaEMMG_Alpha
{
    public class ExcelExport
    {
        public static void ExportToFile(List<DBComprobantes> comprobantes, string fileName)
        {
            SLDocument sl = new SLDocument();

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
            //rows.Add(new Row("Pichula", "Perponia", "papapa", "pichita));
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
                row++;
            }
            sl.SaveAs($"{fileName}.xlsx");
        }
    }
}
