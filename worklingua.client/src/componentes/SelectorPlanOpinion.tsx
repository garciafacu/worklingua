import { useTranslation } from 'react-i18next';
import type { CaracteristicaResponse, PlanResponse } from '../tipos/planes';
import { TarjetaPlan } from './TarjetaPlan';

interface SelectorPlanOpinionProps {
    planes: PlanResponse[];
    caracteristicas: CaracteristicaResponse[];
    planId: number | null;
    alSeleccionar: (planId: number) => void;
}

export function SelectorPlanOpinion({
    planes,
    caracteristicas,
    planId,
    alSeleccionar,
}: SelectorPlanOpinionProps) {
    const { t } = useTranslation();

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
