using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaEMMG_Alpha
{
    public struct BancosData
    {
        public BancosData(long id, string nom)
        {
            bc_id = id;
            bc_nombre = nom;
        }
        public long bc_id { get; set; }
        public string bc_nombre { get; set; }
    }
    public class DBBancos
    {
    }
}
