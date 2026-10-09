import { useEffect, useId, useRef, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

interface ModalProps {
    titulo: string;
    descripcion?: string;
    abierto: boolean;
    /**
     * `ancho` da lugar a un listado con acciones; `normal` es el diálogo de
     * confirmación o el formulario corto.
     */
    ancho?: 'normal' | 'ancho';
    alCerrar: () => void;
    children: ReactNode;
}

const ANCHOS: Record<'normal' | 'ancho', string> = {
    normal: 'w-[min(32rem,calc(100vw-2rem))]',
    ancho: 'w-[min(48rem,calc(100vw-2rem))]',
};

/**
 * Diálogo modal sobre el elemento nativo `<dialog>`.
 *
 * Se usa `showModal()` y no un div posicionado a mano porque el elemento nativo
 * ya resuelve lo difícil: contiene el foco, cierra con Escape, oscurece el fondo
 * con `::backdrop` y se anuncia como diálogo a los lectores de pantalla. Un
 * modal casero tendría que reimplementar las cuatro cosas y suele fallar en la
 * primera.
 *
 * El evento `close` del elemento se escucha igual, porque Escape cierra el
 * diálogo por fuera de React y el estado de arriba tiene que enterarse.
 *
 * El id del título sale de `useId` y no de una constante, para que dos modales
 * montados a la vez no dejen el `aria-labelledby` apuntando al título del otro.
 */
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
            // Clic sobre el fondo: el target es el propio dialog solo cuando se
            // toca el ::backdrop, porque el contenido vive en el div de adentro.
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
