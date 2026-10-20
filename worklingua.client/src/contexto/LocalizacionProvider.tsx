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

export function LocalizacionProvider({ children }: { children: ReactNode }) {
    const [datos, setDatos] = useState<EstadoCarga>({ idiomas: [], culturas: [] });
    const [idiomaActual, setIdiomaActual] = useState(IDIOMA_BASE);
    const [cargando, setCargando] = useState(true);

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
            }

            const codigos = idiomas.map((idioma) => idioma.codigoISO);
            const elegido = idiomaInicial(codigos.length > 0 ? codigos : [IDIOMA_BASE]);

            try {
                await inicializarI18n(elegido);
            } catch {
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

    const locale = cultura?.codigo ?? idiomaActual;

    const formatearMoneda = useCallback(
        (importe: number) => {
            const convertido = importe * (cultura?.tasaConversion ?? 1);
            const moneda = cultura?.moneda ?? 'ARS';

            const seVeComoCero = convertido !== 0 && Math.round(convertido) === 0;
            const decimales = seVeComoCero ? 2 : 0;

            try {
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

            const texto =
                typeof valor === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(valor)
                    ? `${valor}T00:00:00`
                    : valor;

            const fecha = texto instanceof Date ? texto : new Date(texto);

            if (Number.isNaN(fecha.getTime())) {
                return '—';
            }

            try {
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
