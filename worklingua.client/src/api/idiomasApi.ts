import type {
    BundleTraducciones,
    CriteriosTraduccion,
    CulturaAdminResponse,
    CulturaResponse,
    GuardarCulturaRequest,
    GuardarIdiomaRequest,
    GuardarTraduccionesRequest,
    IdiomaAdminResponse,
    IdiomaResponse,
    TraduccionAdminResponse,
} from '../tipos/idiomas';
import type { MensajeResponse } from '../tipos/usuarios';
import { aConsulta, clienteHttp } from './clienteHttp';

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

function armarConsulta(criterios: CriteriosTraduccion): string {
    return aConsulta({
        idiomaId: String(criterios.idiomaId),
        clave: criterios.clave,
        texto: criterios.texto,
        soloPendientes: criterios.soloPendientes,
    });
}

export const traduccionesApi = {
    obtenerBundle: (codigoISO: string) =>
        clienteHttp.get<BundleTraducciones>(`/traducciones/${encodeURIComponent(codigoISO)}`),

    listarAdministracion: (criterios: CriteriosTraduccion) =>
        clienteHttp.get<TraduccionAdminResponse[]>(
            `/traducciones/administracion${armarConsulta(criterios)}`,
        ),

    guardar: (cuerpo: GuardarTraduccionesRequest) =>
        clienteHttp.put<MensajeResponse>('/traducciones', cuerpo),
};
