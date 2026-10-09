/**
 * Opinión de un usuario sobre un plan, espejo de `BE/BEComentarioRespuesta.cs`.
 *
 * Los nombres del backend siguen diciendo "comentario" porque así se llama la
 * entidad en el modelo de datos; "opinión" es cómo la muestra la interfaz.
 */
export interface OpinionResponse {
    comentarioId: number;
    planId: number;
    plan: string;
    autor: string;
    /** Vacío cuando la valoración se publicó sin comentario. */
    texto: string;
    /**
     * Estrellas de 1 a 5. Es `null` solo en los comentarios anteriores a la
     * valoración, que se muestran sin estrellas y no cuentan para el promedio.
     */
    puntaje: number | null;
    fechaAlta: string;
    /** El backend lo calcula contra la sesión: no viaja el id del autor. */
    esPropio: boolean;
    /**
     * Si esta sesión puede eliminarla: propia, o con permiso de moderación.
     *
     * Lo decide el servidor y no el front combinando `esPropio` con sus permisos,
     * porque los del front son los que capturó el login y quedaron en
     * `sessionStorage`: una revocación no se vería hasta volver a entrar.
     */
    puedeEliminar: boolean;
}

/**
 * Valoración a guardar. El autor lo resuelve el backend con la sesión, y si ya
 * existe una valoración de esa persona para el plan, la actualiza.
 */
export interface GuardarValoracionRequest {
    planId: number;
    puntaje: number;
    texto: string;
}

/**
 * Criterios de la búsqueda privada de opiniones.
 *
 * La búsqueda simple usa solo `texto`; la avanzada suma plan y rango de fechas.
 */
export interface CriteriosOpinion {
    planId?: number;
    texto?: string;
    desde?: string;
    hasta?: string;
}

/** Tope de `Comentario.Texto` en el modelo de datos. */
export const LARGO_MAXIMO_OPINION = 1000;

/** Máximo de estrellas de una valoración (`CK_Comentario_Puntaje`). */
export const PUNTAJE_MAXIMO = 5;
