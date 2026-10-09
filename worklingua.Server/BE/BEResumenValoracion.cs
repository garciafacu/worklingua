namespace worklingua.Server.BE
{
    public class BEResumenValoracion
    {
        #region Propiedades
        public int PlanId { get; set; }
        public decimal Promedio { get; set; }
        public int Cantidad { get; set; }
        #endregion

        public BEResumenValoracion()
        {

        }

        public BEResumenValoracion(int planId, decimal promedio, int cantidad)
        {
            this.PlanId = planId;
            this.Promedio = promedio;
            this.Cantidad = cantidad;
        }
    }
}
