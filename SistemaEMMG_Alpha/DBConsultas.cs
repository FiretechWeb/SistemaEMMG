using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.Windows;

// CustomerName LIKE '%or%'

namespace SistemaEMMG_Alpha
{
    public static class DBConsultas
    {
        public static List<DBEntidades> DBEntidadesWith(MySqlConnection conn, DBEmpresa cuenta, string toFind)
        {
            List<DBEntidades> returnList = new List<DBEntidades>();
            try
            {
                string te_table = DBTipoEntidad.GetDBTableName();
                string query = $"SELECT * FROM {DBEntidades.GetDBTableName()} JOIN {te_table} ON {te_table}.te_id = {DBEntidades.GetDBTableName()}.ec_te_id WHERE ec_em_id = {cuenta.GetID()} AND (LOWER(ec_rs) LIKE '%{toFind.ToLower()}%' OR ec_cuit LIKE '%{toFind.ToLower()}%')";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    returnList.Add(new DBEntidades(cuenta, new DBTipoEntidad(reader.GetInt64Safe("te_id"), reader.GetStringSafe("te_nombre")), reader.GetInt64Safe("ec_id"), reader.GetInt64Safe("ec_cuit"), reader.GetStringSafe("ec_rs"), reader.GetStringSafe("ec_email"), reader.GetStringSafe("ec_telefono"), reader.GetStringSafe("ec_celular"))); //Waste of persformance but helps with making the code less propense to error.
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
