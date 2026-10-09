namespace worklingua.Server.BE
{
    public class BEProgreso
    {
        #region Propiedades
        public int ProgresoId { get; set; }
        public int UsuarioId { get; set; }
        public int ModuloId { get; set; }
        public decimal? PorcentajeAvance { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
        public string Estado { get; set; }
        #endregion

        public BEProgreso()
        {

        }

        public BEProgreso(
            int progresoId,
            int usuarioId,
            int moduloId,
            decimal? porcentajeAvance,
            DateTime? fechaInicio,
            DateTime? fechaFinalizacion,
            string estado)
        {
            this.ProgresoId = progresoId;
            this.UsuarioId = usuarioId;
            this.ModuloId = moduloId;
            this.PorcentajeAvance = porcentajeAvance;
            this.FechaInicio = fechaInicio;
            this.FechaFinalizacion = fechaFinalizacion;
            this.Estado = estado;
        }
    }
}
