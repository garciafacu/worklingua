import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { RECAPTCHA_SITE_KEY } from '../configuracion/recaptcha';

interface OpcionesWidget {
    sitekey: string;
    size: 'normal' | 'compact';
    hl: string;
    callback: (token: string) => void;
    'expired-callback': () => void;
    'error-callback': () => void;
}

interface ApiRecaptcha {
    render: (contenedor: HTMLElement, opciones: OpcionesWidget) => number;
    reset: (identificador: number) => void;
    ready: (alEstarLista: () => void) => void;
}

declare global {
    interface Window {
        grecaptcha?: ApiRecaptcha;
    }
}

const URL_API = 'https://www.google.com/recaptcha/api.js?render=explicit';
const ANCHO_COMPACTO = 480;

let promesaApi: Promise<ApiRecaptcha> | null = null;

function cargarApi(): Promise<ApiRecaptcha> {
    if (promesaApi) {
        return promesaApi;
    }

    promesaApi = new Promise<ApiRecaptcha>((resolver, rechazar) => {
        if (window.grecaptcha?.render) {
            resolver(window.grecaptcha);

            return;
        }

        const etiqueta = document.createElement('script');
        etiqueta.src = URL_API;
        etiqueta.async = true;
        etiqueta.defer = true;

        etiqueta.onload = () => {
            const api = window.grecaptcha;

            if (api) {
                api.ready(() => resolver(api));
            } else {
                rechazar(new Error('grecaptcha no quedó disponible'));
            }
        };

        etiqueta.onerror = () => {
            // Se limpia para que un reintento posterior vuelva a pedir el script.
            promesaApi = null;
            rechazar(new Error('no se pudo descargar el script de reCAPTCHA'));
        };

        document.head.appendChild(etiqueta);
    });

    return promesaApi;
}

interface CampoCaptchaProps {
    onCambio: (token: string) => void;
    error?: string;
    reiniciar?: number;
}

export function CampoCaptcha({ onCambio, error, reiniciar = 0 }: CampoCaptchaProps) {
    const { t, i18n } = useTranslation();

    const contenedor = useRef<HTMLDivElement>(null);
    const identificador = useRef<number | null>(null);
    const alCambiar = useRef(onCambio);
    const [falloCarga, setFalloCarga] = useState(false);

    useEffect(() => {
        alCambiar.current = onCambio;
    }, [onCambio]);

    useEffect(() => {
        let cancelado = false;

        cargarApi()
            .then((api) => {
                if (cancelado || !contenedor.current || identificador.current !== null) {
                    return;
                }

                identificador.current = api.render(contenedor.current, {
                    sitekey: RECAPTCHA_SITE_KEY,
                    size: window.innerWidth <= ANCHO_COMPACTO ? 'compact' : 'normal',
                    hl: i18n.language,
                    callback: (token: string) => alCambiar.current(token),
                    'expired-callback': () => alCambiar.current(''),
                    'error-callback': () => alCambiar.current(''),
                });
            })
            .catch(() => {
                if (!cancelado) {
                    setFalloCarga(true);
                }
            });

        return () => {
            cancelado = true;
        };
    }, [i18n.language]);

    useEffect(() => {
        if (reiniciar === 0 || identificador.current === null) {
            return;
        }

        window.grecaptcha?.reset(identificador.current);
        alCambiar.current('');
    }, [reiniciar]);

    const mensaje = falloCarga ? t('auth.registro.captchaNoDisponible') : error;

    return (
        <div className="campo">
            <div ref={contenedor} />
            {mensaje && (
                <span id="captcha-error" className="campo-error">
                    {mensaje}
                </span>
            )}
        </div>
    );
}
