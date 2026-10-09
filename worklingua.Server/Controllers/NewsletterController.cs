using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/newsletter")]
    public class NewsletterController : ControladorBase
    {
        BLLSuscriptorNewsletter oBLLSus;
        BLLEnvioNewsletter oBLLEnv;

        public NewsletterController()
        {
            oBLLSus = new BLLSuscriptorNewsletter();
            oBLLEnv = new BLLEnvioNewsletter();
        }

        [HttpPost("suscripciones")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Suscribir([FromBody] BESuscribirNewsletter oSuscripcionBE)
        {
            oBLLSus.Guardar(oSuscripcionBE);

            return Ok(new BEMensajeRespuesta(
                "Si el correo es válido, te enviamos un mensaje para confirmar la suscripción."));
        }

        [HttpPost("suscripciones/confirmar")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Confirmar([FromBody] BETokenNewsletter oTokenBE)
        {
            oBLLSus.Confirmar(oTokenBE);

            return Ok(new BEMensajeRespuesta("Tu suscripción al newsletter quedó confirmada."));
        }

        [HttpPost("suscripciones/baja")]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Baja([FromBody] BETokenNewsletter oTokenBE)
        {
            oBLLSus.Baja(oTokenBE);

            return Ok(new BEMensajeRespuesta("Ya no vas a recibir el newsletter de WorkLingua."));
        }

        [HttpGet("suscriptores")]
        [ProducesResponseType(typeof(List<BESuscriptorNewsletterRespuesta>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BESuscriptorNewsletterRespuesta>> ListarSuscriptores()
        {
            ExigirPermiso(Permisos.NewsletterListar);

            List<BESuscriptorNewsletterRespuesta> respuesta = new List<BESuscriptorNewsletterRespuesta>();

            foreach (BESuscriptorNewsletter oSuscriptorBE in oBLLSus.ListarTodo())
            {
                respuesta.Add(new BESuscriptorNewsletterRespuesta(
                    oSuscriptorBE.SuscriptorId,
                    oSuscriptorBE.Email,
                    oSuscriptorBE.IdiomaId,
                    oSuscriptorBE.Confirmado,
                    oSuscriptorBE.FechaAlta,
                    oSuscriptorBE.FechaConfirmacion,
                    oSuscriptorBE.FechaBaja,
                    oSuscriptorBE.Activo));
            }

            return Ok(respuesta);
        }

        [HttpGet("envios")]
        [ProducesResponseType(typeof(List<BEEnvioNewsletter>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<List<BEEnvioNewsletter>> ListarEnvios()
        {
            ExigirPermiso(Permisos.NewsletterListar);

            return Ok(oBLLEnv.ListarTodo());
        }

        [HttpPost("envios/previsualizar")]
        [ProducesResponseType(typeof(BEVistaPreviaNewsletterRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEVistaPreviaNewsletterRespuesta> Previsualizar([FromBody] BEEnviarNewsletter oEnvioBE)
        {
            ExigirPermiso(Permisos.NewsletterEnviar);

            string html = oBLLEnv.Previsualizar(oEnvioBE);

            BEIdioma oIdiomaBE = new BEIdioma();
            oIdiomaBE.IdiomaId = oEnvioBE.IdiomaId;

            return Ok(new BEVistaPreviaNewsletterRespuesta(html, oBLLSus.ListarConfirmados(oIdiomaBE).Count));
        }

        [HttpPost("envios")]
        [ProducesResponseType(typeof(BEEnvioNewsletter), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<BEEnvioNewsletter> Enviar([FromBody] BEEnviarNewsletter oEnvioBE)
        {
            BESesion oSesionBE = ExigirPermiso(Permisos.NewsletterEnviar);

            return StatusCode(StatusCodes.Status201Created, oBLLEnv.Guardar(oEnvioBE, oSesionBE));
        }
    }
}
