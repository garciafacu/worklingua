namespace worklingua.Server.BE
{
    public class BEOferta
    {
        #region Propiedades
        public int OfertaId { get; set; }
        public int EmpresaId { get; set; }
        public int? PlanId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateOnly FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public bool Activo { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaAlta { get; set; }
        #endregion

        public BEOferta()
        {

        }

        public BEOferta(
            int ofertaId,
            int empresaId,
            int? planId,
            string titulo,
            string descripcion,
            DateOnly fechaDesde,
            DateOnly? fechaHasta,
            bool activo,
            int usuarioId,
            DateTime fechaAlta)
        {
            this.OfertaId = ofertaId;
            this.EmpresaId = empresaId;
            this.PlanId = planId;
            this.Titulo = titulo;
            this.Descripcion = descripcion;
            this.FechaDesde = fechaDesde;
            this.FechaHasta = fechaHasta;
            this.Activo = activo;
            this.UsuarioId = usuarioId;
            this.FechaAlta = fechaAlta;
        }
    }
}
