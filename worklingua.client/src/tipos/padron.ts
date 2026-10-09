/** Una fila del CSV tal como se manda al backend, espejo de `BE/BEFilaPadron.cs`. */
export interface FilaPadronRequest {
    nombre: string;
    apellido: string;
    email: string;
    documento: string | null;
    /** Nombre del departamento; el backend lo resuelve contra el organigrama. */
    departamento: string | null;
}

/** Cuerpo del procesamiento, espejo de `BE/BEPadron.cs`. */
export interface PadronRequest {
    empresaId: number;
    /** false previsualiza sin escribir nada; true manda las invitaciones. */
    confirmar: boolean;
    filas: FilaPadronRequest[];
}

/** El veredicto de una fila, espejo de `BE/BEResultadoFilaPadron.cs`. */
export interface ResultadoFilaPadronResponse {
    fila: number;
    nombre: string | null;
    apellido: string | null;
    email: string | null;
    departamento: string | null;
    valida: boolean;
    /** Por qué se rechaza; null cuando la fila está bien. */
    motivo: string | null;
}

/** El resultado completo, espejo de `BE/BEResultadoPadron.cs`. */
export interface ResultadoPadronResponse {
    leidas: number;
    validas: number;
    conError: number;
    confirmado: boolean;
    cupoDisponible: number;
    filas: ResultadoFilaPadronResponse[];
}
