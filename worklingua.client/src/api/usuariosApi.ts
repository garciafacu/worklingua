import type {
    CambiarClaveRequest,
    CompletarInvitacionRequest,
    GuardarOperadorRequest,
    InvitarUsuarioRequest,
    LoginRequest,
    LoginResponse,
    MensajeResponse,
    ModificarPerfilRequest,
    PerfilResponse,
    RegistroUsuarioRequest,
    RestablecerClaveRequest,
    RolResponse,
    UsuarioAdminResponse,
} from '../tipos/usuarios';
import { clienteHttp } from './clienteHttp';

export const usuariosApi = {
    listarAdministracion: () => clienteHttp.get<UsuarioAdminResponse[]>('/usuarios/administracion'),

    invitar: (cuerpo: InvitarUsuarioRequest) =>
        clienteHttp.post<MensajeResponse>('/usuarios/invitar', cuerpo),

    reinvitar: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/usuarios/${usuarioId}/reinvitar`),

    desbloquear: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/usuarios/${usuarioId}/desbloquear`),

    modificar: (usuarioId: number, cuerpo: InvitarUsuarioRequest) =>
        clienteHttp.put<MensajeResponse>(`/usuarios/${usuarioId}`, cuerpo),

    baja: (usuarioId: number) => clienteHttp.borrar<void>(`/usuarios/${usuarioId}`),

    completarInvitacion: (cuerpo: CompletarInvitacionRequest) =>
        clienteHttp.post<MensajeResponse>('/usuarios/completar-invitacion', cuerpo),

    listarRoles: () => clienteHttp.get<RolResponse[]>('/roles'),

    obtenerPerfil: () => clienteHttp.get<PerfilResponse>('/usuarios/perfil'),

    modificarPerfil: (cuerpo: ModificarPerfilRequest) =>
        clienteHttp.put<MensajeResponse>('/usuarios/perfil', cuerpo),
};

export const operadoresApi = {
    listar: () => clienteHttp.get<UsuarioAdminResponse[]>('/operadores'),

    crear: (cuerpo: GuardarOperadorRequest) =>
        clienteHttp.post<UsuarioAdminResponse>('/operadores', cuerpo),

    modificar: (usuarioId: number, cuerpo: GuardarOperadorRequest) =>
        clienteHttp.put<UsuarioAdminResponse>(`/operadores/${usuarioId}`, cuerpo),

    baja: (usuarioId: number) => clienteHttp.borrar<void>(`/operadores/${usuarioId}`),

    reinvitar: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/operadores/${usuarioId}/reinvitar`),

    desbloquear: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/operadores/${usuarioId}/desbloquear`),

    listarRoles: (usuarioId: number) =>
        clienteHttp.get<RolResponse[]>(`/operadores/${usuarioId}/roles`),

    asignarRol: (usuarioId: number, rolId: number) =>
        clienteHttp.post<void>(`/operadores/${usuarioId}/roles/${rolId}`),

    quitarRol: (usuarioId: number, rolId: number) =>
        clienteHttp.borrar<void>(`/operadores/${usuarioId}/roles/${rolId}`),
};

export const autenticacionApi = {
    registrar: (datos: RegistroUsuarioRequest) =>
        clienteHttp.post<MensajeResponse>('/usuarios/registro', datos),

    confirmarCuenta: (token: string) =>
        clienteHttp.post<MensajeResponse>('/usuarios/confirmar', { token }),

    login: (datos: LoginRequest) =>
        clienteHttp.post<LoginResponse>('/autenticacion/login', datos),

    sesion: () => clienteHttp.get<LoginResponse>('/autenticacion/sesion'),

    logout: () => clienteHttp.post<void>('/autenticacion/logout'),

    recuperarClave: (email: string) =>
        clienteHttp.post<MensajeResponse>('/autenticacion/recuperar-clave', { email }),

    restablecerClave: (datos: RestablecerClaveRequest) =>
        clienteHttp.post<MensajeResponse>('/autenticacion/restablecer-clave', datos),

    cambiarClave: (datos: CambiarClaveRequest) =>
        clienteHttp.post<MensajeResponse>('/autenticacion/cambiar-clave', datos),
};
