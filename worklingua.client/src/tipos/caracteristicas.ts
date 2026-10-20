export interface CaracteristicaResponse {
    caracteristicaId: number;
    nombre: string;
    orden: number;
}

export interface CaracteristicaAdminResponse extends CaracteristicaResponse {
    activo: boolean;
}

export interface GuardarCaracteristicaRequest {
    nombre: string;
    orden: number;
}
