import { clienteHttp } from './clienteHttp';
import type { MensajeResponse } from '../tipos/autenticacion';
import type {
    EnviarNewsletterRequest,
    EnvioNewsletterResponse,
    SuscribirNewsletterRequest,
    SuscriptorNewsletterResponse,
    VistaPreviaNewsletterResponse,
} from '../tipos/newsletter';

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
