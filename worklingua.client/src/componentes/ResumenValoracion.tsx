import { useTranslation } from 'react-i18next';
import { Estrellas } from './Estrellas';
import { useLocalizacion } from '../contexto/useLocalizacion';

interface ResumenValoracionProps {
    promedio: number;
    cantidad: number;
}

/**
 * Promedio de estrellas y cantidad de valoraciones de un plan, en una línea.
 *
 * Mide siempre lo mismo, tenga o no valoraciones, para no desalinear las filas
 * de las tarjetas de plan puestas lado a lado.
 */
export function ResumenValoracion({ promedio, cantidad }: ResumenValoracionProps) {
    const { t } = useTranslation();
    const { formatearNumero } = useLocalizacion();

    if (cantidad === 0) {
        return (
            <p className="m-0 flex min-h-6 items-center gap-2 text-sm text-texto-suave">
                <Estrellas valor={0} />
                {t('comun.valoracion.sinValoraciones')}
            </p>
        );
    }

    return (
        <p className="m-0 flex min-h-6 items-center gap-2 text-sm">
            <span className="font-semibold text-texto">{formatearNumero(promedio, 1)}</span>
            <Estrellas valor={promedio} />
            <span className="text-texto-suave">
                (
                {cantidad === 1
                    ? t('comun.valoracion.cantidadUna')
                    : t('comun.valoracion.cantidad', { cantidad: formatearNumero(cantidad) })}
                )
            </span>
        </p>
    );
}
