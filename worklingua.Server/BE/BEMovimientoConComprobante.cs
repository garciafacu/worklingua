namespace worklingua.Server.BE
{
    public class BEMovimientoConComprobante
    {
        #region Propiedades
        public BEMovimientoCuentaCorriente Movimiento { get; set; }
        public decimal SaldoAcumulado { get; set; }
        public string Comprobante { get; set; }
        public string Plan { get; set; }
        #endregion

        public BEMovimientoConComprobante()
        {

        }

        public BEMovimientoConComprobante(
            BEMovimientoCuentaCorriente movimiento, decimal saldoAcumulado, string comprobante, string plan)
        {
            this.Movimiento = movimiento;
            this.SaldoAcumulado = saldoAcumulado;
            this.Comprobante = comprobante;
            this.Plan = plan;
        }
    }
}
