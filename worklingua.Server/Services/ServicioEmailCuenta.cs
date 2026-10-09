using System.Collections.Generic;
using worklingua.Server.BE;

namespace worklingua.Server.Services
{
    public class ServicioEmailCuenta
    {
        private const string NotaPieAutomatica =
            "Este es un mensaje automatico de WorkLingua. Por favor no respondas a esta direccion.";

        private const string NotaPieContacto =
            "Consulta enviada desde el formulario de contacto del sitio. " +
            "Podes responder este correo: la respuesta le llega directamente a quien escribio.";

        private const string TextoEnlaceAlternativo =
            "Si el boton no funciona, copiá y pegá este enlace en tu navegador:";

        ServicioEmail oServicioEmail;
        ServicioPlantillaEmail oServicioPlantilla;

        public ServicioEmailCuenta()
        {
            oServicioEmail = new ServicioEmail();
            oServicioPlantilla = new ServicioPlantillaEmail();
        }

        public void EnviarConfirmacionRegistro(BEUsuario Objeto, Guid token)
        {
            Dictionary<string, string> valoresExtra = new Dictionary<string, string>();
            valoresExtra.Add("HorasVigencia", Configuracion.HorasVigenciaConfirmacion.ToString());

            Enviar(
                Objeto,
                "confirmacion-registro",
                "Confirmá tu cuenta de WorkLingua",
                "Confirmá tu dirección de correo",
                "Confirmar mi cuenta",
                ConstruirUrl("confirmar-cuenta", token),
                valoresExtra);
        }

        public void EnviarBienvenida(BEUsuario Objeto)
        {
            Enviar(
                Objeto,
                "bienvenida",
                "Tu cuenta de WorkLingua ya está activa",
                "¡Tu cuenta está activa!",
                "Ingresar a WorkLingua",
                ConstruirUrl("login", null),
                null);
        }

        public void EnviarRecuperoClave(BEUsuario Objeto, Guid token)
        {
            Dictionary<string, string> valoresExtra = new Dictionary<string, string>();
            valoresExtra.Add("HorasVigencia", Configuracion.HorasVigenciaRecupero.ToString());

            Enviar(
                Objeto,
                "recupero-clave",
                "Restablecé tu clave de WorkLingua",
                "Restablecer tu clave",
                "Definir una clave nueva",
                ConstruirUrl("restablecer-clave", token),
                valoresExtra);
        }

        public void EnviarInvitacion(BEUsuario Objeto, Guid token)
        {
            Dictionary<string, string> valoresExtra = new Dictionary<string, string>();
            valoresExtra.Add("HorasVigencia", Configuracion.HorasVigenciaInvitacion.ToString());

            Enviar(
                Objeto,
                "invitacion",
                "Te invitaron a WorkLingua",
                "Activá tu cuenta de WorkLingua",
                "Definir mi clave",
                ConstruirUrl("completar-invitacion", token),
                valoresExtra);
        }

        public void EnviarClaveModificada(BEUsuario Objeto)
        {
            Dictionary<string, string> valoresExtra = new Dictionary<string, string>();
            valoresExtra.Add("FechaHora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            Enviar(
                Objeto,
                "clave-modificada",
                "Tu clave de WorkLingua fue modificada",
                "Tu clave fue modificada",
                "Ingresar a WorkLingua",
                ConstruirUrl("login", null),
                valoresExtra);
        }

        public void EnviarConsultaContacto(BEEnviarConsultaContacto Objeto)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Titulo", "Nueva consulta de contacto");
            valores.Add("Nombre", Objeto.Nombre);
            valores.Add("EmailVisitante", Objeto.Email);
            valores.Add("Mensaje", Objeto.Mensaje);
            valores.Add("TextoBoton", "Responder a " + Objeto.Nombre);
            valores.Add("UrlBoton", "mailto:" + Objeto.Email);
            valores.Add("TextoEnlaceAlternativo", TextoEnlaceAlternativo);
            valores.Add("NotaPie", NotaPieContacto);

            string html = oServicioPlantilla.Renderizar("consulta-contacto", valores);

            oServicioEmail.Enviar(
                Configuracion.ContactoCorreoDestino, "[Contacto] " + Objeto.Asunto, html, Objeto.Email);
        }

        private void Enviar(
            BEUsuario Objeto,
            string plantilla,
            string asunto,
            string titulo,
            string textoBoton,
            string urlBoton,
            Dictionary<string, string> valoresExtra)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Nombre", Objeto.Nombre);
            valores.Add("Titulo", titulo);
            valores.Add("TextoBoton", textoBoton);
            valores.Add("UrlBoton", urlBoton);
            valores.Add("TextoEnlaceAlternativo", TextoEnlaceAlternativo);
            valores.Add("NotaPie", NotaPieAutomatica);

            if (valoresExtra != null)
            {
                foreach (KeyValuePair<string, string> par in valoresExtra)
                {
                    valores[par.Key] = par.Value;
                }
            }

            string html = oServicioPlantilla.Renderizar(plantilla, valores);

            oServicioEmail.Enviar(Objeto.Email, asunto, html);
        }

        private string ConstruirUrl(string ruta, Guid? token)
        {
            string baseUrl = Configuracion.UrlBaseAplicacion.TrimEnd('/');
            string url = baseUrl + "/" + ruta;

            return token.HasValue ? url + "?token=" + token.Value : url;
        }
    }
}
