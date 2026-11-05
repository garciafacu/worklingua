import { type FormEvent, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { faqsApi } from '../../../api/faqsApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { Boton, BotonAccion } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type {
    CategoriaFaqResponse,
    GuardarCategoriaFaqRequest,
    GuardarPreguntaFrecuenteRequest,
    PreguntaFrecuenteResponse,
} from '../../../tipos/faqs';

const CATEGORIA_VACIA = {
    idiomaId: '',
    titulo: '',
    orden: '10',
};

const PREGUNTA_VACIA = {
    pregunta: '',
    respuesta: '',
    enlace: '',
    enlaceEtiqueta: '',
    orden: '10',
};

type FormularioCategoria = typeof CATEGORIA_VACIA;
type FormularioPregunta = typeof PREGUNTA_VACIA;

const CLASE_ACCION =
    'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo';

export function AdminFaqs() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { idiomas } = useLocalizacion();

    const [categorias, setCategorias] = useState<CategoriaFaqResponse[]>([]);
    const [preguntas, setPreguntas] = useState<PreguntaFrecuenteResponse[]>([]);
    const [seleccionadaId, setSeleccionadaId] = useState<number | null>(null);
    const [busqueda, setBusqueda] = useState('');

    const [formCategoria, setFormCategoria] = useState<FormularioCategoria>(CATEGORIA_VACIA);
    const [editandoCategoriaId, setEditandoCategoriaId] = useState<number | null>(null);
    const [formPregunta, setFormPregunta] = useState<FormularioPregunta>(PREGUNTA_VACIA);
    const [editandoPreguntaId, setEditandoPreguntaId] = useState<number | null>(null);

    const [confirmandoCategoria, setConfirmandoCategoria] = useState<number | null>(null);
    const [confirmandoPregunta, setConfirmandoPregunta] = useState<number | null>(null);

    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [recarga, setRecarga] = useState(0);

    const puedeCrear = tienePermiso(PERMISOS.faqAlta);
    const puedeModificar = tienePermiso(PERMISOS.faqModificar);
    const puedeDarDeBaja = tienePermiso(PERMISOS.faqBaja);

    const mensajeDeError = useMensajeDeError();

    useEffect(() => {
        faqsApi
            .listarCategorias()
            .then(setCategorias)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.faqs.error')))
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    useEffect(() => {
        if (seleccionadaId === null) {
            return;
        }

        faqsApi
            .listarPreguntas(seleccionadaId)
            .then(setPreguntas)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.faqs.error')));
    }, [seleccionadaId, recarga, mensajeDeError]);

    const seleccionada = useMemo(
        () => categorias.find((item) => item.categoriaFaqId === seleccionadaId) ?? null,
        [categorias, seleccionadaId],
    );

    const categoriasVisibles = useMemo(() => {
        const termino = busqueda.trim().toLowerCase();

        if (termino === '') {
            return categorias;
        }

        return categorias.filter(
            (item) =>
                item.titulo.toLowerCase().includes(termino) ||
                item.idioma.toLowerCase().includes(termino),
        );
    }, [categorias, busqueda]);

    function recargar() {
        setRecarga((numero) => numero + 1);
    }

    function limpiarAvisos() {
        setError(null);
        setExito(null);
        setConfirmandoCategoria(null);
        setConfirmandoPregunta(null);
    }

    function limpiarCategoria() {
        setFormCategoria(CATEGORIA_VACIA);
        setEditandoCategoriaId(null);
    }

    function limpiarPregunta() {
        setFormPregunta(PREGUNTA_VACIA);
        setEditandoPreguntaId(null);
    }

    function editarCategoria(categoria: CategoriaFaqResponse) {
        limpiarAvisos();
        setEditandoCategoriaId(categoria.categoriaFaqId);
        setFormCategoria({
            idiomaId: String(categoria.idiomaId),
            titulo: categoria.titulo,
            orden: String(categoria.orden),
        });
    }

    function verPreguntas(categoria: CategoriaFaqResponse) {
        limpiarAvisos();
        limpiarPregunta();
        setSeleccionadaId(categoria.categoriaFaqId);
    }

    function editarPregunta(pregunta: PreguntaFrecuenteResponse) {
        limpiarAvisos();
        setEditandoPreguntaId(pregunta.preguntaFrecuenteId);
        setFormPregunta({
            pregunta: pregunta.pregunta,
            respuesta: pregunta.respuesta,
            enlace: pregunta.enlace ?? '',
            enlaceEtiqueta: pregunta.enlaceEtiqueta ?? '',
            orden: String(pregunta.orden),
        });
    }

    function activoDe(categoriaFaqId: number | null): boolean {
        if (categoriaFaqId === null) {
            return true;
        }

        return categorias.find((item) => item.categoriaFaqId === categoriaFaqId)?.activo ?? true;
    }

    function activoDePregunta(preguntaFrecuenteId: number | null): boolean {
        if (preguntaFrecuenteId === null) {
            return true;
        }

        return (
            preguntas.find((item) => item.preguntaFrecuenteId === preguntaFrecuenteId)?.activo ??
            true
        );
    }

    async function guardarCategoria(evento: FormEvent) {
        evento.preventDefault();
        limpiarAvisos();
        setGuardando(true);

        const cuerpo: GuardarCategoriaFaqRequest = {
            idiomaId: Number(formCategoria.idiomaId),
            titulo: formCategoria.titulo.trim(),
            orden: Number(formCategoria.orden),
            activo: activoDe(editandoCategoriaId),
        };

        try {
            if (editandoCategoriaId === null) {
                await faqsApi.crearCategoria(cuerpo);
            } else {
                await faqsApi.modificarCategoria(editandoCategoriaId, cuerpo);
            }

            setExito(t('admin.faqs.exito'));
            limpiarCategoria();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.faqs.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function guardarPregunta(evento: FormEvent) {
        evento.preventDefault();

        if (seleccionadaId === null) {
            return;
        }

        limpiarAvisos();
        setGuardando(true);

        const enlace = formPregunta.enlace.trim();

        const cuerpo: GuardarPreguntaFrecuenteRequest = {
            categoriaFaqId: seleccionadaId,
            pregunta: formPregunta.pregunta.trim(),
            respuesta: formPregunta.respuesta.trim(),
            enlace: enlace === '' ? null : enlace,
            enlaceEtiqueta:
                formPregunta.enlaceEtiqueta.trim() === ''
                    ? null
                    : formPregunta.enlaceEtiqueta.trim(),
            orden: Number(formPregunta.orden),
            activo: activoDePregunta(editandoPreguntaId),
        };

        try {
            if (editandoPreguntaId === null) {
                await faqsApi.crearPregunta(cuerpo);
            } else {
                await faqsApi.modificarPregunta(editandoPreguntaId, cuerpo);
            }

            setExito(t('admin.faqs.exito'));
            limpiarPregunta();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.faqs.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function cambiarEstadoCategoria(categoria: CategoriaFaqResponse) {
        limpiarAvisos();

        try {
            await faqsApi.cambiarEstadoCategoria(categoria.categoriaFaqId, !categoria.activo);
            setExito(t('admin.faqs.exito'));
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.faqs.errorGuardar'));
        }
    }

    async function darDeBajaCategoria(categoria: CategoriaFaqResponse) {
        limpiarAvisos();

        try {
            await faqsApi.bajaCategoria(categoria.categoriaFaqId);
            setExito(t('admin.faqs.exito'));

            if (editandoCategoriaId === categoria.categoriaFaqId) {
                limpiarCategoria();
            }

            if (seleccionadaId === categoria.categoriaFaqId) {
                setSeleccionadaId(null);
                setPreguntas([]);
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.faqs.errorGuardar'));
        }
    }

    async function cambiarEstadoPregunta(pregunta: PreguntaFrecuenteResponse) {
        limpiarAvisos();

        try {
            await faqsApi.cambiarEstadoPregunta(pregunta.preguntaFrecuenteId, !pregunta.activo);
            setExito(t('admin.faqs.exito'));
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.faqs.errorGuardar'));
        }
    }

    async function darDeBajaPregunta(pregunta: PreguntaFrecuenteResponse) {
        limpiarAvisos();

        try {
            await faqsApi.bajaPregunta(pregunta.preguntaFrecuenteId);
            setExito(t('admin.faqs.exito'));

            if (editandoPreguntaId === pregunta.preguntaFrecuenteId) {
                limpiarPregunta();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.faqs.errorGuardar'));
        }
    }

    function nombreIdiomaDeCategoria(idiomaId: number): string {
        const dePlataforma = idiomas.find((idioma) => idioma.idiomaId === idiomaId);

        if (dePlataforma !== undefined) {
            return dePlataforma.nombre;
        }

        return (
            categorias.find((item) => item.idiomaId === idiomaId)?.idioma ?? `#${idiomaId}`
        );
    }

    function etiquetaEstado(activo: boolean) {
        return (
            <span
                className={
                    activo
                        ? 'rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario'
                        : 'rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave'
                }
            >
                {activo ? t('comun.estado.activo') : t('comun.estado.inactivo')}
            </span>
        );
    }

    const columnasCategoria: ColumnaAbm<CategoriaFaqResponse>[] = [
        { encabezado: t('admin.faqs.categoria.titulo'), celda: (fila) => fila.titulo },
        { encabezado: t('comun.campo.idioma'), celda: (fila) => fila.idioma },
        { encabezado: t('admin.faqs.orden'), celda: (fila) => fila.orden, numerica: true },
        {
            encabezado: t('admin.faqs.col.preguntas'),
            celda: (fila) => fila.totalPreguntas,
            numerica: true,
        },
        { encabezado: t('comun.campo.estado'), celda: (fila) => etiquetaEstado(fila.activo) },
    ];

    const columnasPregunta: ColumnaAbm<PreguntaFrecuenteResponse>[] = [
        { encabezado: t('admin.faqs.pregunta.pregunta'), celda: (fila) => fila.pregunta },
        {
            encabezado: t('admin.faqs.col.enlace'),
            celda: (fila) => fila.enlace ?? t('comun.valor.vacio'),
        },
        { encabezado: t('admin.faqs.orden'), celda: (fila) => fila.orden, numerica: true },
        { encabezado: t('comun.campo.estado'), celda: (fila) => etiquetaEstado(fila.activo) },
    ];

    const idiomaDelFormularioListado = idiomas.some(
        (idioma) => String(idioma.idiomaId) === formCategoria.idiomaId,
    );

    const puedeFormularioCategoria = editandoCategoriaId === null ? puedeCrear : puedeModificar;
    const puedeFormularioPregunta = editandoPreguntaId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.faqs.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.faqs.descripcion')}</p>

            <AlertasResultado error={error} exito={exito} />

            {puedeFormularioCategoria && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoCategoriaId === null
                            ? t('admin.faqs.categoria.nueva')
                            : t('admin.faqs.categoria.editar')}
                    </h2>

                    <form onSubmit={guardarCategoria} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.faqs.categoria.titulo')}
                                identificador="tituloCategoria"
                                required
                                maxLength={150}
                                value={formCategoria.titulo}
                                onChange={(evento) =>
                                    setFormCategoria({
                                        ...formCategoria,
                                        titulo: evento.target.value,
                                    })
                                }
                            />

                            <CampoSelect
                                etiqueta={t('comun.campo.idioma')}
                                identificador="idiomaCategoria"
                                required
                                value={formCategoria.idiomaId}
                                onChange={(evento) =>
                                    setFormCategoria({
                                        ...formCategoria,
                                        idiomaId: evento.target.value,
                                    })
                                }
                            >
                                <option value="" disabled>
                                    —
                                </option>

                                {!idiomaDelFormularioListado && formCategoria.idiomaId !== '' && (
                                    <option value={formCategoria.idiomaId}>
                                        {nombreIdiomaDeCategoria(Number(formCategoria.idiomaId))}
                                    </option>
                                )}

                                {idiomas.map((idioma) => (
                                    <option key={idioma.idiomaId} value={idioma.idiomaId}>
                                        {idioma.nombre}
                                    </option>
                                ))}
                            </CampoSelect>

                            <CampoTexto
                                etiqueta={t('admin.faqs.orden')}
                                identificador="ordenCategoria"
                                type="number"
                                required
                                min={0}
                                max={1000}
                                value={formCategoria.orden}
                                onChange={(evento) =>
                                    setFormCategoria({
                                        ...formCategoria,
                                        orden: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoCategoriaId === null
                                    ? t('admin.faqs.categoria.crear')
                                    : t('comun.boton.guardarCambios')}
                            </Boton>

                            {editandoCategoriaId !== null && (
                                <button type="button" onClick={limpiarCategoria} className={CLASE_ACCION}>
                                    {t('comun.boton.cancelar')}
                                </button>
                            )}
                        </div>
                    </form>
                </section>
            )}

            <section className="mt-6">
                <div className="flex flex-wrap items-end justify-between gap-4">
                    <h2 className="text-lg font-semibold text-texto">
                        {t('admin.faqs.categorias')}
                    </h2>

                    <div className="w-full sm:w-72">
                        <CampoTexto
                            etiqueta={t('admin.faqs.buscar')}
                            identificador="busquedaFaq"
                            type="search"
                            placeholder={t('admin.faqs.buscarPlaceholder')}
                            value={busqueda}
                            onChange={(evento) => setBusqueda(evento.target.value)}
                        />
                    </div>
                </div>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.faqs.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnasCategoria}
                            filas={categoriasVisibles}
                            claveDe={(fila) => fila.categoriaFaqId}
                            inactiva={(fila) => !fila.activo}
                            mensajeVacio={
                                busqueda.trim() === ''
                                    ? t('admin.faqs.categoria.vacio')
                                    : t('admin.faqs.sinCoincidencias')
                            }
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    <button
                                        type="button"
                                        onClick={() => verPreguntas(fila)}
                                        className={CLASE_ACCION}
                                    >
                                        {t('comun.boton.ver')}
                                    </button>

                                    {puedeModificar && (
                                        <>
                                            <button
                                                type="button"
                                                onClick={() => editarCategoria(fila)}
                                                className={CLASE_ACCION}
                                            >
                                                {t('comun.boton.editar')}
                                            </button>

                                            <BotonAccion
                                                variante="primario"
                                                onClick={() => void cambiarEstadoCategoria(fila)}
                                            >
                                                {fila.activo
                                                    ? t('comun.boton.desactivar')
                                                    : t('comun.boton.activar')}
                                            </BotonAccion>
                                        </>
                                    )}

                                    {puedeDarDeBaja &&
                                        fila.activo &&
                                        (confirmandoCategoria === fila.categoriaFaqId ? (
                                            <>
                                                <BotonAccion
                                                    variante="confirmar"
                                                    onClick={() => void darDeBajaCategoria(fila)}
                                                >
                                                    {t('comun.boton.confirmarBaja')}
                                                </BotonAccion>
                                                <button
                                                    type="button"
                                                    onClick={() => setConfirmandoCategoria(null)}
                                                    className={CLASE_ACCION}
                                                >
                                                    {t('comun.boton.no')}
                                                </button>
                                            </>
                                        ) : (
                                            <BotonAccion
                                                variante="peligro"
                                                onClick={() =>
                                                    setConfirmandoCategoria(fila.categoriaFaqId)
                                                }
                                            >
                                                {t('comun.boton.darDeBaja')}
                                            </BotonAccion>
                                        ))}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>

            <section className="mt-10">
                <h2 className="text-lg font-semibold text-texto">
                    {seleccionada === null
                        ? t('admin.faqs.preguntas', { categoria: '—' })
                        : t('admin.faqs.preguntas', { categoria: seleccionada.titulo })}
                </h2>

                {seleccionada === null ? (
                    <p className="mt-2 text-sm text-texto-suave">
                        {t('admin.faqs.categoria.elegir')}
                    </p>
                ) : (
                    <>
                        {puedeFormularioPregunta && (
                            <div className="mt-4 rounded-2xl border border-borde bg-superficie p-6">
                                <h3 className="text-base font-semibold text-texto">
                                    {editandoPreguntaId === null
                                        ? t('admin.faqs.pregunta.nueva')
                                        : t('admin.faqs.pregunta.editar')}
                                </h3>

                                <form
                                    onSubmit={guardarPregunta}
                                    noValidate
                                    className="mt-4 flex flex-col gap-4"
                                >
                                    <CampoTexto
                                        etiqueta={t('admin.faqs.pregunta.pregunta')}
                                        identificador="pregunta"
                                        required
                                        maxLength={300}
                                        value={formPregunta.pregunta}
                                        onChange={(evento) =>
                                            setFormPregunta({
                                                ...formPregunta,
                                                pregunta: evento.target.value,
                                            })
                                        }
                                    />

                                    <div className="campo">
                                        <label htmlFor="respuesta">
                                            {t('admin.faqs.pregunta.respuesta')}
                                        </label>
                                        <textarea
                                            id="respuesta"
                                            name="respuesta"
                                            rows={5}
                                            required
                                            maxLength={2000}
                                            value={formPregunta.respuesta}
                                            onChange={(evento) =>
                                                setFormPregunta({
                                                    ...formPregunta,
                                                    respuesta: evento.target.value,
                                                })
                                            }
                                        />
                                    </div>

                                    <div className="fila">
                                        <CampoTexto
                                            etiqueta={t('admin.faqs.pregunta.enlace')}
                                            identificador="enlace"
                                            maxLength={200}
                                            placeholder="/catalogo"
                                            value={formPregunta.enlace}
                                            onChange={(evento) =>
                                                setFormPregunta({
                                                    ...formPregunta,
                                                    enlace: evento.target.value,
                                                })
                                            }
                                        />

                                        <CampoTexto
                                            etiqueta={t('admin.faqs.pregunta.enlaceEtiqueta')}
                                            identificador="enlaceEtiqueta"
                                            maxLength={100}
                                            value={formPregunta.enlaceEtiqueta}
                                            onChange={(evento) =>
                                                setFormPregunta({
                                                    ...formPregunta,
                                                    enlaceEtiqueta: evento.target.value,
                                                })
                                            }
                                        />

                                        <CampoTexto
                                            etiqueta={t('admin.faqs.orden')}
                                            identificador="ordenPregunta"
                                            type="number"
                                            required
                                            min={0}
                                            max={1000}
                                            value={formPregunta.orden}
                                            onChange={(evento) =>
                                                setFormPregunta({
                                                    ...formPregunta,
                                                    orden: evento.target.value,
                                                })
                                            }
                                        />
                                    </div>

                                    <p className="ayuda">{t('admin.faqs.pregunta.enlace.ayuda')}</p>

                                    <div className="flex flex-wrap gap-3">
                                        <Boton type="submit" cargando={guardando}>
                                            {editandoPreguntaId === null
                                                ? t('admin.faqs.pregunta.crear')
                                                : t('comun.boton.guardarCambios')}
                                        </Boton>

                                        {editandoPreguntaId !== null && (
                                            <button
                                                type="button"
                                                onClick={limpiarPregunta}
                                                className={CLASE_ACCION}
                                            >
                                                {t('comun.boton.cancelar')}
                                            </button>
                                        )}
                                    </div>
                                </form>
                            </div>
                        )}

                        <div className="mt-4">
                            <TablaAbm
                                columnas={columnasPregunta}
                                filas={preguntas}
                                claveDe={(fila) => fila.preguntaFrecuenteId}
                                inactiva={(fila) => !fila.activo}
                                mensajeVacio={t('admin.faqs.pregunta.vacio')}
                                acciones={(fila) => (
                                    <div className="flex justify-end gap-2">
                                        {puedeModificar && (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => editarPregunta(fila)}
                                                    className={CLASE_ACCION}
                                                >
                                                    {t('comun.boton.editar')}
                                                </button>

                                                <BotonAccion
                                                    variante="primario"
                                                    onClick={() => void cambiarEstadoPregunta(fila)}
                                                >
                                                    {fila.activo
                                                        ? t('comun.boton.desactivar')
                                                        : t('comun.boton.activar')}
                                                </BotonAccion>
                                            </>
                                        )}

                                        {puedeDarDeBaja &&
                                            fila.activo &&
                                            (confirmandoPregunta === fila.preguntaFrecuenteId ? (
                                                <>
                                                    <BotonAccion
                                                        variante="confirmar"
                                                        onClick={() => void darDeBajaPregunta(fila)}
                                                    >
                                                        {t('comun.boton.confirmarBaja')}
                                                    </BotonAccion>
                                                    <button
                                                        type="button"
                                                        onClick={() => setConfirmandoPregunta(null)}
                                                        className={CLASE_ACCION}
                                                    >
                                                        {t('comun.boton.no')}
                                                    </button>
                                                </>
                                            ) : (
                                                <BotonAccion
                                                    variante="peligro"
                                                    onClick={() =>
                                                        setConfirmandoPregunta(
                                                            fila.preguntaFrecuenteId,
                                                        )
                                                    }
                                                >
                                                    {t('comun.boton.darDeBaja')}
                                                </BotonAccion>
                                            ))}
                                    </div>
                                )}
                            />
                        </div>
                    </>
                )}
            </section>
        </div>
    );
}
