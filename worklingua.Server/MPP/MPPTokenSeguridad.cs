using System.Collections;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPTokenSeguridad
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPTokenSeguridad()
        {
            oDatos = new Acceso();
        }

        public BETokenSeguridad Guardar(BETokenSeguridad Objeto)
        {
            if (Objeto.TokenId != 0)
            {
                throw new NotImplementedException("No existe sp_TokenSeguridad_Modificar: un token se genera, se usa o se invalida.");
            }

            int MinutosVigencia = (int)(Objeto.FechaExpiracion - Objeto.FechaGeneracion).TotalMinutes;

            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Tipo", Objeto.Tipo);
            Hdatos.Add("@MinutosVigencia", MinutosVigencia);

            DataTable Dt = oDatos.Leer("sp_TokenSeguridad_Alta", Hdatos);

            if (Dt.Rows.Count == 0)
            {
                throw new InvalidOperationException("sp_TokenSeguridad_Alta no devolvió el token creado.");
            }

            return Mapear(Dt.Rows[0]);
        }

        public BETokenSeguridad ListarObjeto(BETokenSeguridad Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            DataTable Dt = oDatos.Leer("sp_TokenSeguridad_ObtenerPorToken", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                return Mapear(Dt.Rows[0]);
            }
            else
            {
                return null;
            }
        }

        public bool MarcarUsado(BETokenSeguridad Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@Token", Objeto.Token);

            object filas = oDatos.LeerEscalar("sp_TokenSeguridad_MarcarUsado", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas) > 0;
        }

        public int InvalidarPorUsuarioYTipo(BETokenSeguridad Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@Tipo", Objeto.Tipo);

            object filas = oDatos.LeerEscalar("sp_TokenSeguridad_InvalidarPorUsuarioYTipo", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }

        private BETokenSeguridad Mapear(DataRow Item)
        {
            BETokenSeguridad oTokenBE = new BETokenSeguridad();

            oTokenBE.TokenId = ServicioLectorFila.LeerEntero(Item, "TokenId");
            oTokenBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
            oTokenBE.Token = ServicioLectorFila.LeerGuid(Item, "Token");
            oTokenBE.Tipo = ServicioLectorFila.LeerTexto(Item, "Tipo");
            oTokenBE.FechaGeneracion = ServicioLectorFila.LeerFechaHora(Item, "FechaGeneracion");
            oTokenBE.FechaExpiracion = ServicioLectorFila.LeerFechaHora(Item, "FechaExpiracion");
            oTokenBE.FechaUso = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaUso");
            oTokenBE.Activo = ServicioLectorFila.LeerBooleano(Item, "Activo");

            return oTokenBE;
        }
    }
}
