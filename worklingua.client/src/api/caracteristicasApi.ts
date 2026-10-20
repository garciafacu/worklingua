import { clienteHttp } from './clienteHttp';
import type {
    CaracteristicaAdminResponse,
    CaracteristicaResponse,
    GuardarCaracteristicaRequest,
} from '../tipos/caracteristicas';

export const caracteristicasApi = {
    listar: () => clienteHttp.get<CaracteristicaResponse[]>('/caracteristicas'),

    listarAdministracion: () =>
        clienteHttp.get<CaracteristicaAdminResponse[]>('/caracteristicas/administracion'),

    crear: (cuerpo: GuardarCaracteristicaRequest) =>
        clienteHttp.post<CaracteristicaAdminResponse>('/caracteristicas', cuerpo),

    modificar: (caracteristicaId: number, cuerpo: GuardarCaracteristicaRequest) =>
        clienteHttp.put<CaracteristicaAdminResponse>(`/caracteristicas/${caracteristicaId}`, cuerpo),

    baja: (caracteristicaId: number) =>
        clienteHttp.borrar<void>(`/caracteristicas/${caracteristicaId}`),
};
