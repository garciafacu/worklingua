using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    /// <summary>
    /// Activos pedagógicos de un curso (CU-004-001).
    ///
    /// Sin permisos propios: se gestionan dentro del ABM de Cursos y reusan
    /// Curso.Listar y Curso.Modificar, igual que los módulos.
    /// </summary>
    [Route("api/activos")]
    public class ActivoController : ControladorBase
    {
        BLLActivoPedagogico oBLLAct;
        BLLBitacora oBLLBit;

        public ActivoController()
        {
            oBLLAct = new BLLActivoPedagogico();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEActivoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<BEActivoRespuesta>> Listar([FromQuery] int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            BEActivoPedagogico oFiltroBE = new BEActivoPedagogico();
            oFiltroBE.CursoId = cursoId;

            List<BEActivoRespuesta> respuesta = new List<BEActivoRespuesta>();

            foreach (BEActivoPedagogico oActivoBE in oBLLAct.ListarPorCurso(oFiltroBE, oSesionBE))
            {
                respuesta.Add(Mapear(oActivoBE));
            }

            return Ok(respuesta);
        }

        /// <summary>
        /// El escenario principal. Llega como multipart porque trae el archivo;
        /// es el único endpoint del proyecto que no recibe JSON.
        ///
        /// Con confirmarNombre en true el usuario ya aceptó la nomenclatura
        /// sugerida del camino alternativo 1.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BEActivoRespuesta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEActivoRespuesta> Crear(
            [FromForm] int cursoId,
            [FromForm] string nombre,
            [FromForm] string tipoContenido,
            [FromForm] string descripcion,
            [FromForm] bool confirmarNombre,
            [FromForm] int? moduloId,
            IFormFile archivo)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEActivoPedagogico oActivoBE = new BEActivoPedagogico();
            oActivoBE.CursoId = cursoId;
            oActivoBE.ModuloId = moduloId;
            oActivoBE.Nombre = nombre;
            oActivoBE.TipoContenido = tipoContenido;
            oActivoBE.Descripcion = descripcion;

            BEActivoPedagogico oGuardadoBE =
                oBLLAct.Guardar(oActivoBE, ALeido(archivo), confirmarNombre, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloActivo,
                "Alta",
                "Alta del activo " + oGuardadoBE.Nombre + " (id " + oGuardadoBE.ActivoPedagogicoId +
                ") en el curso " + oGuardadoBE.CursoId + ".",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, Mapear(oGuardadoBE));
        }

        /// <summary>Publicar o volver a borrador.</summary>
        [HttpPatch("{activoId:int}/publicacion")]
        [ProducesResponseType(typeof(BEActivoRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEActivoRespuesta> CambiarPublicacion(
            int activoId, [FromBody] BEActivoPedagogico oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            oPeticionBE.ActivoPedagogicoId = activoId;

            BEActivoPedagogico oActivoBE = oBLLAct.CambiarEstado(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloActivo,
                oActivoBE.Estado == BLLActivoPedagogico.EstadoPublicado ? "Publicacion" : "VueltaABorrador",
                "El activo " + oActivoBE.Nombre + " (id " + activoId + ") quedó en estado " +
                oActivoBE.Estado + ".",
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oActivoBE));
        }

        /// <summary>Baja lógica y reactivación.</summary>
        [HttpPatch("{activoId:int}/estado")]
        [ProducesResponseType(typeof(BEActivoRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEActivoRespuesta> CambiarEstado(
            int activoId, [FromBody] BEActivoPedagogico oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            oPeticionBE.ActivoPedagogicoId = activoId;

            BEActivoPedagogico oActivoBE = oBLLAct.CambiarActivo(oPeticionBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloActivo,
                oActivoBE.Activo == true ? "Reactivacion" : "Baja",
                "El activo " + oActivoBE.Nombre + " (id " + activoId + ") quedó " +
                (oActivoBE.Activo == true ? "vigente." : "dado de baja."),
                BLLBitacora.NivelInformativo));

            return Ok(Mapear(oActivoBE));
        }

        /// <summary>
        /// Pasa el archivo de la petición a una BE: la BLL valida y guarda sin
        /// conocer IFormFile, que es un tipo de la capa web.
        /// </summary>
        private BEArchivoSubido ALeido(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return null;
            }

            using (MemoryStream memoria = new MemoryStream())
            {
                archivo.CopyTo(memoria);

                return new BEArchivoSubido(archivo.FileName, memoria.ToArray());
            }
        }

        private BEActivoRespuesta Mapear(BEActivoPedagogico oActivoBE)
        {
            BEActivoRespuesta respuesta = new BEActivoRespuesta();

            respuesta.ActivoPedagogicoId = oActivoBE.ActivoPedagogicoId;
            respuesta.CursoId = oActivoBE.CursoId;
            respuesta.ModuloId = oActivoBE.ModuloId;
            respuesta.Modulo = oActivoBE.Modulo;
            respuesta.Nombre = oActivoBE.Nombre;
            respuesta.TipoContenido = oActivoBE.TipoContenido;
            respuesta.Descripcion = oActivoBE.Descripcion;
            respuesta.UrlArchivo = oActivoBE.UrlArchivo;
            respuesta.Activo = oActivoBE.Activo.HasValue && oActivoBE.Activo.Value;
            respuesta.Estado = oActivoBE.Estado;

            return respuesta;
        }
    }
}
