namespace worklingua.Server.BE
{
    public class BEPago
    {
        #region Propiedades
        public int PagoId { get; set; }
        public int FacturaId { get; set; }
        public DateTime? FechaPago { get; set; }
        public string MedioPago { get; set; }
        public decimal? Importe { get; set; }
        public string NumeroOperacion { get; set; }
        public string Estado { get; set; }
        #endregion

        public BEPago()
        {

        }

        public BEPago(
            int pagoId,
            int facturaId,
            DateTime? fechaPago,
            string medioPago,
            decimal? importe,
            string numeroOperacion,
            string estado)
        {
            this.PagoId = pagoId;
            this.FacturaId = facturaId;
            this.FechaPago = fechaPago;
            this.MedioPago = medioPago;
            this.Importe = importe;
            this.NumeroOperacion = numeroOperacion;
            this.Estado = estado;
        }
    }
}
