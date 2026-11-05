export interface EmpresaAdminResponse {
    empresaId: number;
    razonSocial: string;
    cuit: string;
    email: string;
    telefono: string | null;
    direccion: string | null;
    ciudad: string | null;
    provincia: string | null;
    pais: string | null;
    activo: boolean;
    protegido: boolean;
    seleccionable: boolean;
}

export interface GuardarEmpresaRequest {
    razonSocial: string;
    cuit: string;
    email: string;
    telefono: string | null;
    direccion: string | null;
    ciudad: string | null;
    provincia: string | null;
    pais: string | null;
}

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
