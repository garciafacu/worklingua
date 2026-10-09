import { clienteHttp } from './clienteHttp';
import type { MensajeResponse } from '../tipos/autenticacion';
import type { EnviarConsultaContactoRequest } from '../tipos/contacto';

export const contactoApi = {
    enviar: (datos: EnviarConsultaContactoRequest) =>
        clienteHttp.post<MensajeResponse>('/contacto', datos),
};
