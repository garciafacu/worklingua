import { type FormEvent, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { departamentosApi, empresasApi } from '../../../api/empresasApi';
import { usuariosApi } from '../../../api/usuariosApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { BotonAccion, BotonesFormulario } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { ImportadorPadron } from '../../../componentes/ImportadorPadron';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import { PERMISOS } from '../../../rutas/itemsMenu';
import { estaBloqueada } from '../../../servicios/cuenta';
import type { DepartamentoResponse } from '../../../tipos/empresas';
import type {
    EmpresaResponse,
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

function claseModo(activo: boolean): string {
    const base = 'rounded-lg border px-4 py-2 text-sm font-semibold';

    return activo
        ? `${base} border-primario bg-info-fondo text-primario`
        : `${base} border-borde bg-superficie text-texto hover:bg-fondo`;
}

function estaPendiente(usuario: UsuarioAdminResponse): boolean {
    return usuario.ultimoAcceso === null;
}

export function AdminUsuarios() {
    const { t } = useTranslation();
    const { usuario: usuarioEnSesion, tienePermiso } = useSesion();
    const { formatearFecha } = useLocalizacion();

    const mensajeDeError = useMensajeDeError();

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
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);
    const [modoAlta, setModoAlta] = useState<'individual' | 'padron'>('individual');

    const puedeInvitar = tienePermiso('Usuario.Invitar');
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

            <AlertasResultado error={error} exito={exito} />

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.usuarios.formulario.invitar')
                            : t('admin.usuarios.formulario.editar')}
                    </h2>

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
                            {alcanceGlobal && (
                                <CampoSelect
                                    etiqueta={t('comun.campo.empresa')}
                                    identificador="empresaId"
                                    required
                                    value={formulario.empresaId}
                                    onChange={(evento) =>
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

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.usuarios.formulario.enviar')}
                            onCancelar={limpiar}
                        />
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
                                        <BotonAccion variante="primario" onClick={() => void reinvitar(fila)}>
                                            {t('admin.usuarios.reenviar')}
                                        </BotonAccion>
                                    )}

                                    {puedeModificar && estaBloqueada(fila) && (
                                        <BotonAccion
                                            variante="primario"
                                            onClick={() => void desbloquear(fila)}
                                        >
                                            {t('admin.usuarios.desbloquear')}
                                        </BotonAccion>
                                    )}

                                    {puedeModificar && (
                                        <BotonAccion onClick={() => editar(fila)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {fila.usuarioId === usuarioEnSesion?.usuarioId ? (
                                        <span className="px-3 py-1.5 text-sm text-texto-suave">
                                            {t('admin.usuarios.sosVos')}
                                        </span>
                                    ) : (
                                        puedeDarDeBaja &&
                                        (confirmandoBaja === fila.usuarioId ? (
                                            <>
                                                <BotonAccion
                                                    variante="confirmar"
                                                    onClick={() => void darDeBaja(fila)}
                                                >
                                                    {t('comun.boton.confirmarBaja')}
                                                </BotonAccion>
                                                <BotonAccion onClick={() => setConfirmandoBaja(null)}>
                                                    {t('comun.boton.no')}
                                                </BotonAccion>
                                            </>
                                        ) : (
                                            <BotonAccion
                                                variante="peligro"
                                                onClick={() => setConfirmandoBaja(fila.usuarioId)}
                                            >
                                                {t('comun.boton.desactivar')}
                                            </BotonAccion>
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
