namespace worklingua.Server.BE
{
    public class BEMensajeTicketConAutor
    {
        #region Propiedades
        public BEMensajeTicket Mensaje { get; set; }
        public string Autor { get; set; }
        #endregion

        public BEMensajeTicketConAutor()
        {

        }

        public BEMensajeTicketConAutor(BEMensajeTicket mensaje, string autor)
        {
            this.Mensaje = mensaje;
            this.Autor = autor;
        }
    }
}
