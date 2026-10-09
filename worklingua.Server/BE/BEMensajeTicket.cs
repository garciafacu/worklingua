namespace worklingua.Server.BE
{
    public class BEMensajeTicket
    {
        #region Propiedades
        public int MensajeId { get; set; }
        public int TicketId { get; set; }
        public int UsuarioId { get; set; }
        public bool EsOperador { get; set; }
        public string Texto { get; set; }
        public DateTime FechaEnvio { get; set; }
        #endregion

        public BEMensajeTicket()
        {

        }

        public BEMensajeTicket(
            int mensajeId,
            int ticketId,
            int usuarioId,
            bool esOperador,
            string texto,
            DateTime fechaEnvio)
        {
            this.MensajeId = mensajeId;
            this.TicketId = ticketId;
            this.UsuarioId = usuarioId;
            this.EsOperador = esOperador;
            this.Texto = texto;
            this.FechaEnvio = fechaEnvio;
        }
    }
}
