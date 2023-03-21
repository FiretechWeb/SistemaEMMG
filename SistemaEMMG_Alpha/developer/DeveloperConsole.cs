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
        private DeveloperConsole()
        {
            commandsList.Add(new KeyValuePair<string, Action<string>>("SQL>", (x) => ProcessSQLCommand(x)));
            commandsList.Add(new KeyValuePair<string, Action<string>>("sql>", (x) => ProcessSQLCommand(x))); //alias
            commandsList.Add(new KeyValuePair<string, Action<string>>("$>", (x) => ProcessInternalCommand(x)));
        }
        ///<summary>
        ///Process an input string and retrieves the result.
        ///</summary
        public string ProcessInput(string input)
        {
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
                    break;
                }
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
                            _outputStr += $"\tColumns> ";
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
                        _outputStr += $"\tRow {row}> ";
                        for (int column = 0; column < reader.FieldCount; column++)
                        {
                            _outputStr += $"{reader.GetStringSafe(column)}";
                            if (column < reader.FieldCount-1)
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
            } catch (Exception ex)
            {
                _outputStr += $"SQL error: {ex.ToString()}";
            }
        }

        public string RetrieveConsoleHistory(string input, bool back=true)
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
            } else
            {
                _currentRegisterSelected += 1;
            }
            _currentRegisterSelected = _currentRegisterSelected.Clamp<int>(0, _inputRegister.Count - 1);

           return _inputRegister[_currentRegisterSelected];
        }
        private void ProcessInternalCommand(string command)
        {
            _outputStr = $"Executed Internal COMMAND: {command}";
        }
    }
}
