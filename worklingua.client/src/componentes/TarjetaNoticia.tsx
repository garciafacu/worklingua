import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useLocalizacion } from '../contexto/useLocalizacion';
import type { NoticiaResponse } from '../tipos/noticias';

const LONGITUD_EXTRACTO = 220;

/** El resumen cargado, o el comienzo del contenido cuando no hay resumen. */
function extracto(noticia: NoticiaResponse): string {
    if (noticia.resumen) {
        return noticia.resumen;
    }

    const contenido = noticia.contenido.trim();

    return contenido.length <= LONGITUD_EXTRACTO
        ? contenido
        : `${contenido.slice(0, LONGITUD_EXTRACTO).trimEnd()}...`;
}

/** Una noticia del listado público: fecha, título, extracto y enlace al detalle. */
export function TarjetaNoticia({ noticia }: { noticia: NoticiaResponse }) {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();
    const ruta = `/novedades/${noticia.noticiaId}`;

    return (
        <article className="flex flex-col rounded-2xl border border-borde bg-superficie p-6">
            <p className="text-xs font-semibold tracking-wide text-texto-suave uppercase">
                {formatearFecha(noticia.fechaPublicacion)}
            </p>

            <h2 className="mt-2 text-lg font-semibold text-texto">
                <Link to={ruta} className="text-texto no-underline hover:text-primario">
                    {noticia.titulo}
                </Link>
            </h2>

            <p className="mt-3 flex-1 text-sm whitespace-pre-line text-texto-suave">
                {extracto(noticia)}
            </p>

            <Link
                to={ruta}
                className="mt-4 text-sm font-semibold text-primario no-underline hover:text-primario-hover"
            >
                {t('publico.novedades.leerMas')}
            </Link>
        </article>
    );
}
