namespace worklingua.Server.BE
{
    public class BEContratarPlan
    {
        public int PlanId { get; set; }
        public string CodigoCultura { get; set; }
        public decimal ImporteNotaCredito { get; set; }
        public decimal ImporteCuentaCorriente { get; set; }
        public BETarjeta Tarjeta { get; set; }
    }

    public class BEContratacionPorPlan
    {
        public string Plan { get; set; }
        public int Cantidad { get; set; }

        public BEContratacionPorPlan()
        {
        }

        public BEContratacionPorPlan(string plan, int cantidad)
        {
            this.Plan = plan;
            this.Cantidad = cantidad;
        }
    }

    public class BEContratacionRespuesta
    {
        public int SuscripcionId { get; set; }
        public int PlanId { get; set; }
        public string Plan { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }
        public string NumeroFactura { get; set; }
        public List<BEPago> Pagos { get; set; } = new List<BEPago>();
        public List<BENotaCreditoDebito> Notas { get; set; } = new List<BENotaCreditoDebito>();
    }

    public class BECotizacionContratacion
    {
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

    public class BECancelarContratacion
    {
        public int SuscripcionId { get; set; }
        public string CodigoCultura { get; set; }
    }

    public class BEResultadoContratacion
    {
        public BESuscripcion Suscripcion { get; set; }
        public string Plan { get; set; }
        public string Motivo { get; set; }
        public decimal Importe { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }
        public BEFactura Factura { get; set; }
        public List<BEPago> Pagos { get; set; } = new List<BEPago>();
        public List<BENotaCreditoDebito> Notas { get; set; } = new List<BENotaCreditoDebito>();
    }

    public class BETarjeta
    {
        public string Numero { get; set; }
        public string Titular { get; set; }
        public string Vencimiento { get; set; }
        public string CodigoSeguridad { get; set; }
    }

    public class BEResultadoValidacionTarjeta
    {
        public bool FormatoValido { get; set; }
        public bool Aprobada { get; set; }
        public string Marca { get; set; }
        public string UltimosDigitos { get; set; }
        public string CodigoAutorizacion { get; set; }
        public string Motivo { get; set; }

        public BEResultadoValidacionTarjeta()
        {
        }

        public BEResultadoValidacionTarjeta(
            bool formatoValido,
            bool aprobada,
            string marca,
            string ultimosDigitos,
            string codigoAutorizacion,
            string motivo)
        {
            this.FormatoValido = formatoValido;
            this.Aprobada = aprobada;
            this.Marca = marca;
            this.UltimosDigitos = ultimosDigitos;
            this.CodigoAutorizacion = codigoAutorizacion;
            this.Motivo = motivo;
        }
    }

    public class BEFactura
    {
        public int FacturaId { get; set; }
        public int SuscripcionId { get; set; }
        public string NumeroFactura { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public decimal Importe { get; set; }
        public string Estado { get; set; }
        public string Moneda { get; set; }
        public decimal TasaConversion { get; set; }

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

    public class BEFacturaConPlan
    {
        public BEFactura Factura { get; set; }
        public string Plan { get; set; }

        public BEFacturaConPlan()
        {
        }

        public BEFacturaConPlan(BEFactura factura, string plan)
        {
            this.Factura = factura;
            this.Plan = plan;
        }
    }

    public class BEPago
    {
        public int PagoId { get; set; }
        public int FacturaId { get; set; }
        public DateTime? FechaPago { get; set; }
        public string MedioPago { get; set; }
        public decimal? Importe { get; set; }
        public string NumeroOperacion { get; set; }
        public string Estado { get; set; }

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

    public class BEPagoConFactura
    {
        public BEPago Pago { get; set; }
        public string NumeroFactura { get; set; }

        public BEPagoConFactura()
        {
        }

        public BEPagoConFactura(BEPago pago, string numeroFactura)
        {
            this.Pago = pago;
            this.NumeroFactura = numeroFactura;
        }
    }

    public class BENotaCreditoDebito
    {
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

    public class BENotaConFactura
    {
        public BENotaCreditoDebito Nota { get; set; }
        public string NumeroFactura { get; set; }

        public BENotaConFactura()
        {
        }

        public BENotaConFactura(BENotaCreditoDebito nota, string numeroFactura)
        {
            this.Nota = nota;
            this.NumeroFactura = numeroFactura;
        }
    }

    public class BEMovimientoCuentaCorriente
    {
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

    public class BEMovimientoConComprobante
    {
        public BEMovimientoCuentaCorriente Movimiento { get; set; }
        public decimal SaldoAcumulado { get; set; }
        public string Comprobante { get; set; }
        public string Plan { get; set; }

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

    public class BEEstadoCuentaCorriente
    {
        public decimal Saldo { get; set; }
        public decimal SaldoNotasCredito { get; set; }
        public decimal LimiteCuentaCorriente { get; set; }
        public List<BEMovimientoConComprobante> Movimientos { get; set; }
        public List<BEFacturaConPlan> Facturas { get; set; }
        public List<BEPagoConFactura> Pagos { get; set; }
        public List<BENotaConFactura> Notas { get; set; }

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
