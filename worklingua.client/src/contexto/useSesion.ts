import { useContext } from 'react';
import { SesionContexto, type EstadoSesion } from './sesionContexto';

export function useSesion(): EstadoSesion {
    const contexto = useContext(SesionContexto);

    if (!contexto) {
        throw new Error('useSesion debe usarse dentro de un SesionProvider.');
    }

    return contexto;
}
