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
                //--outdir outdir input.pdf
                /*var pdfProcess = new Process();
pdfProcess.StartInfo.FileName = exePdf;
pdfProcess.StartInfo.Arguments = "-norestore -nofirststartwizard -headless -convert-to pdf  \"TheFile.xlsx\"";
pdfProcess.StartInfo.WorkingDirectory = docPath; //This is really important
pdfProcess.Start();
                */
                string workingDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(excelFilePath));
                string inputFileName = System.IO.Path.GetFileName(excelFilePath);
                string outputFileName = System.IO.Path.GetFileName(outPdfPath);

                Console.WriteLine($"Working Path: {workingDirectory}");
                Console.WriteLine($"Arguments: {Config.GetGlobalConfig().GetExternalAppsData().GetXlsToPdfArgsParsed(inputFileName, outputFileName)}");
                Console.WriteLine($"Converter path: {Config.GetGlobalConfig().GetExternalAppsData().pathXLStoPDF}");
                var pdfProcess = new Process();
                pdfProcess.StartInfo.FileName = Config.GetGlobalConfig().GetExternalAppsData().pathXLStoPDF;
                pdfProcess.StartInfo.Arguments = Config.GetGlobalConfig().GetExternalAppsData().GetXlsToPdfArgsParsed(inputFileName, outputFileName);
                pdfProcess.StartInfo.WorkingDirectory = workingDirectory;
                pdfProcess.Start();


            }
            return outPdfPath;
        }

        public static void PrintPDF(string pdfFilePath)
        {
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += (sender, e) =>
            {
                //Use GhotscriptSharp to print the PDF

                GhostscriptSharp.GhostscriptSettings settings = new GhostscriptSharp.GhostscriptSettings();

                settings.Page.AllPages = false;
                settings.Page.Start = 1;
                settings.Page.End = 1;
                settings.Size.Native = GhostscriptSharp.Settings.GhostscriptPageSizes.a4;
                settings.Device = GhostscriptSharp.Settings.GhostscriptDevices.png16m;
                settings.Resolution = new System.Drawing.Size(72, 72);

                GhostscriptSharp.GhostscriptWrapper.GenerateOutput(pdfFilePath, "output.png", settings);
            };

            printDoc.Print();
        }


    }
}
