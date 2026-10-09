using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/reportes")]
    public class ReporteController : ControladorBase
    {
        BLLReporte oBLLRep;

        public ReporteController()
        {
            oBLLRep = new BLLReporte();
        }

        [HttpGet("ganancias")]
        [ProducesResponseType(typeof(List<BEGananciaPeriodo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEGananciaPeriodo>> ListarGanancias([FromQuery] BEFiltroReporte oFiltroBE)
        {
            ExigirPermiso(Permisos.ReporteVer);

            return Ok(oBLLRep.ListarGanancias(oFiltroBE));
        }

        [HttpGet("ganancias/zonas")]
        [ProducesResponseType(typeof(List<BEGananciaZona>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEGananciaZona>> ListarGananciasPorZona([FromQuery] BEFiltroReporte oFiltroBE)
        {
            ExigirPermiso(Permisos.ReporteVer);

            return Ok(oBLLRep.ListarGananciasPorZona(oFiltroBE));
        }

        [HttpGet("tablero")]
        [ProducesResponseType(typeof(BETablero), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BETablero> ObtenerTablero()
        {
            ExigirPermiso(Permisos.ReporteVer);

            return Ok(oBLLRep.ObtenerTablero());
        }
    }
}
