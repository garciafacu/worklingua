import { useContext } from 'react';
import { LocalizacionContexto, type EstadoLocalizacion } from './localizacionContexto';

export function useLocalizacion(): EstadoLocalizacion {
    const contexto = useContext(LocalizacionContexto);

    if (contexto === undefined) {
        throw new Error('useLocalizacion tiene que usarse dentro de LocalizacionProvider.');
    }

    return contexto;
}
