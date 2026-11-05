using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/bitacora")]
    public class BitacoraController : ControladorBase
    {
        BLLBitacora oBLLBit = new BLLBitacora();

        [HttpGet]
        [ProducesResponseType(typeof(BEPaginaBitacora), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEPaginaBitacora> Buscar(
            [FromQuery] BEFiltroBitacora oFiltroBE)
        {
            oBLLBit.Validar(oFiltroBE);

            ExigirPermiso(Permisos.BitacoraListar);

            return Ok(oBLLBit.Buscar(oFiltroBE));
        }
    }
}
