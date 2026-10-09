namespace worklingua.Server.BE
{
    public class BETicket
    {
        #region Propiedades
        public int TicketId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public int SuscripcionId { get; set; }
        public int? CursoId { get; set; }
        public string Asunto { get; set; }
        public string Estado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime FechaUltimoMovimiento { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int? IdiomaId { get; set; }
        #endregion

        public BETicket()
        {

        }

        public BETicket(
            int ticketId,
            int empresaId,
            int usuarioId,
            int suscripcionId,
            int? cursoId,
            string asunto,
            string estado,
            DateTime fechaAlta,
            DateTime fechaUltimoMovimiento,
            DateTime? fechaCierre,
            int? idiomaId)
        {
            this.TicketId = ticketId;
            this.EmpresaId = empresaId;
            this.UsuarioId = usuarioId;
            this.SuscripcionId = suscripcionId;
            this.CursoId = cursoId;
            this.Asunto = asunto;
            this.Estado = estado;
            this.FechaAlta = fechaAlta;
            this.FechaUltimoMovimiento = fechaUltimoMovimiento;
            this.FechaCierre = fechaCierre;
            this.IdiomaId = idiomaId;
        }
    }
}
