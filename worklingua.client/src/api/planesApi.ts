import { clienteHttp } from './clienteHttp';
import type {
    GuardarPlanCaracteristicasRequest,
    GuardarPlanRequest,
    PlanAdminResponse,
    PlanCaracteristicaResponse,
    PlanResponse,
} from '../tipos/planes';

export const planesApi = {
    /** Catálogo público: no requiere sesión. */
    listar: () => clienteHttp.get<PlanResponse[]>('/planes'),

    /** Backoffice: incluye los planes dados de baja. Requiere el permiso Plan.Listar. */
    listarAdministracion: () => clienteHttp.get<PlanAdminResponse[]>('/planes/administracion'),

    crear: (cuerpo: GuardarPlanRequest) => clienteHttp.post<PlanAdminResponse>('/planes', cuerpo),

    modificar: (planId: number, cuerpo: GuardarPlanRequest) =>
        clienteHttp.put<PlanAdminResponse>(`/planes/${planId}`, cuerpo),

    baja: (planId: number) => clienteHttp.borrar<void>(`/planes/${planId}`),

    /** Características asociadas a un plan. Requiere el permiso Plan.Listar. */
    listarCaracteristicas: (planId: number) =>
        clienteHttp.get<PlanCaracteristicaResponse[]>(`/planes/${planId}/caracteristicas`),

    /**
     * Reemplaza el conjunto completo de características del plan: lo que no viene
     * en la lista queda desasociado. Requiere el permiso Plan.Modificar.
     */
    guardarCaracteristicas: (planId: number, cuerpo: GuardarPlanCaracteristicasRequest) =>
        clienteHttp.put<PlanCaracteristicaResponse[]>(`/planes/${planId}/caracteristicas`, cuerpo),
};
