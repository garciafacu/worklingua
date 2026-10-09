import { clienteHttp } from './clienteHttp';
import type {
    ContratacionResponse,
    ContratarPlanRequest,
    CotizacionContratacionResponse,
} from '../tipos/contrataciones';

/** Contratación de planes. Todos los endpoints exigen el permiso Suscripcion.Contratar. */
export const contratacionesApi = {
    cotizar: (planId: number, codigoCultura: string) =>
        clienteHttp.get<CotizacionContratacionResponse>(
            `/contrataciones/cotizacion?planId=${planId}&codigoCultura=${encodeURIComponent(codigoCultura)}`,
        ),

    /**
     * Responde 200 tanto si se aprueba como si se rechaza el pago: el rechazo es
     * un resultado de la operación (queda registrado), no un error del pedido.
     */
    contratar: (cuerpo: ContratarPlanRequest) =>
        clienteHttp.post<ContratacionResponse>('/contrataciones', cuerpo),

    cancelar: (suscripcionId: number, codigoCultura: string) =>
        clienteHttp.post<ContratacionResponse>(`/contrataciones/${suscripcionId}/cancelar`, {
            codigoCultura,
        }),
};
