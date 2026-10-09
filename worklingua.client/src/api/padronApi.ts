import { clienteHttp } from './clienteHttp';
import type { PadronRequest, ResultadoPadronResponse } from '../tipos/padron';

export const padronApi = {
    /**
     * Procesa el padrón. Requiere Usuario.Invitar.
     *
     * Con `confirmar` en false solo valida y devuelve el detalle fila por fila;
     * en true manda las invitaciones de las filas válidas. El archivo entero
     * viaja las dos veces: el backend revalida y no confía en la primera pasada.
     */
    procesar: (cuerpo: PadronRequest) =>
        clienteHttp.post<ResultadoPadronResponse>('/padron', cuerpo),
};
