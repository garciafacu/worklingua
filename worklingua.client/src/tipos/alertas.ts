export interface AlertaResponse {
    alertaId: number;
    empresaId: number;
    empresa: string;
    departamentoId: number | null;
    departamento: string | null;
    titulo: string;
    mensaje: string;
    diasInactividad: number;
    activo: boolean;
    fechaAlta: string | null;
    ultimaEmision: string | null;
    destinatarios: number;
}

export interface EmpleadoInactivoResponse {
    usuarioId: number;
    empleado: string;
    email: string;
    departamento: string | null;
    diasSinActividad: number;
}

export interface EmisionResponse {
    alerta: AlertaResponse;
    destinatarios: number;
    empleados: EmpleadoInactivoResponse[];
}

export interface GuardarAlertaRequest {
    empresaId: number;
    departamentoId: number | null;
    titulo: string;
    mensaje: string;
    diasInactividad: number;
}

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
