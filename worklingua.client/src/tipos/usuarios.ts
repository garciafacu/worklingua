/** Usuario visto desde el Backoffice, espejo de `Contratos/UsuarioAdminResponse.cs`. */
export interface UsuarioAdminResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    documento: string | null;
    email: string;
    empresaId: number;
    empresa: string;
    departamentoId: number | null;
    departamento: string | null;
    rolId: number;
    roles: string[];
    fechaAlta: string | null;
    ultimoAcceso: string | null;
    activo: boolean;
    /** Hasta cuándo está bloqueada por intentos fallidos; null si no lo está. */
    bloqueadoHasta: string | null;
}

/**
 * Cuerpo de la invitación y de la modificación, espejo de `InvitarUsuarioRequest.cs`.
 *
 * No lleva clave: la define la persona invitada desde el enlace del correo.
 */
export interface InvitarUsuarioRequest {
    empresaId: number;
    rolId: number;
    nombre: string;
    apellido: string;
    documento: string | null;
    email: string;
    /** null deja al empleado sin departamento. */
    departamentoId: number | null;
}

/** Rol asignable, espejo de `Contratos/RolResponse.cs`. */
export interface RolResponse {
    rolId: number;
    nombre: string;
    descripcion: string | null;
}

/** Cuerpo del alta de clave desde el enlace de invitación. */
export interface CompletarInvitacionRequest {
    token: string;
    clave: string;
}
