import { clienteHttp } from './clienteHttp';
import type {
    CriteriosBusqueda,
    PaginaBuscableResponse,
    SeccionBusquedaResponse,
} from '../tipos/busqueda';

function armarConsulta(criterios: CriteriosBusqueda, idioma: string): string {
    const parametros = new URLSearchParams();

    if (criterios.texto?.trim()) {
        parametros.set('texto', criterios.texto.trim());
    }

    if (criterios.area) {
        parametros.set('area', criterios.area);
    }

    if (criterios.seccion) {
        parametros.set('seccion', criterios.seccion);
    }

    if (criterios.ordenarPor) {
        parametros.set('ordenarPor', criterios.ordenarPor);
    }

    if (criterios.soloTitulo) {
        parametros.set('soloTitulo', 'true');
    }

    parametros.set('idioma', idioma);

    return `?${parametros.toString()}`;
}

/**
 * Buscador global de páginas y secciones.
 *
 * Es público: sin sesión devuelve solo el sitio público. Con sesión,
 * `clienteHttp` manda el token y el backend suma las pantallas privadas a las
 * que la persona tiene permiso. El idioma decide sobre qué textos se busca.
 */
export const busquedaApi = {
    buscar: (criterios: CriteriosBusqueda, idioma: string) =>
        clienteHttp.get<PaginaBuscableResponse[]>(`/busqueda${armarConsulta(criterios, idioma)}`),

    listarSecciones: () => clienteHttp.get<SeccionBusquedaResponse[]>('/busqueda/secciones'),
};
