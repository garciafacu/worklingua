using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/ofertas")]
    public class OfertaController : ControladorBase
    {
        BLLOferta oBLLOfe;

        public OfertaController()
        {
            oBLLOfe = new BLLOferta();
        }

        [HttpGet("mias")]
        [ProducesResponseType(typeof(List<BEOfertaConDetalle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEOfertaConDetalle>> ListarMias()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OfertaConsultar);

            return Ok(oBLLOfe.ListarVigentes(oSesionBE));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BEOfertaConDetalle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEOfertaConDetalle>> ListarAdministracion()
        {
            ExigirPermiso(Permisos.OfertaListar);

            return Ok(oBLLOfe.ListarTodo());
        }

        [HttpPost("administracion")]
        [ProducesResponseType(typeof(BEOfertaConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEOfertaConDetalle> Crear([FromBody] BEGuardarOferta peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OfertaAlta);

            return Ok(oBLLOfe.Guardar(Mapear(0, peticion), oSesionBE));
        }

        [HttpPut("administracion/{ofertaId:int}")]
        [ProducesResponseType(typeof(BEOfertaConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEOfertaConDetalle> Modificar(int ofertaId, [FromBody] BEGuardarOferta peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OfertaModificar);

            return Ok(oBLLOfe.Guardar(Mapear(ofertaId, peticion), oSesionBE));
        }

        [HttpDelete("administracion/{ofertaId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Eliminar(int ofertaId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.OfertaBaja);

            BEOferta oOfertaBE = new BEOferta();
            oOfertaBE.OfertaId = ofertaId;

            oBLLOfe.Baja(oOfertaBE, oSesionBE);

            return NoContent();
        }

        private BEOferta Mapear(int ofertaId, BEGuardarOferta peticion)
        {
            BEOferta oOfertaBE = new BEOferta();

            oOfertaBE.OfertaId = ofertaId;
            oOfertaBE.EmpresaId = peticion.EmpresaId;
            oOfertaBE.PlanId = peticion.PlanId;
            oOfertaBE.Titulo = peticion.Titulo;
            oOfertaBE.Descripcion = peticion.Descripcion;
            oOfertaBE.FechaDesde = peticion.FechaDesde ?? DateOnly.FromDateTime(DateTime.Now);
            oOfertaBE.FechaHasta = peticion.FechaHasta;
            oOfertaBE.Activo = peticion.Activo;

            return oOfertaBE;
        }
    }
}
