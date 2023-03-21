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

        private List<DBCuenta> _cuentas = new List<DBCuenta>();
        private List<DBTipoEntidad> _tipos_entidades = new List<DBTipoEntidad>();
        private List<DBTiposComprobantes> _tipos_comprobantes = new List<DBTiposComprobantes>();
        private List<DBFormasPago> _formas_pago = new List<DBFormasPago>();

        public IReadOnlyCollection<DBCuenta> GetCuentas() => DBCuenta.GetAll().AsReadOnly();
        public IReadOnlyCollection<DBTiposComprobantes> GetTiposDeComprobantes() => _tipos_comprobantes.AsReadOnly();
        public IReadOnlyCollection<DBTipoEntidad> GetTiposDeEntidades() => _tipos_entidades.AsReadOnly();
        public IReadOnlyCollection<DBFormasPago> GetFormasDePago() => _formas_pago.AsReadOnly();
        public IReadOnlyCollection<DBCuenta> GetLocalCuentas() => _cuentas.Where(x => x.GetID() < 0).ToList().AsReadOnly();
        public IReadOnlyCollection<DBTiposComprobantes> GetLocalTiposDeComprobantes() => _tipos_comprobantes.Where(x => x.GetID() < 0).ToList().AsReadOnly();
        public IReadOnlyCollection<DBTipoEntidad> GetLocalTiposDeEntidades() => _tipos_entidades.Where(x => x.GetID() < 0).ToList().AsReadOnly();
        public IReadOnlyCollection<DBFormasPago> GetLocalFormasDePago() => _formas_pago.Where(x => x.GetID() < 0).ToList().AsReadOnly();

        public void RefreshBasicDataDB(MySqlConnection conn)
        {
            DBCuenta.UpdateAll(conn);
            DBTiposComprobantes.UpdateAll(conn);
            DBTipoEntidad.UpdateAll(conn);
            DBFormasPago.UpdateAll(conn);
        }

        //All Everything from below here it's not ideal. Better not to implement the global push/pull and keep pushing and pulling things individually from element by element, to better control.
        //TODO: Remove PushChangesToDB, PullFromDB, PushChangesToDB, RegisterEntity, UnregisterEntity and _datos list, keep the software simple, this is not a game engine!
    }
}
