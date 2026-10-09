import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { encuestasApi } from '../../../api/encuestasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { GraficoBarras, type DatoGrafico } from '../../../componentes/GraficoBarras';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { EncuestaResponse, GuardarEncuestaRequest } from '../../../tipos/encuestas';

function hoy(): string {
    const fecha = new Date();
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${fecha.getFullYear()}-${mes}-${dia}`;
}

const OPCIONES_VACIAS = ['', ''];

const FORMULARIO_VACIO = {
    idiomaId: '',
    pregunta: '',
    descripcion: '',
    fechaDesde: '',
    fechaVencimiento: '',
    activo: true,
    opciones: OPCIONES_VACIAS,
};

type Formulario = typeof FORMULARIO_VACIO;

const MAXIMO_OPCIONES = 10;

/**
 * ABM de encuestas y sus resultados (Formulario, puntos 3, 10 y 15.a).
 *
 * Las opciones solo se pueden cambiar mientras la encuesta no tenga respuestas:
 * después, el formulario las muestra deshabilitadas y el backend las ignora.
 */
export function AdminEncuestas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { idiomas, formatearFecha } = useLocalizacion();

    const [encuestas, setEncuestas] = useState<EncuestaResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>({ ...FORMULARIO_VACIO, fechaDesde: hoy() });
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [tieneRespuestas, setTieneRespuestas] = useState(false);
    const [verResultadosDe, setVerResultadosDe] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso(PERMISOS.encuestaAlta);
    const puedeModificar = tienePermiso(PERMISOS.encuestaModificar);
    const puedeDarDeBaja = tienePermiso(PERMISOS.encuestaBaja);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        encuestasApi
            .listarAdministracion()
            .then(setEncuestas)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.encuestas.errorCargar')))
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiar() {
        setFormulario({ ...FORMULARIO_VACIO, fechaDesde: hoy() });
        setEditandoId(null);
        setTieneRespuestas(false);
    }

    function editar(item: EncuestaResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(item.encuesta.encuestaId);
        setTieneRespuestas(item.totalRespuestas > 0);
        setFormulario({
            idiomaId: String(item.encuesta.idiomaId),
            pregunta: item.encuesta.pregunta,
            descripcion: item.encuesta.descripcion ?? '',
            fechaDesde: item.encuesta.fechaDesde,
            fechaVencimiento: item.encuesta.fechaVencimiento,
            activo: item.encuesta.activo,
            opciones: item.opciones.map((opcion) => opcion.texto),
        });
    }

    function cambiarOpcion(indice: number, texto: string) {
        setFormulario({
            ...formulario,
            opciones: formulario.opciones.map((valor, posicion) => (posicion === indice ? texto : valor)),
        });
    }

    function agregarOpcion() {
        if (formulario.opciones.length >= MAXIMO_OPCIONES) {
            return;
        }

        setFormulario({ ...formulario, opciones: [...formulario.opciones, ''] });
    }

    function quitarOpcion(indice: number) {
        setFormulario({
            ...formulario,
            opciones: formulario.opciones.filter((_, posicion) => posicion !== indice),
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);

        const opciones = formulario.opciones.map((texto) => texto.trim()).filter((texto) => texto.length > 0);

        if (
            !formulario.idiomaId ||
            !formulario.pregunta.trim() ||
            !formulario.fechaDesde ||
            !formulario.fechaVencimiento ||
            (!tieneRespuestas && opciones.length < 2)
        ) {
            setError(t('admin.encuestas.incompleto'));

            return;
        }

        setGuardando(true);

        const cuerpo: GuardarEncuestaRequest = {
            idiomaId: Number(formulario.idiomaId),
            pregunta: formulario.pregunta.trim(),
            descripcion: formulario.descripcion.trim() || null,
            fechaDesde: formulario.fechaDesde,
            fechaVencimiento: formulario.fechaVencimiento,
            activo: formulario.activo,
            opciones: tieneRespuestas ? [] : opciones,
        };

        try {
            if (editandoId === null) {
                await encuestasApi.crear(cuerpo);
                setExito(t('admin.encuestas.exitoAlta'));
            } else {
                await encuestasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.encuestas.exitoModificacion'));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.encuestas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function darDeBaja(item: EncuestaResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await encuestasApi.baja(item.encuesta.encuestaId);
            setExito(t('admin.encuestas.exitoBaja'));

            if (editandoId === item.encuesta.encuestaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.encuestas.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<EncuestaResponse>[] = [
        { encabezado: t('admin.encuestas.col.pregunta'), celda: (item) => item.encuesta.pregunta },
        { encabezado: t('admin.encuestas.col.idioma'), celda: (item) => item.idioma },
        {
            encabezado: t('admin.encuestas.col.vencimiento'),
            celda: (item) => formatearFecha(item.encuesta.fechaVencimiento),
        },
        { encabezado: t('admin.encuestas.col.respuestas'), celda: (item) => String(item.totalRespuestas) },
        {
            encabezado: t('admin.encuestas.col.estado'),
            celda: (item) => (item.encuesta.activo ? t('admin.encuestas.activa') : t('admin.encuestas.inactiva')),
        },
    ];

    const seleccionada = encuestas.find((item) => item.encuesta.encuestaId === verResultadosDe);
    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    function datosDelGrafico(item: EncuestaResponse): DatoGrafico[] {
        return item.opciones.map((opcion) => ({ etiqueta: opcion.texto, valor: opcion.total }));
    }

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.encuestas.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.encuestas.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null ? t('admin.encuestas.nueva') : t('admin.encuestas.editar')}
                    </h2>

                    {tieneRespuestas && (
                        <p className="mt-2 text-sm text-texto-suave">{t('admin.encuestas.conRespuestas')}</p>
                    )}

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoSelect
                                etiqueta={t('admin.encuestas.col.idioma')}
                                identificador="encuesta-idioma"
                                required
                                value={formulario.idiomaId}
                                onChange={(evento) => setFormulario({ ...formulario, idiomaId: evento.target.value })}
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
                        </div>

                        <CampoTexto
                            etiqueta={t('admin.encuestas.col.pregunta')}
                            identificador="encuesta-pregunta"
                            required
                            maxLength={300}
                            value={formulario.pregunta}
                            onChange={(evento) => setFormulario({ ...formulario, pregunta: evento.target.value })}
                        />

                        <div className="campo">
                            <label htmlFor="encuesta-descripcion">{t('admin.encuestas.descripcionCampo')}</label>
                            <textarea
                                id="encuesta-descripcion"
                                rows={3}
                                maxLength={600}
                                value={formulario.descripcion}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, descripcion: evento.target.value })
                                }
                                className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                            />
                        </div>

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.encuestas.fechaDesde')}
                                identificador="encuesta-desde"
                                type="date"
                                required
                                value={formulario.fechaDesde}
                                onChange={(evento) => setFormulario({ ...formulario, fechaDesde: evento.target.value })}
                            />
                            <CampoTexto
                                etiqueta={t('admin.encuestas.fechaVencimiento')}
                                identificador="encuesta-vencimiento"
                                type="date"
                                required
                                value={formulario.fechaVencimiento}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, fechaVencimiento: evento.target.value })
                                }
                            />
                        </div>

                        <fieldset className="flex flex-col gap-3 border-0 p-0">
                            <legend className="text-sm font-semibold text-texto">
                                {t('admin.encuestas.opciones')}
                            </legend>

                            {formulario.opciones.map((texto, indice) => (
                                <div key={indice} className="flex items-end gap-2">
                                    <div className="flex-1">
                                        <CampoTexto
                                            etiqueta={t('admin.encuestas.opcion', { numero: indice + 1 })}
                                            identificador={`encuesta-opcion-${indice}`}
                                            maxLength={200}
                                            disabled={tieneRespuestas}
                                            value={texto}
                                            onChange={(evento) => cambiarOpcion(indice, evento.target.value)}
                                        />
                                    </div>

                                    {!tieneRespuestas && formulario.opciones.length > 2 && (
                                        <button
                                            type="button"
                                            onClick={() => quitarOpcion(indice)}
                                            className="mb-1 rounded-lg border border-borde bg-superficie px-3 py-2 text-sm font-semibold text-error hover:bg-error-fondo"
                                        >
                                            {t('admin.encuestas.quitarOpcion')}
                                        </button>
                                    )}
                                </div>
                            ))}

                            {!tieneRespuestas && formulario.opciones.length < MAXIMO_OPCIONES && (
                                <div>
                                    <button
                                        type="button"
                                        onClick={agregarOpcion}
                                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {t('admin.encuestas.agregarOpcion')}
                                    </button>
                                </div>
                            )}
                        </fieldset>

                        <label className="flex items-center gap-2.5 text-sm text-texto">
                            <input
                                type="checkbox"
                                className="size-4"
                                checked={formulario.activo}
                                onChange={(evento) => setFormulario({ ...formulario, activo: evento.target.checked })}
                            />
                            {t('admin.encuestas.activa')}
                        </label>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null ? t('admin.encuestas.crear') : t('comun.boton.guardarCambios')}
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
                <h2 className="text-lg font-semibold text-texto">{t('admin.encuestas.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.encuestas.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={encuestas}
                            claveDe={(item) => item.encuesta.encuestaId}
                            inactiva={(item) => !item.encuesta.activo}
                            mensajeVacio={t('admin.encuestas.vacio')}
                            acciones={(item) => (
                                <div className="flex justify-end gap-2">
                                    <button
                                        type="button"
                                        onClick={() =>
                                            setVerResultadosDe(
                                                verResultadosDe === item.encuesta.encuestaId
                                                    ? null
                                                    : item.encuesta.encuestaId,
                                            )
                                        }
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {t('admin.encuestas.verResultados')}
                                    </button>

                                    {puedeModificar && (
                                        <button
                                            type="button"
                                            onClick={() => editar(item)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                        >
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {puedeDarDeBaja &&
                                        item.encuesta.activo &&
                                        (confirmandoBaja === item.encuesta.encuestaId ? (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => void darDeBaja(item)}
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
                                                onClick={() => setConfirmandoBaja(item.encuesta.encuestaId)}
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

            {seleccionada && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="m-0 text-lg font-semibold text-texto">
                        {t('admin.encuestas.estadisticas', { pregunta: seleccionada.encuesta.pregunta })}
                    </h2>
                    <p className="mt-1 text-sm text-texto-suave">
                        {t('encuestas.totalVotos', { total: seleccionada.totalRespuestas })}
                    </p>

                    <div className="mt-4">
                        <GraficoBarras
                            datos={datosDelGrafico(seleccionada)}
                            etiquetaSerie={t('admin.encuestas.col.respuestas')}
                            mensajeVacio={t('comun.grafico.sinDatos')}
                        />
                    </div>
                </section>
            )}
        </div>
    );
}
