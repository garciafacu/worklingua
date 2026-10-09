// Tipos de los contratos que expone la API de WorkLingua.
// Se mantienen alineados con worklingua.Server/Contratos.

/**
 * Auto-registro: da de alta la empresa y su primer usuario.
 *
 * No lleva empresaId (antes se elegia una existente) ni rolId: el rol lo decide
 * el servidor.
 */
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

/** Usuario autenticado tal como lo guarda el contexto de sesión. */
export interface UsuarioSesion {
    usuarioId: number;
    nombre: string;
    apellido: string;
    email: string;
    roles: string[];
    permisos: string[];
}
