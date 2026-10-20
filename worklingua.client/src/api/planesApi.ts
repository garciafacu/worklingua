import { clienteHttp } from './clienteHttp';
import type {
    GuardarPlanCaracteristicasRequest,
    GuardarPlanRequest,
    PlanAdminResponse,
    PlanCaracteristicaResponse,
    PlanResponse,
} from '../tipos/planes';

export const planesApi = {
    listar: () => clienteHttp.get<PlanResponse[]>('/planes'),

    listarAdministracion: () => clienteHttp.get<PlanAdminResponse[]>('/planes/administracion'),

    crear: (cuerpo: GuardarPlanRequest) => clienteHttp.post<PlanAdminResponse>('/planes', cuerpo),

    modificar: (planId: number, cuerpo: GuardarPlanRequest) =>
        clienteHttp.put<PlanAdminResponse>(`/planes/${planId}`, cuerpo),

    baja: (planId: number) => clienteHttp.borrar<void>(`/planes/${planId}`),

    listarCaracteristicas: (planId: number) =>
        clienteHttp.get<PlanCaracteristicaResponse[]>(`/planes/${planId}/caracteristicas`),

    guardarCaracteristicas: (planId: number, cuerpo: GuardarPlanCaracteristicasRequest) =>
        clienteHttp.put<PlanCaracteristicaResponse[]>(`/planes/${planId}/caracteristicas`, cuerpo),
};
