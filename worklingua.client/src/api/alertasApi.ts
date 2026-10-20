import { clienteHttp } from './clienteHttp';
import type {
    AlertaResponse,
    EmisionResponse,
    EmpleadoInactivoResponse,
    GuardarAlertaRequest,
} from '../tipos/alertas';

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
