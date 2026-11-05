import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocalizacion } from '../contexto/useLocalizacion';
import type { CaracteristicaResponse, PlanResponse } from '../tipos/planes';
import { ResumenValoracion } from './Estrellas';

interface TarjetaPlanProps {
    plan: PlanResponse;
    caracteristicas: CaracteristicaResponse[];
    idsVisibles: number[];
    idActivo: number | null;
    alSenalarCaracteristica: (caracteristicaId: number | null) => void;
    seleccionada?: boolean;
    pie?: ReactNode;
}

const ALTO_FILA = 'min-h-[2.375rem]';

export function TarjetaPlan({
    plan,
    caracteristicas,
    idsVisibles,
    idActivo,
    alSenalarCaracteristica,
    seleccionada = false,
    pie,
}: TarjetaPlanProps) {
    const { t } = useTranslation();
    const { formatearMoneda, formatearNumero } = useLocalizacion();

    const esGratuito = plan.precioMensual === 0;

    const visibles = caracteristicas.filter((fila) => idsVisibles.includes(fila.caracteristicaId));

    return (
        <article
            className={`flex h-full flex-col overflow-hidden rounded-2xl border bg-superficie transition-shadow ${
                plan.destacado
                    ? 'border-marca shadow-lg shadow-marca/10'
                    : 'border-borde'
            } ${seleccionada ? 'ring-2 ring-primario ring-offset-2 ring-offset-fondo' : ''}`}
        >
            <header
                className={`flex min-h-[3.5rem] items-center justify-between gap-2 px-6 py-3 ${
                    plan.destacado ? 'bg-marca' : 'border-b border-borde bg-fondo'
                }`}
            >
                <h3
                    className={`text-base font-semibold tracking-tight ${
                        plan.destacado ? 'text-white' : 'text-texto'
                    }`}
                >
                    {plan.nombre}
                </h3>

                {plan.destacado && (
                    <span className="rounded-full bg-white/15 px-2.5 py-1 text-[11px] font-semibold tracking-wide text-white uppercase">
                        {t('publico.planes.tarjeta.recomendado')}
                    </span>
                )}
            </header>

            <div className="flex flex-1 flex-col p-6">
                <p className="flex min-h-[3rem] items-baseline gap-1.5">
                    <span className="text-[2rem] leading-none font-bold tracking-tight text-texto">
                        {esGratuito
                            ? t('publico.planes.tarjeta.sinCargo')
                            : formatearMoneda(plan.precioMensual)}
                    </span>
                    {!esGratuito && <span className="text-sm text-texto-suave">
                            {t('publico.planes.tarjeta.porMes')}
                        </span>}
                </p>

                <p className="text-sm font-medium text-texto-suave">
                    {plan.cantidadLicencias === 1
                        ? t('publico.planes.tarjeta.licenciaUna')
                        : t('publico.planes.tarjeta.licencias', {
                              cantidad: formatearNumero(plan.cantidadLicencias),
                          })}
                </p>

                <div className="mt-2">
                    <ResumenValoracion
                        promedio={plan.promedioValoracion}
                        cantidad={plan.cantidadValoraciones}
                    />
                </div>

                {plan.descripcion && (
                    <p className="mt-3 min-h-[2.5rem] text-sm text-texto-suave">
                        {plan.descripcion}
                    </p>
                )}

                {visibles.length > 0 && (
                    <ul className="mt-5 border-t border-borde text-sm">
                        {visibles.map((fila) => {
                            const valor = plan.caracteristicas.find(
                                (item) => item.caracteristicaId === fila.caracteristicaId,
                            );
                            const incluido = valor?.incluido === true;
                            const detalle = incluido ? valor?.detalle : null;
                            const senalada = idActivo === fila.caracteristicaId;

                            return (
                                <li
                                    key={fila.caracteristicaId}
                                    onMouseEnter={() =>
                                        alSenalarCaracteristica(fila.caracteristicaId)
                                    }
                                    onMouseLeave={() => alSenalarCaracteristica(null)}
                                    className={`-mx-3 flex ${ALTO_FILA} items-center gap-2.5 rounded-lg px-3 transition-colors ${
                                        senalada ? 'bg-info-fondo' : ''
                                    }`}
                                >
                                    <span
                                        aria-hidden="true"
                                        className={`w-3 shrink-0 text-center ${
                                            incluido ? 'text-primario' : 'text-texto-suave'
                                        }`}
                                    >
                                        {incluido ? '✓' : '—'}
                                    </span>

                                    <span
                                        className={incluido ? 'text-texto' : 'text-texto-suave'}
                                    >
                                        {fila.nombre}
                                        {detalle ? (
                                            <>
                                                :{' '}
                                                <span className="font-medium text-texto">
                                                    {detalle}
                                                </span>
                                            </>
                                        ) : (
                                            <span className="sr-only">
                                                {incluido
                                                    ? t('publico.planes.tarjeta.incluido')
                                                    : t('publico.planes.tarjeta.noIncluido')}
                                            </span>
                                        )}
                                    </span>
                                </li>
                            );
                        })}
                    </ul>
                )}

                {pie && <div className="mt-auto border-t border-borde pt-5">{pie}</div>}
            </div>
        </article>
    );
}
