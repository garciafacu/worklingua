using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/encuestas")]
    public class EncuestaController : ControladorBase
    {
        BLLEncuesta oBLLEnc;

        public EncuestaController()
        {
            oBLLEnc = new BLLEncuesta();
        }

        [HttpGet("vigentes")]
        [ProducesResponseType(typeof(List<BEEncuestaConDetalle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEncuestaConDetalle>> ListarVigentes([FromQuery] BEFiltroEncuesta oFiltroBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EncuestaResponder);

            return Ok(oBLLEnc.ListarVigentes(oFiltroBE, oSesionBE));
        }

        [HttpPost("{encuestaId:int}/respuestas")]
        [ProducesResponseType(typeof(BEEncuestaConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEEncuestaConDetalle> Responder(
            int encuestaId, [FromBody] BEResponderEncuesta peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EncuestaResponder);

            peticion.EncuestaId = encuestaId;

            return Ok(oBLLEnc.Responder(peticion, oSesionBE));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEEncuestaConDetalle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEncuestaConDetalle>> ListarAdministracion()
        {
            ExigirPermiso(Permisos.EncuestaListar);

            return Ok(oBLLEnc.ListarTodo());
        }

        [HttpPost("administracion")]
        [ProducesResponseType(typeof(BEEncuestaConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEEncuestaConDetalle> Crear([FromBody] BEGuardarEncuesta peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EncuestaAlta);

            return Ok(oBLLEnc.Guardar(Mapear(0, peticion), peticion.Opciones, oSesionBE));
        }

        [HttpPut("administracion/{encuestaId:int}")]
        [ProducesResponseType(typeof(BEEncuestaConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEEncuestaConDetalle> Modificar(
            int encuestaId, [FromBody] BEGuardarEncuesta peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EncuestaModificar);

            return Ok(oBLLEnc.Guardar(Mapear(encuestaId, peticion), peticion.Opciones, oSesionBE));
        }

        [HttpDelete("administracion/{encuestaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Eliminar(int encuestaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.EncuestaBaja);

            BEEncuesta oEncuestaBE = new BEEncuesta();
            oEncuestaBE.EncuestaId = encuestaId;

            oBLLEnc.Baja(oEncuestaBE, oSesionBE);

            return NoContent();
        }

        private BEEncuesta Mapear(int encuestaId, BEGuardarEncuesta peticion)
        {
            BEEncuesta oEncuestaBE = new BEEncuesta();

            oEncuestaBE.EncuestaId = encuestaId;
            oEncuestaBE.IdiomaId = peticion.IdiomaId;
            oEncuestaBE.Pregunta = peticion.Pregunta;
            oEncuestaBE.Descripcion = peticion.Descripcion;
            oEncuestaBE.FechaDesde = peticion.FechaDesde ?? DateOnly.FromDateTime(DateTime.Now);
            oEncuestaBE.FechaVencimiento = peticion.FechaVencimiento ?? DateOnly.FromDateTime(DateTime.Now);
            oEncuestaBE.Activo = peticion.Activo;

            return oEncuestaBE;
        }
    }
}
