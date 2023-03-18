using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaEMMG_Alpha
{
    public struct FormasPagoData
    {
        public FormasPagoData(long id, string nom)
        {
            fp_id = id;
            fp_nombre = nom;
        }
        public long fp_id { get; set; }
        public string fp_nombre { get; set; }
    }
    class DBFormasPago
    {
    }
}
