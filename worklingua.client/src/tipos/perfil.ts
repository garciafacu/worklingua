/** Perfil del usuario de la sesión (GET /api/usuarios/perfil). */
export interface PerfilResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    /** `yyyy-MM-dd`, o null si no la cargó. */
    fechaNacimiento: string | null;
    email: string;
    documento: string | null;
    empresa: string;
    departamento: string | null;
    roles: string[];
}

/** Lo que el propio usuario puede cambiar (PUT /api/usuarios/perfil). */
export interface ModificarPerfilRequest {
    nombre: string;
    apellido: string;
    fechaNacimiento: string | null;
}
