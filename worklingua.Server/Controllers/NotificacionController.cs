using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/notificaciones")]
    public class NotificacionController : ControladorBase
    {
        BLLNotificacion oBLLNot;
        BLLSesion oBLLSes;

        public NotificacionController()
        {
            oBLLNot = new BLLNotificacion();
            oBLLSes = new BLLSesion();
        }

        [HttpGet]
        [ProducesResponseType(typeof(BEBandejaNotificaciones), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<BEBandejaNotificaciones> Obtener()
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());

            return Ok(oBLLNot.ObtenerBandeja(oSesionBE));
        }

        [HttpPost("leidas")]
        [ProducesResponseType(typeof(BEBandejaNotificaciones), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<BEBandejaNotificaciones> MarcarTodas()
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());

            return Ok(oBLLNot.MarcarLeidas(null, oSesionBE));
        }

        [HttpPost("{notificacionId:int}/leida")]
        [ProducesResponseType(typeof(BEBandejaNotificaciones), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<BEBandejaNotificaciones> MarcarLeida(int notificacionId)
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());

            return Ok(oBLLNot.MarcarLeidas(notificacionId, oSesionBE));
        }
    }
}
