import { clienteHttp } from './clienteHttp';
import type { EmpresaResponse } from '../tipos/autenticacion';
import type { EmpresaAdminResponse, GuardarEmpresaRequest } from '../tipos/empresas';

export const empresasApi = {
    /**
     * Empresas ofrecidas en los selectores: registro público e invitación del
     * Backoffice. Deja afuera las no seleccionables, como la empresa interna.
     */
    listar: () => clienteHttp.get<EmpresaResponse[]>('/empresas'),

    /** Backoffice: todas las activas. Requiere el permiso Empresa.Listar. */
    listarAdministracion: () => clienteHttp.get<EmpresaAdminResponse[]>('/empresas/administracion'),

    crear: (cuerpo: GuardarEmpresaRequest) =>
        clienteHttp.post<EmpresaAdminResponse>('/empresas', cuerpo),

    modificar: (empresaId: number, cuerpo: GuardarEmpresaRequest) =>
        clienteHttp.put<EmpresaAdminResponse>(`/empresas/${empresaId}`, cuerpo),

    baja: (empresaId: number) => clienteHttp.borrar<void>(`/empresas/${empresaId}`),
};
