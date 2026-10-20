const HEADER_TOKEN_SESION = 'X-Sesion-Token';

export class ErrorApi extends Error {
    readonly estado: number;
    readonly sugerencia: string | null;

    constructor(estado: number, mensaje: string, sugerencia: string | null = null) {
        super(mensaje);
        this.name = 'ErrorApi';
        this.estado = estado;
        this.sugerencia = sugerencia;
    }
}

interface ProblemDetails {
    title?: string;
    detail?: string;
    errors?: Record<string, string[]>;
    sugerencia?: string;
}

let tokenSesion: string | null = null;

export function establecerTokenSesion(token: string | null): void {
    tokenSesion = token;
}

async function construirError(respuesta: Response): Promise<ErrorApi> {
    try {
        const problema = (await respuesta.json()) as ProblemDetails;

        if (problema.errors) {
            const primero = Object.values(problema.errors).flat()[0];
            if (primero) {
                return new ErrorApi(respuesta.status, primero);
            }
        }

        return new ErrorApi(
            respuesta.status,
            problema.detail ?? problema.title ?? 'Ocurrió un error inesperado.',
            problema.sugerencia ?? null,
        );
    } catch {
        return new ErrorApi(respuesta.status, 'No se pudo contactar al servidor. Intentá nuevamente.');
    }
}

async function pedir<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
    const esFormulario = opciones.body instanceof FormData;

    const cabeceras: Record<string, string> = {
        ...(esFormulario ? {} : { 'Content-Type': 'application/json' }),
        ...((opciones.headers as Record<string, string>) ?? {}),
    };

    if (tokenSesion) {
        cabeceras[HEADER_TOKEN_SESION] = tokenSesion;
    }

    const respuesta = await fetch(`/api${ruta}`, { ...opciones, headers: cabeceras });

    if (!respuesta.ok) {
        throw await construirError(respuesta);
    }

    if (respuesta.status === 204) {
        return undefined as T;
    }

    return (await respuesta.json()) as T;
}

export const clienteHttp = {
    get: <T>(ruta: string) => pedir<T>(ruta),

    post: <T>(ruta: string, cuerpo?: unknown) =>
        pedir<T>(ruta, {
            method: 'POST',
            body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
        }),

    put: <T>(ruta: string, cuerpo?: unknown) =>
        pedir<T>(ruta, {
            method: 'PUT',
            body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
        }),

    patch: <T>(ruta: string, cuerpo?: unknown) =>
        pedir<T>(ruta, {
            method: 'PATCH',
            body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
        }),

    subir: <T>(ruta: string, formulario: FormData) =>
        pedir<T>(ruta, { method: 'POST', body: formulario }),

    borrar: <T>(ruta: string) => pedir<T>(ruta, { method: 'DELETE' }),
};
