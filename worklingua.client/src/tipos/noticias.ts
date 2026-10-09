/**
 * Noticia publicada, espejo de `BE/BENoticiaRespuesta.cs`.
 *
 * `idiomaId` es el idioma de PLATAFORMA en el que está escrita. Si el idioma de
 * la interfaz no tiene noticias, la API devuelve las del español y la pantalla
 * lo detecta comparando este campo con el idioma actual.
 */
export interface NoticiaResponse {
    noticiaId: number;
    idiomaId: number;
    titulo: string;
    resumen: string | null;
    /** Texto plano: se muestra respetando los saltos de línea, nunca como HTML. */
    contenido: string;
    fechaPublicacion: string;
}

/** Noticia vista desde el Backoffice, espejo de `BE/BENoticia.cs`. */
export interface NoticiaAdminResponse extends NoticiaResponse {
    usuarioId: number;
    fechaAlta: string;
    activo: boolean;
}

/** Cuerpo del alta y de la modificación, espejo de `BE/BEGuardarNoticia.cs`. */
export interface GuardarNoticiaRequest {
    idiomaId: number;
    titulo: string;
    resumen: string | null;
    contenido: string;
    /** Sin fecha, el backend la publica en el momento. */
    fechaPublicacion: string | null;
}
