import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useParams } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { noticiasApi } from '../../api/noticiasApi';
import { Alerta } from '../../componentes/Alerta';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import type { NoticiaResponse } from '../../tipos/noticias';

interface Resultado {
    noticiaId: number;
    noticia: NoticiaResponse | null;
    error: string | null;
}

/** Detalle público de una noticia. El contenido es texto plano, nunca HTML. */
export function NovedadDetalle() {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();
    const { noticiaId: parametro } = useParams();
    const noticiaId = Number(parametro);
    const idValido = Number.isInteger(noticiaId) && noticiaId > 0;

    const [resultado, setResultado] = useState<Resultado | null>(null);

    useEffect(() => {
        if (!idValido) {
            return;
        }

        let vigente = true;

        noticiasApi
            .obtenerPublicada(noticiaId)
            .then((noticia) => {
                if (vigente) {
                    setResultado({ noticiaId, noticia, error: null });
                }
            })
            .catch((excepcion: unknown) => {
                if (vigente) {
                    setResultado({
                        noticiaId,
                        noticia: null,
                        error:
                            excepcion instanceof ErrorApi && excepcion.estado !== 404
                                ? excepcion.message
                                : t('publico.novedades.noEncontrada'),
                    });
                }
            });

        return () => {
            vigente = false;
        };
    }, [noticiaId, idValido, t]);

    const cargando = idValido && (resultado === null || resultado.noticiaId !== noticiaId);
    const noticia = cargando || !idValido ? null : resultado?.noticia ?? null;
    const error = !idValido ? t('publico.novedades.noEncontrada') : cargando ? null : resultado?.error ?? null;

    return (
        <div className="mx-auto max-w-3xl px-4 py-12 sm:py-16">
            <Link
                to="/novedades"
                className="text-sm font-semibold text-primario no-underline hover:text-primario-hover"
            >
                &larr; {t('publico.novedades.volver')}
            </Link>

            {cargando && (
                <p className="mt-8 text-sm text-texto-suave">{t('publico.novedades.cargando')}</p>
            )}

            <div className="mt-8">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {noticia && (
                <article>
                    <p className="text-sm text-texto-suave">
                        {t('publico.novedades.publicada', {
                            fecha: formatearFecha(noticia.fechaPublicacion),
                        })}
                    </p>

                    <h1 className="mt-2 text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                        {noticia.titulo}
                    </h1>

                    {noticia.resumen && (
                        <p className="mt-4 text-lg text-texto-suave">{noticia.resumen}</p>
                    )}

                    <div className="mt-8 text-base leading-relaxed whitespace-pre-line text-texto">
                        {noticia.contenido}
                    </div>
                </article>
            )}
        </div>
    );
}
