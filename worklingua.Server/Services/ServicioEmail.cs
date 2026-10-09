using System.Net;
using System.Net.Mail;

namespace worklingua.Server.Services
{
    public class ServicioEmail
    {
        private const int TiempoLimiteMs = 20000;

        public void Enviar(string destinatario, string asunto, string cuerpoHtml, string replyTo = null)
        {
            if (string.IsNullOrWhiteSpace(destinatario))
            {
                throw new ArgumentException("El destinatario no puede estar vacío.", nameof(destinatario));
            }

            if (!Configuracion.SmtpConfigurado)
            {
                throw new InvalidOperationException(
                    "No se puede enviar el correo: falta configurar 'Email:Host' y/o 'Email:Remitente' " +
                    "en worklingua.Server/appsettings.Local.json.");
            }

            using (MailMessage mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(Configuracion.EmailRemitente, Configuracion.EmailNombreRemitente);
                mensaje.Subject = asunto;
                mensaje.Body = cuerpoHtml;
                mensaje.IsBodyHtml = true;
                mensaje.To.Add(destinatario);

                if (!string.IsNullOrWhiteSpace(replyTo))
                {
                    try
                    {
                        mensaje.ReplyToList.Add(new MailAddress(replyTo));
                    }
                    catch (FormatException)
                    {
                        ServicioLog.Advertencia("No se pudo usar '" + replyTo + "' como Reply-To: no es un correo válido.");
                    }
                }

                using (SmtpClient cliente = new SmtpClient(Configuracion.EmailHost, Configuracion.EmailPuerto))
                {
                    cliente.EnableSsl = Configuracion.EmailUsarSsl;
                    cliente.Timeout = TiempoLimiteMs;
                    cliente.UseDefaultCredentials = false;

                    if (Configuracion.UsaAutenticacionEmail)
                    {
                        cliente.Credentials = new NetworkCredential(Configuracion.EmailUsuario, Configuracion.EmailPassword);
                    }

                    cliente.Send(mensaje);
                }
            }

            ServicioLog.Informacion("Email '" + asunto + "' enviado a " + destinatario + ".");
        }
    }
}
