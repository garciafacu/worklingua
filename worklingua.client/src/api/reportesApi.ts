import type {
    FiltrosRendimiento,
    FiltrosReporte,
    GananciaPeriodoResponse,
    GananciaZonaResponse,
    RendimientoResponse,
    TableroResponse,
} from '../tipos/reportes';
import { aConsulta, clienteHttp } from './clienteHttp';

function consultaReporte(filtros: FiltrosReporte): string {
    return aConsulta({
        desde: filtros.desde,
        hasta: filtros.hasta,
        agrupacion: filtros.agrupacion,
        provincia: filtros.provincia,
    });
}

export const reportesApi = {
    ganancias: (filtros: FiltrosReporte) =>
        clienteHttp.get<GananciaPeriodoResponse[]>(`/reportes/ganancias${consultaReporte(filtros)}`),

    gananciasPorZona: (filtros: FiltrosReporte) =>
        clienteHttp.get<GananciaZonaResponse[]>(`/reportes/ganancias/zonas${consultaReporte(filtros)}`),

    tablero: () => clienteHttp.get<TableroResponse>('/reportes/tablero'),
};

function consultaRendimiento(filtros: FiltrosRendimiento): string {
    return aConsulta({
        empresaId: filtros.empresaId,
        desde: filtros.desde,
        hasta: filtros.hasta,
        departamentoId: filtros.departamentoId,
    });
}

export const rendimientoApi = {
    obtener: (filtros: FiltrosRendimiento) =>
        clienteHttp.get<RendimientoResponse>(`/rendimiento${consultaRendimiento(filtros)}`),
};
