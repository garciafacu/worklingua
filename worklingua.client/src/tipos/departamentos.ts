export interface DepartamentoResponse {
    departamentoId: number;
    empresaId: number;
    empresa: string;
    nombre: string;
    descripcion: string | null;
}

export interface DepartamentoAdminResponse extends DepartamentoResponse {
    activo: boolean;
    empleados: number;
    fechaAlta: string | null;
}

export interface GuardarDepartamentoRequest {
    empresaId: number;
    nombre: string;
    descripcion: string | null;
}
