import { Outlet } from 'react-router-dom';
import { EncabezadoPublico } from './EncabezadoPublico';

export function LayoutAuth() {
    return (
        <div className="flex min-h-screen flex-col bg-fondo">
            <EncabezadoPublico />
            <Outlet />
        </div>
    );
}
