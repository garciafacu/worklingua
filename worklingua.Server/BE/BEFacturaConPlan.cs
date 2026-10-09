namespace worklingua.Server.BE
{
    public class BEFacturaConPlan
    {
        #region Propiedades
        public BEFactura Factura { get; set; }
        public string Plan { get; set; }
        #endregion

        public BEFacturaConPlan()
        {

        }

        public BEFacturaConPlan(BEFactura factura, string plan)
        {
            this.Factura = factura;
            this.Plan = plan;
        }
    }
}
