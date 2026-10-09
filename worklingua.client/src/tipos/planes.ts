/**
 * Qué ofrece un plan en una característica, espejo de
 * `Contratos/PlanCaracteristicaResponse.cs`.
 *
 * `incluido` dibuja el ✓ o el —; `detalle`, cuando viene, se muestra después de
 * la etiqueta ("Idiomas disponibles: Todos los idiomas"). Un plan **sin entrada**
 * para una característica se dibuja como no incluida: la ausencia de fila es la
 * forma de decir que no la ofrece.
 */
export interface PlanCaracteristicaResponse {
    caracteristicaId: number;
    incluido: boolean;
    detalle: string | null;
}

/** Plan del catálogo, espejo de `Contratos/PlanResponse.cs`. */
export interface PlanResponse {
    planId: number;
    nombre: string;
    descripcion: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    /** Dibuja la banda "Recomendado". Antes era la constante PLAN_DESTACADO. */
    destacado: boolean;
    /** Promedio de estrellas (0 si todavía no tiene valoraciones). Lo calcula el backend. */
    promedioValoracion: number;
    cantidadValoraciones: number;
    caracteristicas: PlanCaracteristicaResponse[];
}

/**
 * Plan visto desde el Backoffice, espejo de `Contratos/PlanAdminResponse.cs`.
 *
 * Suma `activo`, que el contrato público no expone: el listado público filtra
 * los planes dados de baja y el de administración los muestra.
 */
export interface PlanAdminResponse {
    planId: number;
    nombre: string;
    descripcion: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    destacado: boolean;
    activo: boolean;
    /** Un plan protegido sostiene el catálogo público y no se puede dar de baja. */
    protegido: boolean;
}

/** Cuerpo del alta y de la modificación, espejo de `Contratos/GuardarPlanRequest.cs`. */
export interface GuardarPlanRequest {
    nombre: string;
    descripcion: string | null;
    precioMensual: number;
    cantidadLicencias: number;
    destacado: boolean;
}

/** Un ítem del panel de características de un plan, espejo de `ItemPlanCaracteristica`. */
export interface ItemPlanCaracteristica {
    caracteristicaId: number;
    incluido: boolean;
    detalle: string | null;
}

/** Cuerpo del PUT que reemplaza todas las características de un plan. */
export interface GuardarPlanCaracteristicasRequest {
    caracteristicas: ItemPlanCaracteristica[];
}
