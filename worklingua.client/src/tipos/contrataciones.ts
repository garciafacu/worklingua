/**
 * Estados de una contratación (`Suscripcion.Estado`), espejo de las constantes de
 * `BLL/BLLSuscripcion.cs`. `ACTIVA` es la contratación aprobada y vigente.
 */
export type EstadoContratacion = 'PENDIENTE' | 'ACTIVA' | 'RECHAZADA' | 'CANCELADA' | 'FINALIZADA';

/**
 * Lo que costaría contratar un plan y con qué saldos cuenta la empresa, espejo de
 * `BE/BECotizacionContratacion.cs`.
 *
 * Todos los importes están en la moneda base (ARS): se muestran con
 * `formatearMoneda`, que aplica la tasa de la cultura activa.
 */
export interface CotizacionContratacionResponse {
    planId: number;
    plan: string;
    planActual: string | null;
    importe: number;
    moneda: string;
    simboloMoneda: string;
    tasaConversion: number;
    /** Parte no usada del plan vigente que se acredita como NC al cambiar. */
    creditoCambioPlan: number;
    saldoNotasCredito: number;
    /** Tope para pagar con NC: saldo a favor más el crédito que queda libre de deuda. */
    notaCreditoUtilizable: number;
    saldoCuentaCorriente: number;
    limiteCuentaCorriente: number;
    disponibleCuentaCorriente: number;
    porcentajeRecargoCuentaCorriente: number;
}

/**
 * Datos de la tarjeta. Viajan solo en el pedido de contratación: el backend los
 * valida de forma simulada y guarda únicamente la marca y los últimos 4 dígitos.
 */
export interface TarjetaRequest {
    numero: string;
    titular: string;
    vencimiento: string;
    codigoSeguridad: string;
}

/**
 * Cuerpo de `POST /contrataciones`, espejo de `BE/BEContratarPlan.cs`.
 *
 * Los importes de NC y cuenta corriente van en la moneda de la operación (la de
 * la cultura activa): el backend los pasa a ARS y la tarjeta paga el resto.
 */
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

/** Resultado de contratar o cancelar, espejo de `BE/BEContratacionRespuesta.cs`. */
export interface ContratacionResponse {
    suscripcionId: number;
    planId: number;
    plan: string;
    estado: EstadoContratacion;
    /** Por qué se rechazó, cuando `estado` es `RECHAZADA`. */
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
