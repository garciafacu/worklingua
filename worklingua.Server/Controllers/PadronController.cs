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
