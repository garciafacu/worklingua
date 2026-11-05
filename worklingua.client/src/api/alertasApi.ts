import type {
    AlertaResponse,
    BandejaResponse,
    EmisionResponse,
    EmpleadoInactivoResponse,
    GuardarAlertaRequest,
} from '../tipos/alertas';
import { clienteHttp } from './clienteHttp';

export const alertasApi = {
    listar: () => clienteHttp.get<AlertaResponse[]>('/alertas'),

    alcance: (empresaId: number, departamentoId: number, diasInactividad: number) =>
        clienteHttp.get<EmpleadoInactivoResponse[]>(
            `/alertas/alcance?empresaId=${empresaId}&departamentoId=${departamentoId}&diasInactividad=${diasInactividad}`,
        ),

    crear: (cuerpo: GuardarAlertaRequest) => clienteHttp.post<EmisionResponse>('/alertas', cuerpo),

    emitir: (alertaId: number) =>
        clienteHttp.post<EmisionResponse>(`/alertas/${alertaId}/emitir`, {}),

    cambiarEstado: (alertaId: number, activo: boolean) =>
        clienteHttp.patch<AlertaResponse>(`/alertas/${alertaId}/estado`, { activo }),
};

export const notificacionesApi = {
    bandeja: () => clienteHttp.get<BandejaResponse>('/notificaciones'),

    marcarTodas: () => clienteHttp.post<BandejaResponse>('/notificaciones/leidas', {}),

    marcarLeida: (notificacionId: number) =>
        clienteHttp.post<BandejaResponse>(`/notificaciones/${notificacionId}/leida`, {}),
};
