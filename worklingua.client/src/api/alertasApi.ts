import { clienteHttp } from './clienteHttp';
import type {
    AlertaResponse,
    EmisionResponse,
    EmpleadoInactivoResponse,
    GuardarAlertaRequest,
} from '../tipos/alertas';

export const alertasApi = {
    /**
     * El panel de alertas. Requiere Alerta.Listar.
     *
     * Incluye las desactivadas, para poder reactivarlas. Sin
     * Alerta.VerTodasLasEmpresas el backend acota la lista a la empresa del
     * usuario de la sesión.
     */
    listar: () => clienteHttp.get<AlertaResponse[]>('/alertas'),

    /** A quiénes alcanzaría la condición, sin emitir nada. Requiere Alerta.Alta. */
    alcance: (empresaId: number, departamentoId: number, diasInactividad: number) =>
        clienteHttp.get<EmpleadoInactivoResponse[]>(
            `/alertas/alcance?empresaId=${empresaId}&departamentoId=${departamentoId}&diasInactividad=${diasInactividad}`,
        ),

    /** Guarda la regla y la emite en el acto. Requiere Alerta.Alta. */
    crear: (cuerpo: GuardarAlertaRequest) => clienteHttp.post<EmisionResponse>('/alertas', cuerpo),

    /** Vuelve a emitir una alerta activa. Requiere Alerta.Emitir. */
    emitir: (alertaId: number) =>
        clienteHttp.post<EmisionResponse>(`/alertas/${alertaId}/emitir`, {}),

    /** Activa o desactiva la regla. Requiere Alerta.Baja. */
    cambiarEstado: (alertaId: number, activo: boolean) =>
        clienteHttp.patch<AlertaResponse>(`/alertas/${alertaId}/estado`, { activo }),
};
