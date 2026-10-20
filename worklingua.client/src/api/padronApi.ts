import { clienteHttp } from './clienteHttp';
import type { PadronRequest, ResultadoPadronResponse } from '../tipos/padron';

export const padronApi = {
    procesar: (cuerpo: PadronRequest) =>
        clienteHttp.post<ResultadoPadronResponse>('/padron', cuerpo),
};
