import { type FormEvent, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { rolesApi } from '../../../api/permisosApi';
import { operadoresApi } from '../../../api/usuariosApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { Boton, BotonAccion, BotonesFormulario } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import { estaBloqueada } from '../../../servicios/cuenta';
import type {
    GuardarOperadorRequest,
    RolResponse,
    UsuarioAdminResponse,
} from '../../../tipos/usuarios';

const FORMULARIO_VACIO = {
    nombre: '',
    apellido: '',
    email: '',
    documento: '',
};

type Formulario = typeof FORMULARIO_VACIO;

const CLASE_BOTON =
    'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo';
const CLASE_BOTON_PELIGRO =
    'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo';

function opcional(valor: string): string | null {
    return valor.trim() || null;
}

export function AdminOperadores() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearFecha } = useLocalizacion();

    const [operadores, setOperadores] = useState<UsuarioAdminResponse[]>([]);
    const [catalogoRoles, setCatalogoRoles] = useState<RolResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [seleccionado, setSeleccionado] = useState<UsuarioAdminResponse | null>(null);
    const [rolesOperador, setRolesOperador] = useState<RolResponse[]>([]);
    const [rolElegido, setRolElegido] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [cargandoRoles, setCargandoRoles] = useState(false);
    const [recarga, setRecarga] = useState(0);
    const [recargaRoles, setRecargaRoles] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Operador.Alta');
    const puedeModificar = tienePermiso('Operador.Modificar');
    const puedeDarDeBaja = tienePermiso('Operador.Baja');
    const puedeVerRoles = tienePermiso('Rol.Listar');

    const mensajeDeError = useMensajeDeError();

    useEffect(() => {
        operadoresApi
            .listar()
            .then(setOperadores)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.operadores.errorCargar')))
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    useEffect(() => {
        if (!puedeVerRoles) {
            return;
        }

        rolesApi
            .listarAdministracion()
            .then(setCatalogoRoles)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.roles.errorCargar')));
    }, [puedeVerRoles, mensajeDeError]);

    const seleccionadoId = seleccionado?.usuarioId ?? null;

    useEffect(() => {
        if (seleccionadoId === null) {
            return;
        }

        operadoresApi
            .listarRoles(seleccionadoId)
            .then(setRolesOperador)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.operadores.errorRoles')))
            .finally(() => setCargandoRoles(false));
    }, [seleccionadoId, recargaRoles, mensajeDeError]);

    const rolesAsignables = useMemo(
        () => catalogoRoles.filter((rol) => !rolesOperador.some((asignado) => asignado.rolId === rol.rolId)),
        [catalogoRoles, rolesOperador],
    );

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function recargarRoles() {
        setCargandoRoles(true);
        setRecargaRoles((numero) => numero + 1);
    }

    function limpiarMensajes() {
        setError(null);
        setExito(null);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(operador: UsuarioAdminResponse) {
        limpiarMensajes();
        setConfirmandoBaja(null);
        setEditandoId(operador.usuarioId);
        setFormulario({
            nombre: operador.nombre,
            apellido: operador.apellido,
            email: operador.email,
            documento: operador.documento ?? '',
        });
    }

    function verRoles(operador: UsuarioAdminResponse) {
        setRolElegido('');
        setRolesOperador([]);
        setCargandoRoles(true);
        setSeleccionado(operador);
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        limpiarMensajes();
        setGuardando(true);

        const cuerpo: GuardarOperadorRequest = {
            nombre: formulario.nombre.trim(),
            apellido: formulario.apellido.trim(),
            documento: opcional(formulario.documento),
            email: formulario.email.trim().toLowerCase(),
        };

        try {
            if (editandoId === null) {
                const creado = await operadoresApi.crear(cuerpo);
                setExito(t('admin.operadores.exitoAlta', { email: creado.email }));
                verRoles(creado);
            } else {
                const modificado = await operadoresApi.modificar(editandoId, cuerpo);
                setExito(t('admin.operadores.exitoModificacion', { email: modificado.email }));

                if (seleccionado?.usuarioId === editandoId) {
                    setSeleccionado(modificado);
                }
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.operadores.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function reinvitar(operador: UsuarioAdminResponse) {
        limpiarMensajes();

        try {
            await operadoresApi.reinvitar(operador.usuarioId);
            setExito(t('admin.operadores.exitoReinvitar', { email: operador.email }));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.operadores.errorReinvitar'));
        }
    }

    async function desbloquear(operador: UsuarioAdminResponse) {
        limpiarMensajes();

        try {
            await operadoresApi.desbloquear(operador.usuarioId);
            setExito(t('admin.operadores.exitoDesbloquear', { email: operador.email }));

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.operadores.errorDesbloquear'));
        }
    }

    async function darDeBaja(operador: UsuarioAdminResponse) {
        limpiarMensajes();
        setConfirmandoBaja(null);

        try {
            await operadoresApi.baja(operador.usuarioId);
            setExito(t('admin.operadores.exitoBaja', { email: operador.email }));

            if (editandoId === operador.usuarioId) {
                limpiar();
            }

            if (seleccionado?.usuarioId === operador.usuarioId) {
                setSeleccionado(null);
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.operadores.errorBaja'));
        }
    }

    async function asignarRol(operador: UsuarioAdminResponse) {
        const rol = rolesAsignables.find((opcion) => opcion.rolId === Number(rolElegido));

        if (!rol) {
            return;
        }

        limpiarMensajes();

        try {
            await operadoresApi.asignarRol(operador.usuarioId, rol.rolId);
            setExito(t('admin.operadores.exitoAsignarRol', { rol: rol.nombre, email: operador.email }));
            setRolElegido('');
            recargarRoles();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.operadores.errorAsignarRol'));
        }
    }

    async function quitarRol(operador: UsuarioAdminResponse, rol: RolResponse) {
        limpiarMensajes();

        try {
            await operadoresApi.quitarRol(operador.usuarioId, rol.rolId);
            setExito(t('admin.operadores.exitoQuitarRol', { rol: rol.nombre, email: operador.email }));
            recargarRoles();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.operadores.errorQuitarRol'));
        }
    }

    const columnas: ColumnaAbm<UsuarioAdminResponse>[] = [
        { encabezado: t('comun.campo.nombre'), celda: (fila) => `${fila.apellido}, ${fila.nombre}` },
        { encabezado: t('comun.campo.email'), celda: (fila) => fila.email },
        {
            encabezado: t('admin.operadores.tabla.roles'),
            celda: (fila) => fila.roles.join(', ') || t('admin.operadores.sinRoles'),
        },
        {
            encabezado: t('comun.campo.estado'),
            celda: (fila) =>
                estaBloqueada(fila) ? (
                    <span className="rounded-full bg-error-fondo px-2.5 py-0.5 text-xs font-semibold text-error">
                        {t('admin.usuarios.bloqueada')}
                    </span>
                ) : fila.activo ? (
                    t('comun.estado.activo')
                ) : (
                    t('admin.operadores.invitacionPendiente')
                ),
        },
        {
            encabezado: t('admin.operadores.tabla.ultimoAcceso'),
            celda: (fila) => formatearFecha(fila.ultimoAcceso),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.operadores.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.operadores.descripcion')}</p>

            <AlertasResultado error={error} exito={exito} />

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.operadores.formulario.nuevo')
                            : t('admin.operadores.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombreOperador"
                                required
                                maxLength={80}
                                value={formulario.nombre}
                                onChange={(evento) => setFormulario({ ...formulario, nombre: evento.target.value })}
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.apellido')}
                                identificador="apellidoOperador"
                                required
                                maxLength={80}
                                value={formulario.apellido}
                                onChange={(evento) => setFormulario({ ...formulario, apellido: evento.target.value })}
                            />
                        </div>

                        <CampoTexto
                            etiqueta={t('comun.campo.email')}
                            identificador="emailOperador"
                            type="email"
                            required
                            maxLength={150}
                            value={formulario.email}
                            onChange={(evento) => setFormulario({ ...formulario, email: evento.target.value })}
                        />

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.operadores.formulario.documento')}
                                identificador="documentoOperador"
                                maxLength={20}
                                value={formulario.documento}
                                onChange={(evento) => setFormulario({ ...formulario, documento: evento.target.value })}
                            />
                        </div>

                        {editandoId === null && <p className="ayuda">{t('admin.operadores.formulario.ayudaAlta')}</p>}

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.operadores.formulario.crear')}
                            onCancelar={limpiar}
                        />
                    </form>
                </section>
            )}

            <section className="mt-6">
                <h2 className="text-lg font-semibold text-texto">{t('admin.operadores.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.operadores.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={operadores}
                            claveDe={(fila) => fila.usuarioId}
                            inactiva={(fila) => !fila.activo}
                            mensajeVacio={t('admin.operadores.vacio')}
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    <BotonAccion
                                        variante="primario"
                                        onClick={() => {
                                            limpiarMensajes();
                                            verRoles(fila);
                                        }}
                                    >
                                        {t('admin.operadores.roles')}
                                    </BotonAccion>

                                    {puedeModificar && (
                                        <button type="button" onClick={() => editar(fila)} className={CLASE_BOTON}>
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {puedeModificar && estaBloqueada(fila) && (
                                        <button type="button" onClick={() => void desbloquear(fila)} className={CLASE_BOTON}>
                                            {t('admin.operadores.desbloquear')}
                                        </button>
                                    )}

                                    {puedeCrear && !fila.activo && (
                                        <button type="button" onClick={() => void reinvitar(fila)} className={CLASE_BOTON}>
                                            {t('admin.operadores.reinvitar')}
                                        </button>
                                    )}

                                    {puedeDarDeBaja &&
                                        fila.activo &&
                                        (confirmandoBaja === fila.usuarioId ? (
                                            <>
                                                <BotonAccion
                                                    variante="confirmar"
                                                    onClick={() => void darDeBaja(fila)}
                                                >
                                                    {t('comun.boton.confirmarBaja')}
                                                </BotonAccion>
                                                <button
                                                    type="button"
                                                    onClick={() => setConfirmandoBaja(null)}
                                                    className={CLASE_BOTON}
                                                >
                                                    {t('comun.boton.no')}
                                                </button>
                                            </>
                                        ) : (
                                            <button
                                                type="button"
                                                onClick={() => setConfirmandoBaja(fila.usuarioId)}
                                                className={CLASE_BOTON_PELIGRO}
                                            >
                                                {t('comun.boton.desactivar')}
                                            </button>
                                        ))}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>

            {seleccionado && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                        <div>
                            <h2 className="text-lg font-semibold text-texto">
                                {t('admin.operadores.rolesDe', { email: seleccionado.email })}
                            </h2>
                            <p className="mt-1 text-sm text-texto-suave">{t('admin.operadores.rolesAyuda')}</p>
                        </div>

                        <button type="button" onClick={() => setSeleccionado(null)} className={CLASE_BOTON}>
                            {t('comun.boton.cerrar')}
                        </button>
                    </div>

                    {puedeModificar && puedeVerRoles && (
                        <div className="mt-4 flex flex-wrap items-end gap-3">
                            <div className="w-full sm:w-80">
                                <CampoSelect
                                    etiqueta={t('admin.operadores.elegirRol')}
                                    identificador="rolAsignar"
                                    value={rolElegido}
                                    onChange={(evento) => setRolElegido(evento.target.value)}
                                >
                                    <option value="">{t('admin.operadores.elegirRol')}</option>
                                    {rolesAsignables.map((rol) => (
                                        <option key={rol.rolId} value={rol.rolId}>
                                            {rol.nombre}
                                        </option>
                                    ))}
                                </CampoSelect>
                            </div>

                            <Boton onClick={() => void asignarRol(seleccionado)} disabled={rolElegido === ''}>
                                {t('admin.operadores.asignarRol')}
                            </Boton>
                        </div>
                    )}

                    <div className="mt-4">
                        {cargandoRoles ? (
                            <p className="text-sm text-texto-suave">{t('admin.operadores.cargandoRoles')}</p>
                        ) : rolesOperador.length === 0 ? (
                            <p className="text-sm text-texto-suave">{t('admin.operadores.sinRolesAsignados')}</p>
                        ) : (
                            <ul className="divide-y divide-borde rounded-2xl border border-borde bg-superficie">
                                {rolesOperador.map((rol) => (
                                    <li key={rol.rolId} className="flex flex-wrap items-center justify-between gap-2 px-4 py-2">
                                        <div className="min-w-0">
                                            <p className="text-sm font-semibold text-texto">{rol.nombre}</p>
                                            {rol.descripcion && (
                                                <p className="text-xs text-texto-suave">{rol.descripcion}</p>
                                            )}
                                        </div>

                                        {puedeModificar && (
                                            <button
                                                type="button"
                                                onClick={() => void quitarRol(seleccionado, rol)}
                                                className={CLASE_BOTON_PELIGRO}
                                            >
                                                {t('comun.boton.quitar')}
                                            </button>
                                        )}
                                    </li>
                                ))}
                            </ul>
                        )}
                    </div>
                </section>
            )}
        </div>
    );
}
