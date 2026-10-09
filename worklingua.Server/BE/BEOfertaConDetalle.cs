namespace worklingua.Server.BE
{
    public class BEOfertaConDetalle
    {
        #region Propiedades
        public BEOferta Oferta { get; set; }
        public string Empresa { get; set; }
        public string Plan { get; set; }
        public bool PlanActivo { get; set; }
        #endregion

        public BEOfertaConDetalle()
        {

        }

        public BEOfertaConDetalle(BEOferta oferta, string empresa, string plan, bool planActivo)
        {
            this.Oferta = oferta;
            this.Empresa = empresa;
            this.Plan = plan;
            this.PlanActivo = planActivo;
        }
    }
}
