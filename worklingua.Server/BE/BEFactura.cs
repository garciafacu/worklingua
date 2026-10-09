namespace worklingua.Server.BE
{
    public class BEFactura
    {
        #region Propiedades
        public int FacturaId { get; set; }
        public int SuscripcionId { get; set; }
        public string NumeroFactura { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal Importe { get; set; }
        public string Estado { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }
        #endregion

        public BEFactura()
        {

        }

        public BEFactura(
            int facturaId,
            int suscripcionId,
            string numeroFactura,
            DateOnly fechaEmision,
            DateOnly fechaVencimiento,
            decimal importe,
            string estado,
            string moneda,
            decimal tasaConversion)
        {
            this.FacturaId = facturaId;
            this.SuscripcionId = suscripcionId;
            this.NumeroFactura = numeroFactura;
            this.FechaEmision = fechaEmision;
            this.FechaVencimiento = fechaVencimiento;
            this.Importe = importe;
            this.Estado = estado;
            this.Moneda = moneda;
            this.TasaConversion = tasaConversion;
        }
    }
}
