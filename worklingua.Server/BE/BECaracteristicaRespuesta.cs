namespace worklingua.Server.BE
{
    public class BECaracteristicaRespuesta
    {
        #region Propiedades
        public int CaracteristicaId { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }
        #endregion

        public BECaracteristicaRespuesta()
        {

        }

        public BECaracteristicaRespuesta(int caracteristicaId, string nombre, int orden)
        {
            this.CaracteristicaId = caracteristicaId;
            this.Nombre = nombre;
            this.Orden = orden;
        }
    }
}
