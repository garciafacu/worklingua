using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/busqueda")]
    public class BusquedaController : ControladorBase
    {
        BLLBusqueda oBLLBus;
        BLLSesion oBLLSes;

        public BusquedaController()
        {
            oBLLBus = new BLLBusqueda();
            oBLLSes = new BLLSesion();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BEPaginaBuscableRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<List<BEPaginaBuscableRespuesta>> Buscar([FromQuery] BEFiltroBusqueda oFiltroBE)
        {
            List<BEPaginaBuscable> ListaPaginaBE = oBLLBus.Buscar(oFiltroBE, ObtenerSesionOpcional());
            List<BEPaginaBuscableRespuesta> respuesta = new List<BEPaginaBuscableRespuesta>();

            foreach (BEPaginaBuscable oPaginaBE in ListaPaginaBE)
            {
                respuesta.Add(new BEPaginaBuscableRespuesta(
                    oPaginaBE.Ruta,
                    oPaginaBE.ClaveTitulo,
                    oPaginaBE.ClaveDescripcion,
                    oPaginaBE.ClaveSeccion,
                    oPaginaBE.Area));
            }

            return Ok(respuesta);
        }

        [HttpGet("secciones")]
        [ProducesResponseType(typeof(List<BESeccionBusquedaRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<List<BESeccionBusquedaRespuesta>> ListarSecciones()
        {
            return Ok(oBLLBus.ListarSecciones(ObtenerSesionOpcional()));
        }

        private BESesion ObtenerSesionOpcional()
        {
            Guid? token = ObtenerTokenSesionOpcional();

            return token.HasValue ? oBLLSes.ObtenerActiva(token.Value) : null;
        }
    }
}
