namespace worklingua.Server.BE
{
    public class BENotaCreditoDebito
    {
        #region Propiedades
        public int NotaId { get; set; }
        public int FacturaId { get; set; }
        public string Tipo { get; set; }
        public string Numero { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Importe { get; set; }
        public decimal SaldoDisponible { get; set; }
        public string Motivo { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }
        public string Estado { get; set; }
        #endregion

        public BENotaCreditoDebito()
        {

        }

        public BENotaCreditoDebito(
            int notaId,
            int facturaId,
            string tipo,
            string numero,
            DateTime fechaEmision,
            decimal importe,
            decimal saldoDisponible,
            string motivo,
            string moneda,
            decimal tasaConversion,
            string estado)
        {
            this.NotaId = notaId;
            this.FacturaId = facturaId;
            this.Tipo = tipo;
            this.Numero = numero;
            this.FechaEmision = fechaEmision;
            this.Importe = importe;
            this.SaldoDisponible = saldoDisponible;
            this.Motivo = motivo;
            this.Moneda = moneda;
            this.TasaConversion = tasaConversion;
            this.Estado = estado;
        }
    }
}
