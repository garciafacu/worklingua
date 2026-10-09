import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { departamentosApi } from '../../../api/departamentosApi';
import { empresasApi } from '../../../api/empresasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { EmpresaResponse } from '../../../tipos/autenticacion';
import type {
    DepartamentoAdminResponse,
    GuardarDepartamentoRequest,
} from '../../../tipos/departamentos';

const FORMULARIO_VACIO = {
    empresaId: '',
    nombre: '',
    descripcion: '',
};

type Formulario = typeof FORMULARIO_VACIO;

/**
 * Organigrama departamental (CU-001-009).
 *
 * Es la misma pantalla para los dos alcances: con
 * Departamento.VerTodasLasEmpresas se ven los departamentos de todas las
 * empresas y hay que elegir a cuál pertenece cada uno; sin ese permiso el
 * backend acota todo a la empresa del usuario y el selector sobra.
 *
 * Como en el ABM de Idiomas, acá sí se muestran los dados de baja: dar de baja
 * un departamento no borra nada y tiene que poder reactivarse.
 *
 * Un departamento con empleados asignados no se puede dar de baja; el backend
 * responde 409 y el mensaje se muestra tal cual.
 */
export function AdminDepartamentos() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const [departamentos, setDepartamentos] = useState<DepartamentoAdminResponse[]>([]);
    const [empresas, setEmpresas] = useState<EmpresaResponse[]>([]);
    const [busqueda, setBusqueda] = useState('');
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Fuerza releer el listado después de guardar, activar o dar de baja. */
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso(PERMISOS.departamentoAlta);
    const puedeModificar = tienePermiso(PERMISOS.departamentoModificar);
    const puedeDarDeBaja = tienePermiso(PERMISOS.departamentoBaja);
    const alcanceTotal = tienePermiso(PERMISOS.departamentoVerTodasLasEmpresas);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        departamentosApi
            .listar()
            .then(setDepartamentos)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.departamentos.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    // Sin alcance global la empresa no se elige: el backend impone la propia.
    useEffect(() => {
        if (!alcanceTotal) {
            return;
        }

        empresasApi.listar().then(setEmpresas).catch(() => setEmpresas([]));
    }, [alcanceTotal]);

    // El filtro es en cliente a propósito: un organigrama son decenas de filas
    // y ya están todas en memoria.
    const visibles = useMemo(() => {
        const termino = busqueda.trim().toLowerCase();

        if (termino === '') {
            return departamentos;
        }

        return departamentos.filter(
            (departamento) =>
                departamento.nombre.toLowerCase().includes(termino) ||
                (departamento.descripcion ?? '').toLowerCase().includes(termino) ||
                departamento.empresa.toLowerCase().includes(termino),
        );
    }, [departamentos, busqueda]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(departamento: DepartamentoAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(departamento.departamentoId);
        setFormulario({
            empresaId: String(departamento.empresaId),
            nombre: departamento.nombre,
            descripcion: departamento.descripcion ?? '',
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const descripcion = formulario.descripcion.trim();

        const cuerpo: GuardarDepartamentoRequest = {
            empresaId: formulario.empresaId === '' ? 0 : Number(formulario.empresaId),
            nombre: formulario.nombre.trim(),
            descripcion: descripcion === '' ? null : descripcion,
        };

        try {
            if (editandoId === null) {
                await departamentosApi.crear(cuerpo);
                setExito(t('admin.departamentos.exitoAlta', { nombre: cuerpo.nombre }));
            } else {
                await departamentosApi.modificar(editandoId, cuerpo);
                setExito(t('admin.departamentos.exitoModificacion', { nombre: cuerpo.nombre }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.departamentos.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function cambiarEstado(departamento: DepartamentoAdminResponse) {
        setError(null);
        setExito(null);

        try {
            await departamentosApi.cambiarEstado(departamento.departamentoId, !departamento.activo);
            setExito(
                departamento.activo
                    ? t('admin.departamentos.exitoDesactivar', { nombre: departamento.nombre })
                    : t('admin.departamentos.exitoActivar', { nombre: departamento.nombre }),
            );

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.departamentos.errorEstado'));
        }
    }

    async function darDeBaja(departamento: DepartamentoAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await departamentosApi.baja(departamento.departamentoId);
            setExito(t('admin.departamentos.exitoBaja', { nombre: departamento.nombre }));

            if (editandoId === departamento.departamentoId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.departamentos.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<DepartamentoAdminResponse>[] = [
        { encabezado: t('comun.campo.nombre'), celda: (fila) => fila.nombre },
        ...(alcanceTotal
            ? [
                  {
                      encabezado: t('comun.campo.empresa'),
                      celda: (fila: DepartamentoAdminResponse) => fila.empresa,
                  },
              ]
            : []),
        {
            encabezado: t('comun.campo.descripcion'),
            celda: (fila) => fila.descripcion ?? '—',
        },
        {
            encabezado: t('admin.departamentos.col.empleados'),
            numerica: true,
            celda: (fila) => fila.empleados,
        },
        {
            encabezado: t('comun.campo.estado'),
            celda: (fila) => (
                <span
                    className={
                        fila.activo
                            ? 'rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario'
                            : 'rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave'
                    }
                >
                    {fila.activo ? t('comun.estado.activo') : t('comun.estado.inactivo')}
                </span>
            ),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {alcanceTotal
                    ? t('admin.departamentos.titulo')
                    : t('admin.departamentos.miEmpresa.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.departamentos.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.departamentos.formulario.nuevo')
                            : t('admin.departamentos.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        {/* La empresa solo se elige al crear: un departamento no se
                            puede mover de empresa sin arrastrar a sus empleados. */}
                        {alcanceTotal && editandoId === null && (
                            <>
                                <CampoSelect
                                    etiqueta={t('comun.campo.empresa')}
                                    identificador="departamentoEmpresa"
                                    required
                                    value={formulario.empresaId}
                                    onChange={(evento) =>
                                        setFormulario({
                                            ...formulario,
                                            empresaId: evento.target.value,
                                        })
                                    }
                                >
                                    <option value="">{t('comun.campo.empresa')}</option>
                                    {empresas.map((empresa) => (
                                        <option key={empresa.empresaId} value={empresa.empresaId}>
                                            {empresa.razonSocial}
                                        </option>
                                    ))}
                                </CampoSelect>

                                <p className="ayuda">
                                    {t('admin.departamentos.formulario.ayudaEmpresa')}
                                </p>
                            </>
                        )}

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="departamentoNombre"
                                required
                                maxLength={100}
                                value={formulario.nombre}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nombre: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.descripcion')}
                                identificador="departamentoDescripcion"
                                maxLength={250}
                                value={formulario.descripcion}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        descripcion: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.departamentos.formulario.crear')
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
                <div className="flex flex-wrap items-end justify-between gap-4">
                    <h2 className="text-lg font-semibold text-texto">
                        {t('admin.departamentos.listado')}
                    </h2>

                    <div className="w-full sm:w-72">
                        <CampoTexto
                            etiqueta={t('admin.departamentos.buscar')}
                            identificador="busquedaDepartamento"
                            type="search"
                            placeholder={t('admin.departamentos.buscarPlaceholder')}
                            value={busqueda}
                            onChange={(evento) => setBusqueda(evento.target.value)}
                        />
                    </div>
                </div>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">
                            {t('admin.departamentos.cargando')}
                        </p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={visibles}
                            claveDe={(fila) => fila.departamentoId}
                            inactiva={(fila) => !fila.activo}
                            mensajeVacio={
                                busqueda.trim() === ''
                                    ? t('admin.departamentos.vacio')
                                    : t('admin.departamentos.sinCoincidencias')
                            }
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    {puedeModificar && (
                                        <button
                                            type="button"
                                            onClick={() => editar(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                        >
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {puedeModificar && (
                                        <button
                                            type="button"
                                            onClick={() => void cambiarEstado(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-info-fondo"
                                        >
                                            {fila.activo
                                                ? t('comun.boton.desactivar')
                                                : t('comun.boton.activar')}
                                        </button>
                                    )}

                                    {puedeDarDeBaja &&
                                        fila.activo &&
                                        (confirmandoBaja === fila.departamentoId ? (
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
                                                onClick={() =>
                                                    setConfirmandoBaja(fila.departamentoId)
                                                }
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
