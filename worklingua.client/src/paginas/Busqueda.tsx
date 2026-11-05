import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { busquedaApi } from '../api/busquedaApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { BuscadorPlataforma } from '../componentes/BuscadorPlataforma';
import { useLocalizacion } from '../contexto/useLocalizacion';
import { useSesion } from '../contexto/useSesion';
import {
    AREAS_BUSQUEDA,
    ORDENES_BUSQUEDA,
    type CriteriosBusqueda,
    type PaginaBuscableResponse,
    type SeccionBusquedaResponse,
} from '../tipos/busqueda';

interface Resultado {
    clave: string;
    paginas: PaginaBuscableResponse[];
    error: string | null;
}

function leerCriterios(parametros: URLSearchParams): CriteriosBusqueda {
    const area = AREAS_BUSQUEDA.find((opcion) => opcion.valor === parametros.get('area'));
    const ordenarPor = ORDENES_BUSQUEDA.find((opcion) => opcion.valor === parametros.get('ordenarPor'));

    return {
        texto: parametros.get('texto')?.trim() || undefined,
        area: area?.valor,
        seccion: parametros.get('seccion') || undefined,
        ordenarPor: ordenarPor?.valor,
        soloTitulo: parametros.get('soloTitulo') === 'true' || undefined,
    };
}

function aParametros(criterios: CriteriosBusqueda): URLSearchParams {
    const parametros = new URLSearchParams();

    Object.entries(criterios).forEach(([campo, valor]) => {
        if (valor !== undefined) {
            parametros.set(campo, String(valor));
        }
    });

    return parametros;
}

interface BusquedaProps {
    publica?: boolean;
}

export function Busqueda({ publica = false }: BusquedaProps) {
    const { t } = useTranslation();
    const { idiomaActual } = useLocalizacion();
    const { autenticado, refrescarSesion } = useSesion();
    const [parametros, setParametros] = useSearchParams();

    const consulta = parametros.toString();
    const criterios = leerCriterios(parametros);
    const hayCriterios = Boolean(criterios.texto || criterios.area || criterios.seccion);
    const claveActual = `${consulta}|${idiomaActual}|${autenticado}`;

    const [resultado, setResultado] = useState<Resultado | null>(null);
    const [secciones, setSecciones] = useState<SeccionBusquedaResponse[]>([]);

    useEffect(() => {
        let vigente = true;

        busquedaApi
            .listarSecciones()
            .then((recibidas) => {
                if (vigente) {
                    setSecciones(recibidas);
                }
            })
            .catch(() => {
                if (vigente) {
                    setSecciones([]);
                }
            });

        return () => {
            vigente = false;
        };
    }, [autenticado]);

    useEffect(() => {
        if (!hayCriterios) {
            return;
        }

        let vigente = true;

        busquedaApi
            .buscar(leerCriterios(new URLSearchParams(consulta)), idiomaActual)
            .then((paginas) => {
                if (vigente) {
                    setResultado({ clave: claveActual, paginas, error: null });
                }
            })
            .catch((excepcion: unknown) => {
                if (!vigente) {
                    return;
                }

                if (excepcion instanceof ErrorApi && excepcion.estado === 401) {
                    void refrescarSesion();
                }

                setResultado({
                    clave: claveActual,
                    paginas: [],
                    error: excepcion instanceof ErrorApi ? excepcion.message : t('busqueda.errorBuscar'),
                });
            });

        return () => {
            vigente = false;
        };
    }, [claveActual, hayCriterios]);

    const vigente = hayCriterios && resultado?.clave === claveActual ? resultado : null;
    const buscando = hayCriterios && vigente === null;

    function buscar(nuevos: CriteriosBusqueda) {
        setParametros(aParametros(nuevos));
    }

    return (
        <div className={publica ? 'mx-auto max-w-4xl px-4 py-12 sm:py-16' : 'mx-auto max-w-4xl'}>
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('busqueda.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('busqueda.descripcion')}</p>

            <section className="mt-6">
                <BuscadorPlataforma
                    key={consulta}
                    criteriosIniciales={criterios}
                    secciones={secciones}
                    buscando={buscando}
                    alBuscar={buscar}
                />
            </section>

            <section className="mt-6" aria-live="polite">
                {!hayCriterios && <p className="text-sm text-texto-suave">{t('busqueda.inicial')}</p>}

                {buscando && <p className="text-sm text-texto-suave">{t('comun.boton.buscando')}</p>}

                {vigente && <Alerta tipo="error" mensaje={vigente.error} />}

                {vigente && !vigente.error && (
                    <>
                        <p className="text-sm text-texto-suave" role="status">
                            {vigente.paginas.length === 0
                                ? t('busqueda.sinCoincidencias')
                                : vigente.paginas.length === 1
                                  ? t('busqueda.resultadoUno')
                                  : t('busqueda.resultados', { cantidad: vigente.paginas.length })}
                        </p>

                        <ul className="mt-4 flex list-none flex-col gap-3 p-0">
                            {vigente.paginas.map((pagina) => (
                                <li key={pagina.ruta}>
                                    <Link
                                        to={pagina.ruta}
                                        className="block rounded-2xl border border-borde bg-superficie p-5 no-underline transition-colors hover:border-primario hover:bg-fondo"
                                    >
                                        <span className="inline-block rounded-md bg-info-fondo px-2 py-0.5 text-xs font-semibold text-primario">
                                            {t(`busqueda.area.${pagina.area}`)} › {t(pagina.claveSeccion)}
                                        </span>

                                        <span className="mt-2 block text-lg font-semibold text-texto">
                                            {t(pagina.claveTitulo)}
                                        </span>

                                        <span className="mt-1 block text-sm text-texto-suave">
                                            {t(pagina.claveDescripcion)}
                                        </span>

                                        <span className="mt-2 block font-mono text-xs text-texto-suave">
                                            {pagina.ruta}
                                        </span>
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </>
                )}
            </section>
        </div>
    );
}
