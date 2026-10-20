import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useSesion } from '../contexto/useSesion';
import { conPlanPendiente, planIdDeParametros } from './contratacionPendiente';

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
