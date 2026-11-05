import { type FormEvent, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { noticiasApi } from '../../../api/noticiasApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { BotonAccion, BotonesFormulario, ConfirmarBaja } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
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

function aCampoFecha(valor: string): string {
    return valor.slice(0, 16);
}

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

    const mensajeDeError = useMensajeDeError();

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

            <AlertasResultado error={error} exito={exito} />

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

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.noticias.formulario.crear')}
                            onCancelar={limpiar}
                        />
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
                                        <BotonAccion onClick={() => editar(noticia)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {puedeDarDeBaja &&
                                        noticia.activo &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === noticia.noticiaId}
                                            onPedir={() => setConfirmandoBaja(noticia.noticiaId)}
                                            onConfirmar={() => void darDeBaja(noticia)}
                                            onCancelar={() => setConfirmandoBaja(null)}
                                        />)}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
