import { clienteHttp } from './clienteHttp';
import type {
    CambiarClaveRequest,
    LoginRequest,
    LoginResponse,
    MensajeResponse,
    RegistroUsuarioRequest,
    RestablecerClaveRequest,
} from '../tipos/autenticacion';

export const autenticacionApi = {
    registrar: (datos: RegistroUsuarioRequest) =>
        clienteHttp.post<MensajeResponse>('/usuarios/registro', datos),

    confirmarCuenta: (token: string) =>
        clienteHttp.post<MensajeResponse>('/usuarios/confirmar', { token }),

    login: (datos: LoginRequest) =>
        clienteHttp.post<LoginResponse>('/autenticacion/login', datos),

    /** Roles y permisos vigentes de la sesión abierta, resueltos de nuevo contra la base. */
    sesion: () => clienteHttp.get<LoginResponse>('/autenticacion/sesion'),

    logout: () => clienteHttp.post<void>('/autenticacion/logout'),

    recuperarClave: (email: string) =>
        clienteHttp.post<MensajeResponse>('/autenticacion/recuperar-clave', { email }),

    restablecerClave: (datos: RestablecerClaveRequest) =>
        clienteHttp.post<MensajeResponse>('/autenticacion/restablecer-clave', datos),

    cambiarClave: (datos: CambiarClaveRequest) =>
        clienteHttp.post<MensajeResponse>('/autenticacion/cambiar-clave', datos),
};
