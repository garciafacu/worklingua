/**
 * Una fila del inventario de licencias, espejo de `BE/BEInventarioLicencia.cs`.
 *
 * Viajan todos los empleados activos de la empresa, con licencia o sin ella:
 * los que no tienen son justamente a quienes se les puede asignar una.
 */
export interface InventarioLicenciaResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    email: string;
    departamento: string | null;
    /** null cuando el empleado todavía no tiene licencia. */
    licenciaId: number | null;
    codigoLicencia: string | null;
    fechaAsignacion: string | null;
}

/** El inventario completo, espejo de `BE/BEInventarioRespuesta.cs`. */
export interface InventarioResponse {
    empresaId: number;
    empresa: string;
    plan: string;
    /** Cupo del plan contratado. */
    contratadas: number;
    asignadas: number;
    disponibles: number;
    empleados: InventarioLicenciaResponse[];
}

/** Cuerpo de la asignación, espejo de `BE/BEAsignarLicencia.cs`. */
export interface AsignarLicenciaRequest {
    usuarioId: number;
}
