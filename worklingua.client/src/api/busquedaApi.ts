import { aConsulta, clienteHttp } from './clienteHttp';
import type {
    CriteriosBusqueda,
    PaginaBuscableResponse,
    SeccionBusquedaResponse,
} from '../tipos/busqueda';

function armarConsulta(criterios: CriteriosBusqueda, idioma: string): string {
    return aConsulta({
        texto: criterios.texto,
        area: criterios.area,
        seccion: criterios.seccion,
        ordenarPor: criterios.ordenarPor,
        soloTitulo: criterios.soloTitulo,
        idioma,
    });
}

export const busquedaApi = {
    buscar: (criterios: CriteriosBusqueda, idioma: string) =>
        clienteHttp.get<PaginaBuscableResponse[]>(`/busqueda${armarConsulta(criterios, idioma)}`),

    listarSecciones: () => clienteHttp.get<SeccionBusquedaResponse[]>('/busqueda/secciones'),
};
