import { Outlet } from 'react-router-dom';
import { EncabezadoPublico } from './EncabezadoPublico';
import { PieDePagina } from './PieDePagina';

export function LayoutPublico() {
    return (
        <div className="flex min-h-screen flex-col bg-fondo">
            <EncabezadoPublico />

            <main className="flex-1">
                <Outlet />
            </main>

            <PieDePagina />
        </div>
    );
}
