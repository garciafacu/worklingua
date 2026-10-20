export interface NotificacionResponse {
    notificacionId: number;
    usuarioId: number;
    titulo: string;
    mensaje: string;
    leida: boolean | null;
    fechaEnvio: string | null;
}

export interface BandejaResponse {
    noLeidas: number;
    notificaciones: NotificacionResponse[];
}
