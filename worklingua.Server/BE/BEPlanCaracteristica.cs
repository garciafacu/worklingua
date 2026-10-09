namespace worklingua.Server.BE
{
    public class BEPlanCaracteristica
    {
        #region Propiedades
        public int PlanCaracteristicaId { get; set; }
        public int PlanId { get; set; }
        public int CaracteristicaId { get; set; }
        public bool Incluido { get; set; }
        public string Detalle { get; set; }
        #endregion

        public BEPlanCaracteristica()
        {

        }

        public BEPlanCaracteristica(
            int planCaracteristicaId,
            int planId,
            int caracteristicaId,
            bool incluido,
            string detalle)
        {
            this.PlanCaracteristicaId = planCaracteristicaId;
            this.PlanId = planId;
            this.CaracteristicaId = caracteristicaId;
            this.Incluido = incluido;
            this.Detalle = detalle;
        }
    }
}
