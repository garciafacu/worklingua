import type { UsuarioAdminResponse } from '../tipos/usuarios';

/**
 * Una cuenta queda bloqueada tras varios intentos fallidos de ingreso y se
 * libera sola al vencer el plazo (CU-003-004), así que la fecha hay que
 * compararla contra ahora: que `bloqueadoHasta` tenga valor no alcanza.
 *
 * Lo usan el ABM de Usuarios y el de Operadores, que comparten el mismo
 * contrato de respuesta.
 */
export function estaBloqueada(usuario: UsuarioAdminResponse): boolean {
    return usuario.bloqueadoHasta !== null && new Date(usuario.bloqueadoHasta) > new Date();
}
