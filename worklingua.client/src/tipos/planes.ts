export interface PlanCaracteristicaResponse {
    caracteristicaId: number;
    incluido: boolean;
    detalle: string | null;
}

export interface PlanResponse {
    planId: number;
    nombre: string;
    descripcion: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    destacado: boolean;
    promedioValoracion: number;
    cantidadValoraciones: number;
    caracteristicas: PlanCaracteristicaResponse[];
}

export interface PlanAdminResponse {
    planId: number;
    nombre: string;
    descripcion: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    destacado: boolean;
    activo: boolean;
    protegido: boolean;
}

export interface GuardarPlanRequest {
    nombre: string;
    descripcion: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    destacado: boolean;
}

export interface ItemPlanCaracteristica {
    caracteristicaId: number;
    incluido: boolean;
    detalle: string | null;
}

export interface GuardarPlanCaracteristicasRequest {
    caracteristicas: ItemPlanCaracteristica[];
}
