using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/bitacora")]
    public class BitacoraController : ControladorBase
    {
        BLLBitacora oBLLBit;

        public BitacoraController()
        {
            oBLLBit = new BLLBitacora();
        }

        [HttpGet]
        [ProducesResponseType(typeof(BEPaginaBitacoraRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEPaginaBitacoraRespuesta> Buscar(
            [FromQuery] BEFiltroBitacora oFiltroBE)
        {
            ExigirPermiso(Permisos.BitacoraListar);

            BEPaginaBitacora oPaginaBE = oBLLBit.Buscar(oFiltroBE);

            return Ok(Mapear(oPaginaBE));
        }

        private BEPaginaBitacoraRespuesta Mapear(BEPaginaBitacora oPaginaBE)
        {
            BEPaginaBitacoraRespuesta respuesta = new BEPaginaBitacoraRespuesta();

            respuesta.Registros = new List<BEBitacoraEventoRespuesta>();
            respuesta.TotalRegistros = oPaginaBE.TotalRegistros;
            respuesta.Pagina = oPaginaBE.Pagina;
            respuesta.TamanioPagina = oPaginaBE.TamanioPagina;
            respuesta.TotalPaginas =
                (oPaginaBE.TotalRegistros + oPaginaBE.TamanioPagina - 1) / oPaginaBE.TamanioPagina;

            foreach (BEBitacoraEventoConUsuario oRegistroBE in oPaginaBE.Registros)
            {
                respuesta.Registros.Add(Mapear(oRegistroBE));
            }

            return respuesta;
        }

        private BEBitacoraEventoRespuesta Mapear(BEBitacoraEventoConUsuario oRegistroBE)
        {
            BEBitacoraEventoRespuesta item = new BEBitacoraEventoRespuesta();

            item.BitacoraId = oRegistroBE.Evento.BitacoraId;
            item.UsuarioId = oRegistroBE.Evento.UsuarioId;
            item.FechaEvento = oRegistroBE.Evento.FechaEvento;
            item.Modulo = oRegistroBE.Evento.Modulo;
            item.Accion = oRegistroBE.Evento.Accion;
            item.Descripcion = oRegistroBE.Evento.Descripcion;
            item.Nivel = oRegistroBE.Evento.Nivel;
            item.Usuario = oRegistroBE.Usuario;
            item.Email = oRegistroBE.Email;

            return item;
        }
    }
}
