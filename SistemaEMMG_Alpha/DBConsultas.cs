using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Windows;

namespace SistemaEMMG_Alpha
{
    public static class DBConsultas
    {
        public static List<DBEntidades> DBEntidadesWith(MySqlConnection conn, DBCuenta cuenta, string toFind)
        {
            List<DBEntidades> returnList = new List<DBEntidades>();
            try
            {
                string te_table = DBTipoEntidad.db_table;
                string query = $"SELECT * FROM {DBEntidades.db_table} JOIN {te_table} ON {te_table}.te_id = {DBEntidades.db_table}.ec_te_id WHERE ec_em_id = {cuenta.GetID()} AND (LOWER(ec_rs) LIKE '%{toFind.ToLower()}%' OR ec_cuit LIKE '%{toFind.ToLower()}%')";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBEntidades(cuenta, new DBTipoEntidad(reader), reader));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error SQL en  DBConsultas::DBEntidadesWith" + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }
    }
}
