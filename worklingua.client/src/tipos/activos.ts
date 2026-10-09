/**
 * Activos pedagógicos de un curso (CU-004-001), espejo de
 * `worklingua.Server/BE/BEActivoRespuesta.cs`.
 */

/** Tipos de recurso que acepta el Backoffice, espejo de `BLLActivoPedagogico`. */
export const TIPOS_ACTIVO = ['IMAGEN', 'AUDIO', 'VOCABULARIO'] as const;

export type TipoActivo = (typeof TIPOS_ACTIVO)[number];

/** Extensiones aceptadas por tipo; el backend vuelve a validarlas. */
export const EXTENSIONES_POR_TIPO: Record<TipoActivo, string> = {
    IMAGEN: '.png,.jpg,.jpeg,.webp',
    AUDIO: '.mp3,.wav,.ogg',
    VOCABULARIO: '.pdf,.csv,.txt',
};

/** Tope de tamaño, espejo de `BLLActivoPedagogico.MegabytesMaximos`. */
export const MEGABYTES_MAXIMOS = 10;

export interface ActivoResponse {
    activoPedagogicoId: number;
    cursoId: number;
    /** `null` es material general del curso; con valor, contenido de esa lección. */
    moduloId: number | null;
    /** Nombre del módulo al que pertenece, para mostrarlo en el panel. */
    modulo: string | null;
    nombre: string;
    tipoContenido: TipoActivo;
    descripcion: string | null;
    /** Ruta servida por la aplicación, por ejemplo /activos/xxx.png */
    urlArchivo: string | null;
    /** Baja lógica: false es "dado de baja". */
    activo: boolean;
    /** BORRADOR o PUBLICADO. */
    estado: string;
}
