/** Un resultado del buscador global, espejo de `BE/BEPaginaBuscableRespuesta.cs`. */
export interface PaginaBuscableResponse {
    ruta: string;
    /** Claves de traducción: la pantalla las resuelve con `t()` en el idioma activo. */
    claveTitulo: string;
    claveDescripcion: string;
    claveSeccion: string;
    area: AreaBusqueda;
}

/** Una sección que la sesión puede ver, espejo de `BE/BESeccionBusquedaRespuesta.cs`. */
export interface SeccionBusquedaResponse {
    clave: string;
    area: AreaBusqueda;
}

/** Espejo de `BLLBusqueda.AreaPublica` y `BLLBusqueda.AreaPrivada`. */
export type AreaBusqueda = 'Publica' | 'Privada';

/** Espejo de las constantes `Orden*` de `BLLBusqueda`. */
export type OrdenBusqueda = 'Relevancia' | 'NombreAsc' | 'NombreDesc';

/**
 * Criterios del buscador global.
 *
 * La búsqueda simple usa solo `texto`; la avanzada suma el resto. El filtro por
 * permisos lo hace el backend: acá nunca se decide qué páginas ve cada sesión.
 */
export interface CriteriosBusqueda {
    texto?: string;
    area?: AreaBusqueda;
    seccion?: string;
    ordenarPor?: OrdenBusqueda;
    soloTitulo?: boolean;
}

/** Áreas del buscador avanzado. La etiqueta es una CLAVE de traducción. */
export const AREAS_BUSQUEDA: { valor: AreaBusqueda; etiqueta: string }[] = [
    { valor: 'Publica', etiqueta: 'busqueda.area.Publica' },
    { valor: 'Privada', etiqueta: 'busqueda.area.Privada' },
];

/** Criterios de orden. El primero es el que usa el backend por defecto. */
export const ORDENES_BUSQUEDA: { valor: OrdenBusqueda; etiqueta: string }[] = [
    { valor: 'Relevancia', etiqueta: 'busqueda.orden.Relevancia' },
    { valor: 'NombreAsc', etiqueta: 'busqueda.orden.NombreAsc' },
    { valor: 'NombreDesc', etiqueta: 'busqueda.orden.NombreDesc' },
];
