/**
 * Versiones y clonado de cursos (CU-004-004 y CU-004-002), espejo de los BE de
 * `worklingua.Server/BE/BEVersionContenido.cs` y compañía.
 */

/** Un punto de restauración. `contenidoXml` solo viene al pedir la versión completa. */
export interface VersionResponse {
    versionContenidoId: number;
    cursoId: number;
    numeroVersion: string;
    fechaVersion: string | null;
    observaciones: string | null;
    usuarioId: number | null;
    autor: string | null;
    contenidoXml: string | null;
    /** Tamaño del documento XML, en caracteres. */
    largoXml: number;
}

/** Un campo que cambió entre dos versiones, espejo de `BEDiferenciaVersion.cs`. */
export interface DiferenciaVersionResponse {
    /** Clave de traducción: `admin.cursos.versiones.campo.<campo>`. */
    campo: string;
    izquierda: string;
    derecha: string;
}

/**
 * El resultado de restaurar o clonar, espejo de `BEResultadoRestauracion.cs`.
 *
 * `omitidos` son los activos cuyo archivo ya no está en disco: se dejan afuera
 * en vez de arrastrar un enlace roto.
 */
export interface ResultadoRestauracionResponse {
    cursoId: number;
    omitidos: number;
    nombresOmitidos: string[];
}
