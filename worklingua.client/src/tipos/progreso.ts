import type { ActivoResponse } from './activos';
import type { SectorCurso } from './cursos';
import type { EtiquetaResponse } from './etiquetas';

/**
 * Estado de avance de un módulo (`Progreso.Estado`) o de un curso, que el
 * backend deriva de sus módulos. Espejo de `BLL/BLLProgreso.cs`.
 */
export type EstadoProgreso = 'NO_INICIADO' | 'EN_CURSO' | 'COMPLETADO';

export type AccionProgreso = 'INICIAR' | 'COMPLETAR';

/** Módulo de un curso, espejo de `BE/BEModulo.cs`. */
export interface ModuloResponse {
    moduloId: number;
    cursoId: number;
    nombre: string;
    /** El objetivo del módulo. */
    descripcion: string | null;
    /** El texto de la lección. */
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
    /** `null` cuando el usuario todavía no empezó el módulo. */
    progreso: ProgresoResponse | null;
}

/** Un curso con el avance de un usuario, espejo de `BE/BECursoConProgreso.cs`. */
export interface CursoConProgresoResponse {
    curso: {
        cursoId: number;
        nombre: string;
        idioma: string;
        nivel: string;
        descripcion: string | null;
        duracionHoras: number | null;
        /** Categoría sectorial (CU-004-005); null es "sin clasificar". */
        sector: SectorCurso | null;
        /** Etiquetas lógicas del curso (CU-004-005). */
        etiquetas: EtiquetaResponse[];
        /** Ventana de publicación (CU-004-006); null es "siempre disponible". */
        fechaPublicacion: string | null;
        fechaFin: string | null;
    };
    modulos: ModuloConProgresoResponse[];
    /** Material de apoyo del curso: solo los activos publicados y vigentes. */
    activos: ActivoResponse[];
    /** Porcentaje de módulos completados, de 0 a 100. */
    porcentajeAvance: number;
    estado: EstadoProgreso;
    ultimaActividad: string | null;
}

/** El avance de un usuario de la empresa, espejo de `BE/BEProgresoUsuario.cs`. */
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
