namespace worklingua.Server.BE
{
    public class BEFiltroTicket
    {
        #region Propiedades
        public string Estado { get; set; }
        public int? EmpresaId { get; set; }
        public int? UsuarioId { get; set; }
        public int? TicketId { get; set; }
        public bool Administracion { get; set; }
        #endregion

        public BEFiltroTicket()
        {

        }

        public BEFiltroTicket(string estado, int? empresaId, int? usuarioId, int? ticketId, bool administracion)
        {
            this.Estado = estado;
            this.EmpresaId = empresaId;
            this.UsuarioId = usuarioId;
            this.TicketId = ticketId;
            this.Administracion = administracion;
        }
    }
}
