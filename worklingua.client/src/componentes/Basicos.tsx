import type { ReactNode } from 'react';

interface MarcaProps {
    claseAcento?: string;
}

export function Marca({ claseAcento }: MarcaProps) {
    return (
        <>
            Work<span className={claseAcento}>Lingua</span>
        </>
    );
}

interface SeccionProps {
    titulo: string;
    descripcion?: string;
    children: ReactNode;
    fondoAlterno?: boolean;
}

export function Seccion({ titulo, descripcion, children, fondoAlterno = false }: SeccionProps) {
    return (
        <section className={fondoAlterno ? 'bg-superficie' : 'bg-fondo'}>
            <div className="mx-auto max-w-6xl px-4 py-12 sm:py-16">
                <h2 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">{titulo}</h2>

                {descripcion && <p className="mt-3 max-w-2xl text-base text-texto-suave">{descripcion}</p>}

                <div className="mt-8">{children}</div>
            </div>
        </section>
    );
}

interface BarraProgresoProps {
    porcentaje: number;
    etiqueta: string;
}

export function BarraProgreso({ porcentaje, etiqueta }: BarraProgresoProps) {
    const valor = Math.max(0, Math.min(100, porcentaje));

    return (
        <div
            role="progressbar"
            aria-label={etiqueta}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-valuenow={valor}
            className="h-2 w-full overflow-hidden rounded-full bg-fondo"
        >
            <div
                className={`h-full rounded-full transition-[width] ${valor === 100 ? 'bg-exito' : 'bg-primario'}`}
                style={{ width: `${valor}%` }}
            />
        </div>
    );
}

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

export function EtiquetaEstado({ texto, tono }: EtiquetaEstadoProps) {
    return (
        <span
            className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap ${TONOS[tono]}`}
        >
            {texto}
        </span>
    );
}

interface AcordeonProps {
    identificador: string;
    titulo: string;
    abierto: boolean;
    alAlternar: () => void;
    children: ReactNode;
}

export function Acordeon({ identificador, titulo, abierto, alAlternar, children }: AcordeonProps) {
    const idBoton = `${identificador}-boton`;
    const idPanel = `${identificador}-panel`;

    return (
        <div className="rounded-2xl border border-borde bg-superficie">
            <h3 className="m-0">
                <button
                    type="button"
                    id={idBoton}
                    aria-expanded={abierto}
                    aria-controls={idPanel}
                    onClick={alAlternar}
                    className="flex w-full items-center justify-between gap-4 rounded-2xl bg-transparent px-5 py-4 text-left text-base font-semibold text-texto hover:bg-fondo"
                >
                    <span>{titulo}</span>
                    <svg
                        viewBox="0 0 20 20"
                        fill="currentColor"
                        aria-hidden="true"
                        className={`h-5 w-5 shrink-0 text-texto-suave transition-transform ${abierto ? 'rotate-180' : ''}`}
                    >
                        <path
                            fillRule="evenodd"
                            d="M5.23 7.21a.75.75 0 0 1 1.06.02L10 11.17l3.71-3.94a.75.75 0 1 1 1.08 1.04l-4.25 4.5a.75.75 0 0 1-1.08 0l-4.25-4.5a.75.75 0 0 1 .02-1.06Z"
                            clipRule="evenodd"
                        />
                    </svg>
                </button>
            </h3>

            <div
                id={idPanel}
                role="region"
                aria-labelledby={idBoton}
                hidden={!abierto}
                className="px-5 pb-5 text-base leading-relaxed text-texto-suave"
            >
                {children}
            </div>
        </div>
    );
}
