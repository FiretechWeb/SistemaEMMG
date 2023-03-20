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
        //Object = null destroys the object in case there are no more references to it..
        ~DBMain() { } //Destructor, muy importante

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
        private List<DBBaseClass> _datos = new List<DBBaseClass>(); //lista de todos los datos cargados en el programa (no es lo mismo que todos los datos en la BD)

        public IReadOnlyCollection<DBCuenta> GetCuentas() => _cuentas.AsReadOnly();
        public IReadOnlyCollection<DBTiposComprobantes> GetTiposDeComprobantes() => _tipos_comprobantes.AsReadOnly();
        public IReadOnlyCollection<DBTipoEntidad> GetTiposDeEntidades() => _tipos_entidades.AsReadOnly();
        public IReadOnlyCollection<DBFormasPago> GetFormasDePago() => _formas_pago.AsReadOnly();
        public IReadOnlyCollection<DBCuenta> GetLocalCuentas() => _cuentas.Where(x => x.GetID() < 0).ToList().AsReadOnly();
        public IReadOnlyCollection<DBTiposComprobantes> GetLocalTiposDeComprobantes() => _tipos_comprobantes.Where(x => x.GetID() < 0).ToList().AsReadOnly();
        public IReadOnlyCollection<DBTipoEntidad> GetLocalTiposDeEntidades() => _tipos_entidades.Where(x => x.GetID() < 0).ToList().AsReadOnly();
        public IReadOnlyCollection<DBFormasPago> GetLocalFormasDePago() => _formas_pago.Where(x => x.GetID() < 0).ToList().AsReadOnly();

        public void RefreshBasicDataDB(MySqlConnection conn)
        {
            RefreshCuentasDB(conn);
            RefreshTiposDeComprobantesDB(conn);
            RefreshTipoDeEntidadesDB(conn);
            RefreshFormasDePagoDB(conn);
        }
        public void RefreshCuentasDB(MySqlConnection conn)
        {
            _cuentas.Clear();
            _cuentas = DBCuenta.UpdateAll(conn);
        }
        public void RefreshTiposDeComprobantesDB(MySqlConnection conn)
        {
            _tipos_comprobantes.Clear();
            _tipos_comprobantes = DBTiposComprobantes.UpdateAll(conn);
        }
        public void RefreshTipoDeEntidadesDB(MySqlConnection conn)
        {
            _tipos_entidades.Clear();
            _tipos_entidades = DBTipoEntidad.UpdateAll(conn);
        }
        public void RefreshFormasDePagoDB(MySqlConnection conn)
        {
            _formas_pago.Clear();
            _formas_pago = DBFormasPago.UpdateAll(conn);
        }

        public void PushChangesToDB(MySqlConnection conn, List<DBBaseClass> listToPush)
        {
            foreach (DBBaseClass dato in listToPush)
            {
                dato.PushToDatabase(conn); //it does not push to DB if nothing changed...
            }
        }
        public void PullFromDB(MySqlConnection conn, List<DBBaseClass> listToPull)
        {
            foreach (DBBaseClass dato in listToPull)
            {
                dato.PullFromDatabase(conn);
            }
        }

        public void PushChangesToDB(MySqlConnection conn) => PushChangesToDB(conn, _datos);

        public void PullFromDB(MySqlConnection conn) => PullFromDB(conn, _datos);

        public bool RegisterEntity(DBBaseClass dato)
        {
            if (_datos.Contains(dato))
            {
                return false; //already exists, no need to add again.
            }
            _datos.Add(dato);
            return true;
        }

        public bool UnregisterEntity(DBBaseClass dato)
        {
            return _datos.Remove(dato);
        }
    }
}
