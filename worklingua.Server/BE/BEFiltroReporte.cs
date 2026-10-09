namespace worklingua.Server.BE
{
    public class BEFiltroReporte
    {
        #region Propiedades
        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        /// <summary>DIA, SEMANA, MES o ANIO. Vacío se toma como MES.</summary>
        public string Agrupacion { get; set; }

        /// <summary>Provincia de la empresa cliente. Vacío son todas las zonas.</summary>
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
