import { useTranslation } from 'react-i18next';
import { useLocalizacion } from '../../contexto/useLocalizacion';

interface SelectorIdiomaProps {
    /** Clases extra del contenedor, para acomodarlo en cada barra. */
    className?: string;
}

/**
 * Elige el idioma de la interfaz.
 *
 * Los idiomas salen de `GET /api/idiomas`, que devuelve **solo los activos**:
 * un idioma desactivado desde el Backoffice desaparece de acá en el acto.
 *
 * La elección vive en el navegador (`localStorage`), no en `Usuario`: es una
 * preferencia del dispositivo, no un dato de la persona.
 *
 * Con un solo idioma activo no se muestra nada: un selector de una sola opción
 * es ruido.
 */
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
