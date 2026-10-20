import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useSesion } from '../contexto/useSesion';

export function RutaConPermiso({ permiso, children }: { permiso: string; children: ReactNode }) {
    const { autenticado, tienePermiso } = useSesion();

    if (!autenticado) {
        return <Navigate to="/login" replace />;
    }

    return tienePermiso(permiso) ? <>{children}</> : <Navigate to="/inicio" replace />;
}
