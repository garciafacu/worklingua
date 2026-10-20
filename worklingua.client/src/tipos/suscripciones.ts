import type { EstadoContratacion } from './contrataciones';

export interface SuscripcionResponse {
    suscripcionId: number;
    planId: number;
    plan: string;
    estado: EstadoContratacion;
    fechaInicio: string;
    fechaFin: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    cancelable: boolean;
}
