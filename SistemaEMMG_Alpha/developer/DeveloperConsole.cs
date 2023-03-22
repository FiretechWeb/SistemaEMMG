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
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("crear cuenta", (x) => _CMD_CrearCuenta(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("selected", (x) => _CMD_PrintSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("delete", (x) => _CMD_DeleteSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("push", (x) => _CMD_PushSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("pull", (x) => _CMD_PullSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("duplicate", (x) => _CMD_MakeLocalSelected()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("get entidades", (x) => _CMD_GetEntidades(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("print entidades", (x) => _CMD_PrintEntidades()));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("select entidad", (x) => _CMD_SelectEntidad(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("crear entidad", (x) => _CMD_CrearEntidad(x)));
            internalComandsList.Add(new KeyValuePair<string, Action<string>>("go back", (x) => _CMD_GoBack()));
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
            string[] parametros = args.Split(',');
            if (parametros.Length != 2)
            {
                _outputStr = "La cantidad de parámetros introducida es incorrecta.\nFormato: crear cuenta CUIT, Razón Social";
                return;
            }
            _seleccion = new DBCuenta(Convert.ToInt64(parametros[0]), parametros[1]);
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
                case DBComprobantePago pago:
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
                    _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, Convert.ToInt64(parametros[1]), parametros[2]);
                    break;
                case 4:
                    _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, Convert.ToInt64(parametros[1]), parametros[2], parametros[3]);
                    break;
                case 5:
                    _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, Convert.ToInt64(parametros[1]), parametros[2], parametros[3], parametros[4]);
                    break;
                case 6:
                    _seleccion = new DBEntidades((DBCuenta)_seleccion, tipoEntidad, Convert.ToInt64(parametros[1]), parametros[2], parametros[3], parametros[5]);
                    break;
            }
            _outputStr = $"Entidad creada> {_seleccion}";
        }
    }
}
