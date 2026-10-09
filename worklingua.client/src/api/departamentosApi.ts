import { clienteHttp } from './clienteHttp';
import type {
    DepartamentoAdminResponse,
    DepartamentoResponse,
    GuardarDepartamentoRequest,
} from '../tipos/departamentos';

export const departamentosApi = {
    /**
     * Backoffice. Requiere Departamento.Listar.
     *
     * Incluye los dados de baja, para poder reactivarlos. Sin
     * Departamento.VerTodasLasEmpresas el backend acota la lista a la empresa
     * del usuario de la sesión.
     */
    listar: () => clienteHttp.get<DepartamentoAdminResponse[]>('/departamentos'),

    /**
     * Los departamentos asignables a un usuario, para el selector del ABM de
     * Usuarios. Requiere Usuario.Modificar.
     *
     * Solo trae los activos. Quien no tiene alcance sobre todas las empresas
     * recibe los de la suya y puede omitir `empresaId`.
     */
    listarAsignables: (empresaId: number) =>
        clienteHttp.get<DepartamentoResponse[]>(`/departamentos/asignables?empresaId=${empresaId}`),

    crear: (cuerpo: GuardarDepartamentoRequest) =>
        clienteHttp.post<DepartamentoAdminResponse>('/departamentos', cuerpo),

    modificar: (departamentoId: number, cuerpo: GuardarDepartamentoRequest) =>
        clienteHttp.put<DepartamentoAdminResponse>(`/departamentos/${departamentoId}`, cuerpo),

    /** Activa o desactiva un departamento sin darlo de baja. */
    cambiarEstado: (departamentoId: number, activo: boolean) =>
        clienteHttp.patch<DepartamentoAdminResponse>(`/departamentos/${departamentoId}/estado`, {
            activo,
        }),

    baja: (departamentoId: number) => clienteHttp.borrar<void>(`/departamentos/${departamentoId}`),
};
