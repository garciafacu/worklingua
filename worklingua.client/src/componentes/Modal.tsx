import { useEffect, useId, useRef, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

interface ModalProps {
    titulo: string;
    descripcion?: string;
    abierto: boolean;
    ancho?: 'normal' | 'ancho';
    alCerrar: () => void;
    children: ReactNode;
}

const ANCHOS: Record<'normal' | 'ancho', string> = {
    normal: 'w-[min(32rem,calc(100vw-2rem))]',
    ancho: 'w-[min(48rem,calc(100vw-2rem))]',
};

export function Modal({
    titulo,
    descripcion,
    abierto,
    ancho = 'normal',
    alCerrar,
    children,
}: ModalProps) {
    const { t } = useTranslation();
    const referencia = useRef<HTMLDialogElement>(null);
    const idTitulo = useId();

    useEffect(() => {
        const dialogo = referencia.current;

        if (!dialogo) {
            return;
        }

        if (abierto && !dialogo.open) {
            dialogo.showModal();
        }

        if (!abierto && dialogo.open) {
            dialogo.close();
        }
    }, [abierto]);

    return (
        <dialog
            ref={referencia}
            aria-labelledby={idTitulo}
            onClose={alCerrar}
            onClick={(evento) => {
                if (evento.target === referencia.current) {
                    alCerrar();
                }
            }}
            className={`${ANCHOS[ancho]} rounded-2xl border border-borde bg-superficie p-0 text-texto backdrop:bg-texto/40`}
        >
            <div className="flex items-start justify-between gap-4 border-b border-borde bg-marca px-6 py-4">
                <div>
                    <h2 id={idTitulo} className="text-base font-semibold text-white">
                        {titulo}
                    </h2>
                    {descripcion && <p className="mt-1 text-sm text-white/70">{descripcion}</p>}
                </div>

                <button
                    type="button"
                    onClick={alCerrar}
                    aria-label={t('comun.boton.cerrar')}
                    className="shrink-0 rounded-lg border border-white/25 bg-transparent px-2.5 py-1 text-sm font-semibold text-white hover:bg-white/10"
                >
                    {t('comun.boton.cerrar')}
                </button>
            </div>

            <div className="p-6">{children}</div>
        </dialog>
    );
}
