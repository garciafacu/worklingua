import type { SectorCurso } from './cursos';

export interface NoticiaResponse {
    noticiaId: number;
    idiomaId: number;
    titulo: string;
    resumen: string | null;
    contenido: string;
    fechaPublicacion: string;
    sector: SectorCurso | null;
}

export interface NoticiaAdminResponse extends NoticiaResponse {
    usuarioId: number;
    fechaAlta: string;
    activo: boolean;
}

export interface GuardarNoticiaRequest {
    idiomaId: number;
    titulo: string;
    resumen: string | null;
    contenido: string;
    fechaPublicacion: string | null;
    sector: SectorCurso | null;
}

export interface SuscribirNewsletterRequest {
    email: string;
    idioma: string;
    sectores: SectorCurso[];
}

export interface SuscriptorNewsletterResponse {
    suscriptorId: number;
    email: string;
    idiomaId: number;
    confirmado: boolean;
    fechaAlta: string;
    fechaConfirmacion: string | null;
    fechaBaja: string | null;
    activo: boolean;
    sectores: SectorCurso[];
}

export interface EnvioNewsletterResponse {
    envioId: number;
    idiomaId: number;
    usuarioId: number;
    asunto: string;
    fechaEnvio: string;
    cantidadEnviados: number;
    cantidadFallidos: number;
}

export interface EnviarNewsletterRequest {
    idiomaId: number;
    asunto: string;
    noticiaIds: number[];
}

export interface VistaPreviaNewsletterResponse {
    html: string;
    cantidadDestinatarios: number;
    sectores: SectorCurso[];
}
