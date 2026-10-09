import { clienteHttp } from './clienteHttp';
import type {
    GuardarIdiomaRequest,
    IdiomaAdminResponse,
    IdiomaResponse,
} from '../tipos/idiomas';

export const idiomasApi = {
    /**
     * Los idiomas de plataforma activos: los que ofrece el selector del sitio.
     * No requiere sesión.
     */
    listar: () => clienteHttp.get<IdiomaResponse[]>('/idiomas'),

    /**
     * Backoffice. Requiere el permiso Idioma.Listar.
     *
     * Incluye los desactivados, para poder reactivarlos. Es la excepción a la
     * regla de la Etapa 6, que sacó las bajas del resto de los listados.
     */
    listarAdministracion: () => clienteHttp.get<IdiomaAdminResponse[]>('/idiomas/administracion'),

    crear: (cuerpo: GuardarIdiomaRequest) =>
        clienteHttp.post<IdiomaAdminResponse>('/idiomas', cuerpo),

    modificar: (idiomaId: number, cuerpo: GuardarIdiomaRequest) =>
        clienteHttp.put<IdiomaAdminResponse>(`/idiomas/${idiomaId}`, cuerpo),

    /** Activa o desactiva un idioma sin darlo de baja. */
    cambiarEstado: (idiomaId: number, activo: boolean) =>
        clienteHttp.patch<IdiomaAdminResponse>(`/idiomas/${idiomaId}/estado`, { activo }),

    baja: (idiomaId: number) => clienteHttp.borrar<void>(`/idiomas/${idiomaId}`),
};
