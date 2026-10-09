namespace worklingua.Server.BE
{
    public class BEMovimientoCuentaCorriente
    {
        #region Propiedades
        public int MovimientoId { get; set; }
        public int EmpresaId { get; set; }
        public int? SuscripcionId { get; set; }
        public int? FacturaId { get; set; }
        public int? NotaId { get; set; }
        public int? PagoId { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Tipo { get; set; }
        public string Concepto { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        #endregion

        public BEMovimientoCuentaCorriente()
        {

        }

        public BEMovimientoCuentaCorriente(
            int movimientoId,
            int empresaId,
            int? suscripcionId,
            int? facturaId,
            int? notaId,
            int? pagoId,
            DateTime fechaMovimiento,
            string tipo,
            string concepto,
            decimal debe,
            decimal haber)
        {
            this.MovimientoId = movimientoId;
            this.EmpresaId = empresaId;
            this.SuscripcionId = suscripcionId;
            this.FacturaId = facturaId;
            this.NotaId = notaId;
            this.PagoId = pagoId;
            this.FechaMovimiento = fechaMovimiento;
            this.Tipo = tipo;
            this.Concepto = concepto;
            this.Debe = debe;
            this.Haber = haber;
        }
    }
}
