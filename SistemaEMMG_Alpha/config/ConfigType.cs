using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SistemaEMMG_Alpha
{
    public enum TypeFontSize
    {
        Small = 0,
        Normal,
        Big
    }
    public enum TypeVisualTheme
    {
        Dark=0,
        Light
    }

    public class ConfigObject
    {
        public int fontSize;
        public int visualTheme;
        public bool automaticBackups;
        public int automaticBackupsInterval;
        public string defaultPrinter;
        public string comprobantesPrintTemplate;
        public string singleComprobantePrintTemplate;
        public string recibosPrintTemplate;
        public string singleReciboPrintTemplate;
        public string remitosPrintTemplate;
        public string singleRemitoPrintTemplate;
        public string pagosPrintTemplate;
        public string singlePagoPrintTemplate;
    }

    public struct PrinterTemplateData
    {
        public PrinterTemplateData(string _comTemplate, string _singleComTemplate, string _recTemplate, string _singleRecTemplate, string _remTemplate, string _singleRemTemplate, string _pagosTemplate, string _singlePagosTemplate)
        {
            comprobantesPrintTemplate = _comTemplate;
            singleComprobantePrintTemplate = _singleComTemplate;
            recibosPrintTemplate = _recTemplate;
            singleReciboPrintTemplate = _singleRecTemplate;
            remitosPrintTemplate = _remTemplate;
            singleRemitoPrintTemplate = _singleRemTemplate;
            pagosPrintTemplate = _pagosTemplate;
            singlePagoPrintTemplate = _singlePagosTemplate;
        }

        public string comprobantesPrintTemplate;
        public string singleComprobantePrintTemplate;
        public string recibosPrintTemplate;
        public string singleReciboPrintTemplate;
        public string remitosPrintTemplate;
        public string singleRemitoPrintTemplate;
        public string pagosPrintTemplate;
        public string singlePagoPrintTemplate;

        public static PrinterTemplateData GetDefault()
        {
            return new PrinterTemplateData(
                "plantillas/impresora/comprobantes.xlsx",
                "plantillas/impresora/comprobante_individual.xlsx",
                "plantillas/impresora/recibos.xlsx",
                "plantillas/impresora/recibo_individual.xlsx",
                "plantillas/impresora/remitos.xlsx",
                "plantillas/impresora/remito_individual.xlsx",
                "plantillas/impresora/pagos.xlsx",
                "plantillas/impresora/pago_individual.xlsx"
            );
        }
    }

    public struct ConfigData
    { 
        public ConfigData(TypeFontSize _fs, TypeVisualTheme _theme, bool _autoBackup, int _autoInterval, string _defPrinter, PrinterTemplateData _printerTemplates)
        {
            fontSize = _fs;
            visualTheme = _theme;
            automaticBackups = _autoBackup;
            automaticBackupsInterval = _autoInterval;
            defaultPrinter = _defPrinter;
            printerTemplates = _printerTemplates;
        }
        public TypeFontSize fontSize;
        public TypeVisualTheme visualTheme;
        public bool automaticBackups;
        public int automaticBackupsInterval;
        public string defaultPrinter;
        public PrinterTemplateData printerTemplates;

        public static ConfigData GetDefault()
        {
            return new ConfigData(
                TypeFontSize.Normal,
                TypeVisualTheme.Light,
                false,
                120,
                "",
                PrinterTemplateData.GetDefault()
            );
        }

        public ConfigObject GetAsObject()
        {
            return new ConfigObject
            {
                pagosPrintTemplate = this.printerTemplates.pagosPrintTemplate,
                singlePagoPrintTemplate = this.printerTemplates.singlePagoPrintTemplate,
                comprobantesPrintTemplate = this.printerTemplates.comprobantesPrintTemplate,
                singleComprobantePrintTemplate = this.printerTemplates.singleComprobantePrintTemplate,
                recibosPrintTemplate = this.printerTemplates.recibosPrintTemplate,
                singleReciboPrintTemplate = this.printerTemplates.singleReciboPrintTemplate,
                remitosPrintTemplate = this.printerTemplates.remitosPrintTemplate,
                singleRemitoPrintTemplate = this.printerTemplates.singleRemitoPrintTemplate,
                fontSize = (int)this.fontSize,
                visualTheme = (int)this.visualTheme,
                automaticBackups = this.automaticBackups,
                automaticBackupsInterval = this.automaticBackupsInterval,
                defaultPrinter = this.defaultPrinter
            };
        }
    }
    class Config
    {
        private ConfigData _data;

        public Config(ConfigData cfgData)
        {
            _data = cfgData;
        }

        public Config()
        {
            _data = ConfigData.GetDefault();
        }

        public void ExportToJSONFile(string fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(_data.GetAsObject()));
        }

        public TypeFontSize GetFontSize() => _data.fontSize;

        public TypeVisualTheme GetVisualTheme() => _data.visualTheme;

        public bool AutomaticBackupsEnabled() => _data.automaticBackups;

        public int AutomaticBackupsInterval() => _data.automaticBackupsInterval;

        public string GetDefaultPrinter() => _data.defaultPrinter;

        public PrinterTemplateData GetPrinterTemplates() => _data.printerTemplates;

    }
}
