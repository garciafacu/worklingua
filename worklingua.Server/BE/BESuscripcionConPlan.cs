namespace worklingua.Server.BE
{
    public class BESuscripcionConPlan
    {
        #region Propiedades
        public BESuscripcion Suscripcion { get; set; }
        public string Plan { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        #endregion

        public BESuscripcionConPlan()
        {

        }

        public BESuscripcionConPlan(
            BESuscripcion suscripcion, string plan, decimal precioMensual, int cantidadLicencias)
        {
            this.Suscripcion = suscripcion;
            this.Plan = plan;
            this.PrecioMensual = precioMensual;
            this.CantidadLicencias = cantidadLicencias;
        }
    }
}
