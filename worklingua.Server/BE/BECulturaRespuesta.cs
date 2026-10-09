namespace worklingua.Server.BE
{
    public class BECulturaRespuesta
    {
        #region Propiedades
        public int CulturaId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string CodigoIdioma { get; set; }
        public string Moneda { get; set; }
        public string SimboloMoneda { get; set; }
        public string FormatoFecha { get; set; }
        public string SeparadorDecimal { get; set; }
        public string SeparadorMiles { get; set; }
        public decimal TasaConversion { get; set; }
        public bool EsPredeterminada { get; set; }
        #endregion

        public BECulturaRespuesta()
        {

        }

        public BECulturaRespuesta(
            int culturaId,
            string codigo,
            string nombre,
            string codigoIdioma,
            string moneda,
            string simboloMoneda,
            string formatoFecha,
            string separadorDecimal,
            string separadorMiles,
            decimal tasaConversion,
            bool esPredeterminada)
        {
            this.CulturaId = culturaId;
            this.Codigo = codigo;
            this.Nombre = nombre;
            this.CodigoIdioma = codigoIdioma;
            this.Moneda = moneda;
            this.SimboloMoneda = simboloMoneda;
            this.FormatoFecha = formatoFecha;
            this.SeparadorDecimal = separadorDecimal;
            this.SeparadorMiles = separadorMiles;
            this.TasaConversion = tasaConversion;
            this.EsPredeterminada = esPredeterminada;
        }
    }
}
