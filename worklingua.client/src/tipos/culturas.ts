/**
 * Configuración regional de un idioma de plataforma, espejo de
 * `Contratos/CulturaResponse.cs`.
 *
 * Es lo que el frontend necesita para formatear fechas, números e importes sin
 * tener el locale hardcodeado.
 */
export interface CulturaResponse {
    culturaId: number;
    /** Locale, por ejemplo `es-AR`. Se le pasa tal cual a `Intl`. */
    codigo: string;
    nombre: string;
    /** Código ISO del idioma. Es la clave que empareja cultura con idioma elegido. */
    codigoIdioma: string;
    moneda: string;
    simboloMoneda: string;
    formatoFecha: string;
    separadorDecimal: string;
    separadorMiles: string;
    /**
     * Multiplicador sobre los importes, que se guardan en la moneda base (ARS).
     * Se carga a mano desde el ABM: no hay cotización automática.
     */
    tasaConversion: number;
    /** Cuál de las culturas de un idioma se aplica cuando hay más de una. */
    esPredeterminada: boolean;
}

/** Cultura vista desde el Backoffice, espejo de `Contratos/CulturaAdminResponse.cs`. */
export interface CulturaAdminResponse extends CulturaResponse {
    idiomaId: number;
    idioma: string;
    activo: boolean;
}

/** Cuerpo del alta y de la modificación, espejo de `GuardarCulturaRequest.cs`. */
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
