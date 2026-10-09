using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPSesion
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPSesion()
        {
            oDatos = new Acceso();
        }

        public BESesion Guardar(BESesion Objeto)
        {
            if (Objeto.SesionId != 0)
            {
                throw new NotImplementedException("No existe sp_Sesion_Modificar: una sesión se abre o se cierra, no se modifica.");
            }

            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            DataTable Dt = oDatos.Leer("sp_Sesion_Alta", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                throw new InvalidOperationException("sp_Sesion_Alta no devolvió la sesión creada.");
            }

            return Mapear(Dt.Rows[0]);
        }

        public BESesion ListarObjeto(BESesion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            DataTable Dt = oDatos.Leer("sp_Sesion_ObtenerPorToken", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public bool Baja(BESesion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            object filas = oDatos.LeerEscalar("sp_Sesion_Cerrar", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int CerrarPorUsuario(BESesion Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@TokenExcluido", Objeto.Token == Guid.Empty ? null : (object)Objeto.Token);

            object filas = oDatos.LeerEscalar("sp_Sesion_CerrarPorUsuario", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        private BESesion Mapear(DataRow Item)
        {
            BESesion oSesionBE = new BESesion();

            oSesionBE.SesionId = ServicioLectorFila.LeerEntero(Item, "SesionId");
            oSesionBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oSesionBE.Token = ServicioLectorFila.LeerGuid(Item, "Token");
            oSesionBE.FechaInicio = ServicioLectorFila.LeerFechaHora(Item, "FechaInicio");
            oSesionBE.FechaFin = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaFin");
            oSesionBE.Activa = ServicioLectorFila.LeerBooleanoNulo(Item, "Activa");

            return oSesionBE;
        }
    }
}
