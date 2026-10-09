/** Opción de respuesta con sus votos, espejo de `BE/BEOpcionEncuesta.cs`. */
export interface OpcionEncuestaResponse {
    opcionEncuestaId: number;
    encuestaId: number;
    texto: string;
    orden: number;
    /** Votos recibidos. Llega en 0 mientras el usuario no respondió la encuesta. */
    total: number;
}

/** Encuesta con sus opciones, espejo de `BE/BEEncuestaConDetalle.cs`. */
export interface EncuestaResponse {
    encuesta: {
        encuestaId: number;
        idiomaId: number;
        pregunta: string;
        descripcion: string | null;
        /** `yyyy-MM-dd`. */
        fechaDesde: string;
        /** `yyyy-MM-dd`. */
        fechaVencimiento: string;
        activo: boolean;
        usuarioId: number;
        fechaAlta: string;
    };
    idioma: string;
    codigoISO: string;
    totalRespuestas: number;
    /** Activa, ya empezó y todavía no venció. */
    vigente: boolean;
    /** Opción que eligió el usuario de la sesión, o null si no respondió. */
    opcionElegidaId: number | null;
    opciones: OpcionEncuestaResponse[];
}

/** Cuerpo del alta y de la modificación, espejo de `BE/BEGuardarEncuesta.cs`. */
export interface GuardarEncuestaRequest {
    idiomaId: number;
    pregunta: string;
    descripcion: string | null;
    /** `yyyy-MM-dd`. */
    fechaDesde: string;
    /** `yyyy-MM-dd`. */
    fechaVencimiento: string;
    activo: boolean;
    /**
     * Textos de las opciones, en orden. El backend las ignora cuando la encuesta
     * ya tiene respuestas.
     */
    opciones: string[];
}

/** Cuerpo del voto, espejo de `BE/BEResponderEncuesta.cs`. */
export interface ResponderEncuestaRequest {
    opcionEncuestaId: number;
}
