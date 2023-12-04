using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Drawing.Printing;

using System.Diagnostics;

namespace SistemaEMMG_Alpha
{
    public static class PrinterManagment
    {
        public static List<string> GetInstalledPrinters()
        {
            List<string> installedPrinters = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                installedPrinters.Add(printer);
                Console.WriteLine($"Printer: {printer}");
            }

            return installedPrinters;
        }

        private static bool IsMSOfficeExcelInstalled()
        {
            bool interopInstalled = false;
            try
            {
                Application excelApp = new Application();
                interopInstalled = true;
            }
            catch(Exception ex)
            {
                //EMPTY
            }
            return interopInstalled;
        }

        public static string ConvertExcelToPDF(string excelFilePath, string outPdfPath)
        {
            if (IsMSOfficeExcelInstalled()) //If EXCEL installed, we use MS Excel
            {
                Application excelApp = new Application();
                Workbook excelWorkbook = excelApp.Workbooks.Open(excelFilePath);

                excelWorkbook.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, outPdfPath);

                excelWorkbook.Close(false);
                excelApp.Quit();

            } else //We use external apps (default: LibreOffice)
            {
                string workingDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(excelFilePath));
                string inputFileName = System.IO.Path.GetFileName(excelFilePath);
                string outputFileName = System.IO.Path.GetFileName(outPdfPath);
                string relativePath = System.IO.Path.GetDirectoryName(excelFilePath);

                Console.WriteLine($"Working Path: {workingDirectory}");
                Console.WriteLine($"Arguments: {Config.GetGlobalConfig().GetExternalAppsData().GetXlsToPdfArgsParsed(inputFileName, outputFileName)}");
                Console.WriteLine($"Converter path: {Config.GetGlobalConfig().GetExternalAppsData().pathXLStoPDF}");
                var pdfProcess = new Process();
                pdfProcess.StartInfo.FileName = Config.GetGlobalConfig().GetExternalAppsData().pathXLStoPDF;
                pdfProcess.StartInfo.Arguments = Config.GetGlobalConfig().GetExternalAppsData().GetXlsToPdfArgsParsed(inputFileName, outputFileName);
                pdfProcess.StartInfo.WorkingDirectory = workingDirectory;
                pdfProcess.Start();
                pdfProcess.WaitForExit(); //Program waits for process to end.
                System.IO.File.Move(System.IO.Path.ChangeExtension(excelFilePath, ".pdf"), outPdfPath);

            }
            return outPdfPath;
        }

        public static void PrintPDF(string pdfFilePath, string printerName)
        {
            var printer = new PDFtoPrinter.PDFtoPrinterPrinter();
            var printerTimeout = new TimeSpan(0, 5, 0);
            printer.Print(new PDFtoPrinter.PrintingOptions(printerName, System.IO.Path.GetFullPath(pdfFilePath)), printerTimeout);
        }


    }
}
