import { clienteHttp } from './clienteHttp';
import type {
    CulturaAdminResponse,
    CulturaResponse,
    GuardarCulturaRequest,
} from '../tipos/culturas';

export const culturasApi = {
    /** Las culturas activas. Público: el sitio las necesita antes del login. */
    listar: () => clienteHttp.get<CulturaResponse[]>('/culturas'),

    /** Backoffice. Requiere el permiso Cultura.Listar. Incluye las desactivadas. */
    listarAdministracion: () => clienteHttp.get<CulturaAdminResponse[]>('/culturas/administracion'),

    crear: (cuerpo: GuardarCulturaRequest) =>
        clienteHttp.post<CulturaAdminResponse>('/culturas', cuerpo),

    modificar: (culturaId: number, cuerpo: GuardarCulturaRequest) =>
        clienteHttp.put<CulturaAdminResponse>(`/culturas/${culturaId}`, cuerpo),

    cambiarEstado: (culturaId: number, activo: boolean) =>
        clienteHttp.patch<CulturaAdminResponse>(`/culturas/${culturaId}/estado`, { activo }),

    baja: (culturaId: number) => clienteHttp.borrar<void>(`/culturas/${culturaId}`),
};
