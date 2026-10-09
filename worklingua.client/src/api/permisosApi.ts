import { clienteHttp } from './clienteHttp';
import type { GuardarPermisoRequest, PermisoResponse } from '../tipos/permisos';

/** Backoffice. Cada operación exige su permiso `Permiso.*`, exclusivo del rol de plataforma. */
export const permisosApi = {
    /** Las raíces de la jerarquía, cada una con su subárbol. */
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
