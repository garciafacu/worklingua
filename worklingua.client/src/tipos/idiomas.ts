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
