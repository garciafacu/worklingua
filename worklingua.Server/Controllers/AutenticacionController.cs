using Microsoft.AspNetCore.Mvc;
using worklingua.Server.BE;
using worklingua.Server.BLL;
using worklingua.Server.Services;

namespace worklingua.Server.Controllers
{
    [Route("api/autenticacion")]
    public class AutenticacionController : ControladorBase
    {
        BLLUsuario oBLLUsu;
        BLLSesion oBLLSes;
        ServicioHash oServicioHash;

        public AutenticacionController()
        {
            oBLLUsu = new BLLUsuario();
            oBLLSes = new BLLSesion();
            oServicioHash = new ServicioHash();
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(BEResultadoLogin), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<BEResultadoLogin> Login([FromBody] BELogin oLoginBE)
        {
            oLoginBE.Email = oLoginBE.Email.Trim().ToLowerInvariant();

            BEUsuario oFiltroBE = new BEUsuario();
            oFiltroBE.Email = oLoginBE.Email;

            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorEmail(oFiltroBE);

            BESesion oSesionBE = new BESesion();

            if (oUsuarioBE != null && oServicioHash.Verificar(oLoginBE.Clave, oUsuarioBE.PasswordHash))
            {
                oSesionBE.UsuarioId = oUsuarioBE.UsuarioId;
            }

            return Ok(oBLLUsu.Login(oLoginBE, oSesionBE));
        }

        [HttpGet("sesion")]
        [ProducesResponseType(typeof(BEResultadoLogin), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<BEResultadoLogin> ObtenerSesion()
        {
            BESesion oSesionBE = new BESesion();
            oSesionBE.Token = ObtenerTokenSesion();

            return Ok(oBLLUsu.ObtenerSesion(oSesionBE));
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            oBLLUsu.CerrarSesion(ObtenerTokenSesion());

            return NoContent();
        }

        [HttpPost("recuperar-clave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult RecuperarClave([FromBody] BERecuperarClave oRecuperarClaveBE)
        {
            oRecuperarClaveBE.Email = oRecuperarClaveBE.Email.Trim().ToLowerInvariant();

            oBLLUsu.SolicitarRecuperoClave(oRecuperarClaveBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Si el correo corresponde a una cuenta activa, vas a recibir un enlace para restablecer tu clave.";

            return Ok(respuesta);
        }

        [HttpPost("restablecer-clave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult RestablecerClave([FromBody] BERestablecerClave oRestablecerClaveBE)
        {
            oBLLUsu.ValidarClave(oRestablecerClaveBE.ClaveNueva);

            oRestablecerClaveBE.ClaveNueva = oServicioHash.Hashear(oRestablecerClaveBE.ClaveNueva);

            oBLLUsu.RestablecerClave(oRestablecerClaveBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Tu clave fue actualizada. Ingresá con la clave nueva.";

            return Ok(respuesta);
        }

        [HttpPost("cambiar-clave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CambiarClave([FromBody] BECambiarClave oCambiarClaveBE)
        {
            BESesion oSesionBE = oBLLSes.ObtenerActiva(ObtenerTokenSesion());

            oBLLUsu.ValidarClave(oCambiarClaveBE.ClaveNueva);

            BEUsuario oUsuarioBE = oBLLUsu.ObtenerPorId(oSesionBE.UsuarioId);

            bool claveActualValida = oServicioHash.Verificar(oCambiarClaveBE.ClaveActual, oUsuarioBE.PasswordHash);
            string passwordHashNuevo = oServicioHash.Hashear(oCambiarClaveBE.ClaveNueva);
            bool claveNuevaIgualQueActual = oServicioHash.Verificar(oCambiarClaveBE.ClaveActual, passwordHashNuevo);

            oCambiarClaveBE.ClaveActual = claveActualValida ? oCambiarClaveBE.ClaveActual : null;
            oCambiarClaveBE.ClaveNueva = claveNuevaIgualQueActual ? null : passwordHashNuevo;

            oBLLUsu.CambiarClave(oCambiarClaveBE, oSesionBE);

            BEMensajeRespuesta respuesta = new BEMensajeRespuesta();
            respuesta.Mensaje = "Tu clave fue actualizada. Se cerraron las demás sesiones abiertas.";

            return Ok(respuesta);
        }
    }
}
