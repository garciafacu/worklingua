export interface OpinionResponse {
    comentarioId: number;
    planId: number;
    plan: string;
    autor: string;
    texto: string;
    puntaje: number | null;
    fechaAlta: string;
    esPropio: boolean;
    puedeEliminar: boolean;
}

export interface GuardarValoracionRequest {
    planId: number;
    puntaje: number;
    texto: string;
}

export interface CriteriosOpinion {
    planId?: number;
    texto?: string;
    desde?: string;
    hasta?: string;
}

export const LARGO_MAXIMO_OPINION = 1000;

export const PUNTAJE_MAXIMO = 5;
