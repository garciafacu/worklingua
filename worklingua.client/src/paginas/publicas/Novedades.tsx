import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../api/clienteHttp';
import { noticiasApi } from '../../api/noticiasApi';
import { Alerta } from '../../componentes/Alerta';
import { Paginador } from '../../componentes/Paginador';
import { TarjetaNoticia } from '../../componentes/TarjetaNoticia';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import type { NoticiaResponse } from '../../tipos/noticias';

const TAMANIOS_PAGINA = [6, 12, 24] as const;

interface Resultado {
    /** Idioma con el que se pidió: si no es el actual, la carga está en curso. */
    idioma: string;
    noticias: NoticiaResponse[];
    error: string | null;
}

/**
 * Página pública de Novedades.
 *
 * Muestra las noticias escritas en el idioma de la interfaz y se vuelve a pedir
 * al cambiarlo. Si ese idioma no tiene ninguna, la API devuelve las del español
 * y la pantalla lo avisa. La paginación es local: el volumen de noticias no
 * justifica paginar en el servidor.
 */
export function Novedades() {
    const { t } = useTranslation();
    const { idiomaActual, idiomas } = useLocalizacion();

    const [resultado, setResultado] = useState<Resultado | null>(null);
    const [pagina, setPagina] = useState(1);
    const [tamanioPagina, setTamanioPagina] = useState<number>(TAMANIOS_PAGINA[0]);

    useEffect(() => {
        let vigente = true;

        noticiasApi
            .listarPublicadas(idiomaActual)
            .then((noticias) => {
                if (vigente) {
                    setResultado({ idioma: idiomaActual, noticias, error: null });
                }
            })
            .catch((excepcion: unknown) => {
                if (vigente) {
                    setResultado({
                        idioma: idiomaActual,
                        noticias: [],
                        error:
                            excepcion instanceof ErrorApi
                                ? excepcion.message
                                : t('publico.novedades.errorCargar'),
                    });
                }
            });

        return () => {
            vigente = false;
        };
    }, [idiomaActual, t]);

    const cargando = resultado === null || resultado.idioma !== idiomaActual;
    const noticias = cargando ? [] : resultado.noticias;
    const idiomaIdActual = idiomas.find((idioma) => idioma.codigoISO === idiomaActual)?.idiomaId;
    const enIdiomaBase =
        noticias.length > 0 && idiomaIdActual !== undefined && noticias[0].idiomaId !== idiomaIdActual;

    const totalPaginas = Math.max(1, Math.ceil(noticias.length / tamanioPagina));
    const paginaVisible = Math.min(pagina, totalPaginas);
    const visibles = noticias.slice((paginaVisible - 1) * tamanioPagina, paginaVisible * tamanioPagina);

    return (
        <div className="mx-auto max-w-6xl px-4 py-12 sm:py-16">
            <h1 className="text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                {t('publico.novedades.titulo')}
            </h1>
            <p className="mt-3 max-w-2xl text-base text-texto-suave">
                {t('publico.novedades.descripcion')}
            </p>

            <div className="mt-8">
                {cargando ? (
                    <p className="text-sm text-texto-suave">{t('publico.novedades.cargando')}</p>
                ) : (
                    <>
                        <Alerta tipo="error" mensaje={resultado.error} />
                        <Alerta
                            tipo="info"
                            mensaje={enIdiomaBase ? t('publico.novedades.enIdiomaBase') : null}
                        />

                        {!resultado.error && noticias.length === 0 && (
                            <p className="text-sm text-texto-suave">{t('publico.novedades.vacio')}</p>
                        )}

                        <div className="mt-4 grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                            {visibles.map((noticia) => (
                                <TarjetaNoticia key={noticia.noticiaId} noticia={noticia} />
                            ))}
                        </div>

                        <Paginador
                            pagina={paginaVisible}
                            totalPaginas={totalPaginas}
                            totalRegistros={noticias.length}
                            tamanioPagina={tamanioPagina}
                            tamaniosDisponibles={TAMANIOS_PAGINA}
                            alCambiarPagina={setPagina}
                            alCambiarTamanio={(tamanio) => {
                                setTamanioPagina(tamanio);
                                setPagina(1);
                            }}
                        />
                    </>
                )}
            </div>
        </div>
    );
}
