/** Empresa vista desde el Backoffice, espejo de `Contratos/EmpresaAdminResponse.cs`. */
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
    /** Una empresa protegida no se puede dar de baja. */
    protegido: boolean;
    /** Una empresa no seleccionable no se ofrece en ningún selector de empresa. */
    seleccionable: boolean;
}

/** Cuerpo del alta y de la modificación, espejo de `GuardarEmpresaRequest.cs`. */
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
