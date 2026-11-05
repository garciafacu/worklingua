using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/faqs")]
    public class FaqController : ControladorBase
    {
        BLLFaq oBLLFaq = new BLLFaq();

        [HttpGet]
        [ProducesResponseType(typeof(List<BECategoriaFaqConPreguntas>), StatusCodes.Status200OK)]
        public ActionResult<List<BECategoriaFaqConPreguntas>> Listar([FromQuery] string idioma)
        {
            BEFiltroFaq oFiltroBE = new BEFiltroFaq();
            oFiltroBE.Idioma = idioma;

            return Ok(oBLLFaq.ListarPublicadas(oFiltroBE));
        }

        [HttpGet("administracion/categorias")]
        [ProducesResponseType(typeof(List<BECategoriaFaq>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BECategoriaFaq>> ListarCategorias()
        {
            ExigirPermiso(Permisos.FaqListar);

            return Ok(oBLLFaq.ListarCategorias());
        }

        [HttpGet("administracion/preguntas")]
        [ProducesResponseType(typeof(List<BEPreguntaFrecuente>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEPreguntaFrecuente>> ListarPreguntas([FromQuery] int? categoriaFaqId)
        {
            ExigirPermiso(Permisos.FaqListar);

            BEFiltroFaq oFiltroBE = new BEFiltroFaq();
            oFiltroBE.CategoriaFaqId = categoriaFaqId;

            return Ok(oBLLFaq.ListarPreguntas(oFiltroBE));
        }

        [HttpPost("administracion/categorias")]
        [ProducesResponseType(typeof(BECategoriaFaq), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BECategoriaFaq> CrearCategoria([FromBody] BEGuardarCategoriaFaq peticion)
        {
            oBLLFaq.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.FaqAlta);

            return Ok(oBLLFaq.GuardarCategoria(ACategoria(0, peticion), oSesionBE));
        }

        [HttpPut("administracion/categorias/{categoriaFaqId:int}")]
        [ProducesResponseType(typeof(BECategoriaFaq), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BECategoriaFaq> ModificarCategoria(
            int categoriaFaqId, [FromBody] BEGuardarCategoriaFaq peticion)
        {
            oBLLFaq.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.FaqModificar);

            return Ok(oBLLFaq.GuardarCategoria(ACategoria(categoriaFaqId, peticion), oSesionBE));
        }

        [HttpPatch("administracion/categorias/{categoriaFaqId:int}/estado")]
        [ProducesResponseType(typeof(BECategoriaFaq), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BECategoriaFaq> CambiarEstadoCategoria(
            int categoriaFaqId, [FromBody] BECategoriaFaq peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.FaqModificar);

            peticion.CategoriaFaqId = categoriaFaqId;

            return Ok(oBLLFaq.CambiarEstadoCategoria(peticion, oSesionBE));
        }

        [HttpDelete("administracion/categorias/{categoriaFaqId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult EliminarCategoria(int categoriaFaqId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.FaqBaja);

            BECategoriaFaq oCategoriaBE = new BECategoriaFaq();
            oCategoriaBE.CategoriaFaqId = categoriaFaqId;

            oBLLFaq.BajaCategoria(oCategoriaBE, oSesionBE);

            return NoContent();
        }

        [HttpPost("administracion/preguntas")]
        [ProducesResponseType(typeof(BEPreguntaFrecuente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEPreguntaFrecuente> CrearPregunta([FromBody] BEGuardarPreguntaFrecuente peticion)
        {
            oBLLFaq.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.FaqAlta);

            return Ok(oBLLFaq.GuardarPregunta(APregunta(0, peticion), oSesionBE));
        }

        [HttpPut("administracion/preguntas/{preguntaFrecuenteId:int}")]
        [ProducesResponseType(typeof(BEPreguntaFrecuente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEPreguntaFrecuente> ModificarPregunta(
            int preguntaFrecuenteId, [FromBody] BEGuardarPreguntaFrecuente peticion)
        {
            oBLLFaq.Validar(peticion);

            BESesion oSesionBE = ExigirPermiso(Permisos.FaqModificar);

            return Ok(oBLLFaq.GuardarPregunta(APregunta(preguntaFrecuenteId, peticion), oSesionBE));
        }

        [HttpPatch("administracion/preguntas/{preguntaFrecuenteId:int}/estado")]
        [ProducesResponseType(typeof(BEPreguntaFrecuente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BEPreguntaFrecuente> CambiarEstadoPregunta(
            int preguntaFrecuenteId, [FromBody] BEPreguntaFrecuente peticion)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.FaqModificar);

            peticion.PreguntaFrecuenteId = preguntaFrecuenteId;

            return Ok(oBLLFaq.CambiarEstadoPregunta(peticion, oSesionBE));
        }

        [HttpDelete("administracion/preguntas/{preguntaFrecuenteId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult EliminarPregunta(int preguntaFrecuenteId)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.FaqBaja);

            BEPreguntaFrecuente oPreguntaBE = new BEPreguntaFrecuente();
            oPreguntaBE.PreguntaFrecuenteId = preguntaFrecuenteId;

            oBLLFaq.BajaPregunta(oPreguntaBE, oSesionBE);

            return NoContent();
        }

        private BECategoriaFaq ACategoria(int categoriaFaqId, BEGuardarCategoriaFaq peticion)
        {
            BECategoriaFaq oCategoriaBE = new BECategoriaFaq();

            oCategoriaBE.CategoriaFaqId = categoriaFaqId;
            oCategoriaBE.IdiomaId = peticion.IdiomaId;
            oCategoriaBE.Titulo = peticion.Titulo;
            oCategoriaBE.Orden = peticion.Orden;
            oCategoriaBE.Activo = peticion.Activo;

            return oCategoriaBE;
        }

        private BEPreguntaFrecuente APregunta(int preguntaFrecuenteId, BEGuardarPreguntaFrecuente peticion)
        {
            BEPreguntaFrecuente oPreguntaBE = new BEPreguntaFrecuente();

            oPreguntaBE.PreguntaFrecuenteId = preguntaFrecuenteId;
            oPreguntaBE.CategoriaFaqId = peticion.CategoriaFaqId;
            oPreguntaBE.Pregunta = peticion.Pregunta;
            oPreguntaBE.Respuesta = peticion.Respuesta;
            oPreguntaBE.Enlace = peticion.Enlace;
            oPreguntaBE.EnlaceEtiqueta = peticion.EnlaceEtiqueta;
            oPreguntaBE.Orden = peticion.Orden;
            oPreguntaBE.Activo = peticion.Activo;

            return oPreguntaBE;
        }
    }

    [Route("api/contacto")]
    public class ContactoController : ControladorBase
    {
        BLLContacto oBLLContacto = new BLLContacto();

        [HttpPost]
        [ProducesResponseType(typeof(BEConsultaPublicaRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BEConsultaPublicaRespuesta>> Enviar(
            [FromBody] BEEnviarConsultaContacto oConsultaBE)
        {
            oBLLContacto.Validar(oConsultaBE);

            return Ok(await oBLLContacto.EnviarConsulta(oConsultaBE));
        }

        [HttpGet("consulta")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BETicketConDetalle> Obtener([FromQuery] Guid? token)
        {
            BEFiltroTicket oFiltroBE = new BEFiltroTicket();
            oFiltroBE.Token = token;

            return Ok(oBLLContacto.ListarObjeto(oFiltroBE));
        }

        [HttpPost("consulta/mensajes")]
        [ProducesResponseType(typeof(BETicketConDetalle), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<BETicketConDetalle> Responder([FromBody] BEResponderConsultaPublica peticion)
        {
            oBLLContacto.Validar(peticion);

            return Ok(oBLLContacto.GuardarMensaje(peticion));
        }
    }
}
