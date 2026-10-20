import { clienteHttp } from './clienteHttp';
import type { BandejaResponse } from '../tipos/notificaciones';

export const notificacionesApi = {
    bandeja: () => clienteHttp.get<BandejaResponse>('/notificaciones'),

    marcarTodas: () => clienteHttp.post<BandejaResponse>('/notificaciones/leidas', {}),

    marcarLeida: (notificacionId: number) =>
        clienteHttp.post<BandejaResponse>(`/notificaciones/${notificacionId}/leida`, {}),
};
