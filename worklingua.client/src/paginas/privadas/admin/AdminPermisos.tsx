import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { permisosApi } from '../../../api/permisosApi';
import { Alerta } from '../../../componentes/Alerta';
import { ArbolPermisos } from '../../../componentes/ArbolPermisos';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { useSesion } from '../../../contexto/useSesion';
import {
    aplanarPermisos,
    type GuardarPermisoRequest,
    type PermisoResponse,
} from '../../../tipos/permisos';

const FORMULARIO_VACIO = {
    nombre: '',
    descripcion: '',
};

type Formulario = typeof FORMULARIO_VACIO;

const CLASE_BOTON =
    'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo';
const CLASE_BOTON_PELIGRO =
    'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo';

/**
 * ABM de permisos con el patrón Composite.
 *
 * Las patentes son las que chequea el código y vienen del sistema: acá solo se
 * ven. Lo que se administra son las familias: se crean, se editan, se eliminan
 * y se componen con patentes y otras familias. Las reglas (ciclos, permisos
 * reservados, familias en uso) las valida el backend y sus mensajes se
 * muestran tal cual.
 */
export function AdminPermisos() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const [arbol, setArbol] = useState<PermisoResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoEliminar, setConfirmandoEliminar] = useState<number | null>(null);
    const [agregandoEn, setAgregandoEn] = useState<number | null>(null);
    const [hijoElegido, setHijoElegido] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Permiso.Alta');
    const puedeModificar = tienePermiso('Permiso.Modificar');
    const puedeEliminar = tienePermiso('Permiso.Baja');

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        permisosApi
            .listar()
            .then(setArbol)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.permisos.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    const todos = useMemo(() => aplanarPermisos(arbol), [arbol]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiarMensajes() {
        setError(null);
        setExito(null);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(familia: PermisoResponse) {
        limpiarMensajes();
        setConfirmandoEliminar(null);
        setAgregandoEn(null);
        setEditandoId(familia.permisoId);
        setFormulario({ nombre: familia.nombre, descripcion: familia.descripcion ?? '' });
    }

    function abrirAgregar(familia: PermisoResponse) {
        limpiarMensajes();
        setConfirmandoEliminar(null);
        setHijoElegido('');
        setAgregandoEn(agregandoEn === familia.permisoId ? null : familia.permisoId);
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        limpiarMensajes();
        setGuardando(true);

        const cuerpo: GuardarPermisoRequest = {
            nombre: formulario.nombre.trim(),
            descripcion: formulario.descripcion.trim() === '' ? null : formulario.descripcion.trim(),
        };

        try {
            if (editandoId === null) {
                await permisosApi.crear(cuerpo);
                setExito(t('admin.permisos.exitoAlta', { nombre: cuerpo.nombre }));
            } else {
                await permisosApi.modificar(editandoId, cuerpo);
                setExito(t('admin.permisos.exitoModificacion', { nombre: cuerpo.nombre }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.permisos.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function eliminar(familia: PermisoResponse) {
        limpiarMensajes();
        setConfirmandoEliminar(null);

        try {
            await permisosApi.eliminar(familia.permisoId);
            setExito(t('admin.permisos.exitoEliminar', { nombre: familia.nombre }));

            if (editandoId === familia.permisoId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.permisos.errorEliminar'));
        }
    }

    async function agregarHijo(familia: PermisoResponse) {
        const hijo = todos.find((permiso) => permiso.permisoId === Number(hijoElegido));

        if (!hijo) {
            return;
        }

        limpiarMensajes();

        try {
            await permisosApi.agregarHijo(familia.permisoId, hijo.permisoId);
            setExito(t('admin.permisos.exitoAgregarHijo', { hijo: hijo.nombre, familia: familia.nombre }));
            setAgregandoEn(null);
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.permisos.errorAgregarHijo'));
        }
    }

    async function quitarHijo(familia: PermisoResponse, hijo: PermisoResponse) {
        limpiarMensajes();

        try {
            await permisosApi.quitarHijo(familia.permisoId, hijo.permisoId);
            setExito(t('admin.permisos.exitoQuitarHijo', { hijo: hijo.nombre, familia: familia.nombre }));
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.permisos.errorQuitarHijo'));
        }
    }

    function acciones(nodo: PermisoResponse, padre: PermisoResponse | null) {
        return (
            <>
                {nodo.esCompuesto && puedeModificar && (
                    <button type="button" onClick={() => abrirAgregar(nodo)} className={CLASE_BOTON}>
                        {t('admin.permisos.agregarHijo')}
                    </button>
                )}

                {nodo.esCompuesto && puedeModificar && (
                    <button type="button" onClick={() => editar(nodo)} className={CLASE_BOTON}>
                        {t('comun.boton.editar')}
                    </button>
                )}

                {padre !== null && puedeModificar && (
                    <button
                        type="button"
                        onClick={() => void quitarHijo(padre, nodo)}
                        className={CLASE_BOTON_PELIGRO}
                    >
                        {t('comun.boton.quitar')}
                    </button>
                )}

                {nodo.esCompuesto &&
                    padre === null &&
                    puedeEliminar &&
                    (confirmandoEliminar === nodo.permisoId ? (
                        <>
                            <button
                                type="button"
                                onClick={() => void eliminar(nodo)}
                                className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                            >
                                {t('comun.boton.confirmarEliminar')}
                            </button>
                            <button
                                type="button"
                                onClick={() => setConfirmandoEliminar(null)}
                                className={CLASE_BOTON}
                            >
                                {t('comun.boton.no')}
                            </button>
                        </>
                    ) : (
                        <button
                            type="button"
                            onClick={() => setConfirmandoEliminar(nodo.permisoId)}
                            className={CLASE_BOTON_PELIGRO}
                        >
                            {t('comun.boton.eliminar')}
                        </button>
                    ))}

                {agregandoEn === nodo.permisoId && (
                    <div className="flex w-full flex-wrap items-end justify-end gap-2">
                        <div className="w-full sm:w-72">
                            <CampoSelect
                                etiqueta={t('admin.permisos.elegirHijo')}
                                identificador={`hijo-${nodo.permisoId}`}
                                value={hijoElegido}
                                onChange={(evento) => setHijoElegido(evento.target.value)}
                            >
                                <option value="">{t('admin.permisos.elegirHijo')}</option>
                                {todos
                                    .filter(
                                        (permiso) =>
                                            permiso.permisoId !== nodo.permisoId &&
                                            !nodo.hijos.some((hijo) => hijo.permisoId === permiso.permisoId),
                                    )
                                    .map((permiso) => (
                                        <option key={permiso.permisoId} value={permiso.permisoId}>
                                            {permiso.nombre}
                                        </option>
                                    ))}
                            </CampoSelect>
                        </div>
                        <Boton
                            onClick={() => void agregarHijo(nodo)}
                            disabled={hijoElegido === ''}
                        >
                            {t('admin.permisos.confirmarAgregar')}
                        </Boton>
                    </div>
                )}
            </>
        );
    }

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.permisos.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.permisos.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.permisos.formulario.nueva')
                            : t('admin.permisos.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombreFamilia"
                                required
                                maxLength={80}
                                placeholder="Familia.GestionCursos"
                                value={formulario.nombre}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nombre: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.descripcion')}
                                identificador="descripcionFamilia"
                                maxLength={300}
                                value={formulario.descripcion}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, descripcion: evento.target.value })
                                }
                            />
                        </div>

                        <p className="ayuda">{t('admin.permisos.formulario.ayuda')}</p>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.permisos.formulario.crear')
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
                <h2 className="text-lg font-semibold text-texto">{t('admin.permisos.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.permisos.cargando')}</p>
                    ) : (
                        <ArbolPermisos
                            permisos={arbol}
                            acciones={acciones}
                            mensajeVacio={t('admin.permisos.vacio')}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
