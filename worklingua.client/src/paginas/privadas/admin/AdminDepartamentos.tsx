import { type FormEvent, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { departamentosApi, empresasApi } from '../../../api/empresasApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { BotonAccion, BotonesFormulario, ConfirmarBaja } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type {
    DepartamentoAdminResponse,
    GuardarDepartamentoRequest,
} from '../../../tipos/empresas';
import type { EmpresaResponse } from '../../../tipos/usuarios';

const FORMULARIO_VACIO = {
    empresaId: '',
    nombre: '',
    descripcion: '',
};

type Formulario = typeof FORMULARIO_VACIO;

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
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso(PERMISOS.departamentoAlta);
    const puedeModificar = tienePermiso(PERMISOS.departamentoModificar);
    const puedeDarDeBaja = tienePermiso(PERMISOS.departamentoBaja);
    const alcanceTotal = tienePermiso(PERMISOS.departamentoVerTodasLasEmpresas);

    const mensajeDeError = useMensajeDeError();

    useEffect(() => {
        departamentosApi
            .listar()
            .then(setDepartamentos)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.departamentos.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    useEffect(() => {
        if (!alcanceTotal) {
            return;
        }

        empresasApi.listar().then(setEmpresas).catch(() => setEmpresas([]));
    }, [alcanceTotal]);

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

            <AlertasResultado error={error} exito={exito} />

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.departamentos.formulario.nuevo')
                            : t('admin.departamentos.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
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

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.departamentos.formulario.crear')}
                            onCancelar={limpiar}
                        />
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
                                        <BotonAccion onClick={() => editar(fila)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {puedeModificar && (
                                        <BotonAccion
                                            variante="primario"
                                            onClick={() => void cambiarEstado(fila)}
                                        >
                                            {fila.activo
                                                ? t('comun.boton.desactivar')
                                                : t('comun.boton.activar')}
                                        </BotonAccion>
                                    )}

                                    {puedeDarDeBaja &&
                                        fila.activo &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === fila.departamentoId}
                                            onPedir={() =>
                                                    setConfirmandoBaja(fila.departamentoId)}
                                            onConfirmar={() => void darDeBaja(fila)}
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
