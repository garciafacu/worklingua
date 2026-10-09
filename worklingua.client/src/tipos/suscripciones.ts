import type { EstadoContratacion } from './contrataciones';

/**
 * El plan con el que trabaja la empresa de quien consulta, espejo de
 * `BE/BESuscripcionRespuesta.cs`.
 *
 * No trae `empresaId`: el backend la resuelve de la sesión.
 */
export interface SuscripcionResponse {
    suscripcionId: number;
    planId: number;
    plan: string;
    estado: EstadoContratacion;
    fechaInicio: string;
    /** `null` en el plan gratuito, que no vence. */
    fechaFin: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    /** Si la contratación vigente se puede cancelar (con NC total). */
    cancelable: boolean;
}
