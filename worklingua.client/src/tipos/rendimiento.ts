/**
 * Panel de rendimiento formativo (CU-001-004), espejo de los BE de
 * `worklingua.Server/BE/BERendimiento*.cs`.
 */

/** Cifras de cabecera, espejo de `BERendimientoResumen.cs`. */
export interface RendimientoResumenResponse {
    empleadosConActividad: number;
    modulosIniciados: number;
    modulosCompletados: number;
    cursosCompletados: number;
    avancePromedio: number;
    /**
     * Los tres importes vienen en null cuando se filtra por departamento: la
     * capacitación se factura por empresa y repartirla por sector sería
     * inventar un número.
     */
    facturado: number | null;
    costoPorModulo: number | null;
    costoPorEmpleado: number | null;
}

/** Una fila de la comparación entre departamentos, espejo de `BERendimientoDepartamento.cs`. */
export interface RendimientoDepartamentoResponse {
    /** Null es la fila de los empleados sin departamento asignado. */
    departamentoId: number | null;
    departamento: string | null;
    empleados: number;
    empleadosConActividad: number;
    modulosCompletados: number;
    avancePromedio: number;
}

/** Una fila del detalle por empleado, espejo de `BERendimientoEmpleado.cs`. */
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

/** La respuesta completa del panel, espejo de `BERendimiento.cs`. */
export interface RendimientoResponse {
    empresaId: number;
    empresa: string;
    resumen: RendimientoResumenResponse;
    departamentos: RendimientoDepartamentoResponse[];
    empleados: RendimientoEmpleadoResponse[];
}

/** Criterios del panel; todos opcionales. */
export interface FiltrosRendimiento {
    /** Solo la manda quien tiene alcance sobre todas las empresas. */
    empresaId?: number;
    /** `yyyy-MM-dd`. */
    desde?: string;
    /** `yyyy-MM-dd`. */
    hasta?: string;
    departamentoId?: number;
}
