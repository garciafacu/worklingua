/** Cómo se agrupan las ganancias, espejo de las constantes de `BLL/BLLReporte.cs`. */
export const AGRUPACIONES = ['DIA', 'SEMANA', 'MES', 'ANIO'] as const;

export type Agrupacion = (typeof AGRUPACIONES)[number];

/** Una fila del reporte por período, espejo de `BE/BEGananciaPeriodo.cs`. */
export interface GananciaPeriodoResponse {
    /** `2026-09-22`, `2026-W39`, `2026-09` o `2026`, según la agrupación. */
    clave: string;
    facturado: number;
    cobrado: number;
    notasCredito: number;
    /** Cobrado menos notas de crédito: la ganancia. */
    neto: number;
}

/** Una fila del reporte por zona, espejo de `BE/BEGananciaZona.cs`. */
export interface GananciaZonaResponse {
    zona: string;
    facturado: number;
    cobrado: number;
    notasCredito: number;
    neto: number;
    empresas: number;
}

/** Indicadores del tablero, espejo de `BE/BETablero.cs`. */
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

/** Criterios del reporte; todos opcionales. */
export interface FiltrosReporte {
    /** `yyyy-MM-dd`. */
    desde?: string;
    /** `yyyy-MM-dd`. */
    hasta?: string;
    agrupacion?: Agrupacion;
    provincia?: string;
}
