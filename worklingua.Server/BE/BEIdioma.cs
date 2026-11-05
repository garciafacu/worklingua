namespace worklingua.Server.BE
{
    public class BEIdioma
    {
        public int IdiomaId { get; set; }
        public string Nombre { get; set; }
        public string CodigoISO { get; set; }
        public bool? Activo { get; set; }
    }

    public class BEIdiomaAdminRespuesta : BEIdiomaRespuesta
    {
        public bool Activo { get; set; }
    }

    public class BEIdiomaRespuesta
    {
        public int IdiomaId { get; set; }
        public string Nombre { get; set; }
        public string CodigoISO { get; set; }
    }

    public class BEGuardarIdioma
    {
        public string Nombre { get; set; }
        public string CodigoISO { get; set; }
    }

    public class BECultura
    {
        public int CulturaId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int IdiomaId { get; set; }
        public string Moneda { get; set; }
        public string SimboloMoneda { get; set; }
        public string FormatoFecha { get; set; }
        public string SeparadorDecimal { get; set; }
        public string SeparadorMiles { get; set; }
        public decimal TasaConversion { get; set; }
        public bool EsPredeterminada { get; set; }
        public bool Activo { get; set; }
    }

    public class BECulturaAdminRespuesta : BECulturaRespuesta
    {
        public int IdiomaId { get; set; }
        public string Idioma { get; set; }
        public bool Activo { get; set; }
    }

    public class BECulturaConIdioma
    {
        public BECultura Cultura { get; set; }
        public string Idioma { get; set; }
        public string CodigoIdioma { get; set; }

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

    public class BECulturaRespuesta
    {
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
    }

    public class BEGuardarCultura
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int IdiomaId { get; set; }
        public string Moneda { get; set; }
        public string SimboloMoneda { get; set; }
        public string FormatoFecha { get; set; }
        public string SeparadorDecimal { get; set; }
        public string SeparadorMiles { get; set; }
        public decimal TasaConversion { get; set; }
        public bool EsPredeterminada { get; set; }
    }

    public class BETraduccion
    {
        public int TraduccionId { get; set; }
        public int IdiomaId { get; set; }
        public string Clave { get; set; }
        public string Texto { get; set; }
        public bool Activo { get; set; }
    }

    public class BETraduccionAdministracion
    {
        public string Clave { get; set; }
        public string TextoEspanol { get; set; }
        public string Texto { get; set; }
        public bool Pendiente { get; set; }
    }

    public class BETraduccionItem
    {
        public string Clave { get; set; }
        public string Texto { get; set; }
    }

    public class BEGuardarTraducciones
    {
        public int IdiomaId { get; set; }
        public List<BETraduccionItem> Traducciones { get; set; }
    }

    public class BEFiltroTraduccion
    {
        public int IdiomaId { get; set; }
        public string Clave { get; set; }
        public string Texto { get; set; }
        public bool SoloPendientes { get; set; }

        public BEFiltroTraduccion()
        {
        }

        public BEFiltroTraduccion(int idiomaId, string clave, string texto, bool soloPendientes)
        {
            this.IdiomaId = idiomaId;
            this.Clave = clave;
            this.Texto = texto;
            this.SoloPendientes = soloPendientes;
        }
    }
}
