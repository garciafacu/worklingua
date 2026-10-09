import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useSesion } from '../contexto/useSesion';
import { conPlanPendiente, planIdDeParametros } from './contratacionPendiente';

/**
 * Deja pasar solo con sesión abierta.
 *
 * Es una comodidad de navegación, no un control de seguridad: quien realmente
 * autoriza es el backend, que valida el token contra la tabla Sesion en cada
 * pedido.
 *
 * Un enlace a una contratación (`?contratar=<planId>`) conserva el plan al
 * mandar al login, para retomarla después de ingresar.
 */
export function RutaProtegida({ children }: { children: ReactNode }) {
    const { autenticado } = useSesion();
    const { search } = useLocation();

    if (autenticado) {
        return <>{children}</>;
    }

    return (
        <Navigate
            to={conPlanPendiente('/login', planIdDeParametros(new URLSearchParams(search)))}
            replace
        />
    );
}
