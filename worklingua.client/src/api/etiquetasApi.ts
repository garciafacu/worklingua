import { clienteHttp } from './clienteHttp';
import type { EtiquetaResponse } from '../tipos/etiquetas';

/**
 * El diccionario de etiquetas con el que se clasifican los cursos
 * (CU-004-005). Es global: la misma etiqueta sirve para cursos de cualquier
 * empresa.
 */
export const etiquetasApi = {
    /** Requiere Curso.Listar. */
    listar: () => clienteHttp.get<EtiquetaResponse[]>('/etiquetas'),

    /**
     * Agrega una etiqueta al diccionario (camino alternativo 1). Requiere
     * Curso.Modificar. Si ya existía devuelve la que había.
     */
    crear: (nombre: string) => clienteHttp.post<EtiquetaResponse>('/etiquetas', { nombre }),
};
