import { clienteHttp } from './clienteHttp';
import type { AsignarLicenciaRequest, InventarioResponse } from '../tipos/licencias';

export const licenciasApi = {
    inventario: (empresaId: number) =>
        clienteHttp.get<InventarioResponse>(`/licencias?empresaId=${empresaId}`),

    asignar: (cuerpo: AsignarLicenciaRequest) =>
        clienteHttp.post<{ mensaje: string }>('/licencias', cuerpo),

    revocar: (licenciaId: number) =>
        clienteHttp.borrar<{ mensaje: string }>(`/licencias/${licenciaId}`),
};
