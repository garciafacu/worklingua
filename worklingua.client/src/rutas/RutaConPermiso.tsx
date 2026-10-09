import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useSesion } from '../contexto/useSesion';

/**
 * Deja pasar solo si la sesión tiene el permiso indicado.
 *
 * Igual que `RutaProtegida`, es una comodidad de navegación y no un control de
 * seguridad: quien autoriza es el backend, que resuelve los permisos del
 * usuario contra la base en cada pedido y responde 403 si faltan.
 *
 * Sin sesión manda al login; con sesión pero sin el permiso manda al panel, que
 * es una pantalla a la que sí puede entrar: rebotar al login a alguien que ya
 * inició sesión se lee como que la sesión se perdió.
 */
export function RutaConPermiso({ permiso, children }: { permiso: string; children: ReactNode }) {
    const { autenticado, tienePermiso } = useSesion();

    if (!autenticado) {
        return <Navigate to="/login" replace />;
    }

    return tienePermiso(permiso) ? <>{children}</> : <Navigate to="/inicio" replace />;
}
