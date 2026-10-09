namespace worklingua.Server.BE
{
    public class BECotizacionContratacion
    {
        #region Propiedades
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string PlanActual { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public string SimboloMoneda { get; set; }
        public decimal TasaConversion { get; set; }
        public decimal CreditoCambioPlan { get; set; }
        public decimal SaldoNotasCredito { get; set; }
        public decimal NotaCreditoUtilizable { get; set; }
        public decimal SaldoCuentaCorriente { get; set; }
        public decimal LimiteCuentaCorriente { get; set; }
        public decimal DisponibleCuentaCorriente { get; set; }
        public decimal PorcentajeRecargoCuentaCorriente { get; set; }
        #endregion

        public BECotizacionContratacion()
        {

        }

        public BECotizacionContratacion(
            int planId,
            string plan,
            string planActual,
            decimal importe,
            string moneda,
            string simboloMoneda,
            decimal tasaConversion,
            decimal creditoCambioPlan,
            decimal saldoNotasCredito,
            decimal notaCreditoUtilizable,
            decimal saldoCuentaCorriente,
            decimal limiteCuentaCorriente,
            decimal disponibleCuentaCorriente,
            decimal porcentajeRecargoCuentaCorriente)
        {
            this.PlanId = planId;
            this.Plan = plan;
            this.PlanActual = planActual;
            this.Importe = importe;
            this.Moneda = moneda;
            this.SimboloMoneda = simboloMoneda;
            this.TasaConversion = tasaConversion;
            this.CreditoCambioPlan = creditoCambioPlan;
            this.SaldoNotasCredito = saldoNotasCredito;
            this.NotaCreditoUtilizable = notaCreditoUtilizable;
            this.SaldoCuentaCorriente = saldoCuentaCorriente;
            this.LimiteCuentaCorriente = limiteCuentaCorriente;
            this.DisponibleCuentaCorriente = disponibleCuentaCorriente;
            this.PorcentajeRecargoCuentaCorriente = porcentajeRecargoCuentaCorriente;
        }
    }
}
