using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/suscripciones")]
    public class SuscripcionController : ControladorBase
    {
        BLLSuscripcion oBLLSus;
        BLLUsuario oBLLUsu;
        BLLSesion oBLLSes;

        public SuscripcionController()
        {
            oBLLSus = new BLLSuscripcion();
            oBLLUsu = new BLLUsuario();
            oBLLSes = new BLLSesion();
        }

        [HttpGet("mia")]
        [ProducesResponseType(typeof(BESuscripcionRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BESuscripcionRespuesta> ObtenerLaMia()
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());
            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            BESuscripcion oFiltroBE = new BESuscripcion();
            oFiltroBE.EmpresaId = oUsuarioBE.EmpresaId;

            BESuscripcionConPlan oSuscripcion = oBLLSus.ListarObjeto(oFiltroBE);

            if (oSuscripcion == null)
            {
                throw new ExcepcionNegocio(
                    TipoErrorNegocio.NoEncontrado,
                    "Tu empresa todavía no tiene un plan asignado.");
            }

            BESuscripcionRespuesta respuesta = new BESuscripcionRespuesta();

            respuesta.SuscripcionId = oSuscripcion.Suscripcion.SuscripcionId;
            respuesta.PlanId = oSuscripcion.Suscripcion.PlanId;
            respuesta.Plan = oSuscripcion.Plan;
            respuesta.Estado = oSuscripcion.Suscripcion.Estado;
            respuesta.FechaInicio = oSuscripcion.Suscripcion.FechaInicio;
            respuesta.FechaFin = oSuscripcion.Suscripcion.FechaFin;
            respuesta.PrecioMensual = oSuscripcion.PrecioMensual;
            respuesta.CantidadLicencias = oSuscripcion.CantidadLicencias;
            respuesta.Cancelable = oBLLSus.EsCancelable(oSuscripcion);

            return Ok(respuesta);
        }
    }
}
