import { clienteHttp } from './clienteHttp';
import type { FiltrosRendimiento, RendimientoResponse } from '../tipos/rendimiento';

function aConsulta(filtros: FiltrosRendimiento): string {
    const parametros = new URLSearchParams();

    if (filtros.empresaId) {
        parametros.set('empresaId', String(filtros.empresaId));
    }

    if (filtros.desde) {
        parametros.set('desde', filtros.desde);
    }

    if (filtros.hasta) {
        parametros.set('hasta', filtros.hasta);
    }

    if (filtros.departamentoId) {
        parametros.set('departamentoId', String(filtros.departamentoId));
    }

    const consulta = parametros.toString();

    return consulta ? `?${consulta}` : '';
}

export const rendimientoApi = {
    /**
     * El panel completo: cifras, comparación entre departamentos y detalle por
     * empleado. Requiere Curso.VerProgresoEmpresa.
     *
     * Sin Curso.VerTodasLasEmpresas el backend ignora `empresaId` y responde
     * con la empresa del usuario de la sesión.
     */
    obtener: (filtros: FiltrosRendimiento) =>
        clienteHttp.get<RendimientoResponse>(`/rendimiento${aConsulta(filtros)}`),
};
