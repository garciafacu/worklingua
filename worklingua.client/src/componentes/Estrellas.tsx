import { useTranslation } from 'react-i18next';
import { useLocalizacion } from '../contexto/useLocalizacion';
import { PUNTAJE_MAXIMO } from '../tipos/opiniones';

type TamanoEstrellas = 'sm' | 'md' | 'lg';

const CLASE_TAMANO: Record<TamanoEstrellas, string> = {
    sm: 'size-4',
    md: 'size-5',
    lg: 'size-7',
};

const POSICIONES = Array.from({ length: PUNTAJE_MAXIMO }, (_, indice) => indice + 1);

/** Una estrella sólida. El color lo pone quien la usa con `text-*`. */
export function IconoEstrella({ className = '' }: { className?: string }) {
    return (
        <svg
            viewBox="0 0 20 20"
            fill="currentColor"
            aria-hidden="true"
            className={`block shrink-0 ${className}`}
        >
            <path d="M10.868 2.884c-.321-.772-1.415-.772-1.736 0l-1.83 4.401-4.753.381c-.833.067-1.171 1.107-.536 1.651l3.62 3.102-1.106 4.637c-.194.813.691 1.456 1.405 1.02L10 15.591l4.069 2.485c.713.436 1.598-.207 1.404-1.02l-1.106-4.637 3.62-3.102c.635-.544.297-1.584-.536-1.65l-4.752-.382-1.831-4.401z" />
        </svg>
    );
}

interface EstrellasProps {
    /** De 0 a 5. Admite decimales: 4,5 pinta cuatro estrellas y media. */
    valor: number;
    tamano?: TamanoEstrellas;
}

/**
 * Estrellas de solo lectura, para un puntaje o un promedio.
 *
 * Son dos filas superpuestas: la de fondo en gris y la de relleno en ámbar,
 * recortada al porcentaje del valor. Así un promedio se ve con fracciones de
 * estrella sin dibujar medias estrellas a mano. Las estrellas no llevan
 * separación entre sí para que el porcentaje coincida con lo que se ve.
 */
export function Estrellas({ valor, tamano = 'sm' }: EstrellasProps) {
    const { t } = useTranslation();
    const { formatearNumero } = useLocalizacion();

    const acotado = Math.min(Math.max(valor, 0), PUNTAJE_MAXIMO);
    const porcentaje = (acotado / PUNTAJE_MAXIMO) * 100;
    const clase = CLASE_TAMANO[tamano];

    return (
        <span
            role="img"
            aria-label={t('comun.valoracion.estrellas', {
                valor: formatearNumero(acotado, Number.isInteger(acotado) ? 0 : 1),
            })}
            className="relative inline-flex shrink-0 align-middle"
        >
            <span className="flex text-borde">
                {POSICIONES.map((posicion) => (
                    <IconoEstrella key={posicion} className={clase} />
                ))}
            </span>

            <span
                className="absolute inset-y-0 left-0 flex overflow-hidden text-estrella"
                style={{ width: `${porcentaje}%` }}
            >
                {POSICIONES.map((posicion) => (
                    <IconoEstrella key={posicion} className={clase} />
                ))}
            </span>
        </span>
    );
}
