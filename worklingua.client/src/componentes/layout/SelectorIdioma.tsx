import { useTranslation } from 'react-i18next';
import { useLocalizacion } from '../../contexto/useLocalizacion';

interface SelectorIdiomaProps {
    className?: string;
}

export function SelectorIdioma({ className = '' }: SelectorIdiomaProps) {
    const { t } = useTranslation();
    const { idiomas, idiomaActual, cambiarIdioma } = useLocalizacion();

    if (idiomas.length < 2) {
        return null;
    }

    return (
        <div className={className}>
            <label htmlFor="selectorIdioma" className="sr-only">
                {t('layout.selectorIdioma.etiqueta')}
            </label>

            <select
                id="selectorIdioma"
                value={idiomaActual}
                onChange={(evento) => void cambiarIdioma(evento.target.value)}
                aria-label={t('layout.selectorIdioma.etiqueta')}
                className="rounded-lg border border-borde bg-superficie px-2 py-1.5 text-sm font-medium text-texto hover:bg-fondo"
            >
                {idiomas.map((idioma) => (
                    <option key={idioma.idiomaId} value={idioma.codigoISO}>
                        {idioma.nombre}
                    </option>
                ))}
            </select>
        </div>
    );
}
