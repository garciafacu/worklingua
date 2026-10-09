import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';

const DESTINOS = [
    { etiqueta: 'layout.publico.inicio', ruta: '/' },
    { etiqueta: 'layout.publico.catalogo', ruta: '/catalogo' },
    { etiqueta: 'layout.publico.faqs', ruta: '/preguntas-frecuentes' },
    { etiqueta: 'layout.publico.contacto', ruta: '/contacto' },
    { etiqueta: 'layout.publico.iniciarSesion', ruta: '/login' },
];

/**
 * Destino de las rutas que no existen. Se muestra la página en lugar de
 * redirigir a la portada para que quede claro que la dirección estaba mal.
 */
export function NoEncontrado() {
    const { t } = useTranslation();

    return (
        <div className="mx-auto max-w-3xl px-4 py-20 text-center">
            <p className="text-sm font-semibold tracking-wide text-primario uppercase">
                {t('publico.noEncontrado.codigo')}
            </p>

            <h1 className="mt-3 text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                {t('publico.noEncontrado.titulo')}
            </h1>

            <p className="mt-3 text-base text-texto-suave">{t('publico.noEncontrado.detalle')}</p>

            <nav
                aria-label={t('publico.noEncontrado.destinos')}
                className="mt-8 flex flex-wrap justify-center gap-3"
            >
                {DESTINOS.map((destino) => (
                    <Link
                        key={destino.ruta}
                        to={destino.ruta}
                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                    >
                        {t(destino.etiqueta)}
                    </Link>
                ))}
            </nav>
        </div>
    );
}
