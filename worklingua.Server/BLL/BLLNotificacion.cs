using System.Collections.Generic;
using worklingua.Server.BE;
using worklingua.Server.MPP;

namespace worklingua.Server.BLL
{
    /// <summary>
    /// La bandeja de notificaciones del usuario (CU-001-010).
    ///
    /// No pide permiso: recibir notificaciones no es una función de
    /// administración, la tiene cualquier sesión. Todo se acota por el
    /// UsuarioId de la sesión, nunca por un id que mande el cliente.
    /// </summary>
    public class BLLNotificacion
    {
        /// <summary>Lo que entra en el panel desplegable de la campana.</summary>
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

        /// <summary>
        /// Marca como leídas. Con notificacionId en null marca todas.
        /// Devuelve la bandeja ya actualizada para que la campana no tenga que
        /// pedirla de nuevo.
        /// </summary>
        public BEBandejaNotificaciones MarcarLeidas(int? notificacionId, BESesion oSesionBE)
        {
            oMPPNot.MarcarLeidas(oSesionBE.UsuarioId, notificacionId);

            return ObtenerBandeja(oSesionBE);
        }
    }
}
