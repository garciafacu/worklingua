export const RUTA_CONSULTA = '/consulta';

export function rutaDeConsulta(token: string): string {
    return `${RUTA_CONSULTA}?token=${encodeURIComponent(token)}`;
}

export function urlDeConsulta(token: string): string {
    return `${window.location.origin}${rutaDeConsulta(token)}`;
}

const CLAVE_TOKEN_VISITANTE = 'worklingua.tokenVisitante';

export function obtenerTokenVisitante(): string {
    try {
        const guardado = localStorage.getItem(CLAVE_TOKEN_VISITANTE);

        if (guardado) {
            return guardado;
        }

        const nuevo = crypto.randomUUID();
        localStorage.setItem(CLAVE_TOKEN_VISITANTE, nuevo);

        return nuevo;
    } catch {
        return crypto.randomUUID();
    }
}
