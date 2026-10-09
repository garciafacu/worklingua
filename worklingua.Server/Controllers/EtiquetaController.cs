using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    /// <summary>
    /// El diccionario de etiquetas con el que se clasifican los cursos
    /// (CU-004-005).
    ///
    /// Sin permisos propios: leerlo es parte de ver cursos y darle de alta una
    /// etiqueta es parte de clasificarlos, así que reusa Curso.Listar y
    /// Curso.Modificar.
    /// </summary>
    [Route("api/etiquetas")]
    public class EtiquetaController : ControladorBase
    {
        BLLEtiqueta oBLLEti;
        BLLBitacora oBLLBit;

        public EtiquetaController()
        {
            oBLLEti = new BLLEtiqueta();
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEEtiqueta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEtiqueta>> Listar()
        {
            ExigirPermiso(Permisos.CursoListar);

            return Ok(oBLLEti.Listar());
        }

        /// <summary>
        /// Camino alternativo 1: la etiqueta no estaba en el diccionario y el
        /// usuario confirmó agregarla. Pedir una que ya existe devuelve la que
        /// había, no un error.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BEEtiqueta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEEtiqueta> Crear([FromBody] BEEtiqueta oPeticionBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.CursoModificar);

            BEEtiqueta oEtiquetaBE = oBLLEti.Crear(oPeticionBE);

            oBLLBit.Guardar(new BEBitacoraEvento(
                0,
                oSesionBE.UsuarioId,
                DateTime.Now,
                BLLBitacora.ModuloCurso,
                "AltaEtiqueta",
                "Alta de la etiqueta " + oEtiquetaBE.Nombre + " (id " + oEtiquetaBE.EtiquetaId + ").",
                BLLBitacora.NivelInformativo));

            return StatusCode(StatusCodes.Status201Created, oEtiquetaBE);
        }
    }
}
