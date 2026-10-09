using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/cursos/{cursoId:int}/modulos")]
    public class ModuloController : ControladorBase
    {
        BLLModulo oBLLMod;

        public ModuloController()
        {
            oBLLMod = new BLLModulo();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEModulo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEModulo>> Listar(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            BECurso oCursoBE = new BECurso();
            oCursoBE.CursoId = cursoId;

            return Ok(oBLLMod.ListarPorCurso(oCursoBE, oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEModulo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEModulo> Crear(int cursoId, [FromBody] BEGuardarModulo peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEModulo oModuloBE = new BEModulo(
                0, cursoId, peticion.Nombre, peticion.Descripcion, peticion.Contenido, peticion.OrdenModulo, true);

            return Ok(oBLLMod.Guardar(oModuloBE, oSesionBE));
        }

        [HttpDelete("{moduloId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Eliminar(int cursoId, int moduloId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEModulo oModuloBE = new BEModulo();
            oModuloBE.CursoId = cursoId;
            oModuloBE.ModuloId = moduloId;

            oBLLMod.Baja(oModuloBE, oSesionBE);

            return NoContent();
        }
    }
}
