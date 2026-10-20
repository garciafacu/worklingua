export const PARAMETRO_CONTRATAR = 'contratar';

const CLAVE_ALMACENAMIENTO = 'worklingua.contratacionPendiente';

const VIGENCIA_MS = 24 * 60 * 60 * 1000;

interface ContratacionPendiente {
    planId: number;
    guardado: number;
}

function aPlanId(valor: string | null | undefined): number | null {
    const planId = Number(valor);

    return Number.isInteger(planId) && planId > 0 ? planId : null;
}

export function planIdDeParametros(parametros: URLSearchParams): number | null {
    return aPlanId(parametros.get(PARAMETRO_CONTRATAR));
}

export function rutaContratacion(planId: number): string {
    return `/inicio/plan?${PARAMETRO_CONTRATAR}=${planId}`;
}

export function conPlanPendiente(ruta: string, planId: number | null): string {
    return planId === null ? ruta : `${ruta}?${PARAMETRO_CONTRATAR}=${planId}`;
}

export function recordarPlanAContratar(planId: number): void {
    try {
        const valor: ContratacionPendiente = { planId, guardado: Date.now() };
        localStorage.setItem(CLAVE_ALMACENAMIENTO, JSON.stringify(valor));
    } catch {
    }
}

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
    }
}
