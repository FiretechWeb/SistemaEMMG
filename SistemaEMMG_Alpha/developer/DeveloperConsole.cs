using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

/*****************************
 *  VERIFICAR QUE TODO EL INGRESO DE DATOS FUNCIONE CORRECTAMENTE, SI ESO SUCEDE, ENTONCES HACER LA CLASE DBRemitos, volver a chekear, limpiar código y empezar la con la interfaz gráfica.
 * **************************/

namespace SistemaEMMG_Alpha
{
    public class DeveloperConsole
    {
        public static DeveloperConsole Instance()
        {
            if (_instance == null)
            {
                _instance = new DeveloperConsole();
            }
            return _instance;
        }

        private static DeveloperConsole _instance = null;

        private string _outputStr = "";
        private List<string> _inputRegister = new List<string>();
        private int _currentRegisterSelected = -1;
        private List<KeyValuePair<string, Action<string>>> commandsList = new List<KeyValuePair<string, Action<string>>>();
        private List<KeyValuePair<string, Action<string>>> internalCommandsList = new List<KeyValuePair<string, Action<string>>>();
        private DBBaseClass _seleccion;
        private DeveloperConsole()
        {
            commandsList.Add(new KeyValuePair<string, Action<string>>("SQL>", (x) => ProcessSQLCommand(x)));
            commandsList.Add(new KeyValuePair<string, Action<string>>("sql>", (x) => ProcessSQLCommand(x))); //alias
            commandsList.Add(new KeyValuePair<string, Action<string>>("$>", (x) => ProcessInternalCommand(x))); //internal commands
            commandsList.Add(new KeyValuePair<string, Action<string>>("help", (x) => ProcessHelpCommand(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("get data", (x) => _CMD_RefreshBasicDataDB()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print data", (x) => _CMD_PrintBasicDataDB()));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select banco", (x) => _CMD_SelectBanco(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make banco", (x) => _CMD_CreateBanco(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("banco set name", (x) => _CMD_BancoSetName(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("banco set code", (x) => _CMD_BancoSetCode(x)));


            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select tipo comprobante", (x) => _CMD_SelectTipoComprobante(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make tipo comprobante", (x) => _CMD_CrearTipoComprobante(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("tipo comprobante set name", (x) => _CMD_TipoComprobanteSetName(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("add alias", (x) => _CMD_TipoComprobanteAddAlias(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print aliases", (x) => _CMD_TipoComprobanteGetAlias()));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select tipo recibo", (x) => _CMD_SelectTipoRecibo(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make tipo recibo", (x) => _CMD_CrearTipoRecibo(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("tipo recibo set name", (x) => _CMD_TipoReciboSetName(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select tipo remito", (x) => _CMD_SelectTipoRemito(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make tipo remito", (x) => _CMD_CrearTipoRemito(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("tipo remito set name", (x) => _CMD_TipoRemitoSetName(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select tipo entidad", (x) => _CMD_SelectTipoEntidad(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make tipo entidad", (x) => _CMD_CrearTipoEntidad(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("tipo entidad set name", (x) => _CMD_TipoEntidadSetName(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select forma pago", (x) => _CMD_SelectFormaPago(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make forma pago", (x) => _CMD_CrearFormaDePago(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("forma pago set name", (x) => _CMD_FormaPagoSetName(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select moneda", (x) => _CMD_SelectMoneda(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make moneda", (x) => _CMD_CrearMoneda(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("moneda set name", (x) => _CMD_MonedaSetName(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("moneda set extranjera", (x) => _CMD_MonedaSetExtranjera(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select cuenta", (x) => _CMD_SelectCuenta(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make cuenta", (x) => _CMD_CrearCuenta(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("cuenta set name", (x) => _CMD_CuentaSetRazonSocial(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("cuenta set cuit", (x) => _CMD_CuentaSetCUIT(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("selected", (x) => _CMD_PrintSelected()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("delete", (x) => _CMD_DeleteSelected()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("push", (x) => _CMD_PushSelected()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("pull", (x) => _CMD_PullSelected()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("duplicate", (x) => _CMD_MakeLocalSelected()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("unlink all", (x) => _CMD_UnlinkAll()));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("get entidades", (x) => _CMD_GetEntidades(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print entidades", (x) => _CMD_PrintEntidades()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select entidad", (x) => _CMD_SelectEntidad(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make entidad", (x) => _CMD_CrearEntidad(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("get comprobantes", (x) => _CMD_GetComprobantes(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print comprobantes", (x) => _CMD_PrintComprobantes()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select comprobante", (x) => _CMD_SelectComprobante(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make comprobante", (x) => _CMD_CrearComprobante(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("link recibo", (x) => _CMD_LinkRecibo(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("unlink recibo", (x) => _CMD_UnlinkRecibo(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("link remito", (x) => _CMD_LinkRemito(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("unlink remito", (x) => _CMD_UnlinkRemito(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("get recibos", (x) => _CMD_GetRecibos(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print recibos", (x) => _CMD_PrintRecibos()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select recibo", (x) => _CMD_SelectRecibo(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make recibo", (x) => _CMD_CrearRecibo(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("get remitos", (x) => _CMD_GetRemitos(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print remitos", (x) => _CMD_PrintRemitos()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select remito", (x) => _CMD_SelectRemito(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make remito", (x) => _CMD_CrearRemito(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("link comprobante", (x) => _CMD_LinkComprobante(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("unlink comprobante", (x) => _CMD_UnlinkComprobante(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("get pagos", (x) => _CMD_GetPagos(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print pagos", (x) => _CMD_PrintPagos()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("select pago", (x) => _CMD_SelectPago(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("make pago", (x) => _CMD_CrearPago(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("go back", (x) => _CMD_GoBack()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("generate basic data", (x) => _CMD_GenerateBasicData()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("reset database", (x) => _CMD_ResetDatabase()));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("populate random", (x) => _CMD_PopulateRandom()));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("create excel", (x) => _CMD_MakeExcel(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("create pdf", (x) => _CMD_MakePDF(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("print pdf", (x) => _CMD_PrintPDF(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("save config", (x) => _CMD_SaveConfig(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("load config", (x) => _CMD_LoadConfig(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("encrypt", (x) => _CMD_Encrypt(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("decrypt", (x) => _CMD_Decrypt(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("add user", (x) => _CMD_CreateUser(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("remove user", (x) => _CMD_RemoveUser(x)));
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("login", (x) => _CMD_UserLogin(x)));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("import comprobantes", (x) => _CMD_ImportComprobantes(x)));

            //Patch commands only. Used when porting some DB changes to a new version of the software.
            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("install patch", (x) => _CMD_InstallPatch()));

            internalCommandsList.Add(new KeyValuePair<string, Action<string>>("help", (x) => _CMD_Help()));

        }
        ///<summary>
        ///Process an input string and retrieves the result.
        ///</summary>
        public string ProcessInput(string input)
        {
            bool commandNotFound = true;
            _outputStr = "";
            _inputRegister.Add(input);
            if (_inputRegister.Count > 32)
            {
                _inputRegister.RemoveAt(0);
            }
            _currentRegisterSelected = _inputRegister.Count;
            foreach (KeyValuePair<string, Action<string>> command in commandsList)
            {
                if (input.StartsWith(command.Key))
                {
                    command.Value.Invoke(input.ReplaceFirst(command.Key, ""));
                    commandNotFound = false;
                    break;
                }
            }
            if (commandNotFound)
            {
                _outputStr = $"Command '{input}' not found";
            }
            if (_outputStr.Length > 0)
            {
                _outputStr += "\n";
            }
            return $"> {input}\n {_outputStr}";
        }

        private void ProcessSQLCommand(string command)
        {
            MySqlConnection conn = DBConnection.Instance().Connection;

            try
            {
                string query = command;
                var cmd = new MySqlCommand(query, conn);
                if (command.Trim().ToLower().StartsWith("select"))
                {
                    var reader = cmd.ExecuteReader();
                    int row = 0;
                    while (reader.Read())
                    {
                        if (row == 0)
                        {
                            _outputStr += $"\tColumns\t>";
                            for (int column = 0; column < reader.FieldCount; column++)
                            {
                                _outputStr += $"{reader.GetName(column)}";
                                if (column < reader.FieldCount - 1)
                                {
                                    _outputStr += "\t|\t";
                                }
                            }
                            _outputStr += "\n";
                        }
                        _outputStr += $"\tRow {row}\t> ";
                        for (int column = 0; column < reader.FieldCount; column++)
                        {
                            _outputStr += $"{reader.GetStringSafe(column)}";
                            if (column < reader.FieldCount - 1)
                            {
                                _outputStr += "\t|\t";
                            }
                        }
                        _outputStr += "\n";
                        row++;
                    }
                    reader.Close();
                }
                else
                {
                    cmd.ExecuteNonQuery();
                    _outputStr += "SQL command executed without errors.";
                }
            }
            catch (Exception ex)
            {
                _outputStr += $"SQL error: {ex.ToString()}";
            }
        }

        public string RetrieveConsoleHistory(string input, bool back = true)
        {
            if (_inputRegister.Count <= 0)
            {
                return input;
            }
            if (_currentRegisterSelected == -1)
            {
                _currentRegisterSelected = _inputRegister.Count;
            }

            if (back)
            {
                _currentRegisterSelected -= 1;
            }
            else
            {
                _currentRegisterSelected += 1;
            }
            _currentRegisterSelected = _currentRegisterSelected.Clamp<int>(0, _inputRegister.Count - 1);

            return _inputRegister[_currentRegisterSelected];
        }
        private void ProcessInternalCommand(string command)
        {
            bool commandNotFound = true;
            foreach (KeyValuePair<string, Action<string>> internalCommand in internalCommandsList)
            {
                if (command.StartsWith(internalCommand.Key))
                {
                    internalCommand.Value.Invoke(command.ReplaceFirst(internalCommand.Key, ""));
                    commandNotFound = false;
                    break;
                }
            }
            if (commandNotFound)
            {
                _outputStr = $"Internal command '{command}' not found";
            }

        }

        private void _CMD_Help()
        {
            _outputStr = ":: Comandos disponibles ::\n\n";
            foreach (KeyValuePair<string, Action<string>> internalCmd in internalCommandsList)
            {
                _outputStr += $"\t{internalCmd.Key}\n";
            }
            _outputStr += "\n\nPara usar los comandos hay que usar: $>nombre comando";
        }
        private void _CMD_RefreshBasicDataDB()
        {
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBMain.Instance().RefreshBasicDataDB(conn);
            _CMD_PrintBasicDataDB();
        }
        private void _CMD_PrintBasicDataDB()
        {
            _outputStr = "\t:: CUENTAS ::\n";
            _outputStr += DBCuenta.PrintAll();
            _outputStr += "\t:: Tipos de Entidades ::\n";
            _outputStr += DBTipoEntidad.PrintAll();
            _outputStr += "\t:: Tipos de Comprobantes ::\n";
            _outputStr += DBTiposComprobantes.PrintAll();
            _outputStr += "\t:: Formas de Pago ::\n";
            _outputStr += DBFormasPago.PrintAll();
            _outputStr += "\t:: Tipos de Remitos ::\n";
            _outputStr += DBTipoRemito.PrintAll();
            _outputStr += "\t:: Tipos de Recibos ::\n";
            _outputStr += DBTipoRecibo.PrintAll();
            _outputStr += "\t:: Monedas ::\n";
            _outputStr += DBMoneda.PrintAll();
            _outputStr += "\t:: Bancos ::\n";
            _outputStr += DBBancos.PrintAll();
        }

        private void _CMD_SelectBanco(string id)
        {
            _seleccion = DBBancos.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No existe un banco con el ID introducido.";
                return;
            }
            _outputStr = $"Banco seleccionado> {_seleccion}";
        }

        private void _CMD_SelectTipoComprobante(string id)
        {
            _seleccion = DBTiposComprobantes.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise un tipo de comprobante con el ID introducido.";
                return;
            }
            _outputStr = $"Tipo de comprobante seleccionado> {_seleccion}";
        }
        private void _CMD_SelectTipoRemito(string id)
        {
            _seleccion = DBTipoRemito.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise un tipo de remito con el ID introducido.";
                return;
            }
            _outputStr = $"Tipo de remito seleccionado> {_seleccion}";
        }
        private void _CMD_SelectTipoRecibo(string id)
        {
            _seleccion = DBTipoRecibo.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise un tipo de recibo con el ID introducido.";
                return;
            }
            _outputStr = $"Tipo de recibo seleccionado> {_seleccion}";
        }
        private void _CMD_SelectTipoEntidad(string id)
        {
            _seleccion = DBTipoEntidad.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise un tipo de entidad con el ID introducido.";
                return;
            }
            _outputStr = $"Tipo de entidad seleccionada> {_seleccion}";
        }
        private void _CMD_SelectFormaPago(string id)
        {
            _seleccion = DBFormasPago.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise una forma de pago con el ID introducido.";
                return;
            }
            _outputStr = $"Forma de pago seleccionada> {_seleccion}";
        }
        private void _CMD_SelectMoneda(string id)
        {
            _seleccion = DBMoneda.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise una moneda con el ID introducido.";
                return;
            }
            _outputStr = $"Moneda seleccionada> {_seleccion}";
        }

        private void _CMD_SelectCuenta(string id)
        {
            _seleccion = DBCuenta.GetByID(SafeConvert.ToInt64(id.Trim()));
            if (_seleccion is null)
            {
                _outputStr = "No exise una cuenta con el ID introducido.";
                return;
            }
            _outputStr = $"Cuenta seleccionada> {_seleccion}";
        }
        private void _CMD_CrearCuenta(string args)
        {
            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBCuenta.GenerateRandom();
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length != 2)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make cuenta CUIT, Razón Social";
                    return;
                }
                _seleccion = new DBCuenta(SafeConvert.ToInt64(parametros[0]), parametros[1]);
            }
            _outputStr = $"Cuenta creada> {_seleccion}";
        }

        private void _CMD_CreateBanco(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make banco nombre, código";
                return;
            }
            _seleccion = new DBBancos(parametros[0].Trim(), SafeConvert.ToInt32(parametros[1]));
            _outputStr = $"Banco creado> {_seleccion}";
        }

        private void _CMD_CrearTipoComprobante(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 1)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make tipo comprobante Nombre";
                return;
            }
            _seleccion = new DBTiposComprobantes(parametros[0], (int)TipoComprobanteFlag.Gravado | (int)TipoComprobanteFlag.NoGravado | (int)TipoComprobanteFlag.IVA | (int)TipoComprobanteFlag.Percepcion | (int)TipoComprobanteFlag.Acredita);
            _outputStr = $"Tipo de comprobante creado> {_seleccion}";
        }
        private void _CMD_CrearTipoRemito(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 1)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make tipo remito Nombre";
                return;
            }
            _seleccion = new DBTipoRemito(parametros[0]);
            _outputStr = $"Tipo de remito creado> {_seleccion}";
        }
        private void _CMD_CrearTipoRecibo(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 1)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make tipo recibo Nombre";
                return;
            }
            _seleccion = new DBTipoRecibo(parametros[0]);
            _outputStr = $"Tipo de remito creado> {_seleccion}";
        }
        private void _CMD_CrearTipoEntidad(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 1)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make tipo entidad Nombre";
                return;
            }
            _seleccion = new DBTipoEntidad(parametros[0]);
            _outputStr = $"Tipo de entidad creada> {_seleccion}";
        }
        private void _CMD_CrearFormaDePago(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make forma pago Nombre, tipo";
                return;
            }

            if (!Enum.IsDefined(typeof(TipoFormaDePago), SafeConvert.ToInt32(parametros[1])))
            {
                _outputStr = "El valor del tipo de forma de pago es invalido. Lista de valores validos:\n";
                Array values = Enum.GetValues(typeof(TipoFormaDePago));

                foreach (TipoFormaDePago val in values)
                {
                    _outputStr += String.Format("\t{0}: {1}\n", (int)val, Enum.GetName(typeof(TipoFormaDePago), val));
                }
                return;
            }

            _seleccion = new DBFormasPago(parametros[0].Trim(), (TipoFormaDePago)SafeConvert.ToInt32(parametros[1]));
            _outputStr = $"Forma de pago creada> {_seleccion}";
        }
        private void _CMD_CrearMoneda(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: make moneda Nombre, ¿Es Extranjera? (0|1)";
                return;
            }
            _seleccion = new DBMoneda(parametros[0], SafeConvert.ToBoolean(parametros[1]));
            _outputStr = $"Moneda creada> {_seleccion}";
        }

        private void _CMD_CuentaSetRazonSocial(string rs)
        {
            if (_seleccion is null || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay cuenta seleccionada.";
                return;
            }
            DBCuenta cuentaSeleccionada = (DBCuenta)_seleccion;
            cuentaSeleccionada.SetRazonSocial(rs.Trim());
            _outputStr = $"Razón social de la cuenta cambiada.";
        }
        private void _CMD_CuentaSetCUIT(string str)
        {
            if (_seleccion is null || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay cuenta seleccionada.";
                return;
            }
            long cuit;
            if (!long.TryParse(str.Trim(), out cuit))
            {
                _outputStr = "El CUIT introducido es inválido.";
                return;
            }
            DBCuenta cuentaSeleccionada = (DBCuenta)_seleccion;
            cuentaSeleccionada.SetCUIT(cuit);
            _outputStr = $"CUIT de la cuenta cambiada.";
        }

        private void _CMD_BancoSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBBancos))
            {
                _outputStr = "No hay un banco seleccionado.";
                return;
            }

            DBBancos bancoSeleccionado = (DBBancos)_seleccion;
            bancoSeleccionado.SetName(name.Trim());
            _outputStr = "Nombre del banco cambiado.";
        }

        private void _CMD_BancoSetCode(string code)
        {
            if (_seleccion is null || !(_seleccion is DBBancos))
            {
                _outputStr = "No hay un banco seleccionado.";
                return;
            }
            DBBancos bancoSeleccionado = (DBBancos)_seleccion;
            bancoSeleccionado.SetCode(SafeConvert.ToInt32(code));
            _outputStr = "Código del banco cambiado.";
        }
        private void _CMD_TipoComprobanteGetAlias()
        {
            if (_seleccion is null || !(_seleccion is DBTiposComprobantes))
            {
                _outputStr = "No hay un tipo de comprobante seleccionado.";
                return;
            }
            DBTiposComprobantes tipoComprobanteSeleccionado = (DBTiposComprobantes)_seleccion;
            MySqlConnection conn = DBConnection.Instance().Connection;

            List<DBTipoComprobanteAlias> aliases = tipoComprobanteSeleccionado.GetAliases(conn);
            _outputStr = "::Aliases::\n";
            foreach (DBTipoComprobanteAlias alias in aliases)
            {
                _outputStr += $"\tAlias: {alias}\n";
            }
        }
        private void _CMD_TipoComprobanteAddAlias(string alias)
        {
            if (_seleccion is null || !(_seleccion is DBTiposComprobantes))
            {
                _outputStr = "No hay un tipo de comprobante seleccionado.";
                return;
            }

            MySqlConnection conn = DBConnection.Instance().Connection;

            DBTiposComprobantes tipoComprobanteSeleccionado = (DBTiposComprobantes)_seleccion;
            tipoComprobanteSeleccionado.AddAlias(alias.Trim());
            tipoComprobanteSeleccionado.PushAliases(conn);

            _outputStr += $"Alias {alias.Trim()} agregado.";
        }

        private void _CMD_TipoComprobanteSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBTiposComprobantes))
            {
                _outputStr = "No hay un tipo de comprobante seleccionado.";
                return;
            }
            DBTiposComprobantes tipoComprobanteSeleccionado = (DBTiposComprobantes)_seleccion;
            tipoComprobanteSeleccionado.SetName(name.Trim());
            _outputStr = $"Nombre del tipo de comprobante cambiado.";
        }
        private void _CMD_TipoRemitoSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBTipoRemito))
            {
                _outputStr = "No hay un tipo de remito seleccionado.";
                return;
            }
            DBTipoRemito tipoRemitoSeleccionado = (DBTipoRemito)_seleccion;
            tipoRemitoSeleccionado.SetName(name.Trim());
            _outputStr = $"Nombre del tipo de remito cambiado.";
        }
        private void _CMD_TipoReciboSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBTipoRecibo))
            {
                _outputStr = "No hay un tipo de recibo seleccionado.";
                return;
            }
            DBTipoRecibo tipoReciboSeleccionado = (DBTipoRecibo)_seleccion;
            tipoReciboSeleccionado.SetName(name.Trim());
            _outputStr = $"Nombre del tipo de recibo cambiado.";
        }
        private void _CMD_TipoEntidadSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBTipoEntidad))
            {
                _outputStr = "No hay un tipo de entidad seleccionado.";
                return;
            }
            DBTipoEntidad tipoEntidadSeleccionado = (DBTipoEntidad)_seleccion;
            tipoEntidadSeleccionado.SetName(name.Trim());
            _outputStr = $"Nombre del tipo de entidad cambiado.";
        }
        private void _CMD_FormaPagoSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBFormasPago))
            {
                _outputStr = "No hay una forma de pago seleccionada.";
                return;
            }
            DBFormasPago formaPagoSeleccionada = (DBFormasPago)_seleccion;
            formaPagoSeleccionada.SetName(name.Trim());
            _outputStr = $"Nombre de la forma de pago cambiada.";
        }

        private void _CMD_MonedaSetName(string name)
        {
            if (_seleccion is null || !(_seleccion is DBMoneda))
            {
                _outputStr = "No hay una moneda seleccionada.";
                return;
            }
            DBMoneda monedaSeleccionada = (DBMoneda)_seleccion;
            monedaSeleccionada.SetName(name.Trim());
            _outputStr = $"Nombre de la moneda cambiada.";
        }
        private void _CMD_MonedaSetExtranjera(string esExtranjera)
        {
            if (_seleccion is null || !(_seleccion is DBMoneda))
            {
                _outputStr = "No hay una moneda seleccionada.";
                return;
            }
            DBMoneda monedaSeleccionada = (DBMoneda)_seleccion;
            monedaSeleccionada.SetExtranjera(SafeConvert.ToBoolean(esExtranjera));
            _outputStr = $"Estado de la moneda cambiada.";
        }
        

        private void _CMD_PrintSelected()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay entidad seleccionada.";
                return;
            }
            _outputStr = $"Entidad seleccionada> {_seleccion}";
        }

        private void _CMD_PushSelected()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay entidad seleccionada.";
                return;
            }

            MySqlConnection conn = DBConnection.Instance().Connection;
            if (_seleccion.PushToDatabase(conn))
            {
                _outputStr = "La entidad se ha agregado correctamente a la base de datos.";
            } else if (!_seleccion.ShouldPush())
            {
                _outputStr = "La entidad no ha cambiado en nada, no es necesario agregarla a la base de datos.";
            } else
            {
                _outputStr = "No se ha podido agregar la entidad seleccionada a la base de datos..";
            }
        }

        private void _CMD_PullSelected()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay entidad seleccionada.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;
            if (_seleccion.PullFromDatabase(conn))
            {
                _outputStr = "La entidad se ha obtenido correctamente desde la base de datos.";
            }
            else
            {
                _outputStr = "No se ha podido obtener esta entidad desde la base de datos.";
            }
        }
        private void _CMD_DeleteSelected()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay entidad seleccionada.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;
            if (_seleccion.DeleteFromDatabase(conn))
            {
                _outputStr = "La entidad se ha eliminado correctamente de la base de datos. Ahora es local";
            }
            else
            {
                _outputStr = "No se ha podido eliminar la entidad seleccionada de la base de datos.";
            }
        }
        private void _CMD_MakeLocalSelected()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay entidad seleccionada.";
                return;
            }
            _seleccion = _seleccion.GetLocalCopy();
            _outputStr = "Has creado una copia local de la entidad.";
        }
        private void _CMD_UnlinkAll()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay entidad seleccionada.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;

            if (_seleccion is DBComprobantes)
            {
                DBComprobantes comprobanteSeleccionado = (DBComprobantes)_seleccion;
                comprobanteSeleccionado.RemoveAllRelationshipsWithRecibosDB(conn);
                _outputStr = "Se han eliminado las relaciones de este comprobante con los recibos.";
            } else if (_seleccion is DBRecibo)
            {
                DBRecibo reciboSeleccionado = (DBRecibo)_seleccion;
                reciboSeleccionado.RemoveAllRelationshipsWithComprobantesDB(conn);
                _outputStr = "Se han eliminado las relaciones de este recibo con los comprobantes.";
            } else
            {
                _outputStr = "No hay entidad seleccionada válida. Tiene que ser un comprobnate o un recibo.";
            }
        }

        private void _CMD_GoBack()
        {
            if (_seleccion is null)
            {
                _outputStr = "No se puede ir para atras ya que no hay nada seleccionado.";
                return;
            }
            switch (_seleccion)
            {
                case DBCuenta cuenta:
                    _outputStr = "¡Cuenta deseleccionada!. Ahora la seleccion es nula.";
                    _seleccion = null;
                break;
                case DBEntidades entidadComercial:
                    _outputStr = "¡Entidad comercial deseleccionada!. Ahora la seleccion es la cuenta: \n";
                    _seleccion = entidadComercial.GetCuenta();
                    _outputStr += $"{_seleccion}"; 
                    break;
                case DBComprobantes comprobante:
                    _outputStr = "¡Comprobante deseleccionada!. Ahora la seleccion es la entidad comercial: \n";
                    _seleccion = comprobante.GetEntidadComercial();
                    _outputStr += $"{_seleccion}";
                    break;
                case DBRecibo recibo:
                    _outputStr = "¡Recibo deseleccionado!. Ahora la seleccion es la entidad comercial: \n";
                    _seleccion = recibo.GetEntidadComercial();
                    _outputStr += $"{_seleccion}";
                    break;
                case DBRemito remito:
                    _outputStr = "¡Remito deseleccionado!. Ahora la seleccion es la entidad comercial: \n";
                    _seleccion = remito.GetEntidadComercial();
                    _outputStr += $"{_seleccion}";
                    break;
                case DBPago pago:
                        _outputStr = "¡Pago deseleccionada!. Ahora la seleccion es el recibo: \n";
                        _seleccion = pago.GetRecibo();
                        _outputStr += $"{_seleccion}";
               break;
            }
        }

        private void _CMD_GetEntidades(string filter)
        {
            if (_seleccion is null || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay una cuenta seleccionada, seleccione una cuenta primero.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBCuenta cuentaSeleccionada = (DBCuenta)_seleccion;
            cuentaSeleccionada.GetAllEntidadesComerciales(conn);
            _outputStr = "\t:: Entidades Comerciales ::\n";
            _outputStr += cuentaSeleccionada.PrintAllEntidades();
        }

        private void _CMD_PrintEntidades()
        {
            if (_seleccion is null || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay una cuenta seleccionada, seleccione una cuenta primero.";
                return;
            }
            DBCuenta cuentaSeleccionada = (DBCuenta)_seleccion;
            _outputStr = "\t:: Entidades Comerciales ::\n";
            _outputStr += cuentaSeleccionada.PrintAllEntidades();
        }

        private void _CMD_SelectEntidad(string id)
        {
            if (_seleccion is null || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay una cuenta seleccionada, seleccione una cuenta primero.";
                return;
            }
            DBCuenta cuentaSeleccionada = (DBCuenta)_seleccion;
            DBEntidades entidadSeleccionada = cuentaSeleccionada.GetEntidadByID(SafeConvert.ToInt64(id.Trim()));

            if (entidadSeleccionada is null)
            {
                _outputStr = "No existe una entidad comercial en esta cuenta con el ID introducido.";
                return;
            }
            _seleccion = entidadSeleccionada;
            _outputStr = $"Entidad seleccionada> {_seleccion}";

        }
        private void _CMD_CrearEntidad(string args)
        {
            if (_seleccion is null || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay una cuenta seleccionada, seleccione una cuenta primero.";
                return;
            }

            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBEntidades.GenerateRandom((DBCuenta)_seleccion);
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length < 3 || parametros.Length > 6)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear entidad ID Tipo Entidad, CUIT, Razón Social, Email='', Teléfono='', Celular=''";
                    return;
                }
                DBTipoEntidad tipoEntidad = DBTipoEntidad.GetByID(SafeConvert.ToInt64(parametros[0]));
                if (tipoEntidad is null)
                {
                    _outputStr = "ID del tipo de entidad seleccionada invalido, vea los tipos de entidades válidos:\n";
                    _outputStr += DBTipoEntidad.PrintAll();
                    return;
                }
                switch (parametros.Length)
                {
                    case 3:
                        _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, SafeConvert.ToInt64(parametros[1]), parametros[2]);
                        break;
                    case 4:
                        _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, SafeConvert.ToInt64(parametros[1]), parametros[2], parametros[3]);
                        break;
                    case 5:
                        _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, SafeConvert.ToInt64(parametros[1]), parametros[2], parametros[3], parametros[4]);
                        break;
                    case 6:
                        _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, SafeConvert.ToInt64(parametros[1]), parametros[2], parametros[3], parametros[5]);
                        break;
                }
            }
            _outputStr = $"Entidad creada> {_seleccion}";
        }

        private void _CMD_GetComprobantes(string filter)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada. Seleccione una entidad comercial, remito o recibo primero.";
                return;
            }
            if (_seleccion is DBEntidades entidadComercialSeleccionada)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                entidadComercialSeleccionada.GetAllComprobantes(conn);
                _outputStr = "\t:: Comprobantes ::\n";
                _outputStr += entidadComercialSeleccionada.PrintAllComprobantes();
            } else if (_seleccion is DBRecibo reciboSeleccionado)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                reciboSeleccionado.GetAllComprobantes(conn);
                _outputStr = "\t:: Comprobantes Relacionados ::\n";
                _outputStr += reciboSeleccionado.PrintAllComprobantes();
            }
            else if (_seleccion is DBRemito remitoSeleccionado)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                remitoSeleccionado.GetAllComprobantes(conn);
                _outputStr = "\t:: Comprobantes Relacionados ::\n";
                _outputStr += remitoSeleccionado.PrintAllComprobantes();
            }
            else
            {
                _outputStr = "No hay una entidad comercial, recibo o remito seleccionado. Seleccione una entidad comercial, remito o un recibo primero.";
            }
        }
        private void _CMD_PrintComprobantes()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada. Seleccione una entidad comercial o recibo primero.";
                return;
            }
            if (_seleccion is DBEntidades)
            {
                DBEntidades entidadComercialSeleccionada = (DBEntidades)_seleccion;
                _outputStr = "\t:: Comprobantes ::\n";
                _outputStr += entidadComercialSeleccionada.PrintAllComprobantes();
            } else if (_seleccion is DBRecibo)
            {
                DBRecibo reciboSeleccionado = (DBRecibo)_seleccion;
                _outputStr = "\t:: Comprobantes relacionados ::\n";
                _outputStr += reciboSeleccionado.PrintAllComprobantes();
            } else {
                _outputStr = "No hay una entidad comercial o recibo seleccionado. Seleccione una entidad comercial o un recibo primero.";
            }
        }

        private void _CMD_CrearComprobante(string args)
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBComprobantes.GenerateRandom((DBEntidades)_seleccion);
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length < 7 || parametros.Length > 11)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear comprobante ID Tipo Comprobante, ID Moneda, Emitido, Fecha, Numero, Gravado, IVA, No Gravado='0', Percepción='0', Cambio='1.0', Obs=''";
                    return;
                }
                DBTiposComprobantes tipoComprobante = DBTiposComprobantes.GetByID(SafeConvert.ToInt64(parametros[0]));
                if (tipoComprobante is null)
                {
                    _outputStr = "ID del tipo de comprobante seleccionado inválido, vea los tipos de comprobantes válidos:\n";
                    _outputStr += DBTiposComprobantes.PrintAll();
                    return;
                }

                DBMoneda moneda = DBMoneda.GetByID(SafeConvert.ToInt64(parametros[1]));
                if (moneda is null)
                {
                    _outputStr = "ID de moneda seleccionado inválido, vea las monedas válidas:\n";
                    _outputStr += DBMoneda.PrintAll();
                    return;
                }

                DateTime fechaEmitido = new DateTime();
                DateTime? fechaFinal = null;
                if (DateTime.TryParse(parametros[3], out fechaEmitido))
                {
                    fechaFinal = fechaEmitido;
                }

                switch (parametros.Length)
                {
                    case 7:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, moneda, SafeConvert.ToBoolean(parametros[2]), fechaFinal, parametros[4], SafeConvert.ToDouble(parametros[5]), SafeConvert.ToDouble(parametros[6]));
                        break;
                    case 8:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, moneda, SafeConvert.ToBoolean(parametros[2]), fechaFinal, parametros[4], SafeConvert.ToDouble(parametros[5]), SafeConvert.ToDouble(parametros[6]), SafeConvert.ToDouble(parametros[7]));

                        break;
                    case 9:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, moneda, SafeConvert.ToBoolean(parametros[2]), fechaFinal, parametros[4], SafeConvert.ToDouble(parametros[5]), SafeConvert.ToDouble(parametros[6]), SafeConvert.ToDouble(parametros[7]), SafeConvert.ToDouble(parametros[8]));
                        break;
                    case 10:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, moneda, SafeConvert.ToBoolean(parametros[2]), fechaFinal, parametros[4], SafeConvert.ToDouble(parametros[5]), SafeConvert.ToDouble(parametros[6]), SafeConvert.ToDouble(parametros[7]), SafeConvert.ToDouble(parametros[8]), SafeConvert.ToDouble(parametros[9]));
                        break;
                    case 11:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, moneda, SafeConvert.ToBoolean(parametros[2]), fechaFinal, parametros[4], SafeConvert.ToDouble(parametros[5]), SafeConvert.ToDouble(parametros[6]), SafeConvert.ToDouble(parametros[7]), SafeConvert.ToDouble(parametros[8]), SafeConvert.ToDouble(parametros[9]), parametros[10]);
                        break;
                }
            }
            _outputStr = $"Comprobante creado> {_seleccion}";
        }

        private void _CMD_SelectComprobante(string id)
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            DBEntidades entidadComercialSeleccionada = (DBEntidades)_seleccion;

            DBComprobantes comprobanteSeleccionado = entidadComercialSeleccionada.GetComprobanteByID(SafeConvert.ToInt64(id.Trim()));

            if (comprobanteSeleccionado is null)
            {
                _outputStr = "No existe una comprobante en esta entidad comercial con el ID introducido.";
                return;
            }
            _seleccion = comprobanteSeleccionado;
            _outputStr = $"Comprobante seleccionado> {_seleccion}";

        }
        private void _CMD_LinkRecibo(string id)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado, seleccione un comprobante primero.";
                return;
            }
            DBComprobantes comprobante = (DBComprobantes)_seleccion;
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBRecibo recibo = DBRecibo.GetByID(conn, comprobante.GetEntidadComercial(), SafeConvert.ToInt64(id.Trim()));
            if (recibo is null)
            {
                _outputStr = "No existe un recibo con el ID seleccionado.";
                return;
            }
            comprobante.AddRecibo(recibo);
            comprobante.PushAllRelationshipsWithRecibosDB(conn);
            _outputStr = "Recibo relacionado al comprobante.";
        }

        private void _CMD_UnlinkRecibo(string id)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado, seleccione un comprobante primero.";
                return;
            }
            DBComprobantes comprobante = (DBComprobantes)_seleccion;
            MySqlConnection conn = DBConnection.Instance().Connection;
            if (comprobante.RemoveRelationshipReciboDB(conn, SafeConvert.ToInt64(id.Trim())))
            {
                _outputStr = "Recibo removido de la relación con el comprobante exitosamente.";
            }
            else
            {
                _outputStr = "Error, no se pudo eliminar la relación entre el comprobante y el recibo.";
            }
        }

        private void _CMD_LinkRemito(string id)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado, seleccione un comprobante primero.";
                return;
            }
            DBComprobantes comprobante = (DBComprobantes)_seleccion;
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBRemito remito = DBRemito.GetByID(conn, comprobante.GetEntidadComercial(), SafeConvert.ToInt64(id.Trim()));
            if (remito is null)
            {
                _outputStr = "No existe un remito con el ID seleccionado.";
                return;
            }
            comprobante.AddRemito(remito);
            comprobante.PushAllRelationshipsWithRemitosDB(conn);
            _outputStr = "Remito relacionado al comprobante.";
        }

        private void _CMD_UnlinkRemito(string id)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado, seleccione un comprobante primero.";
                return;
            }
            DBComprobantes comprobante = (DBComprobantes)_seleccion;
            MySqlConnection conn = DBConnection.Instance().Connection;
            if (comprobante.RemoveRelationshipRemitoDB(conn, SafeConvert.ToInt64(id.Trim())))
            {
                _outputStr = "Remito removido de la relación con el comprobante exitosamente.";
            }
            else
            {
                _outputStr = "Error, no se pudo eliminar la relación entre el comprobante y el remito.";
            }
        }


        private void _CMD_GetRecibos(string filter)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada. Seleccione una entidad comercial o comprobante primero.";
                return;
            }
            if (_seleccion is DBEntidades entidadComercialSeleccionada)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                entidadComercialSeleccionada.GetAllRecibos(conn);
                _outputStr = "\t:: Recibos ::\n";
                _outputStr += entidadComercialSeleccionada.PrintAllRecibos();
            } else if (_seleccion is DBComprobantes comprobanteSeleccionado) {
                MySqlConnection conn = DBConnection.Instance().Connection;
                comprobanteSeleccionado.GetAllRecibos(conn);
                _outputStr = "\t:: Recibos Relacionados ::\n";
                _outputStr += comprobanteSeleccionado.PrintAllRecibos();
            } else
            {
                _outputStr = "La entidad seleccionada no es válida para esta operación. Seleccione una entidad comercial o un comprobante.";
            }
        }
        private void _CMD_PrintRecibos()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada. Seleccione una entidad comercial o comprobante primero.";
                return;
            }
            if (_seleccion is DBEntidades entidadComercialSeleccionada)
            {
                _outputStr = "\t:: Recibos ::\n";
                _outputStr += entidadComercialSeleccionada.PrintAllRecibos();
            }
            else if (_seleccion is DBComprobantes comprobanteSeleccionado)
            {
                _outputStr = "\t:: Recibos Relacionados ::\n";
                _outputStr += comprobanteSeleccionado.PrintAllRecibos();
            } else
            {
                _outputStr = "La entidad seleccionada no es válida para esta operación. Seleccione una entidad comercial o un comprobante.";
            }
        }

        private void _CMD_CrearRecibo(string args)
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBRecibo.GenerateRandom((DBEntidades)_seleccion);
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length < 4 || parametros.Length > 5)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear comprobante ID Tipo Recibo, Emitido (0|1), Fecha del Recibo (dd/MM/yyyy), Numero de Recibo, Observación=''";
                    return;
                }
                DBTipoRecibo tipoRecibo = DBTipoRecibo.GetByID(SafeConvert.ToInt64(parametros[0]));
                if (tipoRecibo is null)
                {
                    _outputStr = "ID del tipo de recibo seleccionado inválido, vea los tipos de recibos válidos:\n";
                    _outputStr += DBTipoRecibo.PrintAll();
                    return;
                }

                DateTime fechaEmitido = new DateTime();
                DateTime? fechaFinal = null;
                if (DateTime.TryParse(parametros[2], out fechaEmitido))
                {
                    fechaFinal = fechaEmitido;
                }

                switch (parametros.Length)
                {
                    case 4:
                        _seleccion = new DBRecibo((DBEntidades)_seleccion, tipoRecibo, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3]);
                        break;
                    case 5:
                        _seleccion = new DBRecibo((DBEntidades)_seleccion, tipoRecibo, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3], parametros[4]);
                        break;
                }
            }
            _outputStr = $"Recibo creado> {_seleccion}";
        }

        private void _CMD_SelectRecibo(string id)
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            DBEntidades entidadComercialSeleccionada = (DBEntidades)_seleccion;
            DBRecibo reciboSeleccionado = entidadComercialSeleccionada.GetReciboByID(SafeConvert.ToInt64(id.Trim()));

            if (reciboSeleccionado is null)
            {
                _outputStr = "No existe un recibo en esta entidad comercial con el ID introducido.";
                return;
            }
            _seleccion = reciboSeleccionado;
            _outputStr = $"Recibo seleccionado> {_seleccion}";

        }


        private void _CMD_GetRemitos(string filter)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada. Seleccione una entidad comercial o comprobante primero.";
                return;
            }
            if (_seleccion is DBEntidades entidadComercialSeleccionada)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                entidadComercialSeleccionada.GetAllRemitos(conn);
                _outputStr = "\t:: Remitos ::\n";
                _outputStr += entidadComercialSeleccionada.PrintAllRemitos();
            }
            else if (_seleccion is DBComprobantes comprobanteSeleccionado)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                comprobanteSeleccionado.GetAllRemitos(conn);
                _outputStr = "\t:: Remitos Relacionados ::\n";
                _outputStr += comprobanteSeleccionado.PrintAllRemitos();
            }
            else
            {
                _outputStr = "La entidad seleccionada no es válida para esta operación. Seleccione una entidad comercial o un comprobante.";
            }
        }
        private void _CMD_PrintRemitos()
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada. Seleccione una entidad comercial o comprobante primero.";
                return;
            }
            if (_seleccion is DBEntidades entidadComercialSeleccionada)
            {
                _outputStr = "\t:: Remitos ::\n";
                _outputStr += entidadComercialSeleccionada.PrintAllRemitos();
            }
            else if (_seleccion is DBComprobantes comprobanteSeleccionado)
            {
                _outputStr = "\t:: Remitos Relacionados ::\n";
                _outputStr += comprobanteSeleccionado.PrintAllRemitos();
            }
            else
            {
                _outputStr = "La entidad seleccionada no es válida para esta operación. Seleccione una entidad comercial o un comprobante.";
            }
        }

        private void _CMD_CrearRemito(string args)
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBRemito.GenerateRandom((DBEntidades)_seleccion);
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length < 4 || parametros.Length > 5)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear remito ID Tipo Remito, Emitido (0|1), Fecha del Remito (dd/MM/yyyy), Numero de Remito, Observación=''";
                    return;
                }
                DBTipoRemito tipoRemito = DBTipoRemito.GetByID(SafeConvert.ToInt64(parametros[0]));
                if (tipoRemito is null)
                {
                    _outputStr = "ID del tipo de remito seleccionado inválido, vea los tipos de remitos válidos:\n";
                    _outputStr += DBTipoRemito.PrintAll();
                    return;
                }

                DateTime fechaEmitido = new DateTime();
                DateTime? fechaFinal = null;
                if (DateTime.TryParse(parametros[2], out fechaEmitido))
                {
                    fechaFinal = fechaEmitido;
                }

                switch (parametros.Length)
                {
                    case 4:
                        _seleccion = new DBRemito((DBEntidades)_seleccion, tipoRemito, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3]);
                        break;
                    case 5:
                        _seleccion = new DBRemito((DBEntidades)_seleccion, tipoRemito, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3], parametros[4]);
                        break;
                }
            }
            _outputStr = $"Remito creado> {_seleccion}";
        }

        private void _CMD_SelectRemito(string id)
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            DBEntidades entidadComercialSeleccionada = (DBEntidades)_seleccion;
            DBRemito remitoSeleccionado = entidadComercialSeleccionada.GetRemitoByID(SafeConvert.ToInt64(id.Trim()));

            if (remitoSeleccionado is null)
            {
                _outputStr = "No existe un remito en esta entidad comercial con el ID introducido.";
                return;
            }
            _seleccion = remitoSeleccionado;
            _outputStr = $"Remito seleccionado> {_seleccion}";

        }

        private void _CMD_LinkComprobante(string id)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada.";
                return;
            }
            if (_seleccion is DBRecibo recibo)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                DBComprobantes comprobantes = DBComprobantes.GetByID(conn, recibo.GetEntidadComercial(), SafeConvert.ToInt64(id.Trim()));
                if (comprobantes is null)
                {
                    _outputStr = "No existe un comprobante con el ID seleccionado.";
                    return;
                }
                recibo.AddComprobante(comprobantes);
                recibo.PushAllRelationshipsWithComprobantesDB(conn);
                _outputStr = "Comprobante relacionado al recibo.";
            }
            else if (_seleccion is DBRemito remito)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                DBComprobantes comprobantes = DBComprobantes.GetByID(conn, remito.GetEntidadComercial(), SafeConvert.ToInt64(id.Trim()));
                if (comprobantes is null)
                {
                    _outputStr = "No existe un comprobante con el ID seleccionado.";
                    return;
                }
                remito.AddComprobante(comprobantes);
                remito.PushAllRelationshipsWithComprobantesDB(conn);
                _outputStr = "Comprobante relacionado al remito.";
            } else
            {
                _outputStr = "La entidad seleccionada no es ni un remito ni un recibo.";
            }
        }

        private void _CMD_UnlinkComprobante(string id)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay una entidad seleccionada.";
                return;
            }
            if (_seleccion is DBRecibo recibo)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                if (recibo.RemoveRelationshipComprobanteDB(conn, SafeConvert.ToInt64(id.Trim())))
                {
                    _outputStr = "Comprobante removido de la relación con el recibo exitosamente.";
                }
                else
                {
                    _outputStr = "Error, no se pudo eliminar la relación entre el comprobnate y el recibo.";
                }
            } else if (_seleccion is DBRemito remito)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                if (remito.RemoveRelationshipComprobanteDB(conn, SafeConvert.ToInt64(id.Trim())))
                {
                    _outputStr = "Comprobante removido de la relación con el remito exitosamente.";
                }
                else
                {
                    _outputStr = "Error, no se pudo eliminar la relación entre el comprobnate y el remito.";
                }
            } else {
                _outputStr = "La entidad seleccionada no es ni un remito ni un recibo.";
            }
        }

        private void _CMD_GetPagos(string filter)
        {
            if (_seleccion is null || !(_seleccion is DBRecibo))
            {
                _outputStr = "No hay un recibo seleccionado. Seleccione un recibo primero.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBRecibo reciboSeleccionado = (DBRecibo)_seleccion;
            reciboSeleccionado.GetAllPagos(conn);
            _outputStr = "\t:: Pagos ::\n";
            _outputStr += reciboSeleccionado.PrintAllPagos();
        }

        private void _CMD_PrintPagos()
        {
            if (_seleccion is null || !(_seleccion is DBRecibo))
            {
                _outputStr = "No hay un recibo seleccionado. Seleccione un recibo primero.";
                return;
            }
            DBRecibo reciboSeleccionado = (DBRecibo)_seleccion;
            _outputStr = "\t:: Pagos ::\n";
            _outputStr += reciboSeleccionado.PrintAllPagos();
        }

        private void _CMD_SelectPago(string id)
        {
            if (_seleccion is null || !(_seleccion is DBRecibo))
            {
                _outputStr = "No hay un recibo seleccionado. Seleccione un recibo primero.";
                return;
            }
            DBRecibo reciboSeleccionado = (DBRecibo)_seleccion;

            DBPago pagoSeleccionado = reciboSeleccionado.GetPagoByID(SafeConvert.ToInt64(id.Trim()));

            if (pagoSeleccionado is null)
            {
                _outputStr = "No existe un pago en este recibo con el ID introducido.";
                return;
            }
            _seleccion = pagoSeleccionado;
            _outputStr = $"Pago seleccionado> {_seleccion}";

        }

        private void _CMD_CrearPago(string args)
        {
            if (_seleccion is null || !(_seleccion is DBRecibo))
            {
                _outputStr = "No hay un recibo seleccionado. Seleccione un recibo primero.";
                return;
            }

            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBPago.GenerateRandom((DBRecibo)_seleccion);
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length < 4 || parametros.Length > 6)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear entidad ID Forma de Pago, ID Moneday, Importe, Observación, Fecha='', Cambio=1.0";
                    return;
                }
                DBFormasPago formasPago = DBFormasPago.GetByID(SafeConvert.ToInt64(parametros[0]));
                if (formasPago is null)
                {
                    _outputStr = "ID de la forma de pago no es válido, vea las formas de pago válidas:\n";
                    _outputStr += DBFormasPago.PrintAll();
                    return;
                }
                DBMoneda moneda = DBMoneda.GetByID(SafeConvert.ToInt64(parametros[1]));
                if (moneda is null)
                {
                    _outputStr = "ID de moneda no es válido, véa las moneda válidas:\n";
                    _outputStr += DBMoneda.PrintAll();
                    return;
                }
                switch (parametros.Length)
                {
                    case 4:
                        _seleccion = new DBPago((DBRecibo)_seleccion, formasPago, moneda, SafeConvert.ToDouble(parametros[2]), parametros[3]);
                        break;
                    case 5:
                        DateTime fechaPago = new DateTime();
                        DateTime? fechaFinal = null;
                        if (DateTime.TryParse(parametros[4], out fechaPago))
                        {
                            fechaFinal = fechaPago;
                        }
                        _seleccion = new DBPago((DBRecibo)_seleccion, formasPago, moneda, SafeConvert.ToDouble(parametros[2]), parametros[3], fechaFinal);
                        break;
                    case 6:
                        DateTime fechaP = new DateTime();
                        DateTime? fechaF = null;
                        if (DateTime.TryParse(parametros[4], out fechaP))
                        {
                            fechaF = fechaP;
                        }
                        _seleccion = new DBPago((DBRecibo)_seleccion, formasPago, moneda, SafeConvert.ToDouble(parametros[2]), parametros[3], fechaF, SafeConvert.ToDouble(parametros[5]));
                        break;
                }
            }
            _outputStr = $"Pago creado> {_seleccion}";
        }
 
        private void _CMD_PopulateRandom()
        {
            if (_inputRegister.Count < 2 || !_inputRegister[_inputRegister.Count - 2].ToLower().Contains("populate random"))
            {
                _outputStr = "¿Estás realmente seguro de que queres llenar la base de datos de contenido random?, Esto va a eliminar TODOS los datos. Si estás seguro, escribí de nuevo:\npopulate random";
                return;
            }
            _CMD_ResetDatabase(true);
            _CMD_GenerateBasicData(true);
            MySqlConnection conn = DBConnection.Instance().Connection;
            Random r = new Random(Guid.NewGuid().GetHashCode());
            int cuentasCount = r.Next(1, 2);
            for (int i=0; i < cuentasCount; i++)
            {
                DBCuenta randomCuenta = DBCuenta.GenerateRandom();
                if (!randomCuenta.PushToDatabase(conn))
                {
                    continue;
                }
                int entidadesCount = r.Next(5, 10);
                for (int j=0; j < entidadesCount; j++)
                {
                    DBEntidades randomEntidad = DBEntidades.GenerateRandom(randomCuenta);
                    if (!randomEntidad.PushToDatabase(conn))
                    {
                        continue;
                    }
                    int comprobantesCount = r.Next(10, 25);
                    List<DBComprobantes> comprobantes = new List<DBComprobantes>();
                    for (int k=0; k < comprobantesCount; k++)
                    {
                        DBComprobantes randomComprobante = DBComprobantes.GenerateRandom(randomEntidad);
                        if (randomComprobante.PushToDatabase(conn))
                        {
                            comprobantes.Add(randomComprobante);
                        }
                    }
                    int recibosCount = r.Next(10, 25);
                    for (int k = 0; k < recibosCount; k++)
                    {
                        DBRecibo randomRecibo = DBRecibo.GenerateRandom(randomEntidad);
                        if (randomRecibo.PushToDatabase(conn))
                        {
                            int pagosCount = r.Next(1, 4);
                            for (int z =0; z < pagosCount; z++)
                            {
                                DBPago randomPago = DBPago.GenerateRandom(randomRecibo);
                                randomPago.PushToDatabase(conn);
                            }
                            int comprobantesRelacionados = r.Next(0, 4);
                            for (int z = 0; z < comprobantesRelacionados; z++)
                            {
                                randomRecibo.AddComprobante(comprobantes[r.Next(0, comprobantes.Count)]);
                            }
                            randomRecibo.PushAllRelationshipsWithComprobantesDB(conn);
                        }
                    }
                    int remitosCount = r.Next(10, 25);
                    for (int k = 0; k < remitosCount; k++)
                    {
                        DBRemito randomRemito = DBRemito.GenerateRandom(randomEntidad);
                        if (randomRemito.PushToDatabase(conn))
                        {
                            int comprobantesRelacionados = r.Next(0, 4);
                            for (int z = 0; z < comprobantesRelacionados; z++)
                            {
                                randomRemito.AddComprobante(comprobantes[r.Next(0, comprobantes.Count)]);
                            }
                            randomRemito.PushAllRelationshipsWithComprobantesDB(conn);
                        }
                    }
                }
            }
            _outputStr = "Done!";
        }
        private void _CMD_ResetDatabase(bool force = false)
        {
            if (!force && (_inputRegister.Count < 2 || !_inputRegister[_inputRegister.Count-2].ToLower().Contains("reset database")))
            {
                _outputStr = "¿Estás realmente seguro de que queres resetear el contenido de la base de datos?, Esto va a eliminar TODOS los datos. Si estás seguro, escribí de nuevo:\nreset database";
                return;
            }

            MySqlConnection conn = DBConnection.Instance().Connection;
            try
            {
                //Deleting recibos_comprobantes
                string query = $"DELETE FROM {DBRecibo.db_relation_table}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting remitos_comprobantes
                query = $"DELETE FROM remitos_comprobantes";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting pagos
                query = $"DELETE FROM {DBPago.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBPago.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting recibos
                query = $"DELETE FROM {DBRecibo.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBRecibo.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting remitos
                query = "DELETE FROM remitos";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = "ALTER TABLE remitos AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting comprobantes
                query = $"DELETE FROM {DBComprobantes.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBComprobantes.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting ent_comerciales
                query = $"DELETE FROM {DBEntidades.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBEntidades.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting tipos_recibos
                query = $"DELETE FROM {DBTipoRecibo.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBTipoRecibo.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting tipos_remitos
                query = $"DELETE FROM {DBTipoRemito.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBTipoRemito.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting formas_pago
                query = $"DELETE FROM {DBFormasPago.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBFormasPago.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting tipos_entidades
                query = $"DELETE FROM {DBTipoEntidad.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBTipoEntidad.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting tipos_comprobantes
                query = $"DELETE FROM {DBTiposComprobantes.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBTiposComprobantes.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting monedas
                query = $"DELETE FROM {DBMoneda.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBMoneda.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting cuentas
                query = $"DELETE FROM {DBCuenta.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBCuenta.db_table} AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                _outputStr += "Base de datos reseteada sin problemas.";
            }
            catch (Exception ex)
            {
                _outputStr += $"SQL error: {ex.ToString()}";
            }
        }

        private void _CMD_GenerateBasicData(bool force=false)
        {
            if (!force && (_inputRegister.Count < 2 || !_inputRegister[_inputRegister.Count - 2].ToLower().Contains("generate basic data")))
            {
                _outputStr = "¿Estás realmente seguro de que queres volver a generar los datos básicos en la base de datos?, Esto puedo corromper la base de datos. Si estás seguro, escribí de nuevo:\ngenerate basic data";
                return;
            }

            MySqlConnection conn = DBConnection.Instance().Connection;
            try
            {
                //Deleting tipos_recibos
                DBTipoRemito.ResetDBData(conn);

                //Deleting tipos_remitos
                DBTipoRemito.ResetDBData(conn);

                //Deleting formas_pago
                DBFormasPago.ResetDBData(conn);

                //Deleting tipos_entidades
                DBTipoEntidad.ResetDBData(conn);

                //Deleting tipos_comprobantes
                DBTiposComprobantes.ResetDBData(conn);

                //Deleting monedas
                DBMoneda.ResetDBData(conn);

                //Deleting bancos
                DBBancos.ResetDBData(conn);

                //Now populate.

                //POPULATE TiposComprobantes
                DBTiposComprobantes.PushDefaultData(conn);

                //POPULATE FormasPago
                DBFormasPago.PushDefaultData(conn);

                //POPULATE TipoEntidad
                DBTipoEntidad.PushDefaultData(conn);

                //POPULATE TipoRecibos
                DBTipoRecibo.PushDefaultData(conn);

                //POPULATE TipoRemitos
                DBTipoRemito.PushDefaultData(conn);

                //POPULATE Monedas
                DBMoneda.PushDefaultData(conn);

                //POPULATE Bancos
                DBBancos.PushDefaultData(conn);


                _outputStr = "Tipos de datos basicos generados nuevamente.";
            }
            catch (Exception ex)
            {
                _outputStr += $"SQL error: {ex.ToString()}";
            }
        }
        private void _CMD_MakeExcel(string fileName)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay selección.";
                return;
            }
            if (_seleccion is DBCuenta cuenta)
            {

                MySqlConnection conn = DBConnection.Instance().Connection;
                ExcelExport.ExportToFile(cuenta.GetAllComprobantes(conn), fileName);
                _outputStr = "Exportado todos los comprobantes de esta cuenta.";
            }
            else if (_seleccion is DBEntidades entidadComercial)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                ExcelExport.ExportToFile(entidadComercial.GetAllComprobantes(conn), fileName);
                _outputStr = "Exportado todos los comprobantes de esta entidad comercial.";
            } else
            {
                _outputStr = "Es necesario seleccionar una cuenta o entidad comercial para exportar un excel de los comprobantes.";
            }
        }

        private void _CMD_PrintPDF(string fileName)
        {
            if (!System.IO.File.Exists(fileName.Trim()) || fileName.Trim().Length < 2)
            {
                _outputStr = "El archivo indicado no existe!";
            }
            else
            {
                _outputStr = $"Imprimiendo {fileName.Trim()}";
                PrinterManagment.PrintPDF(fileName.Trim(), Config.GetGlobalConfig().GetDefaultPrinter());
            }

        }

        private void _CMD_MakePDF(string fileName)
        {
            if (_seleccion is null)
            {
                _outputStr = "No hay seleccion";
                return;
            }
            fileName = fileName.Trim();
            if (fileName.Length <= 1)
            {
                _outputStr = "Formato: create pdf nombre_archivo.pdf";
                return;
            }
            string fileNameExcel = System.IO.Path.GetFileNameWithoutExtension(fileName) + "_tmp.xlsx";
            if (_seleccion is DBCuenta cuenta)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                ExcelExport.ExportToFile(cuenta.GetAllComprobantes(conn), fileNameExcel);
                PrinterManagment.ConvertExcelToPDF(fileNameExcel, fileName);
                _outputStr = "Exportado todos los comprobantes de esta cuenta.";
            } else if (_seleccion is DBEntidades entidadComercial)
            {
                MySqlConnection conn = DBConnection.Instance().Connection;
                ExcelExport.ExportToFile(entidadComercial.GetAllComprobantes(conn), fileNameExcel);
                PrinterManagment.ConvertExcelToPDF(fileNameExcel, fileName);
                _outputStr = "Exportado todos los comprobantes de esta entidad comercial.";
            } else
            {
                _outputStr = "Es necesario seleccionar una cuenta o entidad comercial para exportar un excel de los comprobantes";
            }
        }

        private void _CMD_Encrypt(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = $"La cantidad de parámetros ({parametros.Length}) introducida es incorrecta.\nFormato: encrypt contraseña, encryptionKey";
                return;
            }

            string stringToEncrypt = parametros[0].Trim();
            string encryptionKey = parametros[1].Trim();

            string encryptedString = EncryptManager.EncryptString(stringToEncrypt, encryptionKey);

            _outputStr = $"string encriptada: {encryptedString}";

        }

        private void _CMD_Decrypt(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: decrypt encryptedPassword, encryptionKey";
                return;
            }

            string encryptedString = parametros[0].Trim();
            string encryptionKey = parametros[1].Trim();

            string decryptedString = EncryptManager.DecryptString(encryptedString, encryptionKey);

            _outputStr = $"string desencriptada: {decryptedString}";
        }

        private void _CMD_SaveConfig(string fileName)
        {
            (new Config()).ExportToJSONFile(fileName.Trim());
            _outputStr = $"Exportado archivo de configuración en {fileName.Trim()}";

        }

        private void _CMD_LoadConfig(string fileName)
        {
            Config loadedCFG = new Config();
            loadedCFG.ImportFromJSONFile(fileName.Trim());
            _outputStr = $"Importado archivo de configuración:\n{loadedCFG}";
        }

        private void _CMD_CreateUser(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: add user user_name, user_pasword";
                return;
            }
            User tmpUser = new User(parametros[0].Trim(), parametros[1].Trim());
            MySqlConnection conn = DBConnection.Instance().Connection;

            if (tmpUser.PushToDatabase(conn))
            {
                _outputStr = $"Usuario creado correctamente.\nPassword: {tmpUser.GetPassword()}\n Encrypted password: {tmpUser.GetEncryptedPassword()}";
            }
            else
            {
                _outputStr = "No se pudo crear el usuario";
            }
        }

        private void _CMD_RemoveUser(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: remove user user_name";
                return;
            }
        }

        private void _CMD_UserLogin(string args)
        {
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: login user_name, user_password";
                return;
            }

            User tmpUser = new User(parametros[0].Trim(), parametros[1].Trim());
            MySqlConnection conn = DBConnection.Instance().Connection;
            if (tmpUser.isValid(conn))
            {
                _outputStr = "Usuario logeado correctamente.";
            } else
            {
                _outputStr = $"Datos ingresados incorrectos.\nPassword: {tmpUser.GetPassword()}\n Encrypted password: {tmpUser.GetEncryptedPassword()}";
            }
        }

        private void _CMD_ImportComprobantes(string fileName)
        {
            if ((_seleccion is null) || !(_seleccion is DBCuenta))
            {
                _outputStr = "No hay cuenta seleccionada.";
                return;
            }
            if (!System.IO.File.Exists(fileName.Trim()) || fileName.Trim().Length < 2)
            {
                _outputStr = "El archivo indicado no existe!";
            }
            List<DBTiposComprobantes> missingTiposComprobantes = new List<DBTiposComprobantes>();
            List<DBEntidades> missingEntidades = new List<DBEntidades>();
            List<DBMoneda> missingMonedas = new List<DBMoneda>();
            AFIPComprobantes.GetMissingTypesFromFile((DBCuenta)_seleccion, fileName.Trim(), missingEntidades, missingTiposComprobantes, missingMonedas);

            foreach (DBEntidades entidad in missingEntidades)
            {
                _outputStr += $"Entidad no encontrada: \n{entidad}\n\n";
            }
            foreach (DBMoneda moneda in missingMonedas)
            {
                _outputStr += $"Moneda no encontrada: \n{moneda}\n\n";
            }
            foreach (DBTiposComprobantes tipoComprobante in missingTiposComprobantes)
            {
                _outputStr += $"Tipo de Comprobante no encontrado: \n{tipoComprobante}\n\n";
            }

            List<DBComprobantes> comprobantesAFIP = AFIPComprobantes.ImportFromFile((DBCuenta)_seleccion, fileName.Trim());

            foreach(DBComprobantes comprobante in comprobantesAFIP){
                _outputStr += $"Comprobante: \n{comprobante}\n\n";
            }
        }

        private void _CMD_InstallPatch()
        {
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBBancos.ResetDBData(conn);
            DBBancos.PushDefaultData(conn);
        }

        private void ProcessHelpCommand(string args)
        {
            _outputStr = ".: Información :.\n\n";
            _outputStr += "Ejecutar SQL: sql>\n";
            _outputStr += "Ejecutar comandos internos: $>\n";
            _outputStr += "Lista de comandos internos: $>help\n";
        }
    }
}
