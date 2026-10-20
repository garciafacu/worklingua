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
    bloqueadoHasta: string | null;
}

export interface InvitarUsuarioRequest {
    empresaId: number;
    rolId: number;
    nombre: string;
    apellido: string;
    documento: string | null;
    email: string;
    departamentoId: number | null;
}

export interface RolResponse {
    rolId: number;
    nombre: string;
    descripcion: string | null;
}

export interface CompletarInvitacionRequest {
    token: string;
    clave: string;
}
