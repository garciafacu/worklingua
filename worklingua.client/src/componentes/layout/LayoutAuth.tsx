import { Outlet } from 'react-router-dom';
import { EncabezadoPublico } from './EncabezadoPublico';

/**
 * Maqueta de las pantallas de cuenta (login, registro, recuperar clave): el
 * mismo encabezado que el sitio público, sin el pie. Estas pantallas se
 * resuelven en una sola tarjeta centrada y no necesitan los enlaces
 * institucionales del pie de página.
 */
export function LayoutAuth() {
    return (
        <div className="flex min-h-screen flex-col bg-fondo">
            <EncabezadoPublico />
            <Outlet />
        </div>
    );
}
