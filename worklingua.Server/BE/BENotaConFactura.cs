namespace worklingua.Server.BE
{
    public class BENotaConFactura
    {
        #region Propiedades
        public BENotaCreditoDebito Nota { get; set; }
        public string NumeroFactura { get; set; }
        #endregion

        public BENotaConFactura()
        {

        }

        public BENotaConFactura(BENotaCreditoDebito nota, string numeroFactura)
        {
            this.Nota = nota;
            this.NumeroFactura = numeroFactura;
        }
    }
}
