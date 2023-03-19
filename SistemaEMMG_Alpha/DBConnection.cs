using System;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace SistemaEMMG_Alpha
{
    public class DBConnection
    {
        private DBConnection()
        {

        }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static DBConnection _instance = null;

        public static DBConnection Instance()
        {
            if (_instance == null)
            {
                _instance = new DBConnection();
            }
            return _instance;
        }

        public bool IsConnected()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(DatabaseName))
                    return false;
                string connstring = $"Server={Server}; database={DatabaseName}; UID={UserName}; password={Password}";
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }
            return true;
        }

        public void Reconnect()
        {
            if (!(Connection is null) && Connection.State.ToString().ToLower() == "open")
            {
                Connection.Close();
            }
            string connstring = $"Server={Server}; database={DatabaseName}; UID={UserName}; password={Password}";
            Connection = new MySqlConnection(connstring);
            Connection.Open();
        }

        public void Close()
        {
            Connection.Close();
        }

        public void Backup(string bakfile_name)
        {
            string constring = $"Server={Server}; database={DatabaseName}; UID={UserName}; password={Password}";
            string file = bakfile_name;
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file);
                        conn.Close();
                    }
                }
            }
        }

        private void Restore(string restore_file) //Stradex: Private by now, this is a bit dangereous and I prefer by now to manually import a backup from phpMyAdmin using uWamp
        {
            string constring = $"Server={Server}; database={DatabaseName}; UID={UserName}; password={Password}";
            string file = restore_file;
            using (MySqlConnection conn = new MySqlConnection(constring))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ImportFromFile(file);
                        conn.Close();
                    }
                }
            }
        }
    }
}
