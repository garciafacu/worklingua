import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { newsletterApi } from '../../../api/newsletterApi';
import { noticiasApi } from '../../../api/noticiasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type {
    EnviarNewsletterRequest,
    EnvioNewsletterResponse,
    SuscriptorNewsletterResponse,
    VistaPreviaNewsletterResponse,
} from '../../../tipos/newsletter';
import type { NoticiaAdminResponse } from '../../../tipos/noticias';

const CLASE_BOTON_SECUNDARIO =
    'rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo disabled:cursor-not-allowed disabled:opacity-60';

interface NoticiasDelIdioma {
    idiomaId: string;
    noticias: NoticiaAdminResponse[];
}

function estadoSuscriptor(suscriptor: SuscriptorNewsletterResponse): string {
    if (!suscriptor.activo) {
        return 'baja';
    }

    return suscriptor.confirmado ? 'confirmado' : 'pendiente';
}

/**
 * Backoffice del newsletter: nuevo envío, suscriptores e historial.
 *
 * Un envío es de UN idioma: se eligen noticias publicadas en ese idioma y llega
 * a los suscriptores confirmados que se anotaron navegando en él. Los textos
 * fijos del correo salen de las traducciones `email.newsletter.*`.
 */
export function AdminNewsletter() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { idiomas, idiomaActual, formatearFecha, formatearNumero } = useLocalizacion();

    const puedeEnviar = tienePermiso(PERMISOS.newsletterEnviar);

    const [suscriptores, setSuscriptores] = useState<SuscriptorNewsletterResponse[]>([]);
    const [envios, setEnvios] = useState<EnvioNewsletterResponse[]>([]);
    const [cargandoSuscriptores, setCargandoSuscriptores] = useState(true);
    const [cargandoEnvios, setCargandoEnvios] = useState(true);
    const [recarga, setRecarga] = useState(0);

    const [idiomaId, setIdiomaId] = useState(
        () => String(idiomas.find((idioma) => idioma.codigoISO === idiomaActual)?.idiomaId ?? ''),
    );
    const [noticiasDelIdioma, setNoticiasDelIdioma] = useState<NoticiasDelIdioma | null>(null);
    const [asunto, setAsunto] = useState('');
    const [seleccionadas, setSeleccionadas] = useState<number[]>([]);
    const [vistaPrevia, setVistaPrevia] = useState<VistaPreviaNewsletterResponse | null>(null);
    const [confirmandoEnvio, setConfirmandoEnvio] = useState(false);
    const [previsualizando, setPrevisualizando] = useState(false);
    const [enviando, setEnviando] = useState(false);

    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        newsletterApi
            .listarSuscriptores()
            .then(setSuscriptores)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.newsletter.suscriptores.errorCargar')),
            )
            .finally(() => setCargandoSuscriptores(false));

        newsletterApi
            .listarEnvios()
            .then(setEnvios)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.newsletter.historial.errorCargar')),
            )
            .finally(() => setCargandoEnvios(false));
    }, [recarga, mensajeDeError]);

    useEffect(() => {
        if (!puedeEnviar || !idiomaId) {
            return;
        }

        let vigente = true;

        noticiasApi
            .listarAdministracion(Number(idiomaId))
            .then((noticias) => {
                if (!vigente) {
                    return;
                }

                const ahora = new Date();

                setNoticiasDelIdioma({
                    idiomaId,
                    noticias: noticias.filter(
                        (noticia) => noticia.activo && new Date(noticia.fechaPublicacion) <= ahora,
                    ),
                });
            })
            .catch((excepcion) => {
                if (vigente) {
                    setNoticiasDelIdioma({ idiomaId, noticias: [] });
                    setError(mensajeDeError(excepcion, 'admin.noticias.errorCargar'));
                }
            });

        return () => {
            vigente = false;
        };
    }, [idiomaId, puedeEnviar, mensajeDeError]);

    function nombreIdioma(id: number): string {
        return idiomas.find((idioma) => idioma.idiomaId === id)?.nombre ?? `#${id}`;
    }

    function cambiarIdioma(valor: string) {
        setIdiomaId(valor);
        setSeleccionadas([]);
        setVistaPrevia(null);
        setConfirmandoEnvio(false);
    }

    function alternarNoticia(noticiaId: number) {
        setVistaPrevia(null);
        setConfirmandoEnvio(false);
        setSeleccionadas((actuales) =>
            actuales.includes(noticiaId)
                ? actuales.filter((id) => id !== noticiaId)
                : [...actuales, noticiaId],
        );
    }

    function cuerpoEnvio(): EnviarNewsletterRequest {
        return { idiomaId: Number(idiomaId), asunto: asunto.trim(), noticiaIds: seleccionadas };
    }

    async function previsualizar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setConfirmandoEnvio(false);
        setPrevisualizando(true);

        try {
            setVistaPrevia(await newsletterApi.previsualizar(cuerpoEnvio()));
        } catch (excepcion) {
            setVistaPrevia(null);
            setError(mensajeDeError(excepcion, 'admin.newsletter.envio.errorPrevisualizar'));
        } finally {
            setPrevisualizando(false);
        }
    }

    async function enviar() {
        setError(null);
        setExito(null);
        setConfirmandoEnvio(false);
        setEnviando(true);

        try {
            const envio = await newsletterApi.enviar(cuerpoEnvio());

            setExito(
                t('admin.newsletter.envio.exito', {
                    enviados: envio.cantidadEnviados,
                    fallidos: envio.cantidadFallidos,
                }),
            );
            setAsunto('');
            setSeleccionadas([]);
            setVistaPrevia(null);
            setCargandoEnvios(true);
            setRecarga((numero) => numero + 1);
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.newsletter.envio.errorEnviar'));
        } finally {
            setEnviando(false);
        }
    }

    const noticiasCargadas = noticiasDelIdioma !== null && noticiasDelIdioma.idiomaId === idiomaId;
    const noticiasDisponibles = noticiasCargadas ? noticiasDelIdioma.noticias : [];
    const puedePrevisualizar = Boolean(idiomaId) && asunto.trim() !== '' && seleccionadas.length > 0;
    const confirmados = suscriptores.filter((suscriptor) => suscriptor.activo && suscriptor.confirmado);

    const columnasSuscriptores: ColumnaAbm<SuscriptorNewsletterResponse>[] = [
        { encabezado: t('admin.newsletter.tabla.email'), celda: (suscriptor) => suscriptor.email },
        { encabezado: t('comun.campo.idioma'), celda: (suscriptor) => nombreIdioma(suscriptor.idiomaId) },
        {
            encabezado: t('admin.newsletter.tabla.estado'),
            celda: (suscriptor) => t(`admin.newsletter.estado.${estadoSuscriptor(suscriptor)}`),
        },
        {
            encabezado: t('admin.newsletter.tabla.solicitud'),
            celda: (suscriptor) => formatearFecha(suscriptor.fechaAlta, true),
        },
    ];

    const columnasEnvios: ColumnaAbm<EnvioNewsletterResponse>[] = [
        {
            encabezado: t('admin.newsletter.historial.tabla.fecha'),
            celda: (envio) => formatearFecha(envio.fechaEnvio, true),
        },
        { encabezado: t('comun.campo.idioma'), celda: (envio) => nombreIdioma(envio.idiomaId) },
        { encabezado: t('admin.newsletter.historial.tabla.asunto'), celda: (envio) => envio.asunto },
        {
            encabezado: t('admin.newsletter.historial.tabla.enviados'),
            numerica: true,
            celda: (envio) => formatearNumero(envio.cantidadEnviados),
        },
        {
            encabezado: t('admin.newsletter.historial.tabla.fallidos'),
            numerica: true,
            celda: (envio) => formatearNumero(envio.cantidadFallidos),
        },
    ];

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.newsletter.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.newsletter.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeEnviar && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">{t('admin.newsletter.envio.titulo')}</h2>

                    <form onSubmit={previsualizar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoSelect
                                etiqueta={t('admin.newsletter.envio.idioma')}
                                identificador="idiomaEnvio"
                                required
                                value={idiomaId}
                                onChange={(evento) => cambiarIdioma(evento.target.value)}
                            >
                                <option value="" disabled>
                                    —
                                </option>
                                {idiomas.map((idioma) => (
                                    <option key={idioma.idiomaId} value={idioma.idiomaId}>
                                        {idioma.nombre}
                                    </option>
                                ))}
                            </CampoSelect>

                            <CampoTexto
                                etiqueta={t('admin.newsletter.envio.asunto')}
                                identificador="asunto"
                                required
                                maxLength={200}
                                value={asunto}
                                onChange={(evento) => {
                                    setAsunto(evento.target.value);
                                    setConfirmandoEnvio(false);
                                }}
                            />
                        </div>

                        <fieldset className="m-0 flex flex-col gap-2 border-0 p-0">
                            <legend className="mb-2 text-sm font-semibold text-texto">
                                {t('admin.newsletter.envio.noticias')}
                            </legend>

                            {idiomaId && !noticiasCargadas && (
                                <p className="text-sm text-texto-suave">{t('admin.noticias.cargando')}</p>
                            )}

                            {noticiasCargadas && noticiasDisponibles.length === 0 && (
                                <p className="text-sm text-texto-suave">
                                    {t('admin.newsletter.envio.sinNoticias')}
                                </p>
                            )}

                            {noticiasDisponibles.map((noticia) => (
                                <label
                                    key={noticia.noticiaId}
                                    className="flex cursor-pointer items-start gap-3 rounded-lg border border-borde px-3 py-2 text-sm text-texto hover:bg-fondo"
                                >
                                    <input
                                        type="checkbox"
                                        className="mt-1"
                                        checked={seleccionadas.includes(noticia.noticiaId)}
                                        onChange={() => alternarNoticia(noticia.noticiaId)}
                                    />
                                    <span>
                                        <span className="font-semibold">{noticia.titulo}</span>
                                        <span className="block text-xs text-texto-suave">
                                            {formatearFecha(noticia.fechaPublicacion)}
                                        </span>
                                    </span>
                                </label>
                            ))}
                        </fieldset>

                        <div className="flex flex-wrap items-center gap-3">
                            <Boton
                                type="submit"
                                className={CLASE_BOTON_SECUNDARIO}
                                cargando={previsualizando}
                                disabled={!puedePrevisualizar || enviando}
                            >
                                {t('admin.newsletter.envio.previsualizar')}
                            </Boton>

                            {vistaPrevia && !confirmandoEnvio && (
                                <Boton
                                    type="button"
                                    cargando={enviando}
                                    disabled={vistaPrevia.cantidadDestinatarios === 0}
                                    onClick={() => setConfirmandoEnvio(true)}
                                >
                                    {t('admin.newsletter.envio.enviar')}
                                </Boton>
                            )}

                            {vistaPrevia && confirmandoEnvio && (
                                <>
                                    <Boton type="button" cargando={enviando} onClick={() => void enviar()}>
                                        {t('admin.newsletter.envio.confirmar')}
                                    </Boton>
                                    <button
                                        type="button"
                                        className={CLASE_BOTON_SECUNDARIO}
                                        onClick={() => setConfirmandoEnvio(false)}
                                    >
                                        {t('comun.boton.no')}
                                    </button>
                                </>
                            )}
                        </div>
                    </form>

                    {vistaPrevia && (
                        <div className="mt-6">
                            <p className="text-sm text-texto-suave">
                                {t('admin.newsletter.envio.destinatarios', {
                                    cantidad: vistaPrevia.cantidadDestinatarios,
                                })}
                            </p>
                            <h3 className="mt-4 text-sm font-semibold text-texto">
                                {t('admin.newsletter.envio.vistaPrevia')}
                            </h3>
                            <iframe
                                title={t('admin.newsletter.envio.vistaPrevia')}
                                sandbox=""
                                srcDoc={vistaPrevia.html}
                                className="mt-2 h-[640px] w-full rounded-xl border border-borde bg-white"
                            />
                        </div>
                    )}
                </section>
            )}

            <section className="mt-8">
                <h2 className="text-lg font-semibold text-texto">
                    {t('admin.newsletter.suscriptores.titulo')}
                </h2>

                <div className="mt-4">
                    {cargandoSuscriptores ? (
                        <p className="text-sm text-texto-suave">
                            {t('admin.newsletter.suscriptores.cargando')}
                        </p>
                    ) : (
                        <>
                            {suscriptores.length > 0 && (
                                <p className="mb-3 text-sm text-texto-suave">
                                    {t('admin.newsletter.suscriptores.resumen', {
                                        confirmados: confirmados.length,
                                        total: suscriptores.length,
                                    })}
                                </p>
                            )}
                            <TablaAbm
                                columnas={columnasSuscriptores}
                                filas={suscriptores}
                                claveDe={(suscriptor) => suscriptor.suscriptorId}
                                inactiva={(suscriptor) => !suscriptor.activo}
                                mensajeVacio={t('admin.newsletter.suscriptores.vacio')}
                            />
                        </>
                    )}
                </div>
            </section>

            <section className="mt-8">
                <h2 className="text-lg font-semibold text-texto">{t('admin.newsletter.historial.titulo')}</h2>

                <div className="mt-4">
                    {cargandoEnvios ? (
                        <p className="text-sm text-texto-suave">{t('admin.newsletter.historial.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnasEnvios}
                            filas={envios}
                            claveDe={(envio) => envio.envioId}
                            mensajeVacio={t('admin.newsletter.historial.vacio')}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
