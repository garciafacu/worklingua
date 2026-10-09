using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/cuenta-corriente")]
    public class CuentaCorrienteController : ControladorBase
    {
        BLLCuentaCorriente oBLLCta;

        public CuentaCorrienteController()
        {
            oBLLCta = new BLLCuentaCorriente();
        }

        [HttpGet]
        [ProducesResponseType(typeof(BEEstadoCuentaCorriente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEEstadoCuentaCorriente> Obtener()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CuentaCorrienteConsultar);

            return Ok(oBLLCta.ListarObjeto(oSesionBE));
        }
    }
}
