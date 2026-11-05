import type {
    ContratacionResponse,
    ContratarPlanRequest,
    CotizacionContratacionResponse,
    EstadoCuentaCorrienteResponse,
} from '../tipos/contrataciones';
import { clienteHttp } from './clienteHttp';

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

export const cuentaCorrienteApi = {
    obtener: () => clienteHttp.get<EstadoCuentaCorrienteResponse>('/cuenta-corriente'),
};
