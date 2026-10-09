import { useId, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { IconoEstrella } from './Estrellas';
import { PUNTAJE_MAXIMO } from '../tipos/opiniones';

interface SelectorEstrellasProps {
    /** Puntaje elegido, o 0 si todavía no se eligió ninguno. */
    valor: number;
    deshabilitado?: boolean;
    alCambiar: (puntaje: number) => void;
}

const PUNTAJES = Array.from({ length: PUNTAJE_MAXIMO }, (_, indice) => indice + 1);

/**
 * Elige de 1 a 5 estrellas.
 *
 * Es un grupo de radios nativo, igual que `SelectorPlanOpinion`: cada estrella es
 * un `<label>` con un `<input type="radio">` oculto. Se navega con las flechas y
 * un lector de pantalla lo anuncia como cualquier grupo de opciones. Al pasar el
 * mouse se ilumina hasta la estrella señalada, como vista previa.
 */
export function SelectorEstrellas({ valor, deshabilitado = false, alCambiar }: SelectorEstrellasProps) {
    const { t } = useTranslation();
    const nombre = useId();
    const [vistaPrevia, setVistaPrevia] = useState<number | null>(null);

    const mostrado = vistaPrevia ?? valor;

    return (
        <fieldset disabled={deshabilitado} className="m-0 min-w-0 border-0 p-0">
            <legend className="p-0 text-sm font-semibold text-texto">
                {t('privado.opiniones.formulario.puntaje')}
            </legend>

            <div className="mt-2 flex flex-wrap items-center gap-x-4 gap-y-2">
                <div className="flex items-center" onMouseLeave={() => setVistaPrevia(null)}>
                    {PUNTAJES.map((puntaje) => (
                        <label
                            key={puntaje}
                            onMouseEnter={() => setVistaPrevia(puntaje)}
                            className="group m-0 cursor-pointer rounded-lg p-1 focus-within:ring-2 focus-within:ring-borde-foco"
                        >
                            <input
                                type="radio"
                                name={nombre}
                                value={puntaje}
                                checked={valor === puntaje}
                                onChange={() => alCambiar(puntaje)}
                                className="sr-only"
                            />
                            <span className="sr-only">
                                {t('comun.valoracion.estrellas', { valor: puntaje })}:{' '}
                                {t(`comun.valoracion.puntaje.${puntaje}`)}
                            </span>
                            <IconoEstrella
                                className={`size-8 transition duration-150 group-hover:scale-110 ${
                                    puntaje <= mostrado ? 'text-estrella' : 'text-borde'
                                }`}
                            />
                        </label>
                    ))}
                </div>

                <span
                    aria-live="polite"
                    className={`text-sm ${
                        mostrado > 0 ? 'font-semibold text-texto' : 'text-texto-suave'
                    }`}
                >
                    {mostrado > 0
                        ? t(`comun.valoracion.puntaje.${mostrado}`)
                        : t('privado.opiniones.formulario.elegiPuntaje')}
                </span>
            </div>
        </fieldset>
    );
}
