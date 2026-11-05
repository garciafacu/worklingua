import type { EnviarConsultaContactoRequest } from '../tipos/contacto';
import type { MensajeResponse } from '../tipos/usuarios';
import { clienteHttp } from './clienteHttp';

export const contactoApi = {
    enviar: (datos: EnviarConsultaContactoRequest) => clienteHttp.post<MensajeResponse>('/contacto', datos),
};
