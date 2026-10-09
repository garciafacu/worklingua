using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/cursos")]
    public class CursoController : ControladorBase
    {
        BLLCurso oBLLCur;
        BLLBitacora oBLLBit;

        public CursoController()
        {
            oBLLCur = new BLLCurso();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BECursoRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECursoRespuesta>> Buscar([FromQuery] BEFiltroCurso oFiltroBE)
        {
            // No hay consulta pública: cada empresa ve los cursos globales y los propios.
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            List<BECurso> ListaCursoBE = oBLLCur.Buscar(oFiltroBE, oSesionBE);
            List<BECursoRespuesta> respuesta = new List<BECursoRespuesta>();

            foreach (BECurso oCursoBE in ListaCursoBE)
            {
                BECursoRespuesta item = new BECursoRespuesta();

                item.CursoId = oCursoBE.CursoId;
                item.Nombre = oCursoBE.Nombre;
                item.Descripcion = oCursoBE.Descripcion;
                item.Nivel = oCursoBE.Nivel;
                item.DuracionHoras = oCursoBE.DuracionHoras;
                item.Idioma = oCursoBE.Idioma;
                item.EsGlobal = !oCursoBE.EmpresaId.HasValue;
                item.Sector = oCursoBE.Sector;
                item.Etiquetas = oCursoBE.Etiquetas;

                respuesta.Add(item);
            }

            return Ok(respuesta);
        }

        [HttpGet("idiomas")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<string>> ListarIdiomas()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLCur.ListarIdiomas(oSesionBE));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BECurso>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECurso>> ListarParaAdministracion()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLCur.ListarParaAdministracion(oSesionBE));
        }

        [HttpGet("{cursoId:int}")]
        [ProducesResponseType(typeof(BECurso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BECurso> Obtener(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoListar);

            BECurso oFiltroBE = new BECurso();
            oFiltroBE.CursoId = cursoId;

            return Ok(oBLLCur.ListarObjeto(oFiltroBE, oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BECurso), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECurso> Crear([FromBody] BEGuardarCurso peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoAlta);

            BECurso oCursoBE = oBLLCur.Guardar(ADominio(0, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "Alta",
                "Alta del curso " + oCursoBE.Nombre + " (id " + oCursoBE.CursoId + ").",
                BLLBitacora.NivelInformativo));

            return CreatedAtAction(
                nameof(Obtener), new { cursoId = oCursoBE.CursoId }, oCursoBE);
        }

        [HttpPut("{cursoId:int}")]
        [ProducesResponseType(typeof(BECurso), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECurso> Modificar(
            int cursoId, [FromBody] BEGuardarCurso peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BECurso oCursoBE = oBLLCur.Guardar(ADominio(cursoId, peticion), oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "Modificacion",
                "Modificación del curso " + oCursoBE.Nombre + " (id " + cursoId + ").",
                BLLBitacora.NivelInformativo));

            return Ok(oCursoBE);
        }

        [HttpDelete("{cursoId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Baja(int cursoId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoBaja);

            BECurso oCursoBE = new BECurso();
            oCursoBE.CursoId = cursoId;

            oBLLCur.Baja(oCursoBE, oSesionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "Baja",
                "Baja lógica del curso con id " + cursoId + ".",
                BLLBitacora.NivelInformativo));

            return NoContent();
        }

        private BECurso ADominio(int cursoId, BEGuardarCurso peticion)
        {
            BECurso oCursoBE = new BECurso();

            oCursoBE.CursoId = cursoId;
            oCursoBE.Idioma = peticion.Idioma;
            oCursoBE.Nombre = peticion.Nombre;
            oCursoBE.Descripcion = peticion.Descripcion;
            oCursoBE.Nivel = peticion.Nivel;
            oCursoBE.DuracionHoras = peticion.DuracionHoras;
            oCursoBE.Sector = peticion.Sector;
            oCursoBE.FechaPublicacion = peticion.FechaPublicacion;
            oCursoBE.FechaFin = peticion.FechaFin;

            if (peticion.EtiquetaIds != null)
            {
                foreach (int etiquetaId in peticion.EtiquetaIds)
                {
                    oCursoBE.Etiquetas.Add(new BEEtiqueta(etiquetaId, null, true, null));
                }
            }

            return oCursoBE;
        }
    }
}
