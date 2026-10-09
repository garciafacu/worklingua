import { useMemo, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Acordeon } from '../../componentes/Acordeon';
import { CampoTexto } from '../../componentes/CampoTexto';

interface PreguntaFrecuente {
    /** Base de las claves `<clave>.pregunta` y `<clave>.respuesta`. */
    clave: string;
    /** Enlace interno que se muestra debajo de la respuesta. */
    enlace?: { ruta: string; etiqueta: string };
}

interface CategoriaFaq {
    /** Base de la clave `<clave>.titulo`. */
    clave: string;
    preguntas: PreguntaFrecuente[];
}

/**
 * Las preguntas y sus categorías, como claves de traducción.
 *
 * El bundle de traducciones es plano (`keySeparator: false`), así que la lista
 * no puede venir de `t()`: vive acá, y el texto de cada pregunta se edita desde
 * el ABM de Traducciones como el resto del sitio.
 */
const CATEGORIAS: CategoriaFaq[] = [
    {
        clave: 'ayuda.faq.general',
        preguntas: [
            { clave: 'ayuda.faq.general.queEs' },
            { clave: 'ayuda.faq.general.sectores' },
            { clave: 'ayuda.faq.general.arIa' },
        ],
    },
    {
        clave: 'ayuda.faq.cuenta',
        preguntas: [
            {
                clave: 'ayuda.faq.cuenta.registro',
                enlace: { ruta: '/registro', etiqueta: 'ayuda.faq.enlace.registro' },
            },
            { clave: 'ayuda.faq.cuenta.equipo' },
            {
                clave: 'ayuda.faq.cuenta.clave',
                enlace: { ruta: '/recuperar-clave', etiqueta: 'ayuda.faq.enlace.recuperarClave' },
            },
        ],
    },
    {
        clave: 'ayuda.faq.planes',
        preguntas: [
            { clave: 'ayuda.faq.planes.sinCargo' },
            {
                clave: 'ayuda.faq.planes.precios',
                enlace: { ruta: '/catalogo', etiqueta: 'ayuda.faq.enlace.planes' },
            },
            { clave: 'ayuda.faq.planes.cambiar' },
            {
                clave: 'ayuda.faq.planes.licencias',
                enlace: { ruta: '/terminos', etiqueta: 'ayuda.faq.enlace.terminos' },
            },
        ],
    },
    {
        clave: 'ayuda.faq.cursos',
        preguntas: [
            {
                clave: 'ayuda.faq.cursos.idiomasNiveles',
                enlace: { ruta: '/catalogo', etiqueta: 'ayuda.faq.enlace.catalogo' },
            },
            { clave: 'ayuda.faq.cursos.progreso' },
        ],
    },
    {
        clave: 'ayuda.faq.privacidad',
        preguntas: [
            {
                clave: 'ayuda.faq.privacidad.datos',
                enlace: { ruta: '/privacidad', etiqueta: 'ayuda.faq.enlace.privacidad' },
            },
            { clave: 'ayuda.faq.privacidad.derechos' },
            { clave: 'ayuda.faq.privacidad.newsletter' },
        ],
    },
];

/** Minúsculas y sin tildes: "contraseña" y "CONTRASENA" tienen que coincidir. */
function normalizar(texto: string): string {
    return texto
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase();
}

/** Una clave de traducción como `id` de HTML: los puntos confunden a los selectores CSS. */
function idHtml(clave: string): string {
    return clave.replace(/\./g, '-');
}

/**
 * Servicio de Ayuda: preguntas frecuentes agrupadas por categoría, con búsqueda
 * por texto. El filtro corre sobre los textos ya traducidos, así que busca en
 * el idioma que se está viendo. Es contenido fijo: no hay llamadas a la API.
 */
export function PreguntasFrecuentes() {
    const { t } = useTranslation();

    const [termino, setTermino] = useState('');
    const [abiertas, setAbiertas] = useState<Set<string>>(() => new Set());

    const busqueda = normalizar(termino.trim());

    const categoriasVisibles = useMemo(() => {
        if (busqueda === '') {
            return CATEGORIAS;
        }

        return CATEGORIAS.map((categoria) => {
            const tituloCategoria = t(`${categoria.clave}.titulo`);

            return {
                ...categoria,
                preguntas: categoria.preguntas.filter((pregunta) =>
                    normalizar(
                        [
                            tituloCategoria,
                            t(`${pregunta.clave}.pregunta`),
                            t(`${pregunta.clave}.respuesta`),
                        ].join(' '),
                    ).includes(busqueda),
                ),
            };
        }).filter((categoria) => categoria.preguntas.length > 0);
    }, [busqueda, t]);

    const cantidadResultados = categoriasVisibles.reduce(
        (total, categoria) => total + categoria.preguntas.length,
        0,
    );

    function alternar(clave: string) {
        setAbiertas((anteriores) => {
            const siguientes = new Set(anteriores);

            if (siguientes.has(clave)) {
                siguientes.delete(clave);
            } else {
                siguientes.add(clave);
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
                {categoriasVisibles.map((categoria) => (
                    <section key={categoria.clave} aria-labelledby={idHtml(`${categoria.clave}.titulo`)}>
                        <h2
                            id={idHtml(`${categoria.clave}.titulo`)}
                            className="text-xl font-semibold text-texto"
                        >
                            {t(`${categoria.clave}.titulo`)}
                        </h2>

                        <div className="mt-4 space-y-3">
                            {categoria.preguntas.map((pregunta) => (
                                <Acordeon
                                    key={pregunta.clave}
                                    identificador={idHtml(pregunta.clave)}
                                    titulo={t(`${pregunta.clave}.pregunta`)}
                                    abierto={abiertas.has(pregunta.clave)}
                                    alAlternar={() => alternar(pregunta.clave)}
                                >
                                    <p className="m-0">{t(`${pregunta.clave}.respuesta`)}</p>

                                    {pregunta.enlace && (
                                        <Link
                                            to={pregunta.enlace.ruta}
                                            className="mt-3 inline-block text-sm font-semibold text-primario hover:underline"
                                        >
                                            {t(pregunta.enlace.etiqueta)}
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
