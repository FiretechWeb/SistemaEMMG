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
    public class DBMain
    {
        private DBMain()
        {

        }

        public static DBMain Instance()
        {
            if (_instance == null)
            {
                _instance = new DBMain();
            }
            return _instance;
        }

        private static DBMain _instance = null;

        public void RefreshBasicDataDB(MySqlConnection conn)
        {
            DBCuenta.UpdateAll(conn);
            DBTiposComprobantes.UpdateAll(conn);
            DBTipoEntidad.UpdateAll(conn);
            DBFormasPago.UpdateAll(conn);
            DBTipoRecibo.UpdateAll(conn);
            DBTipoRemito.UpdateAll(conn);
        }
    }
}
