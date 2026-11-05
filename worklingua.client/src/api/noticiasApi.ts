import type {
    EnviarNewsletterRequest,
    EnvioNewsletterResponse,
    GuardarNoticiaRequest,
    NoticiaAdminResponse,
    NoticiaResponse,
    SuscribirNewsletterRequest,
    SuscriptorNewsletterResponse,
    VistaPreviaNewsletterResponse,
} from '../tipos/noticias';
import type { MensajeResponse } from '../tipos/usuarios';
import { clienteHttp } from './clienteHttp';

export const noticiasApi = {
    listarPublicadas: (idioma: string) =>
        clienteHttp.get<NoticiaResponse[]>(`/noticias?idioma=${encodeURIComponent(idioma)}`),

    obtenerPublicada: (noticiaId: number) =>
        clienteHttp.get<NoticiaResponse>(`/noticias/${noticiaId}`),

    listarAdministracion: (idiomaId?: number) =>
        clienteHttp.get<NoticiaAdminResponse[]>(
            idiomaId ? `/noticias/administracion?idiomaId=${idiomaId}` : '/noticias/administracion',
        ),

    crear: (cuerpo: GuardarNoticiaRequest) =>
        clienteHttp.post<NoticiaAdminResponse>('/noticias', cuerpo),

    modificar: (noticiaId: number, cuerpo: GuardarNoticiaRequest) =>
        clienteHttp.put<NoticiaAdminResponse>(`/noticias/${noticiaId}`, cuerpo),

    baja: (noticiaId: number) => clienteHttp.borrar<void>(`/noticias/${noticiaId}`),
};

export const newsletterApi = {
    suscribir: (datos: SuscribirNewsletterRequest) =>
        clienteHttp.post<MensajeResponse>('/newsletter/suscripciones', datos),

    confirmar: (token: string) =>
        clienteHttp.post<MensajeResponse>('/newsletter/suscripciones/confirmar', { token }),

    darDeBaja: (token: string) =>
        clienteHttp.post<MensajeResponse>('/newsletter/suscripciones/baja', { token }),

    listarSuscriptores: () =>
        clienteHttp.get<SuscriptorNewsletterResponse[]>('/newsletter/suscriptores'),

    listarEnvios: () => clienteHttp.get<EnvioNewsletterResponse[]>('/newsletter/envios'),

    previsualizar: (cuerpo: EnviarNewsletterRequest) =>
        clienteHttp.post<VistaPreviaNewsletterResponse>('/newsletter/envios/previsualizar', cuerpo),

    enviar: (cuerpo: EnviarNewsletterRequest) =>
        clienteHttp.post<EnvioNewsletterResponse>('/newsletter/envios', cuerpo),
};
