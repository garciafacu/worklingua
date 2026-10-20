import type { EtiquetaResponse } from './etiquetas';

export const SECTORES_CURSO = ['TURISMO', 'GASTRONOMIA', 'IT'] as const;

export type SectorCurso = (typeof SECTORES_CURSO)[number];

export interface CursoResponse {
    cursoId: number;
    nombre: string;
    descripcion: string | null;
    nivel: string;
    duracionHoras: number | null;
    idioma: string;
}

export const NIVELES_CURSO = ['Inicial', 'Intermedio', 'Avanzado'] as const;

export interface CursoAdminResponse extends CursoResponse {
    activo: boolean;
    fechaAlta: string;
    empresaId: number | null;
    sector: SectorCurso | null;
    etiquetas: EtiquetaResponse[];
    fechaPublicacion: string | null;
    fechaFin: string | null;
}

export interface GuardarCursoRequest {
    idioma: string;
    nombre: string;
    descripcion: string | null;
    nivel: string;
    duracionHoras: number | null;
    sector: SectorCurso | null;
    fechaPublicacion: string | null;
    fechaFin: string | null;
    etiquetaIds: number[];
}
