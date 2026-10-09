/** Oferta personal para una empresa, espejo de `BE/BEOfertaConDetalle.cs`. */
export interface OfertaResponse {
    oferta: {
        ofertaId: number;
        empresaId: number;
        /** Plan que la oferta sugiere contratar, si lo hay. */
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

/** Cuerpo del alta y de la modificación, espejo de `BE/BEGuardarOferta.cs`. */
export interface GuardarOfertaRequest {
    empresaId: number;
    planId: number | null;
    titulo: string;
    descripcion: string;
    /** `yyyy-MM-dd`. */
    fechaDesde: string;
    fechaHasta: string | null;
    activo: boolean;
}
