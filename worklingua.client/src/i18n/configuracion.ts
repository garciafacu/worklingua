import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';
import { backendTraducciones } from './backendTraducciones';

/** Idioma base. Es al que cae la interfaz cuando falta una traducción. */
export const IDIOMA_BASE = 'es';

/** Dónde se recuerda el idioma elegido. No se guarda en `Usuario`. */
export const CLAVE_ALMACENAMIENTO_IDIOMA = 'worklingua.idioma';

/**
 * El idioma con el que arranca la aplicación.
 *
 * Prioridad: lo que la persona eligió antes, después lo que declara el
 * navegador, y por último el español.
 */
export function idiomaInicial(idiomasDisponibles: string[]): string {
    const candidatos: string[] = [];

    try {
        const guardado = localStorage.getItem(CLAVE_ALMACENAMIENTO_IDIOMA);

        if (guardado) {
            candidatos.push(guardado);
        }
    } catch {
        // localStorage puede fallar (ventana privada, permisos). Se sigue sin él.
    }

    if (typeof navigator !== 'undefined' && navigator.language) {
        // `es-AR` tiene que emparejar con el idioma `es`.
        candidatos.push(navigator.language.split('-')[0]);
    }

    candidatos.push(IDIOMA_BASE);

    const elegido = candidatos.find((codigo) => idiomasDisponibles.includes(codigo));

    return elegido ?? IDIOMA_BASE;
}

export function recordarIdioma(codigo: string): void {
    try {
        localStorage.setItem(CLAVE_ALMACENAMIENTO_IDIOMA, codigo);
    } catch {
        // Sin almacenamiento el idioma igual funciona: se pierde al recargar.
    }
}

let inicializado = false;

/**
 * Configura i18next una sola vez.
 *
 * Dos opciones deciden si todo esto funciona:
 *
 * - `keySeparator: false` — el bundle llega PLANO desde SQL
 *   (`{ "publico.inicio.titulo": "..." }`). Con el separador por defecto (`.`),
 *   i18next buscaría un objeto anidado y no encontraría ninguna clave.
 *
 * - `returnEmptyString: false` — una traducción vacía cae al `fallbackLng`.
 *   Es el mecanismo por el que una clave pendiente se muestra en español sin
 *   una sola línea de lógica en los componentes.
 */
export async function inicializarI18n(idioma: string): Promise<void> {
    if (inicializado) {
        await i18next.changeLanguage(idioma);

        return;
    }

    await i18next
        .use(backendTraducciones)
        .use(initReactI18next)
        .init({
            lng: idioma,
            fallbackLng: IDIOMA_BASE,
            supportedLngs: false,
            keySeparator: false,
            nsSeparator: false,
            returnEmptyString: false,
            interpolation: {
                // React ya escapa todo lo que renderiza.
                escapeValue: false,
            },
            react: {
                useSuspense: false,
            },
        });

    inicializado = true;
}

export { i18next };
