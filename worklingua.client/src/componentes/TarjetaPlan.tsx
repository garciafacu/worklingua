import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { ResumenValoracion } from './ResumenValoracion';
import { useLocalizacion } from '../contexto/useLocalizacion';
import type { CaracteristicaResponse } from '../tipos/caracteristicas';
import type { PlanResponse } from '../tipos/planes';

interface TarjetaPlanProps {
    plan: PlanResponse;
    /**
     * Eje común de comparación, ya ordenado, tal como viene de la API. Las tres
     * tarjetas reciben el mismo, que es lo que permite leerlas de lado a lado.
     */
    caracteristicas: CaracteristicaResponse[];
    /**
     * Ids de las características a mostrar. Es el filtro de la sección: al
     * desmarcar una, desaparece de todas las tarjetas a la vez.
     */
    idsVisibles: number[];
    /**
     * Característica señalada en este momento. Viene de la sección para que la
     * misma fila se resalte en las tres tarjetas y se pueda leer de lado a lado.
     */
    idActivo: number | null;
    alSenalarCaracteristica: (caracteristicaId: number | null) => void;
    /**
     * Marca la tarjeta como la elegida. SOLO cambia el aspecto: quién está elegida
     * y de qué manera se elige lo resuelve quien usa el componente. La página
     * pública no la pasa y se sigue dibujando igual que antes.
     */
    seleccionada?: boolean;
    /** Contenido opcional del pie, por ejemplo el enlace a las opiniones. */
    pie?: ReactNode;
}

/** Altura mínima de cada fila: es lo que alinea las características entre tarjetas. */
const ALTO_FILA = 'min-h-[2.375rem]';

/**
 * Un plan del catálogo: precio, alcance y qué incluye.
 *
 * Las características salen del catálogo que administra el Backoffice, común a
 * todos los planes, y no de viñetas sueltas de cada uno: es lo que permite que
 * las tarjetas se lean como una comparación. Una característica que el plan no
 * incluye se muestra atenuada con un —, en lugar de faltar, para que se note la
 * diferencia frente a los otros planes.
 *
 * Un plan sin fila para una característica se dibuja igual que uno con
 * `incluido: false`: no ofrecerla y no tenerla cargada son lo mismo de cara al
 * visitante.
 *
 * Las alturas fijas del encabezado, el precio y cada fila no son decorativas:
 * mantienen las filas de las tarjetas a la misma altura, que es lo que hace
 * legible el barrido horizontal.
 */
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
    // El precio se formatea con la cultura activa: locale, simbolo de moneda y
    // tasa de conversion salen de la tabla Cultura, no de un Intl hardcodeado.
    const { formatearMoneda, formatearNumero } = useLocalizacion();

    const esGratuito = plan.precioMensual === 0;

    // Se filtra sobre el catálogo y no sobre idsVisibles para que el orden de las
    // características sea siempre el mismo en todas las tarjetas.
    const visibles = caracteristicas.filter((fila) => idsVisibles.includes(fila.caracteristicaId));

    return (
        <article
            className={`flex h-full flex-col overflow-hidden rounded-2xl border bg-superficie transition-shadow ${
                plan.destacado
                    ? 'border-marca shadow-lg shadow-marca/10'
                    : 'border-borde'
            } ${seleccionada ? 'ring-2 ring-primario ring-offset-2 ring-offset-fondo' : ''}`}
        >
            {/* Banda de encabezado: el navy de marca es lo único que separa al
                plan recomendado, y las bandas miden igual para no desalinear
                lo que viene abajo. */}
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

                {/* Siempre presente, con o sin valoraciones, para que las filas
                    de abajo sigan alineadas entre tarjetas. */}
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
                                    {/* El — tiene que verse: mostrar la ausencia
                                        es la mitad de la comparación. */}
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

                {/* `mt-auto` empuja el pie al fondo para que todas las tarjetas de
                    la grilla alineen el enlace a la misma altura. */}
                {pie && <div className="mt-auto border-t border-borde pt-5">{pie}</div>}
            </div>
        </article>
    );
}
