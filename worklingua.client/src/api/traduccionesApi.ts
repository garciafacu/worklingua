import { clienteHttp } from './clienteHttp';
import type {
    BundleTraducciones,
    CriteriosTraduccion,
    GuardarTraduccionesRequest,
    TraduccionAdminResponse,
} from '../tipos/traducciones';
import type { MensajeResponse } from '../tipos/autenticacion';

function armarConsulta(criterios: CriteriosTraduccion): string {
    const parametros = new URLSearchParams();

    parametros.set('idiomaId', String(criterios.idiomaId));

    if (criterios.clave?.trim()) {
        parametros.set('clave', criterios.clave.trim());
    }

    if (criterios.texto?.trim()) {
        parametros.set('texto', criterios.texto.trim());
    }

    if (criterios.soloPendientes) {
        parametros.set('soloPendientes', 'true');
    }

    return `?${parametros.toString()}`;
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
