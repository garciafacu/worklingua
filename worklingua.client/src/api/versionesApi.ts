import { clienteHttp } from './clienteHttp';
import type {
    DiferenciaVersionResponse,
    ResultadoRestauracionResponse,
    VersionResponse,
} from '../tipos/versiones';

export const versionesApi = {
    listar: (cursoId: number) => clienteHttp.get<VersionResponse[]>(`/cursos/${cursoId}/versiones`),

    obtener: (versionId: number) => clienteHttp.get<VersionResponse>(`/cursos/versiones/${versionId}`),

    crear: (cursoId: number, observaciones: string | null) =>
        clienteHttp.post<VersionResponse>(`/cursos/${cursoId}/versiones`, { observaciones }),

    restaurar: (versionId: number) =>
        clienteHttp.post<ResultadoRestauracionResponse>(`/cursos/versiones/${versionId}/restaurar`, {}),

    eliminar: (versionId: number) => clienteHttp.borrar<void>(`/cursos/versiones/${versionId}`),

    comparar: (izquierda: number, derecha: number) =>
        clienteHttp.get<DiferenciaVersionResponse[]>(
            `/cursos/versiones/comparar?izquierda=${izquierda}&derecha=${derecha}`,
        ),

    clonar: (cursoId: number, nombre: string) =>
        clienteHttp.post<ResultadoRestauracionResponse>(`/cursos/${cursoId}/clonar`, { nombre }),
};
