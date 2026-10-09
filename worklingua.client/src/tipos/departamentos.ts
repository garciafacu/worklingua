/**
 * Departamento del organigrama de una empresa (CU-001-009), espejo de
 * `BE/BEDepartamentoRespuesta.cs`.
 *
 * Un departamento pertenece siempre a una empresa: no hay departamentos
 * globales. Ver `docs/modelo-datos.md` (Departamento).
 */
export interface DepartamentoResponse {
    departamentoId: number;
    empresaId: number;
    empresa: string;
    nombre: string;
    descripcion: string | null;
}

/** Departamento visto desde el Backoffice, espejo de `BEDepartamentoAdminRespuesta.cs`. */
export interface DepartamentoAdminResponse extends DepartamentoResponse {
    activo: boolean;
    /** Usuarios activos asignados: con uno o más no se puede dar de baja. */
    empleados: number;
    fechaAlta: string | null;
}

/** Cuerpo del alta y de la modificación, espejo de `BEGuardarDepartamento.cs`. */
export interface GuardarDepartamentoRequest {
    empresaId: number;
    nombre: string;
    descripcion: string | null;
}
