using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/tickets")]
    public class TicketController : ControladorBase
    {
        BLLTicket oBLLTic;

        public TicketController()
        {
            oBLLTic = new BLLTicket();
        }

        [HttpGet("opciones")]
        [ProducesResponseType(typeof(BEOpcionesTicket), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEOpcionesTicket> ListarOpciones()
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketCrear);

            return Ok(oBLLTic.ListarOpciones(oSesionBE));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BETicketConDetalle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BETicketConDetalle>> Listar([FromQuery] string estado)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketCrear);

            return Ok(oBLLTic.Listar(Filtro(estado, null, false), oSesionBE));
        }

        [HttpGet("{ticketId:int}")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BETicketConDetalle> Obtener(int ticketId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketCrear);

            return Ok(oBLLTic.ListarObjeto(Filtro(null, ticketId, false), oSesionBE));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BETicketConDetalle> Crear([FromBody] BECrearTicket peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketCrear);

            return Ok(oBLLTic.Guardar(peticion, oSesionBE));
        }

        [HttpPost("{ticketId:int}/mensajes")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BETicketConDetalle> EnviarMensaje(int ticketId, [FromBody] BEEnviarMensajeTicket peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketCrear);

            return Ok(oBLLTic.GuardarMensaje(Mensaje(ticketId, peticion, false), oSesionBE));
        }

        [HttpPut("{ticketId:int}/cerrar")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BETicketConDetalle> Cerrar(int ticketId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketCrear);

            return Ok(oBLLTic.CambiarEstado(Filtro(BLLTicket.EstadoCerrado, ticketId, false), oSesionBE));
        }

        [HttpGet("administracion")]
        [ProducesResponseType(typeof(List<BETicketConDetalle>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BETicketConDetalle>> ListarAdministracion([FromQuery] string estado)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketAtender);

            return Ok(oBLLTic.Listar(Filtro(estado, null, true), oSesionBE));
        }

        [HttpGet("administracion/{ticketId:int}")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BETicketConDetalle> ObtenerAdministracion(int ticketId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketAtender);

            return Ok(oBLLTic.ListarObjeto(Filtro(null, ticketId, true), oSesionBE));
        }

        [HttpPost("administracion/{ticketId:int}/respuesta")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BETicketConDetalle> Responder(int ticketId, [FromBody] BEEnviarMensajeTicket peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketAtender);

            return Ok(oBLLTic.GuardarMensaje(Mensaje(ticketId, peticion, true), oSesionBE));
        }

        [HttpPut("administracion/{ticketId:int}/estado")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BETicketConDetalle> CambiarEstado(int ticketId, [FromBody] BECambiarEstadoTicket peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.TicketAtender);

            return Ok(oBLLTic.CambiarEstado(Filtro(peticion.Estado, ticketId, true), oSesionBE));
        }

        private BEFiltroTicket Filtro(string estado, int? ticketId, bool administracion)
        {
            BEFiltroTicket oFiltroBE = new BEFiltroTicket();

            oFiltroBE.Estado = estado;
            oFiltroBE.TicketId = ticketId;
            oFiltroBE.Administracion = administracion;

            return oFiltroBE;
        }

        private BEMensajeTicket Mensaje(int ticketId, BEEnviarMensajeTicket peticion, bool esOperador)
        {
            BEMensajeTicket oMensajeBE = new BEMensajeTicket();

            oMensajeBE.TicketId = ticketId;
            oMensajeBE.Texto = peticion.Texto;
            oMensajeBE.EsOperador = esOperador;

            return oMensajeBE;
        }
    }
}
