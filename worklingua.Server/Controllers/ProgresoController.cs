using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/progreso")]
    public class ProgresoController : ControladorBase
    {
        BLLProgreso oBLLPro;

        public ProgresoController()
        {
            oBLLPro = new BLLProgreso();
        }

        [HttpGet("mio")]
        [ProducesResponseType(typeof(List<BECursoConProgreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECursoConProgreso>> ListarMio()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgreso);

            return Ok(oBLLPro.ListarPorUsuario(oSesionBE));
        }

        [HttpGet("empresa")]
        [ProducesResponseType(typeof(List<BEProgresoUsuario>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEProgresoUsuario>> ListarEmpresa()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgresoEmpresa);

            return Ok(oBLLPro.ListarPorEmpresa(oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<BECursoConProgreso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BECursoConProgreso>> Guardar([FromBody] BEGuardarProgreso peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoVerProgreso);

            return Ok(oBLLPro.Guardar(peticion, oSesionBE));
        }
    }
}
