import type { NotaCreditoDebitoResponse, PagoResponse } from './contrataciones';

/**
 * Estado de cuenta corriente de la empresa, espejo de
 * `BE/BEEstadoCuentaCorriente.cs`.
 *
 * Los importes están en la moneda base (ARS): se muestran con `formatearMoneda`.
 * Saldo positivo es deuda; negativo, saldo a favor.
 */
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
