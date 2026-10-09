using System.ServiceModel;
using ServiceReferenceCaptcha;

namespace worklingua.Server.Services
{
    public class ServicioCaptcha
    {
        public async Task<bool> ValidarAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            EndpointAddress oDireccion = new EndpointAddress(Configuracion.ReCaptchaUrlServicioWeb);

            BasicHttpBinding oEnlace = new BasicHttpBinding(
                oDireccion.Uri.Scheme == Uri.UriSchemeHttps
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);

            CaptchaServicioWebSoapClient oCliente = new CaptchaServicioWebSoapClient(oEnlace, oDireccion);

            try
            {
                ValidarCaptchaResponse oRespuesta = await oCliente.ValidarCaptchaAsync(
                    token, Configuracion.ReCaptchaSecretKey);

                await oCliente.CloseAsync();

                bool esValido = oRespuesta != null
                    && oRespuesta.Body != null
                    && oRespuesta.Body.ValidarCaptchaResult;

                if (!esValido)
                {
                    ServicioLog.Advertencia("Google rechazó el token de CAPTCHA.");
                }

                return esValido;
            }
            catch (Exception ex)
            {
                oCliente.Abort();

                ServicioLog.Error(
                    "No se pudo contactar al Web Service de CAPTCHA en " +
                    Configuracion.ReCaptchaUrlServicioWeb + ".", ex);

                return false;
            }
        }
    }
}
