using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    /// <summary>
    /// Versiones de un curso (CU-004-004) y clonado (CU-004-002).
    ///
    /// Sin permisos propios: ver el historial es listar el curso, crear un
    /// punto de restauración o restaurarlo es modificarlo, y clonar es dar de
    /// alta uno nuevo.
    /// </summary>
    [Route("api/cursos")]
    public class VersionCursoController : ControladorBase
    {
        BLLVersionCurso oBLLVer;
        BLLBitacora oBLLBit;

        public VersionCursoController()
        {
            oBLLVer = new BLLVersionCurso();
            oBLLBit = new BLLBitacora();
        }

        /// <summary>La línea de tiempo del curso. No trae el XML.</summary>
        [HttpGet("{cursoId:int}/versiones")]
        [ProducesResponseType(typeof(List<BEVersionContenido>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEVersionContenido>> Listar(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLVer.ListarPorCurso(cursoId, oSesionBE));
        }

        /// <summary>Una versión con su documento XML.</summary>
        [HttpGet("versiones/{versionId:int}")]
        [ProducesResponseType(typeof(BEVersionContenido), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEVersionContenido> Obtener(int versionId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLVer.Obtener(versionId, oSesionBE));
        }

        /// <summary>Camino alternativo 2: el punto de restauración manual.</summary>
        [HttpPost("{cursoId:int}/versiones")]
        [ProducesResponseType(typeof(BEVersionContenido), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEVersionContenido> Crear(
            int cursoId, [FromBody] BEVersionContenido oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEVersionContenido oVersionBE = oBLLVer.Crear(
                cursoId, oPeticionBE == null ? null : oPeticionBE.Observaciones, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "Snapshot",
                "Punto de restauración " + oVersionBE.NumeroVersion + " del curso " + cursoId + ".",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, oVersionBE);
        }

        /// <summary>El escenario principal: vuelve el curso a esa versión.</summary>
        [HttpPost("versiones/{versionId:int}/restaurar")]
        [ProducesResponseType(typeof(BEResultadoRestauracion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEResultadoRestauracion> Restaurar(int versionId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEResultadoRestauracion respuesta = oBLLVer.Restaurar(versionId, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "Restauracion",
                "Restauración de la versión " + versionId + " en el curso " + respuesta.CursoId +
                " (" + respuesta.Omitidos + " activos omitidos).",
                BLLBitacora.NivelInformativo));

            return Ok(respuesta);
        }

        /// <summary>Borra un punto de restauración que ya no hace falta.</summary>
        [HttpDelete("versiones/{versionId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Eliminar(int versionId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEVersionContenido oVersionBE = oBLLVer.Eliminar(versionId, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "BajaVersion",
                "Baja del punto de restauración " + oVersionBE.NumeroVersion + " del curso " +
                oVersionBE.CursoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        /// <summary>Camino alternativo 3: qué cambió entre dos versiones.</summary>
        [HttpGet("versiones/comparar")]
        [ProducesResponseType(typeof(List<BEDiferenciaVersion>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEDiferenciaVersion>> Comparar(
            [FromQuery] int izquierda, [FromQuery] int derecha)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLVer.Comparar(izquierda, derecha, oSesionBE));
        }

        /// <summary>CU-004-002: duplica el curso con sus módulos y activos.</summary>
        [HttpPost("{cursoId:int}/clonar")]
        [ProducesResponseType(typeof(BEResultadoRestauracion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEResultadoRestauracion> Clonar(
            int cursoId, [FromBody] BEClonarCurso peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoAlta);

            BEResultadoRestauracion respuesta = oBLLVer.Clonar(
                cursoId, peticion == null ? null : peticion.Nombre, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "Clonacion",
                "Clonación del curso " + cursoId + " en el curso " + respuesta.CursoId +
                " (" + respuesta.Omitidos + " activos omitidos).",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, respuesta);
        }
    }
}
