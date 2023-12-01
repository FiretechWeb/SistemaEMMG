using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;


namespace SistemaEMMG_Alpha
{
    public struct UserData
    {
        public UserData(string _name, string _pass)
        {
            user_name = _name;
            user_pass = _pass;
        }
        public string user_name;
        public string user_pass;
        public static readonly string NameOf_user_name = nameof(user_name);
        public static readonly string NameOf_user_pass = nameof(user_pass);
    }
    public class User
    {
        private static string db_table = "usuarios";
        private long user_id;
        private UserData _data;
        public static readonly string NameOf_id = nameof(user_id);

        public User(UserData userData)
        {
            _data = userData;
        }

        public User(string userName, string userPass) : this(new UserData(userName, userPass)) { }

        public string GetEncryptedPassword() => EncryptManager.EncryptString(_data.user_pass, Constants.userEncryptionKey);
        public string GetPassword() => _data.user_pass;

        public bool PullFromDatabase(MySqlConnection conn)
        {
            bool wasAbleToPull;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE ";
            } catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en Users::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPull;
        }

        public bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? duplicatedExitsInDB;

            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE UPPER({UserData.NameOf_user_name}) = '{_data.user_name.ToUpper()}' AND {UserData.NameOf_user_pass} = '{GetEncryptedPassword()}'";
                var cmd = new MySqlCommand(query, conn);
                duplicatedExitsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            } catch(Exception ex)
            {
                MessageBox.Show("Error en el método User::DuplicatedExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                duplicatedExitsInDB = null;
            }

            return duplicatedExitsInDB;
        }

        public bool UpdateToDatabase(MySqlConnection conn)
        {
            bool wasAbleToUpdate;

            try
            {
                string query = $"UPDATE {db_table} SET {UserData.NameOf_user_name} = '{_data.user_name}', {UserData.NameOf_user_pass} = '{GetEncryptedPassword()}' WHERE {NameOf_id} = {user_id}";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToUpdate = cmd.ExecuteNonQuery() > 0;
            } catch (Exception ex)
            {
                wasAbleToUpdate = false;
                MessageBox.Show("Error en User::UpdateToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return wasAbleToUpdate;
        }

        public bool InsertIntoToDatabase(MySqlConnection conn)
        {
            bool wasAbleToInsert;
            try
            {
                string query = $"INSERT INTO {db_table} ({UserData.NameOf_user_name}, {UserData.NameOf_user_pass}) VALUES ('{_data.user_name}', '{GetEncryptedPassword()}')";
                var cmd = new MySqlCommand(query, conn);
                wasAbleToInsert = cmd.ExecuteNonQuery() > 0;
                if (wasAbleToInsert)
                {
                    user_id = cmd.LastInsertedId;
                }
            } catch (Exception ex)
            {
                wasAbleToInsert = false;
                MessageBox.Show("Error en User::InsertIntoToDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToInsert;
        }

        public bool PushToDatabase(MySqlConnection conn)
        {
            bool? existsInDB = ExistsInDatabase(conn);
            if (existsInDB is null) //error with DB...
            {
                return false;
            }

            return Convert.ToBoolean(existsInDB) ? UpdateToDatabase(conn) : InsertIntoToDatabase(conn);
        }
    }
}
