import type { UsuarioAdminResponse } from '../tipos/usuarios';

export function estaBloqueada(usuario: UsuarioAdminResponse): boolean {
    return usuario.bloqueadoHasta !== null && new Date(usuario.bloqueadoHasta) > new Date();
}
