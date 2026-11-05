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

export interface EtiquetaResponse {
    etiquetaId: number;
    nombre: string;
    activo: boolean | null;
    fechaAlta: string | null;
}

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

export const TIPOS_ACTIVO = ['IMAGEN', 'AUDIO', 'VOCABULARIO'] as const;

export type TipoActivo = (typeof TIPOS_ACTIVO)[number];

export const EXTENSIONES_POR_TIPO: Record<TipoActivo, string> = {
    IMAGEN: '.png,.jpg,.jpeg,.webp',
    AUDIO: '.mp3,.wav,.ogg',
    VOCABULARIO: '.pdf,.csv,.txt',
};

export const MEGABYTES_MAXIMOS = 10;

export interface ActivoResponse {
    activoPedagogicoId: number;
    cursoId: number;
    moduloId: number | null;
    modulo: string | null;
    nombre: string;
    tipoContenido: TipoActivo;
    descripcion: string | null;
    urlArchivo: string | null;
    activo: boolean;
    estado: string;
}

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
