﻿using System;
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
    public class DBEmpresa
    {
        private EmpresasData _data;

        public static List<DBEmpresa> GetEmpresasFromDataBase(MySqlConnection conn)
        {
            List<DBEmpresa> empresas = new List<DBEmpresa>();
            try
            {
                string query = $"SELECT * FROM empresas";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    empresas.Add(new DBEmpresa(reader.GetInt64("em_id"), reader.GetInt64("em_cuit"), reader.GetString("em_rs")));
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error al tratar de obtener todas las empresas, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return empresas;
        }

        public DBEmpresa(EmpresasData newData)
        {
            _data = newData;
        }

        public DBEmpresa(long id, long cuit, string rs)
        {
            _data = new EmpresasData(id, cuit, rs);
        }

        public DBEmpresa(MySqlConnection conn, int id)
        {
            try
            {
                string query = $"SELECT * FROM empresas WHERE em_id = {id}";
                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _data = new EmpresasData(reader.GetInt64("em_id"), reader.GetInt64("em_cuit"), reader.GetString("em_rs"));
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error en el constructor de DBEmpresa, problemas con la consulta SQL: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public bool PushToDatabase(MySqlConnection conn)
        {
            try
            {
                string query = $"UPDATE empresas SET em_cuit = {_data.em_cuit}, em_rs = '{_data.em_rs}' WHERE em_id = {_data.em_cuit}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            } catch (Exception ex)
            {
                MessageBox.Show("Error tratando de actualizar los datos de la base de datos en DBEmpresa: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return false;
        }

        public bool DeleteFromDatabase(MySqlConnection conn)
        {
            try
            {
                string query = $"DELETE FROM empresas WHERE em_id = {_data.em_cuit}";
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error tratando de eliminar una fila de la base de datos en DBEmpresa: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return false;
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
