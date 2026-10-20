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
