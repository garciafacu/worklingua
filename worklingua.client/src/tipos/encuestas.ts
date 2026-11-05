export interface OpcionEncuestaResponse {
    opcionEncuestaId: number;
    encuestaId: number;
    texto: string;
    orden: number;
    total: number;
}

export interface EncuestaResponse {
    encuesta: {
        encuestaId: number;
        idiomaId: number;
        pregunta: string;
        descripcion: string | null;
        fechaDesde: string;
        fechaVencimiento: string;
        activo: boolean;
        esPublica: boolean;
        usuarioId: number;
        fechaAlta: string;
    };
    idioma: string;
    codigoISO: string;
    totalRespuestas: number;
    vigente: boolean;
    opcionElegidaId: number | null;
    opciones: OpcionEncuestaResponse[];
}

export interface GuardarEncuestaRequest {
    idiomaId: number;
    pregunta: string;
    descripcion: string | null;
    fechaDesde: string;
    fechaVencimiento: string;
    activo: boolean;
    esPublica: boolean;
    opciones: string[];
}

export interface ResponderEncuestaRequest {
    opcionEncuestaId: number;
}

export interface ResponderEncuestaPublicaRequest {
    opcionEncuestaId: number;
    tokenVisitante: string;
    tokenCaptcha: string;
}
