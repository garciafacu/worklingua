import type {
    CategoriaFaqConPreguntasResponse,
    CategoriaFaqResponse,
    ConsultaPublicaResponse,
    EnviarConsultaContactoRequest,
    GuardarCategoriaFaqRequest,
    GuardarPreguntaFrecuenteRequest,
    PreguntaFrecuenteResponse,
    ResponderConsultaPublicaRequest,
} from '../tipos/faqs';
import type { TicketResponse } from '../tipos/tickets';
import { clienteHttp } from './clienteHttp';

export const faqsApi = {
    listarPublicadas: (idioma: string) =>
        clienteHttp.get<CategoriaFaqConPreguntasResponse[]>(
            `/faqs?idioma=${encodeURIComponent(idioma)}`,
        ),

    listarCategorias: () =>
        clienteHttp.get<CategoriaFaqResponse[]>('/faqs/administracion/categorias'),

    listarPreguntas: (categoriaFaqId?: number) =>
        clienteHttp.get<PreguntaFrecuenteResponse[]>(
            categoriaFaqId === undefined
                ? '/faqs/administracion/preguntas'
                : `/faqs/administracion/preguntas?categoriaFaqId=${categoriaFaqId}`,
        ),

    crearCategoria: (cuerpo: GuardarCategoriaFaqRequest) =>
        clienteHttp.post<CategoriaFaqResponse>('/faqs/administracion/categorias', cuerpo),

    modificarCategoria: (categoriaFaqId: number, cuerpo: GuardarCategoriaFaqRequest) =>
        clienteHttp.put<CategoriaFaqResponse>(
            `/faqs/administracion/categorias/${categoriaFaqId}`,
            cuerpo,
        ),

    cambiarEstadoCategoria: (categoriaFaqId: number, activo: boolean) =>
        clienteHttp.patch<CategoriaFaqResponse>(
            `/faqs/administracion/categorias/${categoriaFaqId}/estado`,
            { activo },
        ),

    bajaCategoria: (categoriaFaqId: number) =>
        clienteHttp.borrar<void>(`/faqs/administracion/categorias/${categoriaFaqId}`),

    crearPregunta: (cuerpo: GuardarPreguntaFrecuenteRequest) =>
        clienteHttp.post<PreguntaFrecuenteResponse>('/faqs/administracion/preguntas', cuerpo),

    modificarPregunta: (preguntaFrecuenteId: number, cuerpo: GuardarPreguntaFrecuenteRequest) =>
        clienteHttp.put<PreguntaFrecuenteResponse>(
            `/faqs/administracion/preguntas/${preguntaFrecuenteId}`,
            cuerpo,
        ),

    cambiarEstadoPregunta: (preguntaFrecuenteId: number, activo: boolean) =>
        clienteHttp.patch<PreguntaFrecuenteResponse>(
            `/faqs/administracion/preguntas/${preguntaFrecuenteId}/estado`,
            { activo },
        ),

    bajaPregunta: (preguntaFrecuenteId: number) =>
        clienteHttp.borrar<void>(`/faqs/administracion/preguntas/${preguntaFrecuenteId}`),
};

export const contactoApi = {
    enviar: (datos: EnviarConsultaContactoRequest) =>
        clienteHttp.post<ConsultaPublicaResponse>('/contacto', datos),

    obtenerConsulta: (token: string) =>
        clienteHttp.get<TicketResponse>(`/contacto/consulta?token=${encodeURIComponent(token)}`),

    responderConsulta: (datos: ResponderConsultaPublicaRequest) =>
        clienteHttp.post<TicketResponse>('/contacto/consulta/mensajes', datos),
};
