import { clienteHttp } from './clienteHttp';
import type { EtiquetaResponse } from '../tipos/etiquetas';

export const etiquetasApi = {
    listar: () => clienteHttp.get<EtiquetaResponse[]>('/etiquetas'),

    crear: (nombre: string) => clienteHttp.post<EtiquetaResponse>('/etiquetas', { nombre }),
};
