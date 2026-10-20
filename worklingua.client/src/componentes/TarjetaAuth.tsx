import type { ReactNode } from 'react';
import { Marca } from './Marca';

interface TarjetaAuthProps {
    titulo: string;
    descripcion?: string;
    children: ReactNode;
    pie?: ReactNode;
    contenedor?: 'pantalla' | 'panel';
}

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
