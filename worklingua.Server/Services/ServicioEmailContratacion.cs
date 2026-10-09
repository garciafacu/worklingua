using System.Collections.Generic;
using System.Globalization;
using System.Text;
using worklingua.Server.BE;

namespace worklingua.Server.Services
{
    public class ServicioEmailContratacion
    {
        ServicioEmail oServicioEmail;
        ServicioPlantillaEmail oServicioPlantilla;

        public ServicioEmailContratacion()
        {
            oServicioEmail = new ServicioEmail();
            oServicioPlantilla = new ServicioPlantillaEmail();
        }

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
}
