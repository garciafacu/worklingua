import { aConsulta, clienteHttp } from './clienteHttp';
import type {
    CriteriosOpinion,
    GuardarValoracionRequest,
    OpinionResponse,
} from '../tipos/opiniones';

function armarConsulta(criterios: CriteriosOpinion): string {
    return aConsulta({
        planId: criterios.planId,
        texto: criterios.texto,
        desde: criterios.desde,
        hasta: criterios.hasta && `${criterios.hasta}T23:59:59`,
    });
}

export const opinionesApi = {
    listar: (planId: number) =>
        clienteHttp.get<OpinionResponse[]>(`/comentarios?planId=${planId}`),

    buscar: (criterios: CriteriosOpinion = {}) =>
        clienteHttp.get<OpinionResponse[]>(`/comentarios/buscar${armarConsulta(criterios)}`),

    guardar: (datos: GuardarValoracionRequest) =>
        clienteHttp.post<OpinionResponse>('/comentarios', datos),

    listarPlanesContratados: () => clienteHttp.get<number[]>('/comentarios/planes-contratados'),

    obtenerValoracion: (planId: number) =>
        clienteHttp.get<OpinionResponse | undefined>(`/comentarios/valoracion?planId=${planId}`),

    eliminar: (comentarioId: number) =>
        clienteHttp.borrar<void>(`/comentarios/${comentarioId}`),
};
