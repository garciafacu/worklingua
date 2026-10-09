import { clienteHttp } from './clienteHttp';
import type { SuscripcionResponse } from '../tipos/suscripciones';

export const suscripcionesApi = {
    /**
     * El plan de la empresa de la sesión.
     *
     * Responde 404 cuando la empresa no tiene suscripción, que es el caso de las
     * creadas antes del auto-registro. La pantalla lo trata como "sin plan
     * asignado" y no como un error.
     */
    obtenerLaMia: () => clienteHttp.get<SuscripcionResponse>('/suscripciones/mia'),
};
