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
