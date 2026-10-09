namespace worklingua.Server.BE
{
    public class BECulturaConIdioma
    {
        #region Propiedades
        public BECultura Cultura { get; set; }
        public string Idioma { get; set; }
        public string CodigoIdioma { get; set; }
        #endregion

        public BECulturaConIdioma()
        {

        }

        public BECulturaConIdioma(BECultura cultura, string idioma, string codigoIdioma)
        {
            this.Cultura = cultura;
            this.Idioma = idioma;
            this.CodigoIdioma = codigoIdioma;
        }
    }
}
