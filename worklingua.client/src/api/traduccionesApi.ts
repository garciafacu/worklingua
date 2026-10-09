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
    /**
     * El bundle de un idioma. Público: sin él no se puede pintar ni el login.
     *
     * Lo consume el backend de i18next definido en `src/i18n/backendTraducciones.ts`.
     */
    obtenerBundle: (codigoISO: string) =>
        clienteHttp.get<BundleTraducciones>(`/traducciones/${encodeURIComponent(codigoISO)}`),

    /** Backoffice. Requiere el permiso Traduccion.Listar. */
    listarAdministracion: (criterios: CriteriosTraduccion) =>
        clienteHttp.get<TraduccionAdminResponse[]>(
            `/traducciones/administracion${armarConsulta(criterios)}`,
        ),

    /** Guardado en lote de las filas editadas. Requiere Traduccion.Modificar. */
    guardar: (cuerpo: GuardarTraduccionesRequest) =>
        clienteHttp.put<MensajeResponse>('/traducciones', cuerpo),
};
