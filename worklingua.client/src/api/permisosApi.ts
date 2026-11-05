import type { GuardarPermisoRequest, GuardarRolRequest, PermisoResponse } from '../tipos/permisos';
import type { RolResponse } from '../tipos/usuarios';
import { clienteHttp } from './clienteHttp';

export const permisosApi = {
    listar: () => clienteHttp.get<PermisoResponse[]>('/permisos'),

    crear: (cuerpo: GuardarPermisoRequest) =>
        clienteHttp.post<PermisoResponse>('/permisos', cuerpo),

    modificar: (permisoId: number, cuerpo: GuardarPermisoRequest) =>
        clienteHttp.put<PermisoResponse>(`/permisos/${permisoId}`, cuerpo),

    eliminar: (permisoId: number) => clienteHttp.borrar<void>(`/permisos/${permisoId}`),

    agregarHijo: (permisoId: number, hijoId: number) =>
        clienteHttp.post<void>(`/permisos/${permisoId}/hijos/${hijoId}`),

    quitarHijo: (permisoId: number, hijoId: number) =>
        clienteHttp.borrar<void>(`/permisos/${permisoId}/hijos/${hijoId}`),
};

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
