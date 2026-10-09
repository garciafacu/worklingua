import type { ReactNode } from 'react';

interface SeccionProps {
    titulo: string;
    descripcion?: string;
    children: ReactNode;
    /** Alterna el fondo para separar visualmente secciones contiguas. */
    fondoAlterno?: boolean;
}

/**
 * Bloque de contenido de una página pública: encabezado y cuerpo dentro del
 * mismo ancho máximo. Evita repetir la maqueta en cada página estática.
 */
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
