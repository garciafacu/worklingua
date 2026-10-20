import { clienteHttp } from './clienteHttp';
import type {
    FiltrosReporte,
    GananciaPeriodoResponse,
    GananciaZonaResponse,
    TableroResponse,
} from '../tipos/reportes';

function aConsulta(filtros: FiltrosReporte): string {
    const parametros = new URLSearchParams();

    if (filtros.desde) {
        parametros.set('desde', filtros.desde);
    }

    if (filtros.hasta) {
        parametros.set('hasta', filtros.hasta);
    }

    if (filtros.agrupacion) {
        parametros.set('agrupacion', filtros.agrupacion);
    }

    if (filtros.provincia) {
        parametros.set('provincia', filtros.provincia);
    }

    const consulta = parametros.toString();

    return consulta ? `?${consulta}` : '';
}

export const reportesApi = {
    ganancias: (filtros: FiltrosReporte) =>
        clienteHttp.get<GananciaPeriodoResponse[]>(`/reportes/ganancias${aConsulta(filtros)}`),

    gananciasPorZona: (filtros: FiltrosReporte) =>
        clienteHttp.get<GananciaZonaResponse[]>(`/reportes/ganancias/zonas${aConsulta(filtros)}`),

    tablero: () => clienteHttp.get<TableroResponse>('/reportes/tablero'),
};
