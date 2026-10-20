import { clienteHttp } from './clienteHttp';
import type {
    GuardarIdiomaRequest,
    IdiomaAdminResponse,
    IdiomaResponse,
} from '../tipos/idiomas';

export const idiomasApi = {
    listar: () => clienteHttp.get<IdiomaResponse[]>('/idiomas'),

    listarAdministracion: () => clienteHttp.get<IdiomaAdminResponse[]>('/idiomas/administracion'),

    crear: (cuerpo: GuardarIdiomaRequest) =>
        clienteHttp.post<IdiomaAdminResponse>('/idiomas', cuerpo),

    modificar: (idiomaId: number, cuerpo: GuardarIdiomaRequest) =>
        clienteHttp.put<IdiomaAdminResponse>(`/idiomas/${idiomaId}`, cuerpo),

    cambiarEstado: (idiomaId: number, activo: boolean) =>
        clienteHttp.patch<IdiomaAdminResponse>(`/idiomas/${idiomaId}/estado`, { activo }),

    baja: (idiomaId: number) => clienteHttp.borrar<void>(`/idiomas/${idiomaId}`),
};
