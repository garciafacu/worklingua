import { clienteHttp } from './clienteHttp';
import type { MensajeResponse } from '../tipos/autenticacion';
import type { ModificarPerfilRequest, PerfilResponse } from '../tipos/perfil';
import type {
    CompletarInvitacionRequest,
    InvitarUsuarioRequest,
    RolResponse,
    UsuarioAdminResponse,
} from '../tipos/usuarios';

export const usuariosApi = {
    /** Backoffice: usuarios activos de todas las empresas. Requiere Usuario.Listar. */
    listarAdministracion: () => clienteHttp.get<UsuarioAdminResponse[]>('/usuarios/administracion'),

    /** Crea la cuenta inactiva y manda el correo con el enlace para definir la clave. */
    invitar: (cuerpo: InvitarUsuarioRequest) =>
        clienteHttp.post<MensajeResponse>('/usuarios/invitar', cuerpo),

    /** Reenvía la invitación cuando el enlace anterior venció. */
    reinvitar: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/usuarios/${usuarioId}/reinvitar`),

    /** Desbloquea una cuenta trabada por intentos fallidos. Requiere Usuario.Modificar. */
    desbloquear: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/usuarios/${usuarioId}/desbloquear`),

    modificar: (usuarioId: number, cuerpo: InvitarUsuarioRequest) =>
        clienteHttp.put<MensajeResponse>(`/usuarios/${usuarioId}`, cuerpo),

    baja: (usuarioId: number) => clienteHttp.borrar<void>(`/usuarios/${usuarioId}`),

    /** Destino del enlace del correo de invitación. No requiere sesión. */
    completarInvitacion: (cuerpo: CompletarInvitacionRequest) =>
        clienteHttp.post<MensajeResponse>('/usuarios/completar-invitacion', cuerpo),

    /** Roles asignables en el formulario de invitación. Requiere Usuario.Listar. */
    listarRoles: () => clienteHttp.get<RolResponse[]>('/roles'),

    /** Perfil propio. Alcanza con la sesión: no requiere permiso. */
    obtenerPerfil: () => clienteHttp.get<PerfilResponse>('/usuarios/perfil'),

    modificarPerfil: (cuerpo: ModificarPerfilRequest) =>
        clienteHttp.put<MensajeResponse>('/usuarios/perfil', cuerpo),
};
