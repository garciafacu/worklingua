export interface PaginaBuscableResponse {
    ruta: string;
    claveTitulo: string;
    claveDescripcion: string;
    claveSeccion: string;
    area: AreaBusqueda;
}

export interface SeccionBusquedaResponse {
    clave: string;
    area: AreaBusqueda;
}

export type AreaBusqueda = 'Publica' | 'Privada';

export type OrdenBusqueda = 'Relevancia' | 'NombreAsc' | 'NombreDesc';

export interface CriteriosBusqueda {
    texto?: string;
    area?: AreaBusqueda;
    seccion?: string;
    ordenarPor?: OrdenBusqueda;
    soloTitulo?: boolean;
}

export const AREAS_BUSQUEDA: { valor: AreaBusqueda; etiqueta: string }[] = [
    { valor: 'Publica', etiqueta: 'busqueda.area.Publica' },
    { valor: 'Privada', etiqueta: 'busqueda.area.Privada' },
];

export const ORDENES_BUSQUEDA: { valor: OrdenBusqueda; etiqueta: string }[] = [
    { valor: 'Relevancia', etiqueta: 'busqueda.orden.Relevancia' },
    { valor: 'NombreAsc', etiqueta: 'busqueda.orden.NombreAsc' },
    { valor: 'NombreDesc', etiqueta: 'busqueda.orden.NombreDesc' },
];
