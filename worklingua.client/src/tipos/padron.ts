export interface FilaPadronRequest {
    nombre: string;
    apellido: string;
    email: string;
    documento: string | null;
    departamento: string | null;
}

export interface PadronRequest {
    empresaId: number;
    confirmar: boolean;
    filas: FilaPadronRequest[];
}

export interface ResultadoFilaPadronResponse {
    fila: number;
    nombre: string | null;
    apellido: string | null;
    email: string | null;
    departamento: string | null;
    valida: boolean;
    motivo: string | null;
}

export interface ResultadoPadronResponse {
    leidas: number;
    validas: number;
    conError: number;
    confirmado: boolean;
    cupoDisponible: number;
    filas: ResultadoFilaPadronResponse[];
}
