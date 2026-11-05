import { type FormEvent, useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { faqsApi } from '../../api/faqsApi';
import { Alerta } from '../../componentes/Alerta';
import { Acordeon } from '../../componentes/Basicos';
import { CampoTexto } from '../../componentes/Campos';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import type { CategoriaFaqConPreguntasResponse } from '../../tipos/faqs';

function normalizar(texto: string): string {
    return texto
        .normalize('NFD')
        .replace(/[̀-ͯ]/g, '')
        .toLowerCase();
}

export function PreguntasFrecuentes() {
    const { t } = useTranslation();
    const { idiomaActual } = useLocalizacion();

    const [categorias, setCategorias] = useState<CategoriaFaqConPreguntasResponse[]>([]);
    const [termino, setTermino] = useState('');
    const [abiertas, setAbiertas] = useState<Set<number>>(() => new Set());
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    const mensajeDeError = useCallback(
        (excepcion: unknown) => (excepcion instanceof ErrorApi ? excepcion.message : t('ayuda.faq.error')),
        [t],
    );

    useEffect(() => {
        let vigente = true;

        faqsApi
            .listarPublicadas(idiomaActual)
            .then((lista) => {
                if (vigente) {
                    setCategorias(lista);
                    setError(null);
                }
            })
            .catch((excepcion) => {
                if (vigente) {
                    setError(mensajeDeError(excepcion));
                }
            })
            .finally(() => {
                if (vigente) {
                    setCargando(false);
                }
            });

        return () => {
            vigente = false;
        };
    }, [idiomaActual, mensajeDeError]);

    const busqueda = normalizar(termino.trim());

    const categoriasVisibles = useMemo(() => {
        if (busqueda === '') {
            return categorias;
        }

        return categorias
            .map((item) => ({
                ...item,
                preguntas: item.preguntas.filter((pregunta) =>
                    normalizar(
                        [item.categoria.titulo, pregunta.pregunta, pregunta.respuesta].join(' '),
                    ).includes(busqueda),
                ),
            }))
            .filter((item) => item.preguntas.length > 0);
    }, [busqueda, categorias]);

    const cantidadResultados = categoriasVisibles.reduce(
        (total, item) => total + item.preguntas.length,
        0,
    );

    function alternar(preguntaFrecuenteId: number) {
        setAbiertas((anteriores) => {
            const siguientes = new Set(anteriores);

            if (siguientes.has(preguntaFrecuenteId)) {
                siguientes.delete(preguntaFrecuenteId);
            } else {
                siguientes.add(preguntaFrecuenteId);
            }

            return siguientes;
        });
    }

    function limpiar() {
        setTermino('');
        setAbiertas(new Set());
    }

    return (
        <div className="mx-auto max-w-3xl px-4 py-12 sm:py-16">
            <h1 className="text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                {t('ayuda.faq.titulo')}
            </h1>
            <p className="mt-3 text-base text-texto-suave">{t('ayuda.faq.descripcion')}</p>

            <form
                onSubmit={(evento: FormEvent) => evento.preventDefault()}
                className="mt-8 rounded-2xl border border-borde bg-superficie p-6"
                role="search"
            >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
                    <div className="flex-1">
                        <CampoTexto
                            etiqueta={t('ayuda.faq.buscador.etiqueta')}
                            identificador="buscar-faq"
                            type="search"
                            placeholder={t('ayuda.faq.buscador.placeholder')}
                            value={termino}
                            onChange={(evento) => setTermino(evento.target.value)}
                        />
                    </div>

                    <button
                        type="button"
                        onClick={limpiar}
                        disabled={termino === ''}
                        className="rounded-lg border border-borde bg-superficie px-4 py-2.5 text-sm font-semibold text-texto hover:bg-fondo disabled:cursor-not-allowed disabled:opacity-60"
                    >
                        {t('comun.boton.limpiar')}
                    </button>
                </div>
            </form>

            {error !== null && (
                <div className="mt-6">
                    <Alerta tipo="error" mensaje={error} />
                </div>
            )}

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('ayuda.faq.cargando')}</p>}

            {!cargando && !error && categorias.length === 0 && (
                <p className="mt-6 text-sm text-texto-suave">{t('ayuda.faq.vacio')}</p>
            )}

            {busqueda !== '' && (
                <p className="mt-6 text-sm text-texto-suave" role="status">
                    {cantidadResultados === 0
                        ? t('ayuda.faq.sinCoincidencias')
                        : cantidadResultados === 1
                          ? t('ayuda.faq.resultadoUno')
                          : t('ayuda.faq.resultados', { cantidad: cantidadResultados })}
                </p>
            )}

            <div className="mt-10 space-y-10">
                {categoriasVisibles.map((item) => (
                    <section
                        key={item.categoria.categoriaFaqId}
                        aria-labelledby={`categoria-faq-${item.categoria.categoriaFaqId}`}
                    >
                        <h2
                            id={`categoria-faq-${item.categoria.categoriaFaqId}`}
                            className="text-xl font-semibold text-texto"
                        >
                            {item.categoria.titulo}
                        </h2>

                        <div className="mt-4 space-y-3">
                            {item.preguntas.map((pregunta) => (
                                <Acordeon
                                    key={pregunta.preguntaFrecuenteId}
                                    identificador={`pregunta-faq-${pregunta.preguntaFrecuenteId}`}
                                    titulo={pregunta.pregunta}
                                    abierto={abiertas.has(pregunta.preguntaFrecuenteId)}
                                    alAlternar={() => alternar(pregunta.preguntaFrecuenteId)}
                                >
                                    <p className="m-0 whitespace-pre-line">{pregunta.respuesta}</p>

                                    {pregunta.enlace !== null && (
                                        <Link
                                            to={pregunta.enlace}
                                            className="mt-3 inline-block text-sm font-semibold text-primario hover:underline"
                                        >
                                            {pregunta.enlaceEtiqueta}
                                        </Link>
                                    )}
                                </Acordeon>
                            ))}
                        </div>
                    </section>
                ))}
            </div>

            <div className="mt-12 rounded-2xl border border-borde bg-superficie p-6 sm:p-8">
                <h2 className="text-xl font-semibold text-texto">{t('ayuda.faq.contacto.titulo')}</h2>
                <p className="mt-2 text-sm text-texto-suave">{t('ayuda.faq.contacto.descripcion')}</p>
                <Link
                    to="/contacto"
                    className="mt-4 inline-block rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                >
                    {t('ayuda.faq.contacto.boton')}
                </Link>
            </div>
        </div>
    );
}
