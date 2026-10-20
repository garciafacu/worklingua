import { clienteHttp } from './clienteHttp';
import type {
    GuardarNoticiaRequest,
    NoticiaAdminResponse,
    NoticiaResponse,
} from '../tipos/noticias';

export const noticiasApi = {
    listarPublicadas: (idioma: string) =>
        clienteHttp.get<NoticiaResponse[]>(`/noticias?idioma=${encodeURIComponent(idioma)}`),

    obtenerPublicada: (noticiaId: number) =>
        clienteHttp.get<NoticiaResponse>(`/noticias/${noticiaId}`),

    listarAdministracion: (idiomaId?: number) =>
        clienteHttp.get<NoticiaAdminResponse[]>(
            idiomaId ? `/noticias/administracion?idiomaId=${idiomaId}` : '/noticias/administracion',
        ),

    crear: (cuerpo: GuardarNoticiaRequest) =>
        clienteHttp.post<NoticiaAdminResponse>('/noticias', cuerpo),

    modificar: (noticiaId: number, cuerpo: GuardarNoticiaRequest) =>
        clienteHttp.put<NoticiaAdminResponse>(`/noticias/${noticiaId}`, cuerpo),

    baja: (noticiaId: number) => clienteHttp.borrar<void>(`/noticias/${noticiaId}`),
};
