using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPNotificacion
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPNotificacion()
        {
            oDatos = new Acceso();
        }

        public List<BENotificacion> ListarPorUsuario(int usuarioId, int tope)
        {
            List<BENotificacion> ListaBE = new List<BENotificacion>();

            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", usuarioId);
            Hdatos.Add("@Tope", tope);

            DataTable Dt = oDatos.Leer("sp_Notificacion_ListarPorUsuario", Hdatos);

            foreach (DataRow Item in Dt.Rows)
            {
                BENotificacion oNotificacionBE = new BENotificacion();

                oNotificacionBE.NotificacionId = ServicioLectorFila.LeerEntero(Item, "NotificacionId");
                oNotificacionBE.UsuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");
                oNotificacionBE.Titulo = ServicioLectorFila.LeerTexto(Item, "Titulo");
                oNotificacionBE.Mensaje = ServicioLectorFila.LeerTexto(Item, "Mensaje");
                oNotificacionBE.Leida = ServicioLectorFila.LeerBooleanoNulo(Item, "Leida");
                oNotificacionBE.FechaEnvio = ServicioLectorFila.LeerFechaHoraNula(Item, "FechaEnvio");

                ListaBE.Add(oNotificacionBE);
            }

            return ListaBE;
        }

        public int ContarNoLeidas(int usuarioId)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", usuarioId);

            object cantidad = oDatos.LeerEscalar("sp_Notificacion_ContarNoLeidas", Hdatos);

            return ServicioLectorFila.ATotalFilas(cantidad);
        }

        /// <summary>
        /// Marca como leídas las del usuario. Con notificacionId en null las
        /// marca todas; el filtro por usuario va siempre, así que nadie puede
        /// marcar una notificación ajena.
        /// </summary>
        public int MarcarLeidas(int usuarioId, int? notificacionId)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", usuarioId);

            if (notificacionId.HasValue)
            {
                Hdatos.Add("@NotificacionId", notificacionId.Value);
            }

            object filas = oDatos.LeerEscalar("sp_Notificacion_MarcarLeidas", Hdatos);

            return ServicioLectorFila.ATotalFilas(filas);
        }
    }
}
