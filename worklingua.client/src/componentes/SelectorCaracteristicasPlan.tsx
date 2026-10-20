import { useTranslation } from 'react-i18next';
import type { CaracteristicaResponse } from '../tipos/caracteristicas';

export interface FilaCaracteristicaPlan {
    caracteristica: CaracteristicaResponse;
    asociada: boolean;
    incluido: boolean;
    detalle: string;
}

interface SelectorCaracteristicasPlanProps {
    filas: FilaCaracteristicaPlan[];
    cargando: boolean;
    puedeGuardar: boolean;
    alAdministrarCatalogo?: () => void;
    alCambiar: (caracteristicaId: number, cambios: Partial<FilaCaracteristicaPlan>) => void;
}

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
