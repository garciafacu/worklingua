import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

interface PaginaEstaticaProps {
    titulo: string;
    /** Fecha de última actualización, para los textos legales. */
    actualizado?: string;
    children: ReactNode;
}

/**
 * Maqueta de las páginas de texto largo (institucional, términos, privacidad).
 * Fija el ancho de lectura y el ritmo de los títulos y párrafos en un solo lugar,
 * para que las cuatro páginas se vean iguales sin repetir clases.
 */
export function PaginaEstatica({ titulo, actualizado, children }: PaginaEstaticaProps) {
    const { t } = useTranslation();

    return (
        <article className="mx-auto max-w-3xl px-4 py-12 sm:py-16">
            <h1 className="text-3xl font-semibold tracking-tight text-texto sm:text-4xl">{titulo}</h1>

            {actualizado && (
                <p className="mt-2 text-sm text-texto-suave">
                    {t('legal.paginaEstatica.actualizacion', { fecha: actualizado })}
                </p>
            )}

            <div
                className="mt-8 space-y-4 text-base text-texto-suave [&_h2]:mt-10 [&_h2]:mb-2 [&_h2]:text-xl [&_h2]:font-semibold [&_h2]:text-texto [&_li]:ml-5 [&_li]:list-disc [&_p]:leading-relaxed [&_ul]:space-y-2"
            >
                {children}
            </div>
        </article>
    );
}
