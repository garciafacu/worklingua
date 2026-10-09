using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/contrataciones")]
    public class ContratacionController : ControladorBase
    {
        BLLContratacion oBLLCon;

        public ContratacionController()
        {
            oBLLCon = new BLLContratacion();
        }

        [HttpGet("cotizacion")]
        [ProducesResponseType(typeof(BECotizacionContratacion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECotizacionContratacion> Cotizar(
            [FromQuery] int planId, [FromQuery] string codigoCultura)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.SuscripcionContratar);

            BEContratarPlan oContratacionBE = new BEContratarPlan();
            oContratacionBE.PlanId = planId;
            oContratacionBE.CodigoCultura = codigoCultura;

            return Ok(oBLLCon.Cotizar(oContratacionBE, oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEContratacionRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEContratacionRespuesta> Contratar([FromBody] BEContratarPlan peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.SuscripcionContratar);

            return Ok(Mapear(oBLLCon.Contratar(peticion, oSesionBE)));
        }

        [HttpPost("{suscripcionId:int}/cancelar")]
        [ProducesResponseType(typeof(BEContratacionRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEContratacionRespuesta> Cancelar(
            int suscripcionId, [FromBody] BECancelarContratacion peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.SuscripcionContratar);

            peticion.SuscripcionId = suscripcionId;

            return Ok(Mapear(oBLLCon.Cancelar(peticion, oSesionBE)));
        }

        private BEContratacionRespuesta Mapear(BEResultadoContratacion oResultadoBE)
        {
            BEContratacionRespuesta respuesta = new BEContratacionRespuesta();

            respuesta.SuscripcionId = oResultadoBE.Suscripcion.SuscripcionId;
            respuesta.PlanId = oResultadoBE.Suscripcion.PlanId;
            respuesta.Plan = oResultadoBE.Plan;
            respuesta.Estado = oResultadoBE.Suscripcion.Estado;
            respuesta.Motivo = oResultadoBE.Motivo;
            respuesta.FechaInicio = oResultadoBE.Suscripcion.FechaInicio;
            respuesta.FechaFin = oResultadoBE.Suscripcion.FechaFin;
            respuesta.Importe = oResultadoBE.Importe;
            respuesta.Moneda = oResultadoBE.Moneda;
            respuesta.TasaConversion = oResultadoBE.TasaConversion;
            respuesta.Pagos = oResultadoBE.Pagos;
            respuesta.Notas = oResultadoBE.Notas;

            if (oResultadoBE.Factura != null)
            {
                respuesta.NumeroFactura = oResultadoBE.Factura.NumeroFactura;
            }

            return respuesta;
        }
    }
}
