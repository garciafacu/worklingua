namespace worklingua.Server.BE
{
    public class BECaracteristica
    {
        #region Propiedades
        public int CaracteristicaId { get; set; }
        public string Nombre { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
        #endregion

        public BECaracteristica()
        {

        }

        public BECaracteristica(int caracteristicaId, string nombre, int orden, bool activo)
        {
            this.CaracteristicaId = caracteristicaId;
            this.Nombre = nombre;
            this.Orden = orden;
            this.Activo = activo;
        }
    }
}
