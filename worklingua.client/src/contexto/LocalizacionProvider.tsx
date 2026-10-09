import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { culturasApi } from '../api/culturasApi';
import { idiomasApi } from '../api/idiomasApi';
import {
    IDIOMA_BASE,
    idiomaInicial,
    inicializarI18n,
    i18next,
    recordarIdioma,
} from '../i18n/configuracion';
import type { CulturaResponse } from '../tipos/culturas';
import type { IdiomaResponse } from '../tipos/idiomas';
import { LocalizacionContexto, type EstadoLocalizacion } from './localizacionContexto';

/**
 * Elige la cultura que corresponde a un idioma.
 *
 * Un idioma puede tener varias culturas (en-US, en-GB): gana la marcada como
 * predeterminada, y si ninguna lo está, la primera. Hoy hay una sola por
 * idioma, pero la regla ya está escrita para cuando haya más.
 */
function resolverCultura(culturas: CulturaResponse[], codigoIdioma: string): CulturaResponse | null {
    const candidatas = culturas.filter((cultura) => cultura.codigoIdioma === codigoIdioma);

    if (candidatas.length === 0) {
        return null;
    }

    return candidatas.find((cultura) => cultura.esPredeterminada) ?? candidatas[0];
}

interface EstadoCarga {
    idiomas: IdiomaResponse[];
    culturas: CulturaResponse[];
}

/**
 * Mantiene el idioma y la cultura activos y los comparte con toda la aplicación.
 *
 * El idioma se recuerda en `localStorage` y **no** se guarda en `Usuario`: es
 * una preferencia del navegador, no un dato de la persona. Un mismo usuario
 * puede tener el sitio en inglés en su máquina y en español en otra.
 */
export function LocalizacionProvider({ children }: { children: ReactNode }) {
    const [datos, setDatos] = useState<EstadoCarga>({ idiomas: [], culturas: [] });
    const [idiomaActual, setIdiomaActual] = useState(IDIOMA_BASE);
    const [cargando, setCargando] = useState(true);

    // Una sola vez al montar: el catálogo de idiomas, las culturas y el bundle
    // del idioma elegido. Hasta que los tres estén, no se renderiza nada, para
    // que la primera pintura no muestre las claves crudas.
    useEffect(() => {
        let vigente = true;

        async function cargar() {
            let idiomas: IdiomaResponse[] = [];
            let culturas: CulturaResponse[] = [];

            try {
                [idiomas, culturas] = await Promise.all([
                    idiomasApi.listar(),
                    culturasApi.listar(),
                ]);
            } catch {
                // Si la API no responde, la aplicación arranca igual en español
                // con las claves sin resolver. Es preferible a una pantalla en
                // blanco: el resto del sitio sigue siendo navegable.
            }

            const codigos = idiomas.map((idioma) => idioma.codigoISO);
            const elegido = idiomaInicial(codigos.length > 0 ? codigos : [IDIOMA_BASE]);

            try {
                await inicializarI18n(elegido);
            } catch {
                // Idem: sin bundle se ven las claves, pero la aplicación carga.
            }

            if (!vigente) {
                return;
            }

            setDatos({ idiomas, culturas });
            setIdiomaActual(elegido);
            setCargando(false);
        }

        void cargar();

        return () => {
            vigente = false;
        };
    }, []);

    const cambiarIdioma = useCallback(async (codigoISO: string) => {
        await i18next.changeLanguage(codigoISO);

        recordarIdioma(codigoISO);
        setIdiomaActual(codigoISO);
    }, []);

    const cultura = useMemo(
        () => resolverCultura(datos.culturas, idiomaActual),
        [datos.culturas, idiomaActual],
    );

    // El locale que se le pasa a Intl. Sin cultura cargada se usa el código de
    // idioma a secas, que Intl entiende igual ('es', 'en').
    const locale = cultura?.codigo ?? idiomaActual;

    const formatearMoneda = useCallback(
        (importe: number) => {
            // Los precios se guardan en la moneda base (ARS) en una sola
            // columna. La tasa la carga una persona desde el ABM de Culturas:
            // no hay cotización automática, así que puede estar desactualizada.
            const convertido = importe * (cultura?.tasaConversion ?? 1);
            const moneda = cultura?.moneda ?? 'ARS';

            // Sin decimales, un importe que convierte a menos de 0.5 en la
            // moneda de destino redondea a 0 y un plan pago se ve idéntico a
            // uno gratuito (p. ej. 500 ARS a 0.0008 son 0.4 USD). Se le dan dos
            // decimales solo a ese caso: el resto de los montos sigue
            // mostrándose igual que siempre, sin decimales.
            const seVeComoCero = convertido !== 0 && Math.round(convertido) === 0;
            const decimales = seVeComoCero ? 2 : 0;

            try {
                // Intl.NumberFormat con style: 'currency' se usa solo para
                // resolver, a partir de (locale, moneda), las reglas que SÍ
                // dependen de esos dos: dónde va el símbolo (antes o después
                // del número, con o sin espacio) y los separadores por
                // defecto de ese locale.
                //
                // El símbolo que Intl elige NO se usa tal cual: para 'USD' en
                // 'en-US' es '$', el mismo glyph que 'ARS' en 'es-AR', así que
                // dos monedas distintas se verían idénticas. formatToParts
                // separa el resultado en partes (moneda, miles, decimal,
                // dígitos) y se reemplaza cada una por el dato que carga el
                // ABM de Culturas (SimboloMoneda, SeparadorMiles,
                // SeparadorDecimal), manteniendo la posición que Intl calculó.
                const partes = new Intl.NumberFormat(locale, {
                    style: 'currency',
                    currency: moneda,
                    minimumFractionDigits: decimales,
                    maximumFractionDigits: decimales,
                }).formatToParts(convertido);

                return partes
                    .map((parte) => {
                        if (parte.type === 'currency') {
                            return cultura?.simboloMoneda ?? parte.value;
                        }

                        if (parte.type === 'group') {
                            return cultura?.separadorMiles ?? parte.value;
                        }

                        if (parte.type === 'decimal') {
                            return cultura?.separadorDecimal ?? parte.value;
                        }

                        return parte.value;
                    })
                    .join('');
            } catch {
                // Un código de moneda inválido cargado desde el ABM haría
                // explotar Intl. Se degrada al símbolo más el número.
                return `${cultura?.simboloMoneda ?? '$'} ${Math.round(convertido)}`;
            }
        },
        [cultura, locale],
    );

    const formatearFecha = useCallback(
        (valor: string | Date | null, conHora = false) => {
            if (valor === null || valor === undefined || valor === '') {
                return '—';
            }

            // Una fecha sola ("2026-09-10", como la que devuelve un DateOnly del
            // backend) la especifica ECMAScript como UTC, mientras que una con
            // hora se interpreta en la zona local. Sin el T00:00:00, en Argentina
            // (UTC-3) toda fecha sin hora se mostraría un día antes.
            const texto =
                typeof valor === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(valor)
                    ? `${valor}T00:00:00`
                    : valor;

            const fecha = texto instanceof Date ? texto : new Date(texto);

            if (Number.isNaN(fecha.getTime())) {
                return '—';
            }

            try {
                // Con hora se piden las partes explícitamente: con las opciones
                // por defecto, Chrome formatea es-AR en reloj de 12 horas sin
                // "a. m./p. m." y las 15:02 se leían como 03:02.
                return conHora
                    ? fecha.toLocaleString(locale, {
                          year: 'numeric',
                          month: 'numeric',
                          day: 'numeric',
                          hour: 'numeric',
                          minute: '2-digit',
                          second: '2-digit',
                      })
                    : fecha.toLocaleDateString(locale);
            } catch {
                return fecha.toISOString().slice(0, 10);
            }
        },
        [locale],
    );

    const formatearNumero = useCallback(
        (valor: number, decimales?: number) => {
            try {
                return new Intl.NumberFormat(
                    locale,
                    decimales === undefined
                        ? undefined
                        : { minimumFractionDigits: decimales, maximumFractionDigits: decimales },
                ).format(valor);
            } catch {
                return String(valor);
            }
        },
        [locale],
    );

    const valor = useMemo<EstadoLocalizacion>(
        () => ({
            idiomas: datos.idiomas,
            idiomaActual,
            cambiarIdioma,
            cultura,
            formatearMoneda,
            formatearFecha,
            formatearNumero,
        }),
        [
            datos.idiomas,
            idiomaActual,
            cambiarIdioma,
            cultura,
            formatearMoneda,
            formatearFecha,
            formatearNumero,
        ],
    );

    if (cargando) {
        return null;
    }

    return (
        <LocalizacionContexto.Provider value={valor}>{children}</LocalizacionContexto.Provider>
    );
}
