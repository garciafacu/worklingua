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
