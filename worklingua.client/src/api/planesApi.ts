import type {
    AsignarLicenciaRequest,
    CaracteristicaAdminResponse,
    CaracteristicaResponse,
    GuardarCaracteristicaRequest,
    GuardarPlanCaracteristicasRequest,
    GuardarPlanRequest,
    InventarioResponse,
    PlanAdminResponse,
    PlanCaracteristicaResponse,
    PlanResponse,
    SuscripcionResponse,
} from '../tipos/planes';
import { clienteHttp } from './clienteHttp';

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

export const caracteristicasApi = {
    listar: () => clienteHttp.get<CaracteristicaResponse[]>('/caracteristicas'),

    listarAdministracion: () =>
        clienteHttp.get<CaracteristicaAdminResponse[]>('/caracteristicas/administracion'),

    crear: (cuerpo: GuardarCaracteristicaRequest) =>
        clienteHttp.post<CaracteristicaAdminResponse>('/caracteristicas', cuerpo),

    modificar: (caracteristicaId: number, cuerpo: GuardarCaracteristicaRequest) =>
        clienteHttp.put<CaracteristicaAdminResponse>(`/caracteristicas/${caracteristicaId}`, cuerpo),

    baja: (caracteristicaId: number) =>
        clienteHttp.borrar<void>(`/caracteristicas/${caracteristicaId}`),
};

export const suscripcionesApi = {
    obtenerLaMia: () => clienteHttp.get<SuscripcionResponse>('/suscripciones/mia'),
};

export const licenciasApi = {
    inventario: (empresaId: number) =>
        clienteHttp.get<InventarioResponse>(`/licencias?empresaId=${empresaId}`),

    asignar: (cuerpo: AsignarLicenciaRequest) =>
        clienteHttp.post<{ mensaje: string }>('/licencias', cuerpo),

    revocar: (licenciaId: number) =>
        clienteHttp.borrar<{ mensaje: string }>(`/licencias/${licenciaId}`),
};
