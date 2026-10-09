import type { ReactNode } from 'react';
import { Marca } from './Marca';

interface TarjetaAuthProps {
    titulo: string;
    descripcion?: string;
    children: ReactNode;
    pie?: ReactNode;
    /**
     * `pantalla` centra la tarjeta en un alto completo de viewport: es lo que
     * necesitan login, registro y recupero, que se ven solas.
     * `panel` omite ese envoltorio para que la tarjeta pueda vivir dentro del
     * área privada, que ya tiene su propio encabezado y menú.
     */
    contenedor?: 'pantalla' | 'panel';
}

/** Contenedor común de las pantallas de cuenta, para no repetir la maqueta. */
export function TarjetaAuth({
    titulo,
    descripcion,
    children,
    pie,
    contenedor = 'pantalla',
}: TarjetaAuthProps) {
    const tarjeta = (
        <section className="tarjeta">
            {contenedor === 'pantalla' && (
                <p className="marca">
                    <Marca />
                </p>
            )}

            <h1>{titulo}</h1>
            {descripcion && <p className="descripcion">{descripcion}</p>}

            {children}

            {pie && <div className="pie-tarjeta">{pie}</div>}
        </section>
    );

    if (contenedor === 'panel') {
        return tarjeta;
    }

    return <main className="pantalla-auth">{tarjeta}</main>;
}
