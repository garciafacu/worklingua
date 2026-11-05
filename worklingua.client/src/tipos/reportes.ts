export const AGRUPACIONES = ['DIA', 'SEMANA', 'MES', 'ANIO'] as const;

export type Agrupacion = (typeof AGRUPACIONES)[number];

export interface GananciaPeriodoResponse {
    clave: string;
    facturado: number;
    cobrado: number;
    notasCredito: number;
    neto: number;
}

export interface GananciaZonaResponse {
    zona: string;
    facturado: number;
    cobrado: number;
    notasCredito: number;
    neto: number;
    empresas: number;
}

export interface TableroResponse {
    ingresosMes: number;
    ingresosMesAnterior: number;
    deudaTotal: number;
    empresasConDeuda: number;
    empresasActivas: number;
    contratacionesActivas: number;
    licenciasAsignadas: number;
    licenciasContratadas: number;
    empresasSinCupo: number;
    planes: { plan: string; cantidad: number }[];
}

export interface FiltrosReporte {
    desde?: string;
    hasta?: string;
    agrupacion?: Agrupacion;
    provincia?: string;
}

export interface RendimientoResumenResponse {
    empleadosConActividad: number;
    modulosIniciados: number;
    modulosCompletados: number;
    cursosCompletados: number;
    avancePromedio: number;
    facturado: number | null;
    costoPorModulo: number | null;
    costoPorEmpleado: number | null;
}

export interface RendimientoDepartamentoResponse {
    departamentoId: number | null;
    departamento: string | null;
    empleados: number;
    empleadosConActividad: number;
    modulosCompletados: number;
    avancePromedio: number;
}

export interface RendimientoEmpleadoResponse {
    usuarioId: number;
    empleado: string;
    email: string;
    departamento: string | null;
    modulosIniciados: number;
    modulosCompletados: number;
    avancePromedio: number;
    ultimaActividad: string | null;
}

export interface RendimientoResponse {
    empresaId: number;
    empresa: string;
    resumen: RendimientoResumenResponse;
    departamentos: RendimientoDepartamentoResponse[];
    empleados: RendimientoEmpleadoResponse[];
}

export interface FiltrosRendimiento {
    empresaId?: number;
    desde?: string;
    hasta?: string;
    departamentoId?: number;
}
