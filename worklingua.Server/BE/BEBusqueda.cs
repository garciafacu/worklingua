namespace worklingua.Server.BE
{
    public class BEFiltroBusqueda
    {
        public string Texto { get; set; }
        public string Idioma { get; set; }
        public string Area { get; set; }
        public string Seccion { get; set; }
        public string OrdenarPor { get; set; }
        public bool? SoloTitulo { get; set; }
    }

    public class BEPaginaBuscable
    {
        public string Ruta { get; set; }
        public string ClaveTitulo { get; set; }
        public string ClaveDescripcion { get; set; }
        public string ClaveSeccion { get; set; }
        public string ClavePalabrasClave { get; set; }
        public string Area { get; set; }
        public bool RequiereSesion { get; set; }
        public string Permiso { get; set; }
        public int Orden { get; set; }

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

    public class BEPaginaBuscableRespuesta
    {
        public string Ruta { get; set; }
        public string ClaveTitulo { get; set; }
        public string ClaveDescripcion { get; set; }
        public string ClaveSeccion { get; set; }
        public string Area { get; set; }

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

    public class BESeccionBusquedaRespuesta
    {
        public string Clave { get; set; }
        public string Area { get; set; }

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
