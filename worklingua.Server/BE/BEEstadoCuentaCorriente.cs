using System.Collections.Generic;

namespace worklingua.Server.BE
{
    public class BEEstadoCuentaCorriente
    {
        #region Propiedades
        public decimal Saldo { get; set; }
        public decimal SaldoNotasCredito { get; set; }
        public decimal LimiteCuentaCorriente { get; set; }
        public List<BEMovimientoConComprobante> Movimientos { get; set; }
        public List<BEFacturaConPlan> Facturas { get; set; }
        public List<BEPagoConFactura> Pagos { get; set; }
        public List<BENotaConFactura> Notas { get; set; }
        #endregion

        public BEEstadoCuentaCorriente()
        {
            this.Movimientos = new List<BEMovimientoConComprobante>();
            this.Facturas = new List<BEFacturaConPlan>();
            this.Pagos = new List<BEPagoConFactura>();
            this.Notas = new List<BENotaConFactura>();
        }

        public BEEstadoCuentaCorriente(
            decimal saldo,
            decimal saldoNotasCredito,
            decimal limiteCuentaCorriente,
            List<BEMovimientoConComprobante> movimientos,
            List<BEFacturaConPlan> facturas,
            List<BEPagoConFactura> pagos,
            List<BENotaConFactura> notas)
        {
            this.Saldo = saldo;
            this.SaldoNotasCredito = saldoNotasCredito;
            this.LimiteCuentaCorriente = limiteCuentaCorriente;
            this.Movimientos = movimientos;
            this.Facturas = facturas;
            this.Pagos = pagos;
            this.Notas = notas;
        }
    }
}
