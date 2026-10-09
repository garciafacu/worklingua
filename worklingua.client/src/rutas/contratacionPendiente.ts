/**
 * El plan que un visitante eligió contratar desde el Catálogo antes de tener
 * sesión.
 *
 * Viaja en el query string (`?contratar=<planId>`) entre Catálogo, Login y
 * Registro. Además se recuerda en `localStorage`, porque la cuenta nueva se
 * confirma desde el enlace del correo, que suele abrirse en otra pestaña, y ahí
 * el query string ya se perdió.
 *
 * No contrata nada: solo recuerda a dónde volver. La contratación la sigue
 * resolviendo `MiPlan` con `/inicio/plan?contratar=<planId>`.
 */

/** Nombre del parámetro de query, el mismo que abre el formulario en `MiPlan`. */
export const PARAMETRO_CONTRATAR = 'contratar';

const CLAVE_ALMACENAMIENTO = 'worklingua.contratacionPendiente';

/** Coincide con `Registro:HorasVigenciaConfirmacion`: lo que dura el enlace de confirmación. */
const VIGENCIA_MS = 24 * 60 * 60 * 1000;

interface ContratacionPendiente {
    planId: number;
    guardado: number;
}

function aPlanId(valor: string | null | undefined): number | null {
    const planId = Number(valor);

    return Number.isInteger(planId) && planId > 0 ? planId : null;
}

/** Plan pedido en el query string, o `null` si no hay uno válido. */
export function planIdDeParametros(parametros: URLSearchParams): number | null {
    return aPlanId(parametros.get(PARAMETRO_CONTRATAR));
}

/** Ruta privada que abre el formulario de contratación existente. */
export function rutaContratacion(planId: number): string {
    return `/inicio/plan?${PARAMETRO_CONTRATAR}=${planId}`;
}

/** Agrega el plan pendiente a una ruta pública (`/login`, `/registro`), si lo hay. */
export function conPlanPendiente(ruta: string, planId: number | null): string {
    return planId === null ? ruta : `${ruta}?${PARAMETRO_CONTRATAR}=${planId}`;
}

export function recordarPlanAContratar(planId: number): void {
    try {
        const valor: ContratacionPendiente = { planId, guardado: Date.now() };
        localStorage.setItem(CLAVE_ALMACENAMIENTO, JSON.stringify(valor));
    } catch {
        // Sin almacenamiento (ventana privada, permisos) el plan sigue viajando
        // en el query string; solo se pierde si se confirma en otra pestaña.
    }
}

/** El plan recordado, si no venció. */
export function planRecordado(): number | null {
    try {
        const texto = localStorage.getItem(CLAVE_ALMACENAMIENTO);

        if (!texto) {
            return null;
        }

        const valor = JSON.parse(texto) as Partial<ContratacionPendiente>;
        const vigente = typeof valor.guardado === 'number' && Date.now() - valor.guardado < VIGENCIA_MS;

        return vigente ? aPlanId(String(valor.planId)) : null;
    } catch {
        return null;
    }
}

export function olvidarPlanAContratar(): void {
    try {
        localStorage.removeItem(CLAVE_ALMACENAMIENTO);
    } catch {
        // Nada que limpiar si el almacenamiento no está disponible.
    }
}
