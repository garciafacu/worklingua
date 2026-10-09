import { Outlet } from 'react-router-dom';
import { EncabezadoPublico } from './EncabezadoPublico';
import { PieDePagina } from './PieDePagina';

/**
 * Maqueta del sitio público: encabezado de navegación, contenido de la página y
 * pie. Todas las rutas públicas cuelgan de acá, así que el encabezado no se
 * remonta al navegar entre ellas.
 */
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
