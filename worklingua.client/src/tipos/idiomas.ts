/**
 * Idioma de la PLATAFORMA, espejo de `Contratos/IdiomaResponse.cs`.
 *
 * Este catálogo representa únicamente los idiomas en los que se puede mostrar
 * WorkLingua. El idioma que enseña un curso es `curso.idioma`, texto libre y
 * sin relación con esta tabla. Ver `docs/modelo-datos.md` (Idioma).
 */
export interface IdiomaResponse {
    idiomaId: number;
    nombre: string;
    /** Código ISO. Es la clave con la que se pide el bundle de traducciones. */
    codigoISO: string;
}

/** Idioma visto desde el Backoffice, espejo de `Contratos/IdiomaAdminResponse.cs`. */
export interface IdiomaAdminResponse extends IdiomaResponse {
    activo: boolean;
}

/** Cuerpo del alta y de la modificación, espejo de `GuardarIdiomaRequest.cs`. */
export interface GuardarIdiomaRequest {
    nombre: string;
    codigoISO: string;
}
