import type {
    DepartamentoAdminResponse,
    DepartamentoResponse,
    EmpresaAdminResponse,
    GuardarDepartamentoRequest,
    GuardarEmpresaRequest,
    PadronRequest,
    ResultadoPadronResponse,
} from '../tipos/empresas';
import type { EmpresaResponse } from '../tipos/usuarios';
import { clienteHttp } from './clienteHttp';

export const empresasApi = {
    listar: () => clienteHttp.get<EmpresaResponse[]>('/empresas'),

    listarAdministracion: () => clienteHttp.get<EmpresaAdminResponse[]>('/empresas/administracion'),

    crear: (cuerpo: GuardarEmpresaRequest) =>
        clienteHttp.post<EmpresaAdminResponse>('/empresas', cuerpo),

    modificar: (empresaId: number, cuerpo: GuardarEmpresaRequest) =>
        clienteHttp.put<EmpresaAdminResponse>(`/empresas/${empresaId}`, cuerpo),

    baja: (empresaId: number) => clienteHttp.borrar<void>(`/empresas/${empresaId}`),
};

export const departamentosApi = {
    listar: () => clienteHttp.get<DepartamentoAdminResponse[]>('/departamentos'),

    listarAsignables: (empresaId: number) =>
        clienteHttp.get<DepartamentoResponse[]>(`/departamentos/asignables?empresaId=${empresaId}`),

    crear: (cuerpo: GuardarDepartamentoRequest) =>
        clienteHttp.post<DepartamentoAdminResponse>('/departamentos', cuerpo),

    modificar: (departamentoId: number, cuerpo: GuardarDepartamentoRequest) =>
        clienteHttp.put<DepartamentoAdminResponse>(`/departamentos/${departamentoId}`, cuerpo),

    cambiarEstado: (departamentoId: number, activo: boolean) =>
        clienteHttp.patch<DepartamentoAdminResponse>(`/departamentos/${departamentoId}/estado`, {
            activo,
        }),

    baja: (departamentoId: number) => clienteHttp.borrar<void>(`/departamentos/${departamentoId}`),
};

export const padronApi = {
    procesar: (cuerpo: PadronRequest) =>
        clienteHttp.post<ResultadoPadronResponse>('/padron', cuerpo),
};
