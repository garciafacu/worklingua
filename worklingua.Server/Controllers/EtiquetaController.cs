using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
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
