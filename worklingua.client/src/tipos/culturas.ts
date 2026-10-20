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
