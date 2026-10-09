import { clienteHttp } from './clienteHttp';
import type { CriteriosBitacora, PaginaBitacoraResponse } from '../tipos/bitacora';

function armarConsulta(criterios: CriteriosBitacora): string {
    const parametros = new URLSearchParams();

    if (criterios.texto?.trim()) {
        parametros.set('texto', criterios.texto.trim());
    }

    if (criterios.usuario?.trim()) {
        parametros.set('usuario', criterios.usuario.trim());
    }

    if (criterios.modulo) {
        parametros.set('modulo', criterios.modulo);
    }

    if (criterios.accion?.trim()) {
        parametros.set('accion', criterios.accion.trim());
    }

    if (criterios.nivel) {
        parametros.set('nivel', criterios.nivel);
    }

    if (criterios.desde) {
        parametros.set('desde', criterios.desde);
    }

    if (criterios.hasta) {
        // El input date da la fecha sin hora, así que un evento de esa misma
        // tarde quedaría afuera del rango. Se lleva al final del día.
        parametros.set('hasta', `${criterios.hasta}T23:59:59`);
    }

    if (criterios.pagina) {
        parametros.set('pagina', String(criterios.pagina));
    }

    if (criterios.tamanioPagina) {
        parametros.set('tamanioPagina', String(criterios.tamanioPagina));
    }

    const consulta = parametros.toString();

    return consulta ? `?${consulta}` : '';
}

/**
 * Consulta de la bitácora. Exige el permiso Bitacora.Listar: sin él el backend
 * responde 403 y deja el intento asentado como Seguridad/PermisoDenegado.
 *
 * Los filtros y la paginación los resuelve `sp_Bitacora_Buscar`: acá nunca se
 * descarga la bitácora entera para filtrarla en el navegador.
 */
export const bitacoraApi = {
    buscar: (criterios: CriteriosBitacora = {}) =>
        clienteHttp.get<PaginaBitacoraResponse>(`/bitacora${armarConsulta(criterios)}`),
};
