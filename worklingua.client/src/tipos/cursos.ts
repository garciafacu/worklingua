import type { EtiquetaResponse } from './etiquetas';

/** Categorías sectoriales del negocio, espejo de `BLL/BLLCurso.cs`. */
export const SECTORES_CURSO = ['TURISMO', 'GASTRONOMIA', 'IT'] as const;

export type SectorCurso = (typeof SECTORES_CURSO)[number];

/** Datos de un curso, espejo de `Contratos/CursoResponse.cs`. */
export interface CursoResponse {
    cursoId: number;
    nombre: string;
    descripcion: string | null;
    nivel: string;
    duracionHoras: number | null;
    /**
     * El idioma que el curso enseña. Es texto libre y no tiene relación con el
     * catálogo de idiomas de la plataforma: ver `docs/modelo-datos.md` (Curso).
     */
    idioma: string;
}

/**
 * Niveles que se pueden asignar a un curso.
 *
 * Son los valores de `Curso.Nivel` en la base. Se declaran acá y no se piden al
 * backend porque no hay tabla de niveles: es una columna `varchar(20)` con un
 * conjunto acotado de valores.
 */
export const NIVELES_CURSO = ['Inicial', 'Intermedio', 'Avanzado'] as const;

/**
 * Curso visto desde el Backoffice, espejo de `Contratos/CursoAdminResponse.cs`.
 *
 * Suma `activo` y `fechaAlta`. Ya no hay `idiomaId`: el idioma es la columna de
 * texto que hereda de `CursoResponse`.
 */
export interface CursoAdminResponse extends CursoResponse {
    activo: boolean;
    fechaAlta: string;
    /**
     * Empresa dueña del curso. `null` es un curso global del catálogo de
     * WorkLingua, que ven todas las empresas y solo administra la plataforma.
     */
    empresaId: number | null;
    /** Categoría sectorial (CU-004-005); null es "sin clasificar". */
    sector: SectorCurso | null;
    /** Etiquetas lógicas del curso (CU-004-005). */
    etiquetas: EtiquetaResponse[];
    /**
     * Ventana de despliegue (CU-004-006). Las dos en null significan que el
     * curso está disponible siempre.
     */
    fechaPublicacion: string | null;
    fechaFin: string | null;
}

/** Cuerpo del alta y de la modificación, espejo de `Contratos/GuardarCursoRequest.cs`. */
export interface GuardarCursoRequest {
    idioma: string;
    nombre: string;
    descripcion: string | null;
    nivel: string;
    duracionHoras: number | null;
    sector: SectorCurso | null;
    /** `yyyy-MM-dd` o null. */
    fechaPublicacion: string | null;
    fechaFin: string | null;
    /** Tienen que existir en el diccionario. */
    etiquetaIds: number[];
}
