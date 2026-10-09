/** Una notificación de la campana, espejo de `BE/BENotificacion.cs`. */
export interface NotificacionResponse {
    notificacionId: number;
    usuarioId: number;
    titulo: string;
    mensaje: string;
    leida: boolean | null;
    fechaEnvio: string | null;
}

/** Lo que devuelve la bandeja, espejo de `BE/BEBandejaNotificaciones.cs`. */
export interface BandejaResponse {
    noLeidas: number;
    notificaciones: NotificacionResponse[];
}
