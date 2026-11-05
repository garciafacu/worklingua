import type { EstadoContratacion } from './contrataciones';

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

export interface CaracteristicaResponse {
    caracteristicaId: number;
    nombre: string;
    orden: number;
}

export interface CaracteristicaAdminResponse extends CaracteristicaResponse {
    activo: boolean;
}

export interface GuardarCaracteristicaRequest {
    nombre: string;
    orden: number;
}

export interface SuscripcionResponse {
    suscripcionId: number;
    planId: number;
    plan: string;
    estado: EstadoContratacion;
    fechaInicio: string;
    fechaFin: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    cancelable: boolean;
}

export interface InventarioLicenciaResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    email: string;
    departamento: string | null;
    licenciaId: number | null;
    codigoLicencia: string | null;
    fechaAsignacion: string | null;
}

export interface InventarioResponse {
    empresaId: number;
    empresa: string;
    plan: string;
    contratadas: number;
    asignadas: number;
    disponibles: number;
    empleados: InventarioLicenciaResponse[];
}

export interface AsignarLicenciaRequest {
    usuarioId: number;
}
