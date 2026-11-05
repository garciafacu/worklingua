export interface IdiomaResponse {
    idiomaId: number;
    nombre: string;
    codigoISO: string;
}

export interface IdiomaAdminResponse extends IdiomaResponse {
    activo: boolean;
}

export interface GuardarIdiomaRequest {
    nombre: string;
    codigoISO: string;
}

export interface CulturaResponse {
    culturaId: number;
    codigo: string;
    nombre: string;
    codigoIdioma: string;
    moneda: string;
    simboloMoneda: string;
    formatoFecha: string;
    separadorDecimal: string;
    separadorMiles: string;
    tasaConversion: number;
    esPredeterminada: boolean;
}

export interface CulturaAdminResponse extends CulturaResponse {
    idiomaId: number;
    idioma: string;
    activo: boolean;
}

export interface GuardarCulturaRequest {
    codigo: string;
    nombre: string;
    idiomaId: number;
    moneda: string;
    simboloMoneda: string;
    formatoFecha: string;
    separadorDecimal: string;
    separadorMiles: string;
    tasaConversion: number;
    esPredeterminada: boolean;
}

export type BundleTraducciones = Record<string, string>;

export interface TraduccionAdminResponse {
    clave: string;
    textoEspanol: string;
    texto: string;
    pendiente: boolean;
}

export interface CriteriosTraduccion {
    idiomaId: number;
    clave?: string;
    texto?: string;
    soloPendientes?: boolean;
}

export interface GuardarTraduccionesRequest {
    idiomaId: number;
    traducciones: { clave: string; texto: string }[];
}
