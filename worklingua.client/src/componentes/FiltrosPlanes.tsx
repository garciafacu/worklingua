import { useTranslation } from 'react-i18next';
import type { CaracteristicaResponse, PlanResponse } from '../tipos/planes';

interface FiltrosPlanesProps {
    planes: PlanResponse[];
    planesVisibles: number[];
    caracteristicas: CaracteristicaResponse[];
    idsVisibles: number[];
    alAlternarPlan: (planId: number) => void;
    alAlternarCaracteristica: (caracteristicaId: number) => void;
    alRestablecer: () => void;
    idActivo: number | null;
    alSenalarCaracteristica: (caracteristicaId: number | null) => void;
}

const CLASE_CHIP =
    'cursor-pointer rounded-full border px-3.5 py-1.5 text-sm font-medium transition-colors';

const CHIP_ENCENDIDO = 'border-primario/30 bg-info-fondo text-primario hover:border-primario';
const CHIP_APAGADO =
    'border-borde bg-superficie text-texto-suave line-through hover:border-texto-suave hover:no-underline';

function Chip({
    encendido,
    senalado = false,
    alPulsar,
    alSenalar,
    children,
}: {
    encendido: boolean;
    senalado?: boolean;
    alPulsar: () => void;
    alSenalar?: (activo: boolean) => void;
    children: string;
}) {
    return (
        <button
            type="button"
            aria-pressed={encendido}
            onClick={alPulsar}
            onMouseEnter={() => alSenalar?.(true)}
            onMouseLeave={() => alSenalar?.(false)}
            onFocus={() => alSenalar?.(true)}
            onBlur={() => alSenalar?.(false)}
            className={`${CLASE_CHIP} ${encendido ? CHIP_ENCENDIDO : CHIP_APAGADO} ${
                senalado && !encendido ? 'border-primario text-primario' : ''
            }`}
        >
            {children}
        </button>
    );
}

export function FiltrosPlanes({
    planes,
    planesVisibles,
    caracteristicas,
    idsVisibles,
    alAlternarPlan,
    alAlternarCaracteristica,
    alRestablecer,
    idActivo,
    alSenalarCaracteristica,
}: FiltrosPlanesProps) {
    const { t } = useTranslation();

    const todoMarcado =
        planesVisibles.length === planes.length &&
        idsVisibles.length === caracteristicas.length;

    return (
        <section
            aria-labelledby="titulo-filtros"
            className="mt-10 rounded-2xl border border-borde bg-superficie px-5 py-5 sm:px-6"
        >
            <div className="flex flex-wrap items-baseline justify-between gap-x-4 gap-y-2">
                <h2 id="titulo-filtros" className="text-sm font-semibold text-texto">
                    {t('publico.planes.filtros.titulo')}
                </h2>

                <button
                    type="button"
                    onClick={alRestablecer}
                    disabled={todoMarcado}
                    className="rounded-lg border-0 bg-transparent px-0 py-0 text-sm font-semibold text-primario hover:underline disabled:cursor-not-allowed disabled:text-texto-suave disabled:no-underline"
                >
                    {t('publico.planes.filtros.mostrarTodo')}
                </button>
            </div>

            <div className="mt-4 space-y-4">
                <fieldset className="m-0 flex flex-wrap items-center gap-x-3 gap-y-2 border-0 p-0 mb-4">
                    <legend className="float-left mr-3 py-1.5 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                        {t('publico.planes.filtros.planes')}
                    </legend>

                    {planes.map((plan) => (
                        <Chip
                            key={plan.planId}
                            encendido={planesVisibles.includes(plan.planId)}
                            alPulsar={() => alAlternarPlan(plan.planId)}
                        >
                            {plan.nombre}
                        </Chip>
                    ))}
                </fieldset>

                <fieldset className="m-0 flex flex-wrap items-center gap-x-2 gap-y-2 border-0 border-t border-t-borde p-0 pt-4">
                    <legend className="float-left mr-3 py-1.5 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                        {t('publico.planes.filtros.caracteristicas')}
                    </legend>

                    {caracteristicas.map((fila) => (
                        <Chip
                            key={fila.caracteristicaId}
                            encendido={idsVisibles.includes(fila.caracteristicaId)}
                            senalado={idActivo === fila.caracteristicaId}
                            alPulsar={() => alAlternarCaracteristica(fila.caracteristicaId)}
                            alSenalar={(activo) =>
                                alSenalarCaracteristica(activo ? fila.caracteristicaId : null)
                            }
                        >
                            {fila.nombre}
                        </Chip>
                    ))}
                </fieldset>
            </div>
        </section>
    );
}
