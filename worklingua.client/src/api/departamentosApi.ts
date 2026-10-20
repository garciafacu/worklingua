import { clienteHttp } from './clienteHttp';
import type {
    DepartamentoAdminResponse,
    DepartamentoResponse,
    GuardarDepartamentoRequest,
} from '../tipos/departamentos';

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
