using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    public class BLLNotificacion
    {
        private const int TopeBandeja = 20;

        MPPNotificacion oMPPNot;

        public BLLNotificacion()
        {
            oMPPNot = new MPPNotificacion();
        }

        public BEBandejaNotificaciones ObtenerBandeja(BESesion oSesionBE)
        {
            BEBandejaNotificaciones respuesta = new BEBandejaNotificaciones();

            respuesta.Notificaciones = oMPPNot.ListarPorUsuario(oSesionBE.UsuarioId, TopeBandeja);
            respuesta.NoLeidas = oMPPNot.ContarNoLeidas(oSesionBE.UsuarioId);

            return respuesta;
        }

        public BEBandejaNotificaciones MarcarLeidas(int? notificacionId, BESesion oSesionBE)
        {
            oMPPNot.MarcarLeidas(oSesionBE.UsuarioId, notificacionId);

            return ObtenerBandeja(oSesionBE);
        }
    }
}
