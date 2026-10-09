namespace worklingua.Server.BE
{
    public class BEPaginaBuscable
    {
        #region Propiedades
        public string Ruta { get; set; }
        public string ClaveTitulo { get; set; }
        public string ClaveDescripcion { get; set; }
        public string ClaveSeccion { get; set; }
        public string ClavePalabrasClave { get; set; }
        public string Area { get; set; }
        public bool RequiereSesion { get; set; }
        public string Permiso { get; set; }
        public int Orden { get; set; }
        #endregion

        public BEPaginaBuscable()
        {

        }

        public BEPaginaBuscable(
            string ruta,
            string claveTitulo,
            string claveDescripcion,
            string claveSeccion,
            string clavePalabrasClave,
            string area,
            bool requiereSesion,
            string permiso,
            int orden)
        {
            this.Ruta = ruta;
            this.ClaveTitulo = claveTitulo;
            this.ClaveDescripcion = claveDescripcion;
            this.ClaveSeccion = claveSeccion;
            this.ClavePalabrasClave = clavePalabrasClave;
            this.Area = area;
            this.RequiereSesion = requiereSesion;
            this.Permiso = permiso;
            this.Orden = orden;
        }
    }
}
