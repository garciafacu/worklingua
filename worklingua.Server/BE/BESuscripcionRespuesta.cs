namespace worklingua.Server.BE
{
    public class BESuscripcionRespuesta
    {
        #region Propiedades
        public int SuscripcionId { get; set; }
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string Estado { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Cancelable { get; set; }
        #endregion

        public BESuscripcionRespuesta()
        {

        }

        public BESuscripcionRespuesta(
            int suscripcionId,
            int planId,
            string plan,
            string estado,
            DateOnly fechaInicio,
            DateOnly? fechaFin,
            decimal precioMensual,
            int cantidadLicencias,
            bool cancelable)
        {
            this.SuscripcionId = suscripcionId;
            this.PlanId = planId;
            this.Plan = plan;
            this.Estado = estado;
            this.FechaInicio = fechaInicio;
            this.FechaFin = fechaFin;
            this.PrecioMensual = precioMensual;
            this.CantidadLicencias = cantidadLicencias;
            this.Cancelable = cancelable;
        }
    }
}
