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
    public struct FormasPagoData
    {
        public FormasPagoData(long id, string nom)
        {
            fp_id = id;
            fp_nombre = nom;
        }
        public long fp_id { get; }
        public string fp_nombre { get; set; }

        public override string ToString()
        {
            return $"ID: {fp_id} - Nombre: {fp_nombre}";
        }
    }
    class DBFormasPago : DBInterface, IDBDataType<DBFormasPago>
    {
        private static readonly string db_table = "formas_pago";
        private FormasPagoData _data;
        private readonly List<DBFormasPago> _db_formas_pago = new List<DBFormasPago>();

        public DBFormasPago(FormasPagoData newData)
        {
            _data = newData;
        }

        public DBFormasPago(long id, string nombre) : this(new FormasPagoData(id, nombre)) { }

        public DBFormasPago(string nombre) : this(-1, nombre);

    }
}
