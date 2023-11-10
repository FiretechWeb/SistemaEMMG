using MySql.Data.MySqlClient;


namespace SistemaEMMG_Alpha
{
    //SINGLETON Class
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
            DBMoneda.UpdateAll(conn);
        }
    }
}
