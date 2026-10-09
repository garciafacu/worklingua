/**
 * Nodo de la jerarquía de permisos, espejo de `BE/BEPermisoRespuesta.cs`.
 *
 * Una patente (`esCompuesto: false`) es un permiso que el código chequea por
 * nombre y no tiene hijos. Una familia (`esCompuesto: true`) agrupa patentes y
 * otras familias.
 */
export interface PermisoResponse {
    permisoId: number;
    nombre: string;
    descripcion: string | null;
    esCompuesto: boolean;
    hijos: PermisoResponse[];
}

/** Cuerpo del alta y de la modificación de una familia, espejo de `BEGuardarPermiso.cs`. */
export interface GuardarPermisoRequest {
    nombre: string;
    descripcion: string | null;
}

/**
 * Todos los nodos del árbol una sola vez, ordenados por nombre.
 *
 * Un permiso puede aparecer en varias familias; para los selectores alcanza con
 * una entrada por permiso.
 */
export function aplanarPermisos(nodos: PermisoResponse[]): PermisoResponse[] {
    const porId = new Map<number, PermisoResponse>();

    const recorrer = (lista: PermisoResponse[]) => {
        for (const nodo of lista) {
            if (!porId.has(nodo.permisoId)) {
                porId.set(nodo.permisoId, nodo);
                recorrer(nodo.hijos);
            }
        }
    };

    recorrer(nodos);

    return [...porId.values()].sort((a, b) => a.nombre.localeCompare(b.nombre));
}
