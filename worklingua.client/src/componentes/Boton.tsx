import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    cargando?: boolean;
    children: ReactNode;
}

export function Boton({ cargando = false, disabled, children, ...resto }: BotonProps) {
    const { t } = useTranslation();

    return (
        <button type="button" disabled={disabled || cargando} {...resto}>
            {cargando ? t('comun.estado.procesando') : children}
        </button>
    );
}
