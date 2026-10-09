import { useTranslation } from 'react-i18next';
import { TarjetaPlan } from './TarjetaPlan';
import type { CaracteristicaResponse } from '../tipos/caracteristicas';
import type { PlanResponse } from '../tipos/planes';

interface SelectorPlanOpinionProps {
    planes: PlanResponse[];
    /** Eje de comparación común, tal como lo recibe la sección Planes pública. */
    caracteristicas: CaracteristicaResponse[];
    planId: number | null;
    alSeleccionar: (planId: number) => void;
}

/**
 * Elige el plan sobre el que se va a opinar, con las mismas tarjetas que muestra
 * la sección pública en lugar de un desplegable.
 *
 * Es un GRUPO DE RADIOS NATIVO —un `<input type="radio">` oculto dentro de cada
 * `<label>`— y no tarjetas con `onClick`. Así se navega con las flechas, el foco
 * se comporta como en cualquier formulario, y un lector de pantalla lo anuncia
 * como "opción 2 de 3" sin que haya que declarar roles a mano. Poner un `onClick`
 * en la tarjeta habría necesitado además `tabIndex`, `role="radio"`,
 * `aria-checked` y el manejo de teclado, todo reimplementado a mano.
 *
 * Toda la mecánica del radio vive acá: `TarjetaPlan` solo recibe `seleccionada`
 * para pintarse distinto y no sabe que está dentro de un selector.
 */
export function SelectorPlanOpinion({
    planes,
    caracteristicas,
    planId,
    alSeleccionar,
}: SelectorPlanOpinionProps) {
    const { t } = useTranslation();

    // Todas las características visibles: acá no hay filtros como en la sección
    // pública, porque lo que se está haciendo es elegir, no comparar.
    const idsVisibles = caracteristicas.map((fila) => fila.caracteristicaId);

    return (
        <fieldset className="m-0 border-0 p-0 pb-10">
            <legend className="p-0 text-lg font-semibold text-texto">
                {t('privado.opiniones.elegirPlan')}
            </legend>
            <p className="mt-1 mb-4 text-sm text-texto-suave">
                {t('privado.opiniones.elegirPlanAyuda')}
            </p>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {planes.map((plan) => (
                    <label
                        key={plan.planId}
                        className="group cursor-pointer focus-within:outline-none"
                    >
                        <input
                            type="radio"
                            name="planOpinion"
                            className="sr-only"
                            value={plan.planId}
                            checked={planId === plan.planId}
                            onChange={() => alSeleccionar(plan.planId)}
                        />

                        {/* El anillo de foco se dibuja acá porque el input real es
                            invisible: sin esto, navegar con el teclado no se vería. */}
                        <div className="h-full rounded-2xl group-focus-within:ring-2 group-focus-within:ring-borde-foco group-focus-within:ring-offset-2 group-focus-within:ring-offset-fondo">
                            <TarjetaPlan
                                plan={plan}
                                caracteristicas={caracteristicas}
                                idsVisibles={idsVisibles}
                                idActivo={null}
                                alSenalarCaracteristica={() => {}}
                                seleccionada={planId === plan.planId}
                            />
                        </div>
                    </label>
                ))}
            </div>
        </fieldset>
    );
}
