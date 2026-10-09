import { useTranslation } from 'react-i18next';
import { Estrellas, IconoEstrella } from './Estrellas';
import { useLocalizacion } from '../contexto/useLocalizacion';
import { PUNTAJE_MAXIMO, type OpinionResponse } from '../tipos/opiniones';

interface DistribucionValoracionesProps {
    /** Promedio y cantidad tal como los calcula el backend para el plan. */
    promedio: number;
    cantidad: number;
    /** Opiniones del plan: solo se usan para dibujar las barras. */
    opiniones: OpinionResponse[];
}

const PUNTAJES_DESCENDENTES = Array.from(
    { length: PUNTAJE_MAXIMO },
    (_, indice) => PUNTAJE_MAXIMO - indice,
);

/**
 * Resumen de la valoración de un plan: el promedio grande y cuántas opiniones
 * hay de cada puntaje.
 *
 * El promedio y el total salen del backend. Las barras solo cuentan las
 * opiniones que ya están en pantalla, para mostrarlas; los comentarios
 * anteriores sin puntaje no suman en ninguna barra.
 */
export function DistribucionValoraciones({ promedio, cantidad, opiniones }: DistribucionValoracionesProps) {
    const { t } = useTranslation();
    const { formatearNumero } = useLocalizacion();

    const valoradas = opiniones.filter((opinion) => opinion.puntaje !== null);
    const maximo = Math.max(valoradas.length, 1);

    return (
        <section className="rounded-2xl border border-borde bg-superficie p-6">
            <h3 className="m-0 text-sm font-semibold tracking-wide text-texto-suave uppercase">
                {t('privado.opiniones.distribucion.titulo')}
            </h3>

            <div className="mt-4 grid items-center gap-6 sm:grid-cols-[auto_1fr] sm:gap-10">
                <div className="flex flex-col items-start gap-2 sm:items-center">
                    <p className="m-0 text-5xl leading-none font-bold tracking-tight text-texto">
                        {cantidad > 0 ? formatearNumero(promedio, 1) : '—'}
                    </p>
                    <Estrellas valor={promedio} tamano="md" />
                    <p className="m-0 text-sm text-texto-suave">
                        {cantidad === 0
                            ? t('comun.valoracion.sinValoraciones')
                            : cantidad === 1
                              ? t('comun.valoracion.cantidadUna')
                              : t('comun.valoracion.cantidad', {
                                    cantidad: formatearNumero(cantidad),
                                })}
                    </p>
                </div>

                <ul className="m-0 list-none space-y-2 p-0">
                    {PUNTAJES_DESCENDENTES.map((puntaje) => {
                        const cantidadPuntaje = valoradas.filter(
                            (opinion) => opinion.puntaje === puntaje,
                        ).length;
                        const ancho = (cantidadPuntaje / maximo) * 100;

                        return (
                            <li
                                key={puntaje}
                                className="flex items-center gap-3 text-sm"
                                aria-label={t('privado.opiniones.distribucion.fila', {
                                    cantidad: cantidadPuntaje,
                                    puntaje,
                                })}
                            >
                                <span
                                    aria-hidden="true"
                                    className="flex w-8 shrink-0 items-center gap-1 font-medium text-texto"
                                >
                                    {puntaje}
                                    <IconoEstrella className="size-3.5 text-estrella" />
                                </span>
                                <span
                                    aria-hidden="true"
                                    className="h-2 flex-1 overflow-hidden rounded-full bg-fondo"
                                >
                                    <span
                                        className="block h-full rounded-full bg-estrella transition-[width] duration-500"
                                        style={{ width: `${ancho}%` }}
                                    />
                                </span>
                                <span
                                    aria-hidden="true"
                                    className="w-8 shrink-0 text-right text-texto-suave tabular-nums"
                                >
                                    {formatearNumero(cantidadPuntaje)}
                                </span>
                            </li>
                        );
                    })}
                </ul>
            </div>
        </section>
    );
}
