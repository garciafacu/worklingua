const HEADER_TOKEN_SESION = 'X-Sesion-Token';

/**
 * Error de una llamada a la API, con el código HTTP y el mensaje que devolvió
 * el backend. Las pantallas lo muestran tal cual: los textos de las reglas de
 * negocio viven en la BLL y no se duplican acá.
 */
export class ErrorApi extends Error {
    readonly estado: number;
    /**
     * Valor que el backend propone para resolver el conflicto, cuando hay uno.
     * Lo usa el nombre duplicado de un activo pedagógico: la pantalla ofrece la
     * nomenclatura sugerida en vez de pedir que se invente otra.
     */
    readonly sugerencia: string | null;

    constructor(estado: number, mensaje: string, sugerencia: string | null = null) {
        super(mensaje);
        this.name = 'ErrorApi';
        this.estado = estado;
        this.sugerencia = sugerencia;
    }
}

/** ProblemDetails de ASP.NET Core, más el diccionario de errores de validación. */
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

        // Los errores de ModelState llegan agrupados por campo; se muestra el
        // primero para no abrumar con una lista.
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
        // Con FormData el Content-Type lo pone el navegador: lleva el separador
        // (boundary) que se calcula al armar el cuerpo, y fijarlo a mano lo
        // rompe.
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

    /**
     * POST de un formulario con archivos (multipart). Lo usa la subida de
     * activos pedagógicos, que es lo único que no viaja como JSON.
     */
    subir: <T>(ruta: string, formulario: FormData) =>
        pedir<T>(ruta, { method: 'POST', body: formulario }),

    // `delete` es palabra reservada: el método se llama `borrar`.
    borrar: <T>(ruta: string) => pedir<T>(ruta, { method: 'DELETE' }),
};
