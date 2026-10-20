import { clienteHttp } from './clienteHttp';
import type {
    CulturaAdminResponse,
    CulturaResponse,
    GuardarCulturaRequest,
} from '../tipos/culturas';

export const culturasApi = {
    listar: () => clienteHttp.get<CulturaResponse[]>('/culturas'),

    listarAdministracion: () => clienteHttp.get<CulturaAdminResponse[]>('/culturas/administracion'),

    crear: (cuerpo: GuardarCulturaRequest) =>
        clienteHttp.post<CulturaAdminResponse>('/culturas', cuerpo),

    modificar: (culturaId: number, cuerpo: GuardarCulturaRequest) =>
        clienteHttp.put<CulturaAdminResponse>(`/culturas/${culturaId}`, cuerpo),

    cambiarEstado: (culturaId: number, activo: boolean) =>
        clienteHttp.patch<CulturaAdminResponse>(`/culturas/${culturaId}/estado`, { activo }),

    baja: (culturaId: number) => clienteHttp.borrar<void>(`/culturas/${culturaId}`),
};
