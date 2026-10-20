import { clienteHttp } from './clienteHttp';
import type {
    ContratacionResponse,
    ContratarPlanRequest,
    CotizacionContratacionResponse,
} from '../tipos/contrataciones';

export const contratacionesApi = {
    cotizar: (planId: number, codigoCultura: string) =>
        clienteHttp.get<CotizacionContratacionResponse>(
            `/contrataciones/cotizacion?planId=${planId}&codigoCultura=${encodeURIComponent(codigoCultura)}`,
        ),

    contratar: (cuerpo: ContratarPlanRequest) =>
        clienteHttp.post<ContratacionResponse>('/contrataciones', cuerpo),

    cancelar: (suscripcionId: number, codigoCultura: string) =>
        clienteHttp.post<ContratacionResponse>(`/contrataciones/${suscripcionId}/cancelar`, {
            codigoCultura,
        }),
};
