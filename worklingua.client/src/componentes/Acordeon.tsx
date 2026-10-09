import type { ReactNode } from 'react';

interface AcordeonProps {
    /** Base de los `id` del botón y del panel; tiene que ser única en la página. */
    identificador: string;
    titulo: string;
    abierto: boolean;
    alAlternar: () => void;
    children: ReactNode;
}

/**
 * Bloque que se expande y se contrae. El estado lo maneja quien lo usa, para
 * poder abrir o cerrar varios a la vez (por ejemplo, al limpiar una búsqueda).
 */
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

            {/* El panel cerrado queda en el DOM con `hidden` para que `aria-controls` apunte a algo. */}
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
