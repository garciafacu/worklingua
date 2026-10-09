import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { ofertasApi } from '../../api/ofertasApi';
import { Alerta } from '../../componentes/Alerta';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import { rutaContratacion } from '../../rutas/contratacionPendiente';
import type { OfertaResponse } from '../../tipos/ofertas';

/**
 * Ofertas personales (punto 6.d): las que el Backoffice dirigió a la empresa y
 * están vigentes hoy.
 *
 * Son informativas: no cambian precios. Si sugieren un plan, "Ver plan" abre la
 * contratación existente de ese plan.
 */
export function MisOfertas() {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();

    const [ofertas, setOfertas] = useState<OfertaResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    useEffect(() => {
        ofertasApi
            .listarMias()
            .then(setOfertas)
            .catch((excepcion: unknown) =>
                setError(excepcion instanceof ErrorApi ? excepcion.message : t('privado.ofertas.error')),
            )
            .finally(() => setCargando(false));
    }, [t]);

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.ofertas.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.ofertas.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('privado.ofertas.cargando')}</p>}

            {!cargando && !error && ofertas.length === 0 && (
                <p className="mt-6 rounded-2xl border border-dashed border-borde bg-superficie px-6 py-10 text-center text-sm text-texto-suave">
                    {t('privado.ofertas.vacio')}
                </p>
            )}

            <div className="mt-6 grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                {ofertas.map((item) => (
                    <article
                        key={item.oferta.ofertaId}
                        className="flex flex-col overflow-hidden rounded-2xl border border-borde bg-superficie"
                    >
                        <header className="bg-marca px-6 py-4">
                            <h2 className="m-0 text-base font-semibold text-white">{item.oferta.titulo}</h2>
                        </header>

                        <div className="flex flex-1 flex-col gap-3 p-6">
                            <p className="m-0 text-sm whitespace-pre-line text-texto">{item.oferta.descripcion}</p>

                            <p className="m-0 text-xs text-texto-suave">
                                {item.oferta.fechaHasta
                                    ? t('privado.ofertas.vigenteHasta', { fecha: formatearFecha(item.oferta.fechaHasta) })
                                    : t('privado.ofertas.sinVencimiento')}
                            </p>

                            {item.oferta.planId !== null && item.planActivo && (
                                <div className="mt-auto border-t border-borde pt-4">
                                    <Link
                                        to={rutaContratacion(item.oferta.planId)}
                                        className="inline-block rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                                    >
                                        {t('privado.ofertas.verPlan', { plan: item.plan })}
                                    </Link>
                                </div>
                            )}
                        </div>
                    </article>
                ))}
            </div>
        </div>
    );
}
