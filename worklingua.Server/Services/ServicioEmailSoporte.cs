using System.Collections.Generic;
using System.Text;
using worklingua.Server.BE;

namespace worklingua.Server.Services
{
    public class ServicioEmailSoporte
    {
        ServicioEmail oServicioEmail;
        ServicioPlantillaEmail oServicioPlantilla;

        public ServicioEmailSoporte()
        {
            oServicioEmail = new ServicioEmail();
            oServicioPlantilla = new ServicioPlantillaEmail();
        }

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
            AgregarFila(resultado, Texto(textos, "email.ticket.fila.plan"), Objeto.Plan);

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
