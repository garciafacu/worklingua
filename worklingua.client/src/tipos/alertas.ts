/**
 * Alertas preventivas de deserción (CU-001-010), espejo de los BE de
 * `worklingua.Server/BE/BEAlerta*.cs`.
 */

/** Una regla de alerta, espejo de `BEAlertaRespuesta.cs`. */
export interface AlertaResponse {
    alertaId: number;
    empresaId: number;
    empresa: string;
    /** Null significa toda la empresa. */
    departamentoId: number | null;
    departamento: string | null;
    titulo: string;
    mensaje: string;
    diasInactividad: number;
    activo: boolean;
    fechaAlta: string | null;
    /** Null si todavía no se emitió. */
    ultimaEmision: string | null;
    /** A cuántos empleados alcanzó la última emisión. */
    destinatarios: number;
}

/** Un empleado alcanzado por la condición, espejo de `BEEmpleadoInactivo.cs`. */
export interface EmpleadoInactivoResponse {
    usuarioId: number;
    empleado: string;
    email: string;
    departamento: string | null;
    diasSinActividad: number;
}

/** El resultado de emitir, espejo de `BEEmisionRespuesta.cs`. */
export interface EmisionResponse {
    alerta: AlertaResponse;
    destinatarios: number;
    empleados: EmpleadoInactivoResponse[];
}

/** Cuerpo del alta, espejo de `BEGuardarAlerta.cs`. */
export interface GuardarAlertaRequest {
    empresaId: number;
    /** Null o cero significa toda la empresa. */
    departamentoId: number | null;
    titulo: string;
    mensaje: string;
    diasInactividad: number;
}
