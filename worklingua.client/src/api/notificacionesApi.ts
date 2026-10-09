import { clienteHttp } from './clienteHttp';
import type { BandejaResponse } from '../tipos/notificaciones';

/**
 * La bandeja de la campana. No pide permiso: alcanza con tener sesión, y el
 * backend la resuelve con el usuario de esa sesión.
 */
export const notificacionesApi = {
    bandeja: () => clienteHttp.get<BandejaResponse>('/notificaciones'),

    marcarTodas: () => clienteHttp.post<BandejaResponse>('/notificaciones/leidas', {}),

    marcarLeida: (notificacionId: number) =>
        clienteHttp.post<BandejaResponse>(`/notificaciones/${notificacionId}/leida`, {}),
};
