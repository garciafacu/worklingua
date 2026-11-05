export type EstadoContratacion = 'PENDIENTE' | 'ACTIVA' | 'RECHAZADA' | 'CANCELADA' | 'FINALIZADA';

export interface CotizacionContratacionResponse {
    planId: number;
    plan: string;
    planActual: string | null;
    importe: number;
    moneda: string;
    simboloMoneda: string;
    tasaConversion: number;
    creditoCambioPlan: number;
    saldoNotasCredito: number;
    notaCreditoUtilizable: number;
    saldoCuentaCorriente: number;
    limiteCuentaCorriente: number;
    disponibleCuentaCorriente: number;
    porcentajeRecargoCuentaCorriente: number;
}

export interface TarjetaRequest {
    numero: string;
    titular: string;
    vencimiento: string;
    codigoSeguridad: string;
}

export interface ContratarPlanRequest {
    planId: number;
    codigoCultura: string;
    importeNotaCredito: number;
    importeCuentaCorriente: number;
    tarjeta: TarjetaRequest | null;
}

export interface PagoResponse {
    pagoId: number;
    facturaId: number;
    fechaPago: string | null;
    medioPago: 'TARJETA' | 'NOTA_CREDITO' | 'CUENTA_CORRIENTE';
    importe: number | null;
    numeroOperacion: string | null;
    estado: string | null;
}

export interface NotaCreditoDebitoResponse {
    notaId: number;
    facturaId: number;
    tipo: 'NC' | 'ND';
    numero: string;
    fechaEmision: string;
    importe: number;
    saldoDisponible: number;
    motivo: string;
    moneda: string;
    tasaConversion: number;
    estado: string;
}

export interface ContratacionResponse {
    suscripcionId: number;
    planId: number;
    plan: string;
    estado: EstadoContratacion;
    motivo: string | null;
    fechaInicio: string;
    fechaFin: string | null;
    importe: number;
    moneda: string | null;
    tasaConversion: number;
    numeroFactura: string | null;
    pagos: PagoResponse[];
    notas: NotaCreditoDebitoResponse[];
}

export interface EstadoCuentaCorrienteResponse {
    saldo: number;
    saldoNotasCredito: number;
    limiteCuentaCorriente: number;
    movimientos: MovimientoResponse[];
    facturas: FacturaConPlanResponse[];
    pagos: PagoConFacturaResponse[];
    notas: NotaConFacturaResponse[];
}

export type TipoMovimiento = 'FACTURA' | 'PAGO' | 'NC' | 'ND';

export interface MovimientoResponse {
    movimiento: {
        movimientoId: number;
        empresaId: number;
        suscripcionId: number | null;
        facturaId: number | null;
        notaId: number | null;
        pagoId: number | null;
        fechaMovimiento: string;
        tipo: TipoMovimiento;
        concepto: string;
        debe: number;
        haber: number;
    };
    saldoAcumulado: number;
    comprobante: string | null;
    plan: string | null;
}

export interface FacturaResponse {
    facturaId: number;
    suscripcionId: number;
    numeroFactura: string;
    fechaEmision: string;
    fechaVencimiento: string;
    importe: number;
    estado: 'PAGADA' | 'A_CUENTA' | 'ANULADA';
    moneda: string;
    tasaConversion: number;
}

export interface FacturaConPlanResponse {
    factura: FacturaResponse;
    plan: string;
}

export interface PagoConFacturaResponse {
    pago: PagoResponse;
    numeroFactura: string;
}

export interface NotaConFacturaResponse {
    nota: NotaCreditoDebitoResponse;
    numeroFactura: string;
}
