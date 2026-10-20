import { clienteHttp } from './clienteHttp';
import type { PermisoResponse } from '../tipos/permisos';
import type { GuardarRolRequest } from '../tipos/roles';
import type { RolResponse } from '../tipos/usuarios';

export const rolesApi = {
    listarAdministracion: () => clienteHttp.get<RolResponse[]>('/roles/administracion'),

    crear: (cuerpo: GuardarRolRequest) => clienteHttp.post<RolResponse>('/roles', cuerpo),

    modificar: (rolId: number, cuerpo: GuardarRolRequest) =>
        clienteHttp.put<RolResponse>(`/roles/${rolId}`, cuerpo),

    eliminar: (rolId: number) => clienteHttp.borrar<void>(`/roles/${rolId}`),

    listarPermisos: (rolId: number) =>
        clienteHttp.get<PermisoResponse[]>(`/roles/${rolId}/permisos`),

    asignarPermiso: (rolId: number, permisoId: number) =>
        clienteHttp.post<void>(`/roles/${rolId}/permisos/${permisoId}`),

    quitarPermiso: (rolId: number, permisoId: number) =>
        clienteHttp.borrar<void>(`/roles/${rolId}/permisos/${permisoId}`),
};
