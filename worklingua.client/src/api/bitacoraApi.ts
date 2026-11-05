import type { CriteriosBitacora, PaginaBitacoraResponse } from '../tipos/bitacora';
import { aConsulta, clienteHttp } from './clienteHttp';

function armarConsulta(criterios: CriteriosBitacora): string {
    return aConsulta({
        texto: criterios.texto,
        usuario: criterios.usuario,
        modulo: criterios.modulo,
        accion: criterios.accion,
        nivel: criterios.nivel,
        desde: criterios.desde,
        hasta: criterios.hasta && `${criterios.hasta}T23:59:59`,
        pagina: criterios.pagina,
        tamanioPagina: criterios.tamanioPagina,
    });
}

export const bitacoraApi = {
    buscar: (criterios: CriteriosBitacora = {}) =>
        clienteHttp.get<PaginaBitacoraResponse>(`/bitacora${armarConsulta(criterios)}`),
};
