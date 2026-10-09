import { clienteHttp } from './clienteHttp';
import type {
    DiferenciaVersionResponse,
    ResultadoRestauracionResponse,
    VersionResponse,
} from '../tipos/versiones';

/**
 * Versiones de un curso (CU-004-004) y clonado (CU-004-002). Sin permisos
 * propios: ver el historial pide Curso.Listar, versionar y restaurar
 * Curso.Modificar, y clonar Curso.Alta.
 */
export const versionesApi = {
    /** La línea de tiempo, sin el XML. */
    listar: (cursoId: number) => clienteHttp.get<VersionResponse[]>(`/cursos/${cursoId}/versiones`),

    /** Una versión con su documento XML. */
    obtener: (versionId: number) => clienteHttp.get<VersionResponse>(`/cursos/versiones/${versionId}`),

    /** Crea el punto de restauración con el estado actual del curso. */
    crear: (cursoId: number, observaciones: string | null) =>
        clienteHttp.post<VersionResponse>(`/cursos/${cursoId}/versiones`, { observaciones }),

    /** Vuelve el curso a esa versión, dejando antes una automática con lo que había. */
    restaurar: (versionId: number) =>
        clienteHttp.post<ResultadoRestauracionResponse>(`/cursos/versiones/${versionId}/restaurar`, {}),

    /** Borra un punto de restauración. Requiere Curso.Modificar. */
    eliminar: (versionId: number) => clienteHttp.borrar<void>(`/cursos/versiones/${versionId}`),

    comparar: (izquierda: number, derecha: number) =>
        clienteHttp.get<DiferenciaVersionResponse[]>(
            `/cursos/versiones/comparar?izquierda=${izquierda}&derecha=${derecha}`,
        ),

    clonar: (cursoId: number, nombre: string) =>
        clienteHttp.post<ResultadoRestauracionResponse>(`/cursos/${cursoId}/clonar`, { nombre }),
};
