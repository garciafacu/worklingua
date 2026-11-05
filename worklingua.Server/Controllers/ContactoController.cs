using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;

namespace worklingua.Server.Controllers
{
    [Route("api/contacto")]
    public class ContactoController : ControladorBase
    {
        BLLContacto oBLLContacto = new BLLContacto();

        [HttpPost]
        [ProducesResponseType(typeof(BEMensajeRespuesta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<BEMensajeRespuesta> Enviar([FromBody] BEEnviarConsultaContacto oConsultaBE)
        {
            oBLLContacto.Validar(oConsultaBE);

            oBLLContacto.EnviarConsulta(oConsultaBE);

            return Ok(new BEMensajeRespuesta("Tu consulta fue enviada. Te responderemos a la brevedad."));
        }
    }
}
