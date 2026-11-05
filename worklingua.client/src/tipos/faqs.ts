export interface CategoriaFaqResponse {
    categoriaFaqId: number;
    idiomaId: number;
    titulo: string;
    orden: number;
    activo: boolean;
    fechaAlta: string;
    idioma: string;
    codigoISO: string;
    totalPreguntas: number;
}

export interface PreguntaFrecuenteResponse {
    preguntaFrecuenteId: number;
    categoriaFaqId: number;
    pregunta: string;
    respuesta: string;
    enlace: string | null;
    enlaceEtiqueta: string | null;
    orden: number;
    activo: boolean;
    fechaAlta: string;
    categoria: string;
    idiomaId: number;
}

export interface CategoriaFaqConPreguntasResponse {
    categoria: CategoriaFaqResponse;
    preguntas: PreguntaFrecuenteResponse[];
}

export interface GuardarCategoriaFaqRequest {
    idiomaId: number;
    titulo: string;
    orden: number;
    activo: boolean;
}

export interface GuardarPreguntaFrecuenteRequest {
    categoriaFaqId: number;
    pregunta: string;
    respuesta: string;
    enlace: string | null;
    enlaceEtiqueta: string | null;
    orden: number;
    activo: boolean;
}

export interface EnviarConsultaContactoRequest {
    nombre: string;
    email: string;
    asunto: string;
    mensaje: string;
    tokenCaptcha: string;
    codigoIdioma: string;
}

export interface ConsultaPublicaResponse {
    mensaje: string;
    ticketId: number;
    token: string;
}

export interface ResponderConsultaPublicaRequest {
    token: string;
    texto: string;
}
