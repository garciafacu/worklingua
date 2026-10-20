import { clienteHttp } from './clienteHttp';
import type { GuardarOfertaRequest, OfertaResponse } from '../tipos/ofertas';

export const ofertasApi = {
    listarMias: () => clienteHttp.get<OfertaResponse[]>('/ofertas/mias'),

    listarAdministracion: () => clienteHttp.get<OfertaResponse[]>('/ofertas/administracion'),

    crear: (cuerpo: GuardarOfertaRequest) =>
        clienteHttp.post<OfertaResponse>('/ofertas/administracion', cuerpo),

    modificar: (ofertaId: number, cuerpo: GuardarOfertaRequest) =>
        clienteHttp.put<OfertaResponse>(`/ofertas/administracion/${ofertaId}`, cuerpo),

    baja: (ofertaId: number) => clienteHttp.borrar<void>(`/ofertas/administracion/${ofertaId}`),
};
