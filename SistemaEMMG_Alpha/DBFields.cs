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
    public struct BancosData
    {
        public BancosData (long id, string nom)
        {
            bc_id = id;
            bc_nombre = nom;
        }
        public long bc_id { get; set; }
        public string bc_nombre { get; set; }
    }

    public class DBFields
    {
        private DBFields()
        {

        }

        public static DBFields Instance()
        {
            if (_instance == null)
            {
                _instance = new DBFields();
            }
            return _instance;
        }
        private static DBFields _instance = null;
        public long idCuentaSeleccionada = 0;
        public List<DBEmpresa> empresas;
        public List<DBTipoEntidad> tipos_entidades;

        public void ReadTiposEntidadesFromDB(MySqlConnection conn)
        {
            if (!(tipos_entidades is null))
            {
                tipos_entidades.Clear();
            }
            tipos_entidades = DBTipoEntidad.UpdateAll(conn);
        }
        public void ReadEmpresasFromDB(MySqlConnection conn)
        {
            if (!(empresas is null))
            {
                empresas.Clear();
            }
            empresas = DBEmpresa.UpdateAll(conn);
        }
        public void ReadEntidadesComercialesFromDB(MySqlConnection conn)
        {
            ReadTiposEntidadesFromDB(conn); //We need to make sure we have the tipos_entidades data first.
            DBEmpresa empresaSeleccionada = GetCurrentAccount();
            empresaSeleccionada.GetAllEntidadesComerciales(conn);
        }

        public DBEmpresa GetCurrentAccount() => empresas[GetCuentaIndexByID(idCuentaSeleccionada)];

        public int GetCuentaIndexByID(long cuentaID)
        {
            for (int i=0; i < empresas.Count; i++)
            {
                if (empresas[i].GetID() == cuentaID)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool EliminarCuentaDeEmpresa(int index, MySqlConnection conn)
        {
            if (index < 0 || index >= empresas.Count)
            {
                return false;
            }
            if (empresas[index].DeleteFromDatabase(conn))
            {
                empresas.RemoveAt(index);
            }

            return true;

        }

        public bool AgregarNuevaCuentaDeEmpresa(string nombreCuenta, long cuitCuenta, MySqlConnection conn)
        {
            if (DBEmpresa.EmpresaYaExiste(nombreCuenta, cuitCuenta, empresas))
            {
                MessageBox.Show("¡La cuenta de empresa que quiso crear ya existe!, el CUIT y la razón social deben ser únicas.");
                return false;
            }
            DBEmpresa newEmpresa = new DBEmpresa(cuitCuenta, nombreCuenta);

            if (!newEmpresa.PushToDatabase(conn))
            {
                return false;
            }
            empresas.Add(newEmpresa);

            return true;
        }
    }
}
