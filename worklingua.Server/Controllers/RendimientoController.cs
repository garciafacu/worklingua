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

        /// <summary>
        /// El panel de rendimiento formativo (CU-001-004). Sin
        /// Curso.VerTodasLasEmpresas la BLL ignora empresaId y devuelve el de la
        /// empresa propia.
        ///
        /// Es consulta: no escribe en bitácora, igual que los reportes.
        /// </summary>
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
