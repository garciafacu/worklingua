import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { noticiasApi } from '../../../api/noticiasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { GuardarNoticiaRequest, NoticiaAdminResponse } from '../../../tipos/noticias';

const FORMULARIO_VACIO = {
    idiomaId: '',
    titulo: '',
    resumen: '',
    contenido: '',
    fechaPublicacion: '',
};

type Formulario = typeof FORMULARIO_VACIO;

type EstadoNoticia = 'baja' | 'programada' | 'publicada';

function estadoDe(noticia: NoticiaAdminResponse): EstadoNoticia {
    if (!noticia.activo) {
        return 'baja';
    }

    return new Date(noticia.fechaPublicacion) > new Date() ? 'programada' : 'publicada';
}

/** `datetime-local` trabaja con "yyyy-MM-ddTHH:mm", sin segundos ni zona. */
function aCampoFecha(valor: string): string {
    return valor.slice(0, 16);
}

/**
 * ABM de las noticias que muestra la página pública de Novedades.
 *
 * Cada noticia se escribe en UN idioma de la plataforma. El contenido es texto
 * plano: el sitio y el newsletter lo muestran respetando saltos de línea y
 * nunca lo interpretan como HTML.
 */
export function AdminNoticias() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { idiomas, formatearFecha } = useLocalizacion();

    const [noticias, setNoticias] = useState<NoticiaAdminResponse[]>([]);
    const [filtroIdiomaId, setFiltroIdiomaId] = useState('');
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso(PERMISOS.noticiaAlta);
    const puedeModificar = tienePermiso(PERMISOS.noticiaModificar);
    const puedeDarDeBaja = tienePermiso(PERMISOS.noticiaBaja);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        noticiasApi
            .listarAdministracion(filtroIdiomaId ? Number(filtroIdiomaId) : undefined)
            .then(setNoticias)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.noticias.errorCargar')))
            .finally(() => setCargando(false));
    }, [recarga, filtroIdiomaId, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function nombreIdioma(idiomaId: number): string {
        return idiomas.find((idioma) => idioma.idiomaId === idiomaId)?.nombre ?? `#${idiomaId}`;
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(noticia: NoticiaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(noticia.noticiaId);
        setFormulario({
            idiomaId: String(noticia.idiomaId),
            titulo: noticia.titulo,
            resumen: noticia.resumen ?? '',
            contenido: noticia.contenido,
            fechaPublicacion: aCampoFecha(noticia.fechaPublicacion),
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: GuardarNoticiaRequest = {
            idiomaId: Number(formulario.idiomaId),
            titulo: formulario.titulo.trim(),
            resumen: formulario.resumen.trim() || null,
            contenido: formulario.contenido.trim(),
            fechaPublicacion: formulario.fechaPublicacion ? `${formulario.fechaPublicacion}:00` : null,
        };

        try {
            if (editandoId === null) {
                await noticiasApi.crear(cuerpo);
                setExito(t('admin.noticias.exitoAlta', { titulo: cuerpo.titulo }));
            } else {
                await noticiasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.noticias.exitoModificacion', { titulo: cuerpo.titulo }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.noticias.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function darDeBaja(noticia: NoticiaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await noticiasApi.baja(noticia.noticiaId);
            setExito(t('admin.noticias.exitoBaja', { titulo: noticia.titulo }));

            if (editandoId === noticia.noticiaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.noticias.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<NoticiaAdminResponse>[] = [
        { encabezado: t('admin.noticias.tabla.titulo'), celda: (noticia) => noticia.titulo },
        { encabezado: t('comun.campo.idioma'), celda: (noticia) => nombreIdioma(noticia.idiomaId) },
        {
            encabezado: t('admin.noticias.tabla.publicacion'),
            celda: (noticia) => formatearFecha(noticia.fechaPublicacion, true),
        },
        {
            encabezado: t('admin.noticias.tabla.estado'),
            celda: (noticia) => t(`admin.noticias.estado.${estadoDe(noticia)}`),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;
    const idiomaDelFormularioListado = idiomas.some(
        (idioma) => String(idioma.idiomaId) === formulario.idiomaId,
    );

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.noticias.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.noticias.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.noticias.formulario.nuevo')
                            : t('admin.noticias.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <CampoTexto
                            etiqueta={t('admin.noticias.formulario.titulo')}
                            identificador="titulo"
                            required
                            maxLength={150}
                            value={formulario.titulo}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, titulo: evento.target.value })
                            }
                        />

                        <div className="fila">
                            <CampoSelect
                                etiqueta={t('comun.campo.idioma')}
                                identificador="idiomaId"
                                required
                                value={formulario.idiomaId}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, idiomaId: evento.target.value })
                                }
                            >
                                <option value="" disabled>
                                    —
                                </option>
                                {!idiomaDelFormularioListado && formulario.idiomaId && (
                                    <option value={formulario.idiomaId}>
                                        {nombreIdioma(Number(formulario.idiomaId))}
                                    </option>
                                )}
                                {idiomas.map((idioma) => (
                                    <option key={idioma.idiomaId} value={idioma.idiomaId}>
                                        {idioma.nombre}
                                    </option>
                                ))}
                            </CampoSelect>

                            <CampoTexto
                                etiqueta={t('admin.noticias.formulario.fechaPublicacion')}
                                identificador="fechaPublicacion"
                                type="datetime-local"
                                value={formulario.fechaPublicacion}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        fechaPublicacion: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        <CampoTexto
                            etiqueta={t('admin.noticias.formulario.resumen')}
                            identificador="resumen"
                            maxLength={300}
                            value={formulario.resumen}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, resumen: evento.target.value })
                            }
                        />

                        <div className="campo">
                            <label htmlFor="contenido">{t('admin.noticias.formulario.contenido')}</label>
                            <textarea
                                id="contenido"
                                name="contenido"
                                rows={8}
                                required
                                maxLength={20000}
                                value={formulario.contenido}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, contenido: evento.target.value })
                                }
                                className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                            />
                        </div>

                        <p className="ayuda">{t('admin.noticias.formulario.ayuda')}</p>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.noticias.formulario.crear')
                                    : t('comun.boton.guardarCambios')}
                            </Boton>

                            {editandoId !== null && (
                                <button
                                    type="button"
                                    onClick={limpiar}
                                    className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                                >
                                    {t('comun.boton.cancelar')}
                                </button>
                            )}
                        </div>
                    </form>
                </section>
            )}

            <section className="mt-6">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                    <h2 className="text-lg font-semibold text-texto">{t('admin.noticias.listado')}</h2>

                    <div className="sm:w-64">
                        <CampoSelect
                            etiqueta={t('admin.noticias.filtro.idioma')}
                            identificador="filtroIdioma"
                            value={filtroIdiomaId}
                            onChange={(evento) => {
                                setCargando(true);
                                setFiltroIdiomaId(evento.target.value);
                            }}
                        >
                            <option value="">{t('admin.noticias.filtro.todos')}</option>
                            {idiomas.map((idioma) => (
                                <option key={idioma.idiomaId} value={idioma.idiomaId}>
                                    {idioma.nombre}
                                </option>
                            ))}
                        </CampoSelect>
                    </div>
                </div>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.noticias.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={noticias}
                            claveDe={(noticia) => noticia.noticiaId}
                            inactiva={(noticia) => !noticia.activo}
                            mensajeVacio={t('admin.noticias.vacio')}
                            acciones={(noticia) => (
                                <div className="flex justify-end gap-2">
                                    {puedeModificar && (
                                        <button
                                            type="button"
                                            onClick={() => editar(noticia)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                        >
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {puedeDarDeBaja &&
                                        noticia.activo &&
                                        (confirmandoBaja === noticia.noticiaId ? (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => void darDeBaja(noticia)}
                                                    className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                                >
                                                    {t('comun.boton.confirmarBaja')}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => setConfirmandoBaja(null)}
                                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                                >
                                                    {t('comun.boton.no')}
                                                </button>
                                            </>
                                        ) : (
                                            <button
                                                type="button"
                                                onClick={() => setConfirmandoBaja(noticia.noticiaId)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                            >
                                                {t('comun.boton.darDeBaja')}
                                            </button>
                                        ))}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
