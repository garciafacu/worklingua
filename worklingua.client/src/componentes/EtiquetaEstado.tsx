export type TonoEstado = 'neutro' | 'info' | 'alerta' | 'exito' | 'error';

const TONOS: Record<TonoEstado, string> = {
    neutro: 'bg-fondo text-texto-suave',
    info: 'bg-info-fondo text-info',
    alerta: 'bg-alerta-fondo text-alerta',
    exito: 'bg-exito-fondo text-exito',
    error: 'bg-error-fondo text-error',
};

interface EtiquetaEstadoProps {
    texto: string;
    tono: TonoEstado;
}

/**
 * Pastilla de estado (consulta, curso, factura). Quien la usa decide el texto ya
 * traducido y el tono, así el mismo componente sirve para vocabularios distintos.
 */
export function EtiquetaEstado({ texto, tono }: EtiquetaEstadoProps) {
    return (
        <span
            className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap ${TONOS[tono]}`}
        >
            {texto}
        </span>
    );
}
