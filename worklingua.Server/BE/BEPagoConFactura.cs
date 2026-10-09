namespace worklingua.Server.BE
{
    public class BEPagoConFactura
    {
        #region Propiedades
        public BEPago Pago { get; set; }
        public string NumeroFactura { get; set; }
        #endregion

        public BEPagoConFactura()
        {

        }

        public BEPagoConFactura(BEPago pago, string numeroFactura)
        {
            this.Pago = pago;
            this.NumeroFactura = numeroFactura;
        }
    }
}
