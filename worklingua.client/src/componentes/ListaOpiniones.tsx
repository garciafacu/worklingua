import { useTranslation } from 'react-i18next';
import { useLocalizacion } from '../contexto/useLocalizacion';
import type { OpinionResponse } from '../tipos/opiniones';
import { Estrellas } from './Estrellas';

interface ListaOpinionesProps {
    opiniones: OpinionResponse[];
    mostrarPlan?: boolean;
    mensajeVacio: string;
    confirmando: number | null;
    eliminando: boolean;
    alPedirConfirmacion: (comentarioId: number | null) => void;
    alEliminar: (comentarioId: number) => void;
}

const CLASE_ACCION =
    'rounded-lg border bg-superficie px-3 py-1.5 text-sm font-semibold disabled:cursor-not-allowed disabled:opacity-60';

function iniciales(autor: string): string {
    const partes = autor.trim().split(/\s+/).filter(Boolean);

    if (partes.length === 0) {
        return '?';
    }

    const primera = partes[0][0];
    const segunda = partes.length > 1 ? partes[partes.length - 1][0] : '';

    return `${primera}${segunda}`.toUpperCase();
}

export function ListaOpiniones({
    opiniones,
    mostrarPlan = false,
    mensajeVacio,
    confirmando,
    eliminando,
    alPedirConfirmacion,
    alEliminar,
}: ListaOpinionesProps) {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();

    if (opiniones.length === 0) {
        return (
            <p className="rounded-2xl border border-dashed border-borde bg-superficie px-6 py-10 text-center text-sm text-texto-suave">
                {mensajeVacio}
            </p>
        );
    }

    return (
        <ul className="m-0 list-none space-y-3 p-0">
            {opiniones.map((opinion) => {
                const enConfirmacion = confirmando === opinion.comentarioId;

                return (
                    <li
                        key={opinion.comentarioId}
                        className={`rounded-2xl border bg-superficie p-5 transition-colors ${
                            opinion.esPropio ? 'border-primario' : 'border-borde'
                        }`}
                    >
                        <div className="flex items-start gap-3">
                            <span
                                aria-hidden="true"
                                className="flex size-10 shrink-0 items-center justify-center rounded-full bg-info-fondo text-sm font-semibold text-primario"
                            >
                                {iniciales(opinion.autor)}
                            </span>

                            <div className="min-w-0 flex-1">
                                <div className="flex flex-wrap items-baseline gap-x-2 gap-y-1">
                                    <p className="m-0 text-sm font-semibold text-texto">
                                        {opinion.autor}
                                    </p>

                                    {opinion.esPropio && (
                                        <span className="rounded-full bg-info-fondo px-2 py-0.5 text-xs font-semibold text-primario">
                                            {t('privado.opiniones.lista.propio')}
                                        </span>
                                    )}

                                    <p className="m-0 text-xs text-texto-suave">
                                        <time dateTime={opinion.fechaAlta}>
                                            {formatearFecha(opinion.fechaAlta, true)}
                                        </time>
                                    </p>
                                </div>

                                {opinion.puntaje !== null && (
                                    <p className="mt-1.5 mb-0 flex items-center gap-2">
                                        <Estrellas valor={opinion.puntaje} />
                                        <span className="text-xs font-semibold text-texto">
                                            {t(`comun.valoracion.puntaje.${opinion.puntaje}`)}
                                        </span>
                                    </p>
                                )}

                                {mostrarPlan && opinion.plan && (
                                    <p className="mt-1.5 mb-0">
                                        <span className="rounded-full border border-borde px-2 py-0.5 text-xs font-medium text-texto-suave">
                                            {t('privado.opiniones.lista.plan', {
                                                plan: opinion.plan,
                                            })}
                                        </span>
                                    </p>
                                )}

                                {opinion.texto.trim() !== '' && (
                                    <p className="mt-3 mb-0 text-sm whitespace-pre-line text-texto">
                                        {opinion.texto}
                                    </p>
                                )}

                                {opinion.puedeEliminar && (
                                    <div className="mt-4 flex flex-wrap items-center gap-2">
                                        {enConfirmacion ? (
                                            <>
                                                <span className="text-sm text-texto-suave">
                                                    {t('privado.opiniones.confirmarEliminar')}
                                                </span>
                                                <button
                                                    type="button"
                                                    disabled={eliminando}
                                                    onClick={() => alEliminar(opinion.comentarioId)}
                                                    className={`${CLASE_ACCION} border-error text-error hover:bg-error-fondo`}
                                                >
                                                    {t('privado.opiniones.confirmarEliminarSi')}
                                                </button>
                                                <button
                                                    type="button"
                                                    disabled={eliminando}
                                                    onClick={() => alPedirConfirmacion(null)}
                                                    className={`${CLASE_ACCION} border-borde text-texto hover:bg-fondo`}
                                                >
                                                    {t('comun.boton.no')}
                                                </button>
                                            </>
                                        ) : (
                                            <button
                                                type="button"
                                                onClick={() =>
                                                    alPedirConfirmacion(opinion.comentarioId)
                                                }
                                                className={`${CLASE_ACCION} border-borde text-error hover:bg-error-fondo`}
                                            >
                                                {t('privado.opiniones.eliminar')}
                                            </button>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>
                    </li>
                );
            })}
        </ul>
    );
}
