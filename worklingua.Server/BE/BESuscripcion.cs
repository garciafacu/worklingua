namespace worklingua.Server.BE
{
    public class BESuscripcion
    {
        #region Propiedades
        public int SuscripcionId { get; set; }
        public int EmpresaId { get; set; }
        public int PlanId { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public string Estado { get; set; }
        public bool? RenovacionAutomatica { get; set; }
        #endregion

        public BESuscripcion()
        {

        }

        public BESuscripcion(
            int suscripcionId,
            int empresaId,
            int planId,
            DateOnly fechaInicio,
            DateOnly? fechaFin,
            string estado,
            bool? renovacionAutomatica)
        {
            this.SuscripcionId = suscripcionId;
            this.EmpresaId = empresaId;
            this.PlanId = planId;
            this.FechaInicio = fechaInicio;
            this.FechaFin = fechaFin;
            this.Estado = estado;
            this.RenovacionAutomatica = renovacionAutomatica;
        }
    }
}
