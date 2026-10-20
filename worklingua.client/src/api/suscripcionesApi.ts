import { clienteHttp } from './clienteHttp';
import type { SuscripcionResponse } from '../tipos/suscripciones';

export const suscripcionesApi = {
    obtenerLaMia: () => clienteHttp.get<SuscripcionResponse>('/suscripciones/mia'),
};
