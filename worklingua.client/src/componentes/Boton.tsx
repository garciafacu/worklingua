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

const COLORES = {
    neutro: 'border-borde text-texto hover:bg-fondo',
    primario: 'border-borde text-primario hover:bg-info-fondo',
    peligro: 'border-borde text-error hover:bg-error-fondo',
    confirmar: 'border-error text-error hover:bg-error-fondo',
};

interface BotonAccionProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variante?: keyof typeof COLORES;
    grande?: boolean;
}

export function BotonAccion({ variante = 'neutro', grande = false, className, ...resto }: BotonAccionProps) {
    const tamano = grande ? 'px-4 py-2' : 'px-3 py-1.5';

    return (
        <button
            type="button"
            className={`rounded-lg border bg-superficie ${tamano} text-sm font-semibold ${COLORES[variante]} ${className ?? ''}`.trim()}
            {...resto}
        />
    );
}

interface BotonesFormularioProps {
    editando: boolean;
    guardando: boolean;
    textoCrear: string;
    onCancelar: () => void;
}

export function BotonesFormulario({ editando, guardando, textoCrear, onCancelar }: BotonesFormularioProps) {
    const { t } = useTranslation();

    return (
        <div className="flex flex-wrap gap-3">
            <Boton type="submit" cargando={guardando}>
                {editando ? t('comun.boton.guardarCambios') : textoCrear}
            </Boton>

            {editando && (
                <BotonAccion grande onClick={onCancelar}>
                    {t('comun.boton.cancelar')}
                </BotonAccion>
            )}
        </div>
    );
}

interface ConfirmarBajaProps {
    confirmando: boolean;
    onPedir: () => void;
    onConfirmar: () => void;
    onCancelar: () => void;
}

export function ConfirmarBaja({ confirmando, onPedir, onConfirmar, onCancelar }: ConfirmarBajaProps) {
    const { t } = useTranslation();

    if (!confirmando) {
        return (
            <BotonAccion variante="peligro" onClick={onPedir}>
                {t('comun.boton.darDeBaja')}
            </BotonAccion>
        );
    }

    return (
        <>
            <BotonAccion variante="confirmar" onClick={onConfirmar}>
                {t('comun.boton.confirmarBaja')}
            </BotonAccion>
            <BotonAccion onClick={onCancelar}>{t('comun.boton.no')}</BotonAccion>
        </>
    );
}
