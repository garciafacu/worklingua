using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/rendimiento")]
    public class RendimientoController : ControladorBase
    {
        BLLRendimiento oBLLRen;

        public RendimientoController()
        {
            oBLLRen = new BLLRendimiento();
        }

        [HttpGet]
        [ProducesResponseType(typeof(BERendimiento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BERendimiento> Obtener([FromQuery] BEFiltroRendimiento oFiltroBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgresoEmpresa);

            return Ok(oBLLRen.Obtener(oFiltroBE, oSesionBE));
        }
    }
}
