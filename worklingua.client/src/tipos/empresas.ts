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
