import { clienteHttp } from './clienteHttp';
import type { EstadoCuentaCorrienteResponse } from '../tipos/cuentaCorriente';

export const cuentaCorrienteApi = {
    obtener: () => clienteHttp.get<EstadoCuentaCorrienteResponse>('/cuenta-corriente'),
};
