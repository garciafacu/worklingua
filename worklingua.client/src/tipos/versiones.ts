export interface VersionResponse {
    versionContenidoId: number;
    cursoId: number;
    numeroVersion: string;
    fechaVersion: string | null;
    observaciones: string | null;
    usuarioId: number | null;
    autor: string | null;
    contenidoXml: string | null;
    largoXml: number;
}

export interface DiferenciaVersionResponse {
    campo: string;
    izquierda: string;
    derecha: string;
}

export interface ResultadoRestauracionResponse {
    cursoId: number;
    omitidos: number;
    nombresOmitidos: string[];
}
