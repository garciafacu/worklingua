/**
 * Característica del eje de comparación de los planes, espejo de
 * `Contratos/CaracteristicaResponse.cs`.
 *
 * Es el catálogo: el nombre y el orden viven acá una sola vez, y cada plan dice
 * aparte qué ofrece en cada una. Antes esto era la matriz hardcodeada de
 * `datos/caracteristicasPlanes.ts`, que indexaba por nombre de plan.
 */
export interface CaracteristicaResponse {
    caracteristicaId: number;
    nombre: string;
    orden: number;
}

/** Característica vista desde el Backoffice, con las dadas de baja incluidas. */
export interface CaracteristicaAdminResponse extends CaracteristicaResponse {
    activo: boolean;
}

/** Cuerpo del alta y de la modificación, espejo de `GuardarCaracteristicaRequest.cs`. */
export interface GuardarCaracteristicaRequest {
    nombre: string;
    orden: number;
}
