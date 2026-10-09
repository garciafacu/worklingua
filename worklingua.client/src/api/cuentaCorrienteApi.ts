import { clienteHttp } from './clienteHttp';
import type { EstadoCuentaCorrienteResponse } from '../tipos/cuentaCorriente';

export const cuentaCorrienteApi = {
    /** Estado de cuenta de la empresa de la sesión. Requiere CuentaCorriente.Consultar. */
    obtener: () => clienteHttp.get<EstadoCuentaCorrienteResponse>('/cuenta-corriente'),
};
