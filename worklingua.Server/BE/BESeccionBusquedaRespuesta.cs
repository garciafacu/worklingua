namespace worklingua.Server.BE
{
    public class BESeccionBusquedaRespuesta
    {
        #region Propiedades
        public string Clave { get; set; }
        public string Area { get; set; }
        #endregion

        public BESeccionBusquedaRespuesta()
        {

        }

        public BESeccionBusquedaRespuesta(string clave, string area)
        {
            this.Clave = clave;
            this.Area = area;
        }
    }
}
