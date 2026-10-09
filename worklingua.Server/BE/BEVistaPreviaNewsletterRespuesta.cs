namespace worklingua.Server.BE
{
    public class BEVistaPreviaNewsletterRespuesta
    {
        #region Propiedades
        public string Html { get; set; }
        public int CantidadDestinatarios { get; set; }
        #endregion

        public BEVistaPreviaNewsletterRespuesta()
        {

        }

        public BEVistaPreviaNewsletterRespuesta(string html, int cantidadDestinatarios)
        {
            this.Html = html;
            this.CantidadDestinatarios = cantidadDestinatarios;
        }
    }
}
