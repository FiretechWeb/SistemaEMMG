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
        Light = 0,
        Dark
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
        public string db_hostName;
        public string db_databaseName;
        public string db_userName;
        public string db_userPassword;
        public string app_pathXLStoPDF;
        public string app_xlsToPdfArgs;
    }

    public struct ExternalAPPSData {

        public ExternalAPPSData(string _XLStoPDF, string _args)
        {
            pathXLStoPDF = _XLStoPDF;
            xlsToPdfArgs = _args;
        }

        public string pathXLStoPDF;
        public string xlsToPdfArgs; //Special keyworks: %i input file, %o output file.

        public string GetXlsToPdfArgsParsed(string fileInput, string fileOutput)
        {
            return xlsToPdfArgs
                .Replace("%i", fileInput)
                .Replace("%o", fileOutput);
        }

        public static ExternalAPPSData GetDefault()
        {
            return new ExternalAPPSData("C:\\Program Files\\LibreOffice\\program\\soffice.exe", "-norestore -nofirststartwizard -headless -convert-to pdf  \"%i\"");
        }

        public override string ToString()
        {
            return $"ExternalAPPSData:\n\tpathXLStoPDF: {pathXLStoPDF}\n\txlsToPdfArgs: {xlsToPdfArgs}";
        }
    }

    public struct DatabaseAccessData
    {
        public DatabaseAccessData(string _host, string _db, string _user, string _pass)
        {
            hostName = _host;
            databaseName = _db;
            userName = _user;
            userPassword = _pass;
        }
        public string hostName;
        public string databaseName;
        public string userName;
        public string userPassword;

        public static DatabaseAccessData GetDefault()
        {
            return new DatabaseAccessData("localhost", "sistemacomprobantes", "root", "root");
        }

        public override string ToString()
        {
            return $"DatabaseAcessData:\n\thostName: {hostName}\n\tdatabaseName: {databaseName}\n\tuserName: {userName}\n\tuserPassword: {userPassword}\n";
        }
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

        public override string ToString()
        {
            string returnStr = "PrinterTemplateData:\n";
            returnStr += $"\tcomprobantesPrintTemplate={comprobantesPrintTemplate}\n";
            returnStr += $"\tsingleComprobantePrintTemplate={singleComprobantePrintTemplate}\n";
            returnStr += $"\trecibosPrintTemplate={recibosPrintTemplate}\n";
            returnStr += $"\tsingleReciboPrintTemplate={singleReciboPrintTemplate}\n";
            returnStr += $"\tremitosPrintTemplate={remitosPrintTemplate}\n";
            returnStr += $"\tsingleRemitoPrintTemplate={singleRemitoPrintTemplate}\n";
            returnStr += $"\tpagosPrintTemplate={pagosPrintTemplate}\n";
            returnStr += $"\tsinglePagoPrintTemplate={singlePagoPrintTemplate}\n";
            return returnStr;
        }
    }

    public struct ConfigData
    { 
        public ConfigData(TypeFontSize _fs, TypeVisualTheme _theme, bool _autoBackup, int _autoInterval, string _defPrinter, PrinterTemplateData _printerTemplates, DatabaseAccessData _dbData, ExternalAPPSData _appData)
        {
            fontSize = _fs;
            visualTheme = _theme;
            automaticBackups = _autoBackup;
            automaticBackupsInterval = _autoInterval;
            defaultPrinter = _defPrinter;
            printerTemplates = _printerTemplates;
            databaseData = _dbData;
            externalApps = _appData;
        }

        public ConfigData(ConfigObject cfgObject)
        {
            fontSize = (TypeFontSize)cfgObject.fontSize;
            visualTheme = (TypeVisualTheme)cfgObject.visualTheme;
            automaticBackups = cfgObject.automaticBackups;
            automaticBackupsInterval = cfgObject.automaticBackupsInterval;
            defaultPrinter = cfgObject.defaultPrinter;
            printerTemplates = new PrinterTemplateData(
                cfgObject.comprobantesPrintTemplate,
                cfgObject.singleComprobantePrintTemplate,
                cfgObject.recibosPrintTemplate,
                cfgObject.singleReciboPrintTemplate,
                cfgObject.remitosPrintTemplate,
                cfgObject.singleRemitoPrintTemplate,
                cfgObject.pagosPrintTemplate,
                cfgObject.singlePagoPrintTemplate);
            databaseData = new DatabaseAccessData(
                cfgObject.db_hostName,
                cfgObject.db_databaseName,
                cfgObject.db_userName,
                cfgObject.db_userPassword);
            externalApps = new ExternalAPPSData(
                cfgObject.app_pathXLStoPDF, 
                cfgObject.app_xlsToPdfArgs);
        }

        public TypeFontSize fontSize;
        public TypeVisualTheme visualTheme;
        public bool automaticBackups;
        public int automaticBackupsInterval;
        public string defaultPrinter;
        public PrinterTemplateData printerTemplates;
        public DatabaseAccessData databaseData;
        public ExternalAPPSData externalApps;

        public static ConfigData GetDefault()
        {
            return new ConfigData(
                TypeFontSize.Normal,
                TypeVisualTheme.Light,
                false,
                120,
                "",
                PrinterTemplateData.GetDefault(),
                DatabaseAccessData.GetDefault(),
                ExternalAPPSData.GetDefault()
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
                defaultPrinter = this.defaultPrinter,
                db_hostName = this.databaseData.hostName,
                db_databaseName = this.databaseData.databaseName,
                db_userName = this.databaseData.userName,
                db_userPassword = this.databaseData.userPassword,
                app_pathXLStoPDF = this.externalApps.pathXLStoPDF,
                app_xlsToPdfArgs = this.externalApps.xlsToPdfArgs
    };
        }

        public override string ToString()
        {
            string returnStr = "ConfigData:\n";
            returnStr += $"\tfontSize: {fontSize}\n";
            returnStr += $"\tvisualTheme: {visualTheme}\n";
            returnStr += $"\tautomaticBackups: {automaticBackups}\n";
            returnStr += $"\tautomaticBackupsInterval: {automaticBackupsInterval}\n";
            returnStr += $"\tdefaultPrinter: {defaultPrinter}\n";
            returnStr += $"\tprinterTemplates: {printerTemplates}\n";
            returnStr += $"\tdatabaseData: {databaseData}\n";
            returnStr += $"\texternalApps: {externalApps}\n";
            return returnStr;
        }
    }
    class Config
    {
        private ConfigData _data;
        private static Config _globalConfig=null;
        private static readonly string _defaultConfigFile = "configuracion.json";

        public Config(ConfigData cfgData)
        {
            _data = cfgData;
        }

        public Config()
        {
            _data = ConfigData.GetDefault();
        }

        public Config(string jsonFile)
        {
            if (File.Exists(jsonFile))
            {
                ImportFromJSONFile(jsonFile);
            } else
            {
                _data = ConfigData.GetDefault();
                ExportToJSONFile(jsonFile);
            }
        }

        public static Config GetGlobalConfig()
        {
            return _globalConfig;
        }
        public static void SetGlobalConfig(Config newGlobalConfig)
        {
            _globalConfig = newGlobalConfig;
        }

        public static string GetDefaultConfigFileName() => _defaultConfigFile;

        public void ExportToJSONFile(string fileName)
        {
            ConfigObject tmpCFGObject = _data.GetAsObject();
            tmpCFGObject.db_userPassword = EncryptManager.EncryptString(tmpCFGObject.db_userPassword, Constants.databaseEncryptionKey);
            File.WriteAllText(fileName, JsonConvert.SerializeObject(tmpCFGObject));
        }

        public void ImportFromJSONFile(string fileName)
        {
            ConfigObject tmpCFGObject = JsonConvert.DeserializeObject<ConfigObject>(File.ReadAllText(fileName));
            tmpCFGObject.db_userPassword = EncryptManager.DecryptString(tmpCFGObject.db_userPassword, Constants.databaseEncryptionKey);
            _data = new ConfigData(tmpCFGObject);
        }

        //Setters
        public void SetFontSize(TypeFontSize newFontSize) => _data.fontSize = newFontSize;

        public void SetVisualTheme(TypeVisualTheme newVisualTheme) => _data.visualTheme = newVisualTheme;

        public void SetAutomaticBackups(bool automaticBackups) => _data.automaticBackups = automaticBackups;

        public void SetAutomaticBackupsInterval(int interval) => _data.automaticBackupsInterval = interval;

        public void SetDefaultPrinter(string newDefaultPrinter) => _data.defaultPrinter = newDefaultPrinter;

        public void SetDatabaseData(DatabaseAccessData newDatabaseAccessData) => _data.databaseData = newDatabaseAccessData;

        public void SetPrinterTemplates(PrinterTemplateData newPrinterTemplates) => _data.printerTemplates = newPrinterTemplates;

        public void SetExternalAppsData(ExternalAPPSData newAppsData) => _data.externalApps = newAppsData;

        public TypeFontSize GetFontSize() => _data.fontSize;

        public TypeVisualTheme GetVisualTheme() => _data.visualTheme;

        public bool AutomaticBackupsEnabled() => _data.automaticBackups;

        public int AutomaticBackupsInterval() => _data.automaticBackupsInterval;

        public string GetDefaultPrinter() => _data.defaultPrinter;

        public PrinterTemplateData GetPrinterTemplates() => _data.printerTemplates;

        public DatabaseAccessData GetDatabaseData() => _data.databaseData;

        public ExternalAPPSData GetExternalAppsData() => _data.externalApps;

        public override string ToString()
        {
            return _data.ToString();
        }
    }
}
