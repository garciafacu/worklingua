export interface NoticiaResponse {
    noticiaId: number;
    idiomaId: number;
    titulo: string;
    resumen: string | null;
    contenido: string;
    fechaPublicacion: string;
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
}

export interface SuscribirNewsletterRequest {
    email: string;
    idioma: string;
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
}
