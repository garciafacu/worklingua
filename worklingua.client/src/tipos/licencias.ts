export interface InventarioLicenciaResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    email: string;
    departamento: string | null;
    licenciaId: number | null;
    codigoLicencia: string | null;
    fechaAsignacion: string | null;
}

export interface InventarioResponse {
    empresaId: number;
    empresa: string;
    plan: string;
    contratadas: number;
    asignadas: number;
    disponibles: number;
    empleados: InventarioLicenciaResponse[];
}

export interface AsignarLicenciaRequest {
    usuarioId: number;
}
