export interface OfertaResponse {
    oferta: {
        ofertaId: number;
        empresaId: number;
        planId: number | null;
        titulo: string;
        descripcion: string;
        fechaDesde: string;
        fechaHasta: string | null;
        activo: boolean;
        usuarioId: number;
        fechaAlta: string;
    };
    empresa: string;
    plan: string | null;
    planActivo: boolean;
}

export interface GuardarOfertaRequest {
    empresaId: number;
    planId: number | null;
    titulo: string;
    descripcion: string;
    fechaDesde: string;
    fechaHasta: string | null;
    activo: boolean;
}
