/**
 * El bundle de textos de un idioma: un diccionario plano clave/texto.
 *
 * Llega de `GET /api/traducciones/{codigoISO}`. El fallback al español ya viene
 * resuelto por el Stored Procedure, así que nunca tiene huecos.
 */
export type BundleTraducciones = Record<string, string>;

/**
 * Una fila de la grilla del ABM, espejo de `Contratos/TraduccionAdminResponse.cs`.
 */
export interface TraduccionAdminResponse {
    clave: string;
    /** El texto del idioma base. Se muestra siempre, de solo lectura. */
    textoEspanol: string;
    /** El texto del idioma elegido. Cadena vacía cuando está pendiente. */
    texto: string;
    pendiente: boolean;
}

/** Criterios de búsqueda del ABM de Traducciones. */
export interface CriteriosTraduccion {
    idiomaId: number;
    clave?: string;
    texto?: string;
    soloPendientes?: boolean;
}

/**
 * Cuerpo del guardado en lote, espejo de `Contratos/GuardarTraduccionesRequest.cs`.
 *
 * Un `texto` vacío es válido: devuelve la clave al estado pendiente.
 */
export interface GuardarTraduccionesRequest {
    idiomaId: number;
    traducciones: { clave: string; texto: string }[];
}
