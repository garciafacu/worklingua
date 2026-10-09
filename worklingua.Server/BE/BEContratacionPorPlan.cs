namespace worklingua.Server.BE
{
    public class BEContratacionPorPlan
    {
        #region Propiedades
        public string Plan { get; set; }
        public int Cantidad { get; set; }
        #endregion

        public BEContratacionPorPlan()
        {

        }

        public BEContratacionPorPlan(string plan, int cantidad)
        {
            this.Plan = plan;
            this.Cantidad = cantidad;
        }
    }
}
