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

export const busquedaApi = {
    buscar: (criterios: CriteriosBusqueda, idioma: string) =>
        clienteHttp.get<PaginaBuscableResponse[]>(`/busqueda${armarConsulta(criterios, idioma)}`),

    listarSecciones: () => clienteHttp.get<SeccionBusquedaResponse[]>('/busqueda/secciones'),
};
