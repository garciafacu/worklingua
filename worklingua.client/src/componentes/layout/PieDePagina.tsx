import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { FormularioNewsletter } from '../FormularioNewsletter';
import { Marca } from '../Marca';

const ENLACES_LEGALES = [
    { etiqueta: 'layout.publico.novedades', ruta: '/novedades' },
    { etiqueta: 'layout.publico.terminos', ruta: '/terminos' },
    { etiqueta: 'layout.publico.privacidad', ruta: '/privacidad' },
    { etiqueta: 'layout.publico.institucional', ruta: '/institucional' },
    { etiqueta: 'layout.publico.preguntasFrecuentes', ruta: '/preguntas-frecuentes' },
    { etiqueta: 'layout.publico.contacto', ruta: '/contacto' },
];

export function PieDePagina() {
    const { t } = useTranslation();

    return (
        <footer className="border-t border-borde bg-superficie">
            <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 py-8 sm:flex-row sm:items-center sm:justify-between">
                <div>
                    <p className="text-base font-bold tracking-tight text-marca">
                        <Marca claseAcento="text-primario" />
                    </p>
                    <p className="mt-1 text-sm text-texto-suave">{t('layout.pie.lema')}</p>
                </div>

                <nav aria-label={t('layout.pie.enlaces')} className="flex flex-wrap gap-x-5 gap-y-2">
                    {ENLACES_LEGALES.map((enlace) => (
                        <Link
                            key={enlace.ruta}
                            to={enlace.ruta}
                            className="text-sm text-texto-suave no-underline hover:text-primario"
                        >
                            {t(enlace.etiqueta)}
                        </Link>
                    ))}
                </nav>
            </div>

            <div className="border-t border-borde">
                <div className="mx-auto max-w-6xl px-4 py-8">
                    <div className="max-w-xl">
                        <FormularioNewsletter />
                    </div>
                </div>
            </div>

            <div className="border-t border-borde px-4 py-4">
                <p className="mx-auto max-w-6xl text-xs text-texto-suave">
                    {t('layout.pie.derechos', { anio: new Date().getFullYear() })}
                </p>
            </div>
        </footer>
    );
}
