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

export interface GuardarOperadorRequest {
    nombre: string;
    apellido: string;
    documento: string | null;
    email: string;
}

export interface RegistroUsuarioRequest {
    razonSocial: string;
    cuit: string;
    nombre: string;
    apellido: string;
    documento?: string;
    email: string;
    clave: string;
    tokenCaptcha: string;
}

export interface LoginRequest {
    email: string;
    clave: string;
}

export interface LoginResponse {
    token: string;
    usuarioId: number;
    nombre: string;
    apellido: string;
    email: string;
    roles: string[];
    permisos: string[];
}

export interface ConfirmarCuentaRequest {
    token: string;
}

export interface RecuperarClaveRequest {
    email: string;
}

export interface RestablecerClaveRequest {
    token: string;
    claveNueva: string;
}

export interface CambiarClaveRequest {
    claveActual: string;
    claveNueva: string;
}

export interface MensajeResponse {
    mensaje: string;
}

export interface EmpresaResponse {
    empresaId: number;
    razonSocial: string;
}

export interface UsuarioSesion {
    usuarioId: number;
    nombre: string;
    apellido: string;
    email: string;
    roles: string[];
    permisos: string[];
}

export interface PerfilResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    fechaNacimiento: string | null;
    email: string;
    documento: string | null;
    empresa: string;
    departamento: string | null;
    roles: string[];
}

export interface ModificarPerfilRequest {
    nombre: string;
    apellido: string;
    fechaNacimiento: string | null;
}
