import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { departamentosApi } from '../../../api/departamentosApi';
import { empresasApi } from '../../../api/empresasApi';
import { usuariosApi } from '../../../api/usuariosApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { ImportadorPadron } from '../../../componentes/ImportadorPadron';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import { estaBloqueada } from '../../../servicios/cuenta';
import type { EmpresaResponse } from '../../../tipos/autenticacion';
import type { DepartamentoResponse } from '../../../tipos/departamentos';
import type {
    InvitarUsuarioRequest,
    RolResponse,
    UsuarioAdminResponse,
} from '../../../tipos/usuarios';

const FORMULARIO_VACIO = {
    nombre: '',
    apellido: '',
    email: '',
    documento: '',
    empresaId: '',
    departamentoId: '',
    rolId: '',
};

type Formulario = typeof FORMULARIO_VACIO;

function opcional(valor: string): string | null {
    return valor.trim() || null;
}

/** Una cuenta sin último acceso todavía no entró: la invitación sigue pendiente. */
/** Estilo del selector de forma de alta: la activa se ve hundida. */
function claseModo(activo: boolean): string {
    const base = 'rounded-lg border px-4 py-2 text-sm font-semibold';

    return activo
        ? `${base} border-primario bg-info-fondo text-primario`
        : `${base} border-borde bg-superficie text-texto hover:bg-fondo`;
}

function estaPendiente(usuario: UsuarioAdminResponse): boolean {
    return usuario.ultimoAcceso === null;
}

/**
 * ABM de usuarios.
 *
 * Solo empresas clientes: el personal de la plataforma se administra en
 * Operadores y el rol de plataforma no se ofrece acá.
 *
 * El Super Admin ve los de todas las empresas; un Administrador Empresa, solo los
 * de la suya. Quien acota de verdad es el backend, contra el permiso
 * Usuario.VerTodasLasEmpresas.
 *
 * El alta no crea una clave: manda una invitación por correo y la persona
 * define la suya desde el enlace. Hasta que lo haga, la cuenta queda inactiva y
 * el listado la muestra como pendiente, con la opción de reenviar el correo.
 */
export function AdminUsuarios() {
    const { t } = useTranslation();
    const { usuario: usuarioEnSesion, tienePermiso } = useSesion();
    // La fecha se formatea con la cultura activa, no con un locale fijo.
    const { formatearFecha } = useLocalizacion();

    // En useCallback para poder ir en las dependencias de los efectos que lo
    // usan: sin eso el linter avisa, y el mensaje de error quedaría en el idioma
    // anterior después de cambiar de idioma.
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    const [usuarios, setUsuarios] = useState<UsuarioAdminResponse[]>([]);
    const [empresas, setEmpresas] = useState<EmpresaResponse[]>([]);
    const [departamentos, setDepartamentos] = useState<DepartamentoResponse[]>([]);
    const [roles, setRoles] = useState<RolResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Fuerza releer el listado despues de invitar, modificar o desactivar. */
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);
    /** Cómo se da de alta: de a uno o importando un padrón. */
    const [modoAlta, setModoAlta] = useState<'individual' | 'padron'>('individual');

    const puedeInvitar = tienePermiso('Usuario.Invitar');
    /**
     * Un Administrador Empresa solo alcanza a su propia empresa: el backend acota
     * el listado y fuerza la empresa al invitar o modificar. Acá se ocultan los
     * controles que en ese caso no deciden nada.
     */
    const alcanceGlobal = tienePermiso(PERMISOS.usuarioVerTodasLasEmpresas);
    const puedeModificar = tienePermiso('Usuario.Modificar');
    const puedeDarDeBaja = tienePermiso('Usuario.Baja');

    useEffect(() => {
        Promise.all([empresasApi.listar(), usuariosApi.listarRoles()])
            .then(([listaEmpresas, listaRoles]) => {
                setEmpresas(listaEmpresas);
                setRoles(listaRoles);
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.usuarios.errorCatalogos')),
            );
    }, [mensajeDeError]);

    // El organigrama depende de la empresa elegida, así que se recarga cada vez
    // que cambia. Sin alcance global el backend ignora el id y devuelve los de
    // la empresa propia, que es la única posible.
    const empresaElegida = formulario.empresaId;

    useEffect(() => {
        departamentosApi
            .listarAsignables(empresaElegida === '' ? 0 : Number(empresaElegida))
            .then(setDepartamentos)
            .catch(() => setDepartamentos([]));
    }, [empresaElegida]);

    useEffect(() => {
        usuariosApi
            .listarAdministracion()
            .then(setUsuarios)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.usuarios.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(usuario: UsuarioAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setModoAlta('individual');
        setEditandoId(usuario.usuarioId);
        setFormulario({
            nombre: usuario.nombre,
            apellido: usuario.apellido,
            email: usuario.email,
            documento: usuario.documento ?? '',
            empresaId: String(usuario.empresaId),
            departamentoId: usuario.departamentoId === null ? '' : String(usuario.departamentoId),
            rolId: String(usuario.rolId),
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: InvitarUsuarioRequest = {
            empresaId: Number(formulario.empresaId),
            rolId: Number(formulario.rolId),
            nombre: formulario.nombre.trim(),
            apellido: formulario.apellido.trim(),
            documento: opcional(formulario.documento),
            email: formulario.email.trim().toLowerCase(),
            departamentoId:
                formulario.departamentoId === '' ? null : Number(formulario.departamentoId),
        };

        try {
            const respuesta =
                editandoId === null
                    ? await usuariosApi.invitar(cuerpo)
                    : await usuariosApi.modificar(editandoId, cuerpo);

            setExito(respuesta.mensaje);
            limpiar();
            recargar();
        } catch (excepcion) {
            setError(
                mensajeDeError(
                    excepcion,
                    editandoId === null
                        ? 'admin.usuarios.errorInvitar'
                        : 'admin.usuarios.errorGuardar',
                ),
            );
        } finally {
            setGuardando(false);
        }
    }

    async function reinvitar(usuario: UsuarioAdminResponse) {
        setError(null);
        setExito(null);

        try {
            await usuariosApi.reinvitar(usuario.usuarioId);
            setExito(t('admin.usuarios.exitoReinvitar', { email: usuario.email }));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.usuarios.errorReinvitar'));
        }
    }

    async function desbloquear(usuario: UsuarioAdminResponse) {
        setError(null);
        setExito(null);

        try {
            await usuariosApi.desbloquear(usuario.usuarioId);
            setExito(t('admin.usuarios.exitoDesbloquear', { email: usuario.email }));

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.usuarios.errorDesbloquear'));
        }
    }

    async function darDeBaja(usuario: UsuarioAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await usuariosApi.baja(usuario.usuarioId);
            setExito(t('admin.usuarios.exitoDesactivar', { email: usuario.email }));

            if (editandoId === usuario.usuarioId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.usuarios.errorDesactivar'));
        }
    }

    const columnas: ColumnaAbm<UsuarioAdminResponse>[] = [
        {
            encabezado: t('admin.cursos.tabla.nombre'),
            celda: (fila) => `${fila.apellido}, ${fila.nombre}`,
        },
        { encabezado: t('comun.campo.email'), celda: (fila) => fila.email },
        // Con alcance acotado la columna sería la misma empresa en todas las filas.
        ...(alcanceGlobal
            ? [
                  {
                      encabezado: t('comun.campo.empresa'),
                      celda: (fila: UsuarioAdminResponse) => fila.empresa,
                  },
              ]
            : []),
        {
            encabezado: t('admin.usuarios.col.departamento'),
            celda: (fila) => fila.departamento ?? t('comun.valor.vacio'),
        },
        {
            encabezado: t('comun.campo.rol'),
            celda: (fila) => fila.roles.join(', ') || t('comun.valor.vacio'),
        },
        {
            encabezado: t('comun.campo.estado'),
            celda: (fila) =>
                estaBloqueada(fila) ? (
                    <span className="rounded-full bg-error-fondo px-2.5 py-0.5 text-xs font-semibold text-error">
                        {t('admin.usuarios.bloqueada')}
                    </span>
                ) : estaPendiente(fila) ? (
                    t('admin.usuarios.invitacionPendiente')
                ) : (
                    t('comun.estado.activo')
                ),
        },
        {
            encabezado: t('admin.usuarios.tabla.ultimoAcceso'),
            celda: (fila) => formatearFecha(fila.ultimoAcceso),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeInvitar : puedeModificar;
    // Editar es siempre individual: el padrón solo da de alta.
    const mostrarPadron = modoAlta === 'padron' && editandoId === null;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.usuarios.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">
                {t('admin.usuarios.descripcion')}
                {!alcanceGlobal && ` ${t('admin.usuarios.deTuEmpresa')}`}
            </p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.usuarios.formulario.invitar')
                            : t('admin.usuarios.formulario.editar')}
                    </h2>

                    {/* Dar de alta tiene dos formas: de a uno o por padrón. Al
                        editar no hay elección, así que el selector no aparece. */}
                    {editandoId === null && (
                        <div className="mt-4 flex flex-wrap gap-2">
                            <button
                                type="button"
                                aria-pressed={modoAlta === 'individual'}
                                onClick={() => setModoAlta('individual')}
                                className={claseModo(modoAlta === 'individual')}
                            >
                                {t('admin.usuarios.invitarIndividual')}
                            </button>
                            <button
                                type="button"
                                aria-pressed={modoAlta === 'padron'}
                                onClick={() => setModoAlta('padron')}
                                className={claseModo(modoAlta === 'padron')}
                            >
                                {t('admin.usuarios.importarPadron')}
                            </button>
                        </div>
                    )}

                    {mostrarPadron && <ImportadorPadron alImportar={recargar} />}

                    {!mostrarPadron && (
                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombre"
                                required
                                maxLength={80}
                                value={formulario.nombre}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nombre: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.apellido')}
                                identificador="apellido"
                                required
                                maxLength={80}
                                value={formulario.apellido}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, apellido: evento.target.value })
                                }
                            />
                        </div>

                        <CampoTexto
                            etiqueta={t('comun.campo.email')}
                            identificador="email"
                            type="email"
                            required
                            maxLength={150}
                            value={formulario.email}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, email: evento.target.value })
                            }
                        />

                        <div className="fila">
                            {/* Sin alcance global la empresa no se elige: el backend
                                fuerza la propia, así que el campo solo confundiría. */}
                            {alcanceGlobal && (
                                <CampoSelect
                                    etiqueta={t('comun.campo.empresa')}
                                    identificador="empresaId"
                                    required
                                    value={formulario.empresaId}
                                    onChange={(evento) =>
                                        // Cambiar de empresa invalida el departamento
                                        // elegido: pertenece al organigrama anterior.
                                        setFormulario({
                                            ...formulario,
                                            empresaId: evento.target.value,
                                            departamentoId: '',
                                        })
                                    }
                                >
                                    <option value="">{t('auth.registro.elegirEmpresa')}</option>
                                    {empresas.map((empresa) => (
                                        <option key={empresa.empresaId} value={empresa.empresaId}>
                                            {empresa.razonSocial}
                                        </option>
                                    ))}
                                </CampoSelect>
                            )}

                            <CampoSelect
                                etiqueta={t('comun.campo.rol')}
                                identificador="rolId"
                                required
                                value={formulario.rolId}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, rolId: evento.target.value })
                                }
                            >
                                <option value="">{t('admin.usuarios.formulario.elegirRol')}</option>
                                {roles.map((rol) => (
                                    <option key={rol.rolId} value={rol.rolId}>
                                        {rol.nombre}
                                    </option>
                                ))}
                            </CampoSelect>

                            {/* El departamento es opcional y sale del organigrama de
                                la empresa elegida (CU-001-009). */}
                            <CampoSelect
                                etiqueta={t('admin.usuarios.formulario.departamento')}
                                identificador="departamentoId"
                                value={formulario.departamentoId}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        departamentoId: evento.target.value,
                                    })
                                }
                            >
                                <option value="">
                                    {t('admin.usuarios.formulario.sinDepartamento')}
                                </option>
                                {departamentos.map((departamento) => (
                                    <option
                                        key={departamento.departamentoId}
                                        value={departamento.departamentoId}
                                    >
                                        {departamento.nombre}
                                    </option>
                                ))}
                            </CampoSelect>
                        </div>

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('auth.registro.campo.documento')}
                                identificador="documento"
                                maxLength={20}
                                value={formulario.documento}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, documento: evento.target.value })
                                }
                            />
                        </div>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.usuarios.formulario.enviar')
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
                    )}
                </section>
            )}

            <section className="mt-6">
                <h2 className="text-lg font-semibold text-texto">{t('admin.usuarios.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.usuarios.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={usuarios}
                            claveDe={(fila) => fila.usuarioId}
                            inactiva={(fila) => estaPendiente(fila)}
                            mensajeVacio={t('admin.usuarios.vacio')}
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    {puedeInvitar && estaPendiente(fila) && (
                                        <button
                                            type="button"
                                            onClick={() => void reinvitar(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-info-fondo"
                                        >
                                            {t('admin.usuarios.reenviar')}
                                        </button>
                                    )}

                                    {/* Desbloquear no es destructivo, así que no
                                        pide confirmación (CU-003-004, CA4). */}
                                    {puedeModificar && estaBloqueada(fila) && (
                                        <button
                                            type="button"
                                            onClick={() => void desbloquear(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-info-fondo"
                                        >
                                            {t('admin.usuarios.desbloquear')}
                                        </button>
                                    )}

                                    {puedeModificar && (
                                        <button
                                            type="button"
                                            onClick={() => editar(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                        >
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {fila.usuarioId === usuarioEnSesion?.usuarioId ? (
                                        <span className="px-3 py-1.5 text-sm text-texto-suave">
                                            {t('admin.usuarios.sosVos')}
                                        </span>
                                    ) : (
                                        puedeDarDeBaja &&
                                        (confirmandoBaja === fila.usuarioId ? (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => void darDeBaja(fila)}
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
                                                onClick={() => setConfirmandoBaja(fila.usuarioId)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                            >
                                                {t('comun.boton.desactivar')}
                                            </button>
                                        ))
                                    )}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
