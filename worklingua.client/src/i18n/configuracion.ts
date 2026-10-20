import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';
import { backendTraducciones } from './backendTraducciones';

export const IDIOMA_BASE = 'es';

export const CLAVE_ALMACENAMIENTO_IDIOMA = 'worklingua.idioma';

export function idiomaInicial(idiomasDisponibles: string[]): string {
    const candidatos: string[] = [];

    try {
        const guardado = localStorage.getItem(CLAVE_ALMACENAMIENTO_IDIOMA);

        if (guardado) {
            candidatos.push(guardado);
        }
    } catch {
    }

    if (typeof navigator !== 'undefined' && navigator.language) {
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
    }
}

let inicializado = false;

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
                escapeValue: false,
            },
            react: {
                useSuspense: false,
            },
        });

    inicializado = true;
}

export { i18next };
