import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import type { PermisoResponse } from '../tipos/permisos';

type AccionesNodo = (nodo: PermisoResponse, padre: PermisoResponse | null) => ReactNode;

interface ArbolPermisosProps {
    permisos: PermisoResponse[];
    /** Botones de cada nodo. `padre` es null en las raíces. */
    acciones?: AccionesNodo;
    mensajeVacio: string;
}

/**
 * Jerarquía de permisos: las familias muestran anidado todo lo que contienen.
 *
 * La usan el ABM de Permisos (todo el árbol) y el de Roles (lo asignado a un rol).
 */
export function ArbolPermisos({ permisos, acciones, mensajeVacio }: ArbolPermisosProps) {
    if (permisos.length === 0) {
        return <p className="text-sm text-texto-suave">{mensajeVacio}</p>;
    }

    return (
        <ul className="rounded-2xl border border-borde bg-superficie px-4 py-2">
            {permisos.map((permiso) => (
                <NodoPermiso key={permiso.permisoId} nodo={permiso} padre={null} acciones={acciones} />
            ))}
        </ul>
    );
}

interface NodoPermisoProps {
    nodo: PermisoResponse;
    padre: PermisoResponse | null;
    acciones?: AccionesNodo;
}

function NodoPermiso({ nodo, padre, acciones }: NodoPermisoProps) {
    const { t } = useTranslation();

    return (
        <li className="py-1">
            <div className="flex flex-wrap items-center justify-between gap-2 py-1">
                <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                        <span
                            className={
                                nodo.esCompuesto
                                    ? 'rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario'
                                    : 'rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave'
                            }
                        >
                            {nodo.esCompuesto
                                ? t('admin.permisos.tipo.familia')
                                : t('admin.permisos.tipo.patente')}
                        </span>
                        <span className="text-sm font-semibold break-all text-texto">{nodo.nombre}</span>
                    </div>
                    {nodo.descripcion && (
                        <p className="mt-0.5 text-xs text-texto-suave">{nodo.descripcion}</p>
                    )}
                </div>

                {acciones && <div className="flex flex-wrap justify-end gap-2">{acciones(nodo, padre)}</div>}
            </div>

            {nodo.hijos.length > 0 && (
                <ul className="ml-2 border-l border-borde pl-4">
                    {nodo.hijos.map((hijo) => (
                        <NodoPermiso key={hijo.permisoId} nodo={hijo} padre={nodo} acciones={acciones} />
                    ))}
                </ul>
            )}
        </li>
    );
}
