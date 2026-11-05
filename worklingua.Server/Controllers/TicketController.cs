using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/tickets")]
    public class TicketController : ControladorBase
    {
        BLLTicket oBLLTic = new BLLTicket();

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
            oBLLTic.Validar(peticion);

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
            oBLLTic.Validar(peticion);

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
            oBLLTic.Validar(peticion);

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
        public ActionResult<BETicketConDetalle> CambiarEstado(int ticketId, [FromBody] BETicket peticion)
        {
            oBLLTic.Validar(peticion);

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

    [Route("api/ofertas")]
    public class OfertaController : ControladorBase
    {
        BLLOferta oBLLOfe = new BLLOferta();

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
            oBLLOfe.Validar(peticion);

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
            oBLLOfe.Validar(peticion);

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
