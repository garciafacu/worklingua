import { clienteHttp } from './clienteHttp';
import type {
    CaracteristicaAdminResponse,
    CaracteristicaResponse,
    GuardarCaracteristicaRequest,
} from '../tipos/caracteristicas';

export const caracteristicasApi = {
    /** Catálogo público: no requiere sesión. Solo las activas, ya ordenadas. */
    listar: () => clienteHttp.get<CaracteristicaResponse[]>('/caracteristicas'),

    /** Backoffice: incluye las dadas de baja. Requiere el permiso Caracteristica.Listar. */
    listarAdministracion: () =>
        clienteHttp.get<CaracteristicaAdminResponse[]>('/caracteristicas/administracion'),

    crear: (cuerpo: GuardarCaracteristicaRequest) =>
        clienteHttp.post<CaracteristicaAdminResponse>('/caracteristicas', cuerpo),

    modificar: (caracteristicaId: number, cuerpo: GuardarCaracteristicaRequest) =>
        clienteHttp.put<CaracteristicaAdminResponse>(`/caracteristicas/${caracteristicaId}`, cuerpo),

    baja: (caracteristicaId: number) =>
        clienteHttp.borrar<void>(`/caracteristicas/${caracteristicaId}`),
};
