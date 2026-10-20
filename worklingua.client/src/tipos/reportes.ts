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
