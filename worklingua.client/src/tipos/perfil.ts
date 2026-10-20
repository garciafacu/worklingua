export interface PerfilResponse {
    usuarioId: number;
    nombre: string;
    apellido: string;
    fechaNacimiento: string | null;
    email: string;
    documento: string | null;
    empresa: string;
    departamento: string | null;
    roles: string[];
}

export interface ModificarPerfilRequest {
    nombre: string;
    apellido: string;
    fechaNacimiento: string | null;
}
