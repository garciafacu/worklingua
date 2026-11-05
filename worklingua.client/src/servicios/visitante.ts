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
