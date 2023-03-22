using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

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
        private List<KeyValuePair<string, Action<string>>> internalComandsList = new List<KeyValuePair<string, Action<string>>>();
        private DBBaseClass _seleccion;
        private DeveloperConsole()
        {
            commandsList.Add(new KeyValuePair<string, Action<string>>("SQL>", (x) => ProcessSQLCommand(x)));
            commandsList.Add(new KeyValuePair<string, Action<string>>("sql>", (x) => ProcessSQLCommand(x))); //alias
            commandsList.Add(new KeyValuePair<string, Action<string>>("$>", (x) => ProcessInternalCommand(x))); //internal commands

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("get data", (x) => _CMD_RefreshBasicDataDB()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("print data", (x) => _CMD_PrintBasicDataDB()));

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("select cuenta", (x) => _CMD_SelectCuenta(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("make cuenta", (x) => _CMD_CrearCuenta(x)));

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("selected", (x) => _CMD_PrintSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("delete", (x) => _CMD_DeleteSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("push", (x) => _CMD_PushSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("pull", (x) => _CMD_PullSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("duplicate", (x) => _CMD_MakeLocalSelected()));

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("get entidades", (x) => _CMD_GetEntidades(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("print entidades", (x) => _CMD_PrintEntidades()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("select entidad", (x) => _CMD_SelectEntidad(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("make entidad", (x) => _CMD_CrearEntidad(x)));

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("get comprobantes", (x) => _CMD_GetComprobantes(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("print comprobantes", (x) => _CMD_PrintComprobantes()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("select comprobante", (x) => _CMD_SelectComprobante(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("make comprobante", (x) => _CMD_CrearComprobante(x)));

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("get pagos", (x) => _CMD_GetPagos(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("print pagos", (x) => _CMD_PrintPagos()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("select pago", (x) => _CMD_SelectPago(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("make pago", (x) => _CMD_CrearPago(x)));

            internalComandsList.Add(new KeyValuePair<string, Action<string>>("go back", (x) => _CMD_GoBack()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("generate basic data", (x) => _CMD_GenerateBasicData()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("reset database", (x) => _CMD_ResetDatabase()));
        }
        ///<summary>
        ///Process an input string and retrieves the result.
        ///</summary
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
            foreach (KeyValuePair<string, Action<string>> internalCommand in internalComandsList)
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
        }
        private void _CMD_SelectCuenta(string id)
        {
            _seleccion = DBCuenta.GetByID(Convert.ToInt64(id.Trim()));
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
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear cuenta CUIT, Razón Social";
                    return;
                }
                _seleccion = new DBCuenta(Convert.ToInt64(parametros[0]), parametros[1]);
            }
            _outputStr = $"Cuenta creada> {_seleccion}";
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
                case DBPago pago:
                    _outputStr = "¡Pago deseleccionada!. Ahora la seleccion es el comprobante: \n";
                    _seleccion = pago.GetComprobante();
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
            DBEntidades entidadSeleccionada = cuentaSeleccionada.GetEntidadByID(Convert.ToInt64(id.Trim()));

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
                DBTipoEntidad tipoEntidad = DBTipoEntidad.GetByID(Convert.ToInt64(parametros[0]));
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
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBEntidades entidadComercialSeleccionada = (DBEntidades)_seleccion;
            entidadComercialSeleccionada.GetAllComprobantes(conn);
            _outputStr = "\t:: Comprobantes ::\n";
            _outputStr += entidadComercialSeleccionada.PrintAllComprobantes();
        }
        private void _CMD_PrintComprobantes()
        {
            if (_seleccion is null || !(_seleccion is DBEntidades))
            {
                _outputStr = "No hay una entidad comercial seleccionada, seleccione una entidad comercial primero.";
                return;
            }
            DBEntidades entidadComercialSeleccionada = (DBEntidades)_seleccion;
            _outputStr = "\t:: Comprobantes ::\n";
            _outputStr += entidadComercialSeleccionada.PrintAllComprobantes();
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
                if (parametros.Length < 6 || parametros.Length > 8)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear comprobante ID Tipo Comprobante, Emitido, Fecha, Numero, Gravado, IVA, No Gravado='0', Percepción='0'";
                    return;
                }
                DBTiposComprobantes tipoComprobante = DBTiposComprobantes.GetByID(Convert.ToInt64(parametros[0]));
                if (tipoComprobante is null)
                {
                    _outputStr = "ID del tipo de comprobante seleccionado inválido, vea los tipos de comprobantes válidos:\n";
                    _outputStr += DBTiposComprobantes.PrintAll();
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
                    case 6:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3], SafeConvert.ToDouble(parametros[4]), SafeConvert.ToDouble(parametros[5]));
                        break;
                    case 7:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3], SafeConvert.ToDouble(parametros[4]), SafeConvert.ToDouble(parametros[5]));

                        break;
                    case 8:
                        _seleccion = new DBComprobantes((DBEntidades)_seleccion, tipoComprobante, SafeConvert.ToBoolean(parametros[1]), fechaFinal, parametros[3], SafeConvert.ToDouble(parametros[4]), SafeConvert.ToDouble(parametros[5]));
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

            DBComprobantes comprobanteSeleccionado = entidadComercialSeleccionada.GetComprobanteByID(Convert.ToInt64(id.Trim()));

            if (comprobanteSeleccionado is null)
            {
                _outputStr = "No existe una comprobante en esta entidad comercial con el ID introducido.";
                return;
            }
            _seleccion = comprobanteSeleccionado;
            _outputStr = $"Comprobante seleccionado> {_seleccion}";

        }

        private void _CMD_GetPagos(string filter)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado. Seleccione un comprobante primero.";
                return;
            }
            MySqlConnection conn = DBConnection.Instance().Connection;
            DBComprobantes comprobanteSeleccionado = (DBComprobantes)_seleccion;
            comprobanteSeleccionado.GetAllPagos(conn);
            _outputStr = "\t:: Pagos ::\n";
            _outputStr += comprobanteSeleccionado.PrintAllPagos();
        }

        private void _CMD_PrintPagos()
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado. Seleccione un comprobante primero.";
                return;
            }
            DBComprobantes comprobanteSeleccionado = (DBComprobantes)_seleccion;
            _outputStr = "\t:: Pagos ::\n";
            _outputStr += comprobanteSeleccionado.PrintAllPagos();
        }

        private void _CMD_SelectPago(string id)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado. Seleccione un comprobante primero.";
                return;
            }
            DBComprobantes comprobanteSeleccionado = (DBComprobantes)_seleccion;

            DBPago pagoSeleccionado = comprobanteSeleccionado.GetPagoByID(Convert.ToInt64(id.Trim()));

            if (pagoSeleccionado is null)
            {
                _outputStr = "No existe un pago en este comprobante con el ID introducido.";
                return;
            }
            _seleccion = pagoSeleccionado;
            _outputStr = $"Pago seleccionado> {_seleccion}";

        }


        private void _CMD_CrearPago(string args)
        {
            if (_seleccion is null || !(_seleccion is DBComprobantes))
            {
                _outputStr = "No hay un comprobante seleccionado. Seleccione un comprobante primero.";
                return;
            }

            if (args.Trim().ToLower().Equals("random"))
            {
                _seleccion = DBPago.GenerateRandom((DBComprobantes)_seleccion);
            }
            else
            {
                string[] parametros = args.Split(',');
                if (parametros.Length < 3 || parametros.Length > 4)
                {
                    _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear entidad ID Forma de Pago, Importe, Observación, Fecha=''";
                    return;
                }
                DBFormasPago formasPago = DBFormasPago.GetByID(Convert.ToInt64(parametros[0]));
                if (formasPago is null)
                {
                    _outputStr = "ID de la forma de pago no es válido, vea las formas de pago válidas:\n";
                    _outputStr += DBFormasPago.PrintAll();
                    return;
                }
                switch (parametros.Length)
                {
                    case 3:
                        _seleccion = new DBPago((DBComprobantes)_seleccion, formasPago, SafeConvert.ToDouble(parametros[1]), parametros[2]);
                        break;
                    case 4:
                        DateTime fechaPago = new DateTime();
                        DateTime? fechaFinal = null;
                        if (DateTime.TryParse(parametros[3], out fechaPago))
                        {
                            fechaFinal = fechaPago;
                        }
                        _seleccion = new DBPago((DBComprobantes)_seleccion, formasPago, SafeConvert.ToDouble(parametros[1]), parametros[2], fechaFinal);
                        break;
                }
            }
            _outputStr = $"Pago creado> {_seleccion}";
        }


        private void _CMD_ResetDatabase()
        {
            if (_inputRegister.Count < 2 || !_inputRegister[_inputRegister.Count-2].ToLower().Contains("reset database"))
            {
                _outputStr = "¿Estás realmente seguro de que queres resetear el contenido de la base de datos?, Esto va a eliminar TODOS los datos. Si estás seguro, escribí de nuevo:\nreset database";
                return;
            }

            MySqlConnection conn = DBConnection.Instance().Connection;
            try
            {
                //Deleting recibos
                string query = "DELETE FROM recibos";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = "ALTER TABLE recibos AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting comprobantes_pagos
                query = $"DELETE FROM {DBPago.db_table}";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE {DBPago.db_table} AUTO_INCREMENT = 1";
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
                query = $"DELETE FROM tipos_recibos";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE tipos_recibos AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting tipos_remitos
                query = $"DELETE FROM tipos_remitos";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE tipos_remitos AUTO_INCREMENT = 1";
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

        private void _CMD_GenerateBasicData()
        {
            if (_inputRegister.Count < 2 || !_inputRegister[_inputRegister.Count - 2].ToLower().Contains("generate basic data"))
            {
                _outputStr = "¿Estás realmente seguro de que queres volver a generar los datos básicos en la base de datos?, Esto puedo corromper la base de datos. Si estás seguro, escribí de nuevo:\ngenerate basic data";
                return;
            }

            MySqlConnection conn = DBConnection.Instance().Connection;
            try
            {
                //Deleting tipos_recibos
                string query = $"DELETE FROM tipos_recibos";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE tipos_recibos AUTO_INCREMENT = 1";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Deleting tipos_remitos
                query = $"DELETE FROM tipos_remitos";
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                query = $"ALTER TABLE tipos_remitos AUTO_INCREMENT = 1";
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

                //Now populate.
                List<DBTiposComprobantes> tiposComprobantes = new List<DBTiposComprobantes>();
                tiposComprobantes.Add(new DBTiposComprobantes("Factura A"));
                tiposComprobantes.Add(new DBTiposComprobantes("Factura B"));
                tiposComprobantes.Add(new DBTiposComprobantes("Factura C"));
                tiposComprobantes.Add(new DBTiposComprobantes("Ticket"));

                foreach (DBTiposComprobantes tipoComprobante in tiposComprobantes)
                {
                    tipoComprobante.PushToDatabase(conn);
                }

                List<DBFormasPago> formasDePago = new List<DBFormasPago>();
                formasDePago.Add(new DBFormasPago("Cheque"));
                formasDePago.Add(new DBFormasPago("Efectivo"));
                formasDePago.Add(new DBFormasPago("Transferencia Bancaria"));
                formasDePago.Add(new DBFormasPago("Nota de crédito"));

                foreach (DBFormasPago formaPago in formasDePago)
                {
                    formaPago.PushToDatabase(conn);
                }

                List<DBTipoEntidad> tiposEntidad = new List<DBTipoEntidad>();
                tiposEntidad.Add(new DBTipoEntidad("Cliente"));
                tiposEntidad.Add(new DBTipoEntidad("Proovedor"));

                foreach (DBTipoEntidad tipoEntidad in tiposEntidad)
                {
                    tipoEntidad.PushToDatabase(conn);
                }
                _outputStr = "Tipos de datos basicos generados nuevamente.";
            }
            catch (Exception ex)
            {
                _outputStr += $"SQL error: {ex.ToString()}";
            }
        }
    }
}
