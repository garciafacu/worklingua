import { clienteHttp } from './clienteHttp';
import type { GuardarPermisoRequest, PermisoResponse } from '../tipos/permisos';

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
