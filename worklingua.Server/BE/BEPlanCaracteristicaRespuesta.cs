namespace worklingua.Server.BE
{
    public class BEPlanCaracteristicaRespuesta
    {
        #region Propiedades
        public int CaracteristicaId { get; set; }
        public bool Incluido { get; set; }
        public string Detalle { get; set; }
        #endregion

        public BEPlanCaracteristicaRespuesta()
        {

        }

        public BEPlanCaracteristicaRespuesta(int caracteristicaId, bool incluido, string detalle)
        {
            this.CaracteristicaId = caracteristicaId;
            this.Incluido = incluido;
            this.Detalle = detalle;
        }
    }
}
