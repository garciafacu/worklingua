namespace worklingua.Server.BE
{
    public class BEPlanSuscripcion
    {
        #region Propiedades
        public int PlanId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioMensual { get; set; }
        public int CantidadLicencias { get; set; }
        public bool Activo { get; set; }
        public bool Destacado { get; set; }
        public bool Protegido { get; set; }
        #endregion

        public BEPlanSuscripcion()
        {

        }

        public BEPlanSuscripcion(
            int planId,
            string nombre,
            string descripcion,
            decimal precioMensual,
            int cantidadLicencias,
            bool activo,
            bool destacado,
            bool protegido)
        {
            this.PlanId = planId;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.PrecioMensual = precioMensual;
            this.CantidadLicencias = cantidadLicencias;
            this.Activo = activo;
            this.Destacado = destacado;
            this.Protegido = protegido;
        }
    }
}
