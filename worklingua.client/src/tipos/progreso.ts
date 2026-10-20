import type { ActivoResponse } from './activos';
import type { SectorCurso } from './cursos';
import type { EtiquetaResponse } from './etiquetas';

export type EstadoProgreso = 'NO_INICIADO' | 'EN_CURSO' | 'COMPLETADO';

export type AccionProgreso = 'INICIAR' | 'COMPLETAR';

export interface ModuloResponse {
    moduloId: number;
    cursoId: number;
    nombre: string;
    descripcion: string | null;
    contenido: string | null;
    ordenModulo: number;
    activo: boolean | null;
}

export interface ProgresoResponse {
    progresoId: number;
    usuarioId: number;
    moduloId: number;
    porcentajeAvance: number | null;
    fechaInicio: string | null;
    fechaFinalizacion: string | null;
    estado: EstadoProgreso | null;
}

export interface ModuloConProgresoResponse {
    modulo: ModuloResponse;
    progreso: ProgresoResponse | null;
}

export interface CursoConProgresoResponse {
    curso: {
        cursoId: number;
        nombre: string;
        idioma: string;
        nivel: string;
        descripcion: string | null;
        duracionHoras: number | null;
        sector: SectorCurso | null;
        etiquetas: EtiquetaResponse[];
        fechaPublicacion: string | null;
        fechaFin: string | null;
    };
    modulos: ModuloConProgresoResponse[];
    activos: ActivoResponse[];
    porcentajeAvance: number;
    estado: EstadoProgreso;
    ultimaActividad: string | null;
}

export interface ProgresoUsuarioResponse {
    usuarioId: number;
    usuario: string;
    email: string;
    cursos: CursoConProgresoResponse[];
}

export interface GuardarProgresoRequest {
    moduloId: number;
    accion: AccionProgreso;
}

export interface GuardarModuloRequest {
    nombre: string;
    descripcion: string | null;
    contenido: string | null;
    ordenModulo: number;
}
