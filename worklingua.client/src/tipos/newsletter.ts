/** Cuerpo de la suscripción pública, espejo de `BE/BESuscribirNewsletter.cs`. */
export interface SuscribirNewsletterRequest {
    email: string;
    /** Código ISO del idioma de la interfaz: el newsletter llega en ese idioma. */
    idioma: string;
}

/** Suscriptor visto desde el Backoffice, espejo de `BE/BESuscriptorNewsletterRespuesta.cs`. */
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

/** Un envío del historial, espejo de `BE/BEEnvioNewsletter.cs`. */
export interface EnvioNewsletterResponse {
    envioId: number;
    idiomaId: number;
    usuarioId: number;
    asunto: string;
    fechaEnvio: string;
    cantidadEnviados: number;
    cantidadFallidos: number;
}

/** Cuerpo del envío y de la vista previa, espejo de `BE/BEEnviarNewsletter.cs`. */
export interface EnviarNewsletterRequest {
    idiomaId: number;
    asunto: string;
    noticiaIds: number[];
}

/** Espejo de `BE/BEVistaPreviaNewsletterRespuesta.cs`. */
export interface VistaPreviaNewsletterResponse {
    html: string;
    cantidadDestinatarios: number;
}
