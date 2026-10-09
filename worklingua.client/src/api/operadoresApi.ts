import { clienteHttp } from './clienteHttp';
import type { MensajeResponse } from '../tipos/autenticacion';
import type { GuardarOperadorRequest } from '../tipos/operadores';
import type { RolResponse, UsuarioAdminResponse } from '../tipos/usuarios';

/**
 * Backoffice. Operadores de la plataforma: cuentas de la empresa interna.
 * Cada operación exige su permiso `Operador.*`, exclusivo del rol de plataforma.
 */
export const operadoresApi = {
    /** Incluye las invitaciones pendientes (`activo: false`). */
    listar: () => clienteHttp.get<UsuarioAdminResponse[]>('/operadores'),

    /** Crea la cuenta inactiva, sin roles, y manda el correo de invitación. */
    crear: (cuerpo: GuardarOperadorRequest) =>
        clienteHttp.post<UsuarioAdminResponse>('/operadores', cuerpo),

    modificar: (usuarioId: number, cuerpo: GuardarOperadorRequest) =>
        clienteHttp.put<UsuarioAdminResponse>(`/operadores/${usuarioId}`, cuerpo),

    baja: (usuarioId: number) => clienteHttp.borrar<void>(`/operadores/${usuarioId}`),

    reinvitar: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/operadores/${usuarioId}/reinvitar`),

    /** Desbloquea una cuenta trabada por intentos fallidos. Requiere Operador.Modificar. */
    desbloquear: (usuarioId: number) =>
        clienteHttp.post<MensajeResponse>(`/operadores/${usuarioId}/desbloquear`),

    listarRoles: (usuarioId: number) =>
        clienteHttp.get<RolResponse[]>(`/operadores/${usuarioId}/roles`),

    asignarRol: (usuarioId: number, rolId: number) =>
        clienteHttp.post<void>(`/operadores/${usuarioId}/roles/${rolId}`),

    quitarRol: (usuarioId: number, rolId: number) =>
        clienteHttp.borrar<void>(`/operadores/${usuarioId}/roles/${rolId}`),
};
