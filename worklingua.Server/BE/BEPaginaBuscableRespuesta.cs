namespace worklingua.Server.BE
{
    public class BEPaginaBuscableRespuesta
    {
        #region Propiedades
        public string Ruta { get; set; }
        public string ClaveTitulo { get; set; }
        public string ClaveDescripcion { get; set; }
        public string ClaveSeccion { get; set; }
        public string Area { get; set; }
        #endregion

        public BEPaginaBuscableRespuesta()
        {

        }

        public BEPaginaBuscableRespuesta(
            string ruta,
            string claveTitulo,
            string claveDescripcion,
            string claveSeccion,
            string area)
        {
            this.Ruta = ruta;
            this.ClaveTitulo = claveTitulo;
            this.ClaveDescripcion = claveDescripcion;
            this.ClaveSeccion = claveSeccion;
            this.Area = area;
        }
    }
}
