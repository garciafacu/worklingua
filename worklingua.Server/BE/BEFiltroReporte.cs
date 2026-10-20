namespace worklingua.Server.BE
{
    public class BEFiltroReporte
    {
        #region Propiedades
        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        public string Agrupacion { get; set; }

        public string Provincia { get; set; }
        #endregion

        public BEFiltroReporte()
        {

        }

        public BEFiltroReporte(DateOnly? desde, DateOnly? hasta, string agrupacion, string provincia)
        {
            this.Desde = desde;
            this.Hasta = hasta;
            this.Agrupacion = agrupacion;
            this.Provincia = provincia;
        }
    }
}
