import { clienteHttp } from './clienteHttp';
import type {
    CriteriosOpinion,
    GuardarValoracionRequest,
    OpinionResponse,
} from '../tipos/opiniones';

function armarConsulta(criterios: CriteriosOpinion): string {
    const parametros = new URLSearchParams();

    if (criterios.planId) {
        parametros.set('planId', String(criterios.planId));
    }

    if (criterios.texto?.trim()) {
        parametros.set('texto', criterios.texto.trim());
    }

    if (criterios.desde) {
        parametros.set('desde', criterios.desde);
    }

    if (criterios.hasta) {
        // El input date da la fecha sin hora, así que una opinión de esa misma
        // tarde quedaría afuera del rango. Se lleva al final del día.
        parametros.set('hasta', `${criterios.hasta}T23:59:59`);
    }

    const consulta = parametros.toString();

    return consulta ? `?${consulta}` : '';
}

/**
 * Las rutas siguen siendo `/comentarios`: la sección se renombró en la interfaz,
 * no en el modelo de datos ni en el contrato de la API.
 *
 * Los cuatro endpoints exigen sesión: el token lo adjunta `clienteHttp` desde el
 * `SesionProvider`. Sin sesión el backend responde 401 y `ErrorApi` lo propaga.
 */
export const opinionesApi = {
    listar: (planId: number) =>
        clienteHttp.get<OpinionResponse[]>(`/comentarios?planId=${planId}`),

    buscar: (criterios: CriteriosOpinion = {}) =>
        clienteHttp.get<OpinionResponse[]>(`/comentarios/buscar${armarConsulta(criterios)}`),

    /** Crea la valoración o, si la sesión ya valoró ese plan, la actualiza. */
    guardar: (datos: GuardarValoracionRequest) =>
        clienteHttp.post<OpinionResponse>('/comentarios', datos),

    /** La valoración de esta sesión para el plan, o `undefined` (204) si todavía no hay. */
    obtenerValoracion: (planId: number) =>
        clienteHttp.get<OpinionResponse | undefined>(`/comentarios/valoracion?planId=${planId}`),

    /** Baja lógica. El backend decide si esta sesión puede: propia o moderación. */
    eliminar: (comentarioId: number) =>
        clienteHttp.borrar<void>(`/comentarios/${comentarioId}`),
};
