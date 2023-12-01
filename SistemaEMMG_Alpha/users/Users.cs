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
        public UserData(string _name, string _pass, bool isAdmin)
        {
            user_name = _name;
            user_pass = _pass;
            user_admin = isAdmin;
        }
        public string user_name;
        public string user_pass;
        public bool user_admin;
        public static readonly string NameOf_user_name = nameof(user_name);
        public static readonly string NameOf_user_pass = nameof(user_pass);
        public static readonly string NameOf_user_admin = nameof(user_admin);

        public static UserData CreateFromReader(MySqlDataReader reader)
        {
            return new UserData(
                reader.GetStringSafe(NameOf_user_name),
                EncryptManager.DecryptString(reader.GetStringSafe(NameOf_user_pass), Constants.userEncryptionKey),
                Convert.ToBoolean(reader.GetInt32Safe(NameOf_user_admin))
           );
        }
    }
    public class User
    {
        private static string db_table = "usuarios";
        private long user_id;
        private UserData _data;
        public static readonly string NameOf_id = nameof(user_id);
        private static User _currentUser = null;

        public static User GetCurrentUser() => _currentUser;
        public static void SetCurrentUser(User newUser) => _currentUser = newUser;

        public User(UserData userData)
        {
            _data = userData;
        }

        public User(string userName, string userPass, bool userIsAdmin) : this(new UserData(userName, userPass, userIsAdmin)) { }

        public User(string userName, string userPass) : this (userName, userPass, false) { }

        public User(string userName, MySqlConnection conn)
        {
            _data = new UserData(userName, "", false);
            PullFromDatabase(conn);
        }

        public string GetEncryptedPassword() => EncryptManager.EncryptString(_data.user_pass, Constants.userEncryptionKey);
        public string GetPassword() => _data.user_pass;

        public string GetUserName() => _data.user_name;

        public bool IsAdmin() => _data.user_admin;

        public bool PullFromDatabase(MySqlConnection conn)
        {
            bool wasAbleToPull = false;
            try
            {
                string query = $"SELECT * FROM {db_table} WHERE UPPER({UserData.NameOf_user_name}) = '{_data.user_name.ToUpper()}'";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    wasAbleToPull = true;
                    _data = UserData.CreateFromReader(reader);
                }
                reader.Close();
            } catch (Exception ex)
            {
                wasAbleToPull = false;
                MessageBox.Show("Error en Users::PullFromDatabase " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPull;
        }

        public bool? ExistsInDatabase(MySqlConnection conn)
        {
            bool? exitsInDB;

            try
            {
                string query = $"SELECT COUNT(*) FROM {db_table} WHERE UPPER({UserData.NameOf_user_name}) = '{_data.user_name.ToUpper()}'";
                var cmd = new MySqlCommand(query, conn);
                exitsInDB = int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            } catch(Exception ex)
            {
                MessageBox.Show("Error en el método User::ExistsInDatabase: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
                exitsInDB = null;
            }

            return exitsInDB;
        }

        public bool CheckIfValid(MySqlConnection conn) //Login check function
        {
            if (ExistsInDatabase(conn) != true)
            {
                return false;
            }
            User userFromDB = new User(GetUserName(), conn);
            return userFromDB.GetPassword().Equals(GetPassword());
        }

        public bool UpdateToDatabase(MySqlConnection conn)
        {
            bool wasAbleToUpdate;

            try
            {
                string query = $"UPDATE {db_table} SET {UserData.NameOf_user_name} = '{_data.user_name}', {UserData.NameOf_user_pass} = '{GetEncryptedPassword()}', {UserData.NameOf_user_admin} = {Convert.ToInt32(_data.user_admin)} WHERE {NameOf_id} = {user_id}";
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
                string query = $"INSERT INTO {db_table} ({UserData.NameOf_user_name}, {UserData.NameOf_user_pass}, {UserData.NameOf_user_admin}) VALUES ('{_data.user_name}', '{GetEncryptedPassword()}', {Convert.ToInt32(_data.user_admin)})";
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
