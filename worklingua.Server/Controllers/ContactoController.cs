using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/contacto")]
    public class ContactoController : ControladorBase
    {
        BLLContacto oBLLContacto;

        public ContactoController()
        {
            oBLLContacto = new BLLContacto();
        }

        [HttpPost]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Enviar([FromBody] BEEnviarConsultaContacto oConsultaBE)
        {
            oBLLContacto.EnviarConsulta(oConsultaBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Tu consulta fue enviada. Te responderemos a la brevedad.";

            return Ok(respuesta);
        }
    }
}
