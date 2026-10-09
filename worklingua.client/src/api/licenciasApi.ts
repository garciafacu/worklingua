import { clienteHttp } from './clienteHttp';
import type { AsignarLicenciaRequest, InventarioResponse } from '../tipos/licencias';

export const licenciasApi = {
    /**
     * El inventario de licencias. Requiere Licencia.Listar.
     *
     * Sin Licencia.VerTodasLasEmpresas el backend ignora `empresaId` y devuelve
     * el de la empresa del usuario de la sesión.
     */
    inventario: (empresaId: number) =>
        clienteHttp.get<InventarioResponse>(`/licencias?empresaId=${empresaId}`),

    /** Asigna una licencia si hay cupo. Requiere Licencia.Asignar. */
    asignar: (cuerpo: AsignarLicenciaRequest) =>
        clienteHttp.post<{ mensaje: string }>('/licencias', cuerpo),

    /** Revoca una licencia y libera el cupo. Requiere Licencia.Revocar. */
    revocar: (licenciaId: number) =>
        clienteHttp.borrar<{ mensaje: string }>(`/licencias/${licenciaId}`),
};
