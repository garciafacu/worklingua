export interface PermisoResponse {
    permisoId: number;
    nombre: string;
    descripcion: string | null;
    esCompuesto: boolean;
    hijos: PermisoResponse[];
}

export interface GuardarPermisoRequest {
    nombre: string;
    descripcion: string | null;
}

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
