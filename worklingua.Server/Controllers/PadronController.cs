using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/padron")]
    public class PadronController : ControladorBase
    {
        BLLPadron oBLLPad;

        public PadronController()
        {
            oBLLPad = new BLLPadron();
        }

        /// <summary>
        /// Procesa el padrón (CU-001-002). Con `confirmar` en false solo valida
        /// y devuelve el detalle; en true manda las invitaciones.
        ///
        /// Responde 200 incluso con filas rechazadas: el resultado lleva el
        /// detalle fila por fila. Los 4xx quedan para los fallos globales
        /// (archivo vacío o pasado de tamaño, empresa inválida, cupo insuficiente).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BEResultadoPadron), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEResultadoPadron> Procesar([FromBody] BEPadron peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.UsuarioInvitar);

            return Ok(oBLLPad.Procesar(peticion, oSesionBE));
        }
    }
}
