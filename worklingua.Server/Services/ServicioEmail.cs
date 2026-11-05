using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using worklingua.Server.BE;

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

    public class ServicioPlantillaEmail
    {
        private const string PlantillaBase = "base";

        private static readonly HtmlEncoder Codificador =
            HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement);

        public string Renderizar(string nombrePlantilla, Dictionary<string, string> valores)
        {
            return Renderizar(nombrePlantilla, valores, new Dictionary<string, string>());
        }

        public string Renderizar(
            string nombrePlantilla,
            Dictionary<string, string> valores,
            Dictionary<string, string> valoresHtml)
        {
            Dictionary<string, string> codificados = Codificar(valores);

            foreach (KeyValuePair<string, string> par in valoresHtml)
            {
                codificados[par.Key] = par.Value == null ? string.Empty : par.Value;
            }

            string cuerpo = Sustituir(LeerPlantilla(nombrePlantilla), codificados);

            Dictionary<string, string> valoresLayout = new Dictionary<string, string>(codificados);
            valoresLayout["Cuerpo"] = cuerpo;
            valoresLayout["Anio"] = DateTime.Now.Year.ToString();

            return Sustituir(LeerPlantilla(PlantillaBase), valoresLayout);
        }

        public string RenderizarFragmento(string nombrePlantilla, Dictionary<string, string> valores)
        {
            return Sustituir(LeerPlantilla(nombrePlantilla), Codificar(valores));
        }

        private Dictionary<string, string> Codificar(Dictionary<string, string> valores)
        {
            Dictionary<string, string> codificados = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> par in valores)
            {
                codificados.Add(par.Key, Codificador.Encode(par.Value == null ? string.Empty : par.Value));
            }

            return codificados;
        }

        private string LeerPlantilla(string nombre)
        {
            string ruta = Path.Combine(
                Configuracion.RutaContenido, "Services", "Plantillas", nombre + ".html");

            return Regex.Replace(File.ReadAllText(ruta, Encoding.UTF8), @"<!--.*?-->\s*", string.Empty, RegexOptions.Singleline);
        }

        private string Sustituir(string plantilla, Dictionary<string, string> valores)
        {
            StringBuilder resultado = new StringBuilder(plantilla);

            foreach (KeyValuePair<string, string> par in valores)
            {
                resultado.Replace("{{" + par.Key + "}}", par.Value);
            }

            return Regex.Replace(resultado.ToString(), @"\{\{\s*\w+\s*\}\}", string.Empty);
        }
    }

    public class ServicioEmailCuenta
    {
        private const string NotaPieAutomatica =
            "Este es un mensaje automatico de WorkLingua. Por favor no respondas a esta direccion.";

        private const string NotaPieContacto =
            "Consulta enviada desde el formulario de contacto del sitio. " +
            "Podes responder este correo: la respuesta le llega directamente a quien escribio.";

        private const string TextoEnlaceAlternativo =
            "Si el boton no funciona, copiá y pegá este enlace en tu navegador:";

        ServicioEmail oServicioEmail = new ServicioEmail();
        ServicioPlantillaEmail oServicioPlantilla = new ServicioPlantillaEmail();

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

    public class ServicioEmailContratacion
    {
        ServicioEmail oServicioEmail = new ServicioEmail();
        ServicioPlantillaEmail oServicioPlantilla = new ServicioPlantillaEmail();

        public void EnviarAviso(
            string destinatario,
            string nombre,
            BEResultadoContratacion Objeto,
            BECultura oCulturaBE,
            Dictionary<string, string> textos)
        {
            string estado = Objeto.Suscripcion.Estado;
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Titulo", Texto(textos, "email.contratacion.titulo." + estado));
            valores.Add("Saludo", Texto(textos, "email.contratacion.saludo").Replace("{{nombre}}", nombre));
            valores.Add("Mensaje", Texto(textos, "email.contratacion.mensaje." + estado).Replace("{{plan}}", Objeto.Plan));
            valores.Add("TextoBoton", Texto(textos, "email.contratacion.boton"));
            valores.Add("UrlBoton", Configuracion.UrlBaseAplicacion.TrimEnd('/') + "/inicio/plan");
            valores.Add("TextoEnlaceAlternativo", Texto(textos, "email.comun.enlaceAlternativo"));
            valores.Add("NotaPie", Texto(textos, "email.contratacion.pie"));

            Dictionary<string, string> valoresHtml = new Dictionary<string, string>();
            valoresHtml.Add("Detalle", RenderizarDetalle(Objeto, oCulturaBE, textos));

            string html = oServicioPlantilla.Renderizar("contratacion", valores, valoresHtml);
            string asunto = Texto(textos, "email.contratacion.asunto." + estado).Replace("{{plan}}", Objeto.Plan);

            oServicioEmail.Enviar(destinatario, asunto, html);
        }

        private string RenderizarDetalle(
            BEResultadoContratacion Objeto,
            BECultura oCulturaBE,
            Dictionary<string, string> textos)
        {
            StringBuilder resultado = new StringBuilder();

            AgregarFila(resultado, Texto(textos, "email.contratacion.fila.plan"), Objeto.Plan);
            AgregarFila(
                resultado,
                Texto(textos, "email.contratacion.fila.estado"),
                Texto(textos, "comun.contratacion.estado." + Objeto.Suscripcion.Estado));
            AgregarFila(
                resultado,
                Texto(textos, "email.contratacion.fila.fecha"),
                FormatearFecha(DateTime.Now, oCulturaBE));

            if (Objeto.Suscripcion.FechaFin.HasValue)
            {
                AgregarFila(
                    resultado,
                    Texto(textos, "email.contratacion.fila.vigencia"),
                    FormatearFecha(Objeto.Suscripcion.FechaFin.Value.ToDateTime(TimeOnly.MinValue), oCulturaBE));
            }

            if (Objeto.Factura != null)
            {
                AgregarFila(
                    resultado,
                    Texto(textos, "email.contratacion.fila.factura"),
                    Objeto.Factura.NumeroFactura + " · " + FormatearImporte(Objeto.Factura.Importe, oCulturaBE));
            }

            foreach (BEPago oPagoBE in Objeto.Pagos)
            {
                string valor = FormatearImporte(oPagoBE.Importe ?? 0m, oCulturaBE);

                if (!string.IsNullOrWhiteSpace(oPagoBE.NumeroOperacion))
                {
                    valor = valor + " · " + oPagoBE.NumeroOperacion;
                }

                AgregarFila(resultado, Texto(textos, "comun.contratacion.medio." + oPagoBE.MedioPago), valor);
            }

            foreach (BENotaCreditoDebito oNotaBE in Objeto.Notas)
            {
                AgregarFila(
                    resultado,
                    Texto(textos, "comun.contratacion.nota." + oNotaBE.Tipo) + " " + oNotaBE.Numero,
                    FormatearImporte(oNotaBE.Importe, oCulturaBE));
            }

            if (!string.IsNullOrWhiteSpace(Objeto.Motivo))
            {
                AgregarFila(resultado, Texto(textos, "email.contratacion.fila.motivo"), Objeto.Motivo);
            }

            return resultado.ToString();
        }

        private void AgregarFila(StringBuilder resultado, string etiqueta, string valor)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Etiqueta", etiqueta);
            valores.Add("Valor", valor);

            resultado.Append(oServicioPlantilla.RenderizarFragmento("contratacion-fila", valores));
        }

        private string FormatearImporte(decimal importe, BECultura oCulturaBE)
        {
            decimal convertido = Math.Round(importe * oCulturaBE.TasaConversion, 2, MidpointRounding.AwayFromZero);
            string numero = convertido.ToString("#,0.00", CultureInfo.InvariantCulture)
                .Replace(",", "")
                .Replace(".", oCulturaBE.SeparadorDecimal)
                .Replace("", oCulturaBE.SeparadorMiles);

            return oCulturaBE.SimboloMoneda + " " + numero + " " + oCulturaBE.Moneda;
        }

        private string FormatearFecha(DateTime fecha, BECultura oCulturaBE)
        {
            return fecha.ToString(oCulturaBE.FormatoFecha, CultureInfo.InvariantCulture);
        }

        private string Texto(Dictionary<string, string> textos, string clave)
        {
            string valor;

            return textos.TryGetValue(clave, out valor) && !string.IsNullOrWhiteSpace(valor) ? valor : clave;
        }
    }

    public class ServicioEmailNewsletter
    {
        private const int LongitudMaximaResumen = 300;

        ServicioEmail oServicioEmail = new ServicioEmail();
        ServicioPlantillaEmail oServicioPlantilla = new ServicioPlantillaEmail();

        public void EnviarConfirmacion(BESuscriptorNewsletter Objeto, Dictionary<string, string> textos)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Titulo", Texto(textos, "email.newsletterConfirmacion.titulo"));
            valores.Add("Mensaje", Texto(textos, "email.newsletterConfirmacion.mensaje"));
            valores.Add(
                "Vigencia",
                Texto(textos, "email.newsletterConfirmacion.vigencia")
                    .Replace("{{horas}}", Configuracion.HorasVigenciaConfirmacion.ToString()));
            valores.Add("TextoBoton", Texto(textos, "email.newsletterConfirmacion.boton"));
            valores.Add("UrlBoton", ConstruirUrl("newsletter/confirmar?token=" + Objeto.Token));
            valores.Add("TextoEnlaceAlternativo", Texto(textos, "email.comun.enlaceAlternativo"));
            valores.Add("NotaPie", Texto(textos, "email.newsletterConfirmacion.pie"));

            string html = oServicioPlantilla.Renderizar("confirmacion-newsletter", valores);

            oServicioEmail.Enviar(Objeto.Email, Texto(textos, "email.newsletterConfirmacion.asunto"), html);
        }

        public void EnviarNewsletter(
            BESuscriptorNewsletter Objeto,
            string asunto,
            List<BENoticia> ListaNoticiaBE,
            Dictionary<string, string> textos,
            string formatoFecha)
        {
            string html = RenderizarNewsletter(Objeto.Token, ListaNoticiaBE, textos, formatoFecha);

            oServicioEmail.Enviar(Objeto.Email, asunto, html);
        }

        public string RenderizarNewsletter(
            Guid tokenBaja,
            List<BENoticia> ListaNoticiaBE,
            Dictionary<string, string> textos,
            string formatoFecha)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Titulo", Texto(textos, "email.newsletter.titulo"));
            valores.Add("Introduccion", Texto(textos, "email.newsletter.introduccion"));
            valores.Add("TextoBoton", Texto(textos, "email.newsletter.botonTodas"));
            valores.Add("UrlBoton", ConstruirUrl("novedades"));
            valores.Add("TextoEnlaceAlternativo", Texto(textos, "email.comun.enlaceAlternativo"));
            valores.Add("NotaPie", Texto(textos, "email.newsletter.pie"));

            Dictionary<string, string> valoresHtml = new Dictionary<string, string>();

            valoresHtml.Add("Noticias", RenderizarNoticias(ListaNoticiaBE, textos, formatoFecha));
            valoresHtml.Add("PieExtra", RenderizarPie(tokenBaja, textos));

            return oServicioPlantilla.Renderizar("newsletter", valores, valoresHtml);
        }

        private string RenderizarNoticias(
            List<BENoticia> ListaNoticiaBE,
            Dictionary<string, string> textos,
            string formatoFecha)
        {
            StringBuilder resultado = new StringBuilder();

            foreach (BENoticia oNoticiaBE in ListaNoticiaBE)
            {
                Dictionary<string, string> valores = new Dictionary<string, string>();

                valores.Add("Fecha", oNoticiaBE.FechaPublicacion.ToString(formatoFecha, CultureInfo.InvariantCulture));
                valores.Add("TituloNoticia", oNoticiaBE.Titulo);
                valores.Add("Resumen", Resumir(oNoticiaBE));
                valores.Add("UrlNoticia", ConstruirUrl("novedades/" + oNoticiaBE.NoticiaId));
                valores.Add("TextoLeerMas", Texto(textos, "email.newsletter.leerMas"));

                resultado.Append(oServicioPlantilla.RenderizarFragmento("newsletter-noticia", valores));
            }

            return resultado.ToString();
        }

        private string RenderizarPie(Guid tokenBaja, Dictionary<string, string> textos)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("UrlBaja", ConstruirUrl("newsletter/baja?token=" + tokenBaja));
            valores.Add("TextoBaja", Texto(textos, "email.newsletter.baja"));

            return oServicioPlantilla.RenderizarFragmento("newsletter-pie", valores);
        }

        private string Resumir(BENoticia oNoticiaBE)
        {
            if (!string.IsNullOrWhiteSpace(oNoticiaBE.Resumen))
            {
                return oNoticiaBE.Resumen;
            }

            string contenido = oNoticiaBE.Contenido == null ? string.Empty : oNoticiaBE.Contenido.Trim();

            return contenido.Length <= LongitudMaximaResumen
                ? contenido
                : contenido.Substring(0, LongitudMaximaResumen).TrimEnd() + "...";
        }

        private string Texto(Dictionary<string, string> textos, string clave)
        {
            string valor;

            return textos.TryGetValue(clave, out valor) && !string.IsNullOrWhiteSpace(valor) ? valor : clave;
        }

        private string ConstruirUrl(string ruta)
        {
            return Configuracion.UrlBaseAplicacion.TrimEnd('/') + "/" + ruta;
        }
    }

    public class ServicioEmailSoporte
    {
        ServicioEmail oServicioEmail = new ServicioEmail();
        ServicioPlantillaEmail oServicioPlantilla = new ServicioPlantillaEmail();

        public void EnviarAviso(
            string destinatario,
            BETicketConDetalle Objeto,
            string evento,
            string respuesta,
            Dictionary<string, string> textos)
        {
            string numero = Objeto.Ticket.TicketId.ToString();
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Titulo", Texto(textos, "email.ticket.titulo." + evento));
            valores.Add("Saludo", Texto(textos, "email.ticket.saludo").Replace("{{nombre}}", Objeto.AutorNombre));
            valores.Add(
                "Mensaje",
                Texto(textos, "email.ticket.mensaje." + evento)
                    .Replace("{{numero}}", numero)
                    .Replace("{{asunto}}", Objeto.Ticket.Asunto));
            valores.Add("EtiquetaRespuesta", string.IsNullOrWhiteSpace(respuesta) ? string.Empty : Texto(textos, "email.ticket.respuesta"));
            valores.Add("Respuesta", respuesta ?? string.Empty);
            valores.Add("TextoBoton", Texto(textos, "email.ticket.boton"));
            valores.Add(
                "UrlBoton",
                Configuracion.UrlBaseAplicacion.TrimEnd('/') + "/inicio/soporte?ticket=" + numero);
            valores.Add("TextoEnlaceAlternativo", Texto(textos, "email.comun.enlaceAlternativo"));
            valores.Add("NotaPie", Texto(textos, "email.ticket.pie"));

            Dictionary<string, string> valoresHtml = new Dictionary<string, string>();
            valoresHtml.Add("Detalle", RenderizarDetalle(Objeto, textos));

            string html = oServicioPlantilla.Renderizar("ticket", valores, valoresHtml);
            string asunto = Texto(textos, "email.ticket.asunto." + evento)
                .Replace("{{numero}}", numero)
                .Replace("{{asunto}}", Objeto.Ticket.Asunto);

            oServicioEmail.Enviar(destinatario, asunto, html);
        }

        private string RenderizarDetalle(BETicketConDetalle Objeto, Dictionary<string, string> textos)
        {
            StringBuilder resultado = new StringBuilder();

            AgregarFila(resultado, Texto(textos, "email.ticket.fila.numero"), "#" + Objeto.Ticket.TicketId);
            AgregarFila(resultado, Texto(textos, "email.ticket.fila.asunto"), Objeto.Ticket.Asunto);
            AgregarFila(
                resultado,
                Texto(textos, "email.ticket.fila.estado"),
                Texto(textos, "comun.ticket.estado." + Objeto.Ticket.Estado));
            if (!string.IsNullOrWhiteSpace(Objeto.Plan))
            {
                AgregarFila(resultado, Texto(textos, "email.ticket.fila.plan"), Objeto.Plan);
            }

            if (!string.IsNullOrWhiteSpace(Objeto.Curso))
            {
                AgregarFila(resultado, Texto(textos, "email.ticket.fila.curso"), Objeto.Curso);
            }

            return resultado.ToString();
        }

        private void AgregarFila(StringBuilder resultado, string etiqueta, string valor)
        {
            Dictionary<string, string> valores = new Dictionary<string, string>();

            valores.Add("Etiqueta", etiqueta);
            valores.Add("Valor", valor);

            resultado.Append(oServicioPlantilla.RenderizarFragmento("contratacion-fila", valores));
        }

        private string Texto(Dictionary<string, string> textos, string clave)
        {
            string valor;

            return textos.TryGetValue(clave, out valor) && !string.IsNullOrWhiteSpace(valor) ? valor : clave;
        }
    }
}
