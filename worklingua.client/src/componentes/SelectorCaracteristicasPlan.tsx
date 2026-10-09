import { useTranslation } from 'react-i18next';
import type { CaracteristicaResponse } from '../tipos/caracteristicas';

/** Una característica del catálogo más lo que ofrece el plan que se está editando. */
export interface FilaCaracteristicaPlan {
    caracteristica: CaracteristicaResponse;
    asociada: boolean;
    incluido: boolean;
    detalle: string;
}

interface SelectorCaracteristicasPlanProps {
    filas: FilaCaracteristicaPlan[];
    cargando: boolean;
    /** Sin permiso de modificar, el selector queda de solo lectura. */
    puedeGuardar: boolean;
    /** Ausente cuando la sesión no puede ver el catálogo. */
    alAdministrarCatalogo?: () => void;
    alCambiar: (caracteristicaId: number, cambios: Partial<FilaCaracteristicaPlan>) => void;
}

/**
 * Define qué características tiene un plan y con qué valor.
 *
 * Muestra el catálogo activo completo, no solo lo ya asociado: asociar una
 * característica nueva es marcar su casilla, y desasociarla es desmarcarla. Al
 * guardar se manda el conjunto entero y el backend reemplaza lo que había, así
 * que lo desmarcado queda desasociado.
 *
 * "Incluida" separa el ✓ del —: una característica puede estar asociada al plan
 * y aun así mostrarse como no incluida, que es lo que permite dejar constancia
 * explícita de lo que el plan no ofrece.
 *
 * Es un componente controlado: el estado vive en `FormularioPlan` porque el plan
 * y sus características se guardan con un mismo botón.
 */
export function SelectorCaracteristicasPlan({
    filas,
    cargando,
    puedeGuardar,
    alAdministrarCatalogo,
    alCambiar,
}: SelectorCaracteristicasPlanProps) {
    const { t } = useTranslation();

    return (
        <div>
            <p className="text-sm text-texto-suave">{t('admin.planes.panel.ayuda')}</p>

            {cargando ? (
                <p className="mt-4 text-sm text-texto-suave">{t('admin.planes.panel.cargando')}</p>
            ) : filas.length === 0 ? (
                <p className="mt-4 text-sm text-texto-suave">{t('admin.planes.panel.vacio')}</p>
            ) : (
                <ul className="mt-4 divide-y divide-borde border-y border-borde">
                    {filas.map((fila) => (
                        <li
                            key={fila.caracteristica.caracteristicaId}
                            className="flex flex-wrap items-center gap-x-4 gap-y-2 py-3"
                        >
                            <label className="flex min-w-56 flex-1 items-center gap-2.5 text-sm text-texto">
                                <input
                                    type="checkbox"
                                    className="size-4"
                                    disabled={!puedeGuardar}
                                    checked={fila.asociada}
                                    onChange={(evento) =>
                                        alCambiar(fila.caracteristica.caracteristicaId, {
                                            asociada: evento.target.checked,
                                        })
                                    }
                                />
                                {fila.caracteristica.nombre}
                            </label>

                            <label
                                className={`flex items-center gap-2 text-sm ${
                                    fila.asociada ? 'text-texto-suave' : 'text-borde'
                                }`}
                            >
                                <input
                                    type="checkbox"
                                    className="size-4"
                                    disabled={!puedeGuardar || !fila.asociada}
                                    checked={fila.incluido}
                                    onChange={(evento) =>
                                        alCambiar(fila.caracteristica.caracteristicaId, {
                                            incluido: evento.target.checked,
                                        })
                                    }
                                />
                                {t('admin.planes.panel.incluida')}
                            </label>

                            <input
                                type="text"
                                maxLength={100}
                                placeholder={t('admin.planes.panel.detalle')}
                                aria-label={t('admin.planes.panel.detalleDe', {
                                    nombre: fila.caracteristica.nombre,
                                })}
                                disabled={!puedeGuardar || !fila.asociada}
                                value={fila.detalle}
                                onChange={(evento) =>
                                    alCambiar(fila.caracteristica.caracteristicaId, {
                                        detalle: evento.target.value,
                                    })
                                }
                                className="w-56 rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm text-texto disabled:bg-fondo disabled:text-texto-suave"
                            />
                        </li>
                    ))}
                </ul>
            )}

            {alAdministrarCatalogo && (
                <button
                    type="button"
                    onClick={alAdministrarCatalogo}
                    className="mt-4 rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-fondo"
                >
                    {t('admin.planes.panel.administrarCatalogo')}
                </button>
            )}
        </div>
    );
}
