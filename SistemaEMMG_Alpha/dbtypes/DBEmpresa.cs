using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Windows;

namespace SistemaEMMG_Alpha
{
    public struct EmpresasData
    {
        public EmpresasData(long id, long cuit, string rs)
        {
            em_id = id;
            em_cuit = cuit;
            em_rs = rs; //Event handler to check that rs is not longer than 64 characters
        }
        public long em_id { get; } //read only, we should never be able to change an id, Database handles that!
        public long em_cuit { get; set; }
        public string em_rs { get; set; }

        public override string ToString()
        {
            return $"ID: {em_id} - Nombre Empresa: {em_rs} - CUIT: {em_cuit}";
        }
    }
    public class DBEmpresa : DBInterface
    {
        /*************************
         * Global Static STUFFF *
         ************************/
        private static readonly List<DBEmpresa> _db_empresas = new List<DBEmpresa>();

           public static List<DBEmpresa> UpdateAll(MySqlConnection conn)
        {
            List<DBEmpresa> returnList = new List<DBEmpresa>();
            try
            {
                string query = $"SELECT * FROM empresas";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                _db_empresas.Clear();

                while (reader.Read())
                {
                    returnList.Add(new DBEmpresa(reader.GetInt64Safe("em_id"), reader.GetInt64Safe("em_cuit"), reader.GetStringSafe("em_rs")));
                    _db_empresas.Add(new DBEmpresa(reader.GetInt64Safe("em_id"), reader.GetInt64Safe("em_cuit"), reader.GetStringSafe("em_rs")));
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas las empresas, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return returnList;
        }

        public static List<DBEmpresa> GetAll()
        {
            List<DBEmpresa> returnList = new List<DBEmpresa>();
            foreach (DBEmpresa empresa in _db_empresas)
            {
                returnList.Add(new DBEmpresa(empresa.GetID(), empresa.GetCUIT(), empresa.GetRazonSocial()));
            }
            return returnList;
        }

        public static bool EmpresaYaExiste(string str, long cuit, List<DBEmpresa> empresas)
        {
            foreach (DBEmpresa empresa in empresas)
            {
                if (str.ToLower().Equals(empresa.GetRazonSocial().ToLower()) || empresa.GetCUIT() == cuit)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool EmpresaYaExiste(string str, long cuit) => EmpresaYaExiste(str, cuit, _db_empresas);

        /***************
         * Local STUFF *
         ***************/

        private EmpresasData _data;
        private readonly List<DBEntidades> _db_entidades_comerciales = new List<DBEntidades>();

        public DBEmpresa(EmpresasData newData)
        {
            _data = newData;
        }

        public DBEmpresa(long id, long cuit, string rs)
        {
            _data = new EmpresasData(id, cuit, rs);
        }
        public DBEmpresa(long cuit, string rs) : this(-1, cuit, rs) { }

        public DBEmpresa(MySqlConnection conn, int id)
        {
            try
            {
                string query = $"SELECT * FROM empresas WHERE em_id = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
               
                while (reader.Read())
                {
                    _data = new EmpresasData(reader.GetInt64Safe("em_id"), reader.GetInt64Safe("em_cuit"), reader.GetStringSafe("em_rs"));
                }

                reader.Close();
            } catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBEmpresa, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool PushToDatabase(MySqlConnection conn)
        {
            bool wasAbleToPush = false;
            try
            {
                //First check if the record already exists in the DB
                string query = $"SELECT COUNT(*) FROM empresas WHERE em_id = {_data.em_id}";
                var cmd = new MySqlCommand(query, conn);
                int recordsCount = int.Parse(cmd.ExecuteScalar().ToString());

                //if exists already, just update
                if (recordsCount > 0)
                {
                    query = $"UPDATE empresas SET em_cuit = {_data.em_cuit}, em_rs = '{_data.em_rs}' WHERE em_id = {_data.em_id}";

                 } else //if does not exists, insert into
                {
                    query = $"INSERT INTO empresas (em_cuit, em_rs) VALUES ({_data.em_cuit}, '{_data.em_rs}')";
                }
                //if not, add
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                if (recordsCount <= 0) //Recently inserted into the DB, so we need to update the ID generated by the DataBase
                {
                    _data = new EmpresasData(cmd.LastInsertedId, _data.em_cuit, _data.em_rs);
                }
                wasAbleToPush = true;
            } catch (Exception ex)
            {
                wasAbleToPush = false;
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBEmpresa: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return wasAbleToPush;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            bool deletedCorrectly = false;
            try
            {
                string query = $"DELETE FROM empresas WHERE em_id = {_data.em_id}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                deletedCorrectly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBEmpresa: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return deletedCorrectly;
        }

        public List<DBEntidades> GetAllEntidadesComerciales(MySqlConnection conn) //Get directly from database
        {
            List<DBEntidades> returnList = DBEntidades.GetAll(conn, this);
            _db_entidades_comerciales.Clear();
            foreach (DBEntidades entidadComercial in returnList)
            {
                _db_entidades_comerciales.Add(new DBEntidades(this, entidadComercial.GetTipoEntidad(), entidadComercial.Data));
            }
            return returnList;
        }
        public List<DBEntidades> GetAllEntidadesComerciales() //Get CACHE
        {
            List<DBEntidades> returnList = new List<DBEntidades>();

            foreach (DBEntidades entidadComercial in _db_entidades_comerciales)
            {
                returnList.Add(new DBEntidades(this, entidadComercial.GetTipoEntidad(), entidadComercial.Data));
            }

            return returnList;
        }

        public DBEntidades GetEntidadByID(int ec_id)
        {
            return DBEntidades.GetByID(_db_entidades_comerciales, this, ec_id);
        }

        public bool AddNewEntidad(DBEntidades newEntidadComercial)
        {
            if (DBEntidades.CheckIfExistsInList(_db_entidades_comerciales, newEntidadComercial, true))
            {
                return false; //already exists, at least cuit and name-
            } 
            _db_entidades_comerciales.Add(newEntidadComercial);
            return true;
        }
        public void RemoveEntidad(DBEntidades entRemove)
        {
            _db_entidades_comerciales.Remove(entRemove);
        }

        public EmpresasData Data
        {
            get => _data;
            set
            {
                _data = value;
            }
        }

        public void SetRazonSocial(string name)
        {
            _data.em_rs = name;
        }
        public void SetCUIT(long cuit)
        {
            _data.em_cuit = cuit;
        }

        public long GetID()
        {
            return _data.em_id;
        }
        public long GetCUIT()
        {
            return _data.em_cuit;
        }
        public string GetRazonSocial()
        {
            return _data.em_rs;
        }

        public override string ToString()
        {
            return _data.ToString();
        }
    }
}
