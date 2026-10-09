import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { permisosApi } from '../../../api/permisosApi';
import { rolesApi } from '../../../api/rolesApi';
import { Alerta } from '../../../componentes/Alerta';
import { ArbolPermisos } from '../../../componentes/ArbolPermisos';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import { aplanarPermisos, type PermisoResponse } from '../../../tipos/permisos';
import type { GuardarRolRequest } from '../../../tipos/roles';
import type { RolResponse } from '../../../tipos/usuarios';

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
 * ABM de roles y asignación de permisos.
 *
 * A un rol se le asignan patentes o familias; lo que se ve debajo de cada una
 * es lo que el rol recibe por contenerla. Solo se pueden quitar las
 * asignaciones directas (las raíces del árbol del rol): para sacar algo que
 * llega por una familia hay que editar la familia en el ABM de Permisos.
 */
export function AdminRoles() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const [roles, setRoles] = useState<RolResponse[]>([]);
    const [catalogo, setCatalogo] = useState<PermisoResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoEliminar, setConfirmandoEliminar] = useState<number | null>(null);
    const [rolSeleccionado, setRolSeleccionado] = useState<RolResponse | null>(null);
    const [permisosRol, setPermisosRol] = useState<PermisoResponse[]>([]);
    const [permisoElegido, setPermisoElegido] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [cargandoPermisos, setCargandoPermisos] = useState(false);
    const [recarga, setRecarga] = useState(0);
    const [recargaPermisos, setRecargaPermisos] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Rol.Alta');
    const puedeModificar = tienePermiso('Rol.Modificar');
    const puedeEliminar = tienePermiso('Rol.Baja');
    const puedeVerCatalogo = tienePermiso('Permiso.Listar');

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        rolesApi
            .listarAdministracion()
            .then(setRoles)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.roles.errorCargar')))
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    useEffect(() => {
        if (!puedeVerCatalogo) {
            return;
        }

        permisosApi
            .listar()
            .then(setCatalogo)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.permisos.errorCargar')));
    }, [puedeVerCatalogo, mensajeDeError]);

    const rolSeleccionadoId = rolSeleccionado?.rolId ?? null;

    useEffect(() => {
        if (rolSeleccionadoId === null) {
            return;
        }

        rolesApi
            .listarPermisos(rolSeleccionadoId)
            .then(setPermisosRol)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.roles.errorPermisos')))
            .finally(() => setCargandoPermisos(false));
    }, [rolSeleccionadoId, recargaPermisos, mensajeDeError]);

    const asignables = useMemo(
        () =>
            aplanarPermisos(catalogo).filter(
                (permiso) => !permisosRol.some((asignado) => asignado.permisoId === permiso.permisoId),
            ),
        [catalogo, permisosRol],
    );

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function recargarPermisos() {
        setCargandoPermisos(true);
        setRecargaPermisos((numero) => numero + 1);
    }

    function limpiarMensajes() {
        setError(null);
        setExito(null);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(rol: RolResponse) {
        limpiarMensajes();
        setConfirmandoEliminar(null);
        setEditandoId(rol.rolId);
        setFormulario({ nombre: rol.nombre, descripcion: rol.descripcion ?? '' });
    }

    function verPermisos(rol: RolResponse) {
        limpiarMensajes();
        setPermisoElegido('');
        setPermisosRol([]);
        setCargandoPermisos(true);
        setRolSeleccionado(rol);
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        limpiarMensajes();
        setGuardando(true);

        const cuerpo: GuardarRolRequest = {
            nombre: formulario.nombre.trim(),
            descripcion: formulario.descripcion.trim() === '' ? null : formulario.descripcion.trim(),
        };

        try {
            if (editandoId === null) {
                await rolesApi.crear(cuerpo);
                setExito(t('admin.roles.exitoAlta', { nombre: cuerpo.nombre }));
            } else {
                const modificado = await rolesApi.modificar(editandoId, cuerpo);
                setExito(t('admin.roles.exitoModificacion', { nombre: cuerpo.nombre }));

                if (rolSeleccionado?.rolId === editandoId) {
                    setRolSeleccionado(modificado);
                }
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.roles.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function eliminar(rol: RolResponse) {
        limpiarMensajes();
        setConfirmandoEliminar(null);

        try {
            await rolesApi.eliminar(rol.rolId);
            setExito(t('admin.roles.exitoEliminar', { nombre: rol.nombre }));

            if (editandoId === rol.rolId) {
                limpiar();
            }

            if (rolSeleccionado?.rolId === rol.rolId) {
                setRolSeleccionado(null);
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.roles.errorEliminar'));
        }
    }

    async function asignar(rol: RolResponse) {
        const permiso = asignables.find((opcion) => opcion.permisoId === Number(permisoElegido));

        if (!permiso) {
            return;
        }

        limpiarMensajes();

        try {
            await rolesApi.asignarPermiso(rol.rolId, permiso.permisoId);
            setExito(t('admin.roles.exitoAsignar', { permiso: permiso.nombre, rol: rol.nombre }));
            setPermisoElegido('');
            recargarPermisos();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.roles.errorAsignar'));
        }
    }

    async function quitar(rol: RolResponse, permiso: PermisoResponse) {
        limpiarMensajes();

        try {
            await rolesApi.quitarPermiso(rol.rolId, permiso.permisoId);
            setExito(t('admin.roles.exitoQuitar', { permiso: permiso.nombre, rol: rol.nombre }));
            recargarPermisos();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.roles.errorQuitar'));
        }
    }

    const columnas: ColumnaAbm<RolResponse>[] = [
        { encabezado: t('comun.campo.nombre'), celda: (fila) => fila.nombre },
        {
            encabezado: t('comun.campo.descripcion'),
            celda: (fila) => fila.descripcion ?? t('comun.valor.vacio'),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.roles.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.roles.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.roles.formulario.nuevo')
                            : t('admin.roles.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombreRol"
                                required
                                maxLength={60}
                                value={formulario.nombre}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nombre: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.descripcion')}
                                identificador="descripcionRol"
                                maxLength={250}
                                value={formulario.descripcion}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, descripcion: evento.target.value })
                                }
                            />
                        </div>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.roles.formulario.crear')
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
                <h2 className="text-lg font-semibold text-texto">{t('admin.roles.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.roles.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={roles}
                            claveDe={(fila) => fila.rolId}
                            mensajeVacio={t('admin.roles.vacio')}
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    <button
                                        type="button"
                                        onClick={() => verPermisos(fila)}
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-info-fondo"
                                    >
                                        {t('admin.roles.permisos')}
                                    </button>

                                    {puedeModificar && (
                                        <button type="button" onClick={() => editar(fila)} className={CLASE_BOTON}>
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {puedeEliminar &&
                                        (confirmandoEliminar === fila.rolId ? (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => void eliminar(fila)}
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
                                                onClick={() => setConfirmandoEliminar(fila.rolId)}
                                                className={CLASE_BOTON_PELIGRO}
                                            >
                                                {t('comun.boton.eliminar')}
                                            </button>
                                        ))}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>

            {rolSeleccionado && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                        <div>
                            <h2 className="text-lg font-semibold text-texto">
                                {t('admin.roles.permisosDe', { rol: rolSeleccionado.nombre })}
                            </h2>
                            <p className="mt-1 text-sm text-texto-suave">{t('admin.roles.permisosAyuda')}</p>
                        </div>

                        <button type="button" onClick={() => setRolSeleccionado(null)} className={CLASE_BOTON}>
                            {t('comun.boton.cerrar')}
                        </button>
                    </div>

                    {puedeModificar && puedeVerCatalogo && (
                        <div className="mt-4 flex flex-wrap items-end gap-3">
                            <div className="w-full sm:w-80">
                                <CampoSelect
                                    etiqueta={t('admin.roles.elegirPermiso')}
                                    identificador="permisoAsignar"
                                    value={permisoElegido}
                                    onChange={(evento) => setPermisoElegido(evento.target.value)}
                                >
                                    <option value="">{t('admin.roles.elegirPermiso')}</option>
                                    {asignables.map((permiso) => (
                                        <option key={permiso.permisoId} value={permiso.permisoId}>
                                            {permiso.nombre}
                                        </option>
                                    ))}
                                </CampoSelect>
                            </div>

                            <Boton onClick={() => void asignar(rolSeleccionado)} disabled={permisoElegido === ''}>
                                {t('admin.roles.asignar')}
                            </Boton>
                        </div>
                    )}

                    <div className="mt-4">
                        {cargandoPermisos ? (
                            <p className="text-sm text-texto-suave">{t('admin.roles.cargandoPermisos')}</p>
                        ) : (
                            <ArbolPermisos
                                permisos={permisosRol}
                                mensajeVacio={t('admin.roles.sinPermisos')}
                                acciones={
                                    puedeModificar
                                        ? (nodo, padre) =>
                                              padre === null && (
                                                  <button
                                                      type="button"
                                                      onClick={() => void quitar(rolSeleccionado, nodo)}
                                                      className={CLASE_BOTON_PELIGRO}
                                                  >
                                                      {t('comun.boton.quitar')}
                                                  </button>
                                              )
                                        : undefined
                                }
                            />
                        )}
                    </div>
                </section>
            )}
        </div>
    );
}
