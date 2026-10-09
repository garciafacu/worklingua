using System.Collections.Generic;
using System.Globalization;
using System.Text;
using worklingua.Server.BE;

namespace worklingua.Server.Services
{
    public class ServicioEmailNewsletter
    {
        private const int LongitudMaximaResumen = 300;

        ServicioEmail oServicioEmail;
        ServicioPlantillaEmail oServicioPlantilla;

        public ServicioEmailNewsletter()
        {
            oServicioEmail = new ServicioEmail();
            oServicioPlantilla = new ServicioPlantillaEmail();
        }

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
}
