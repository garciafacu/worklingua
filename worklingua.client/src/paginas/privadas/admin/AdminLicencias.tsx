import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { empresasApi } from '../../../api/empresasApi';
import { licenciasApi } from '../../../api/licenciasApi';
import { Alerta } from '../../../componentes/Alerta';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { EmpresaResponse } from '../../../tipos/autenticacion';
import type { InventarioLicenciaResponse, InventarioResponse } from '../../../tipos/licencias';

interface CifraProps {
    titulo: string;
    valor: ReactNode;
    destacada?: boolean;
}

function Cifra({ titulo, valor, destacada }: CifraProps) {
    return (
        <div className="rounded-2xl border border-borde bg-superficie p-6">
            <p className="m-0 text-sm font-semibold text-texto-suave">{titulo}</p>
            <p
                className={
                    destacada
                        ? 'mt-2 text-2xl font-semibold text-primario'
                        : 'mt-2 text-2xl font-semibold text-texto'
                }
            >
                {valor}
            </p>
        </div>
    );
}

/**
 * Licencias SaaS (CU-001-005).
 *
 * El cupo lo da el plan contratado. Se asigna una licencia por empleado y
 * revocarla libera el cupo; no se borra nada, queda el histórico.
 *
 * Es la misma pantalla para los dos alcances: con Licencia.VerTodasLasEmpresas
 * se elige la empresa, y sin ese permiso el backend acota todo a la propia.
 *
 * Una empresa sin contratación activa no tiene licencias que administrar: el
 * backend responde 409 y acá se muestra ese mensaje en lugar de la tabla.
 */
export function AdminLicencias() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearFecha, formatearNumero } = useLocalizacion();

    const [inventario, setInventario] = useState<InventarioResponse | null>(null);
    const [empresas, setEmpresas] = useState<EmpresaResponse[]>([]);
    const [empresaId, setEmpresaId] = useState('');
    const [busqueda, setBusqueda] = useState('');
    const [confirmandoRevocar, setConfirmandoRevocar] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Fuerza releer el inventario después de asignar o revocar. */
    const [recarga, setRecarga] = useState(0);

    const puedeAsignar = tienePermiso(PERMISOS.licenciaAsignar);
    const puedeRevocar = tienePermiso(PERMISOS.licenciaRevocar);
    const alcanceTotal = tienePermiso(PERMISOS.licenciaVerTodasLasEmpresas);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        if (!alcanceTotal) {
            return;
        }

        empresasApi.listar().then(setEmpresas).catch(() => setEmpresas([]));
    }, [alcanceTotal]);

    useEffect(() => {
        licenciasApi
            .inventario(empresaId === '' ? 0 : Number(empresaId))
            .then((datos) => {
                setInventario(datos);
                setError(null);
            })
            .catch((excepcion) => {
                // Sin contratación activa no hay inventario: el mensaje del
                // backend ya lo explica.
                setInventario(null);
                setError(mensajeDeError(excepcion, 'admin.licencias.errorCargar'));
            })
            .finally(() => setCargando(false));
    }, [empresaId, recarga, mensajeDeError]);

    const visibles = useMemo(() => {
        const empleados = inventario === null ? [] : inventario.empleados;
        const termino = busqueda.trim().toLowerCase();

        if (termino === '') {
            return empleados;
        }

        return empleados.filter(
            (empleado) =>
                `${empleado.apellido} ${empleado.nombre}`.toLowerCase().includes(termino) ||
                empleado.email.toLowerCase().includes(termino) ||
                (empleado.departamento ?? '').toLowerCase().includes(termino),
        );
    }, [inventario, busqueda]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    async function asignar(empleado: InventarioLicenciaResponse) {
        setError(null);
        setExito(null);

        try {
            await licenciasApi.asignar({ usuarioId: empleado.usuarioId });
            setExito(
                t('admin.licencias.exitoAsignar', {
                    nombre: `${empleado.nombre} ${empleado.apellido}`,
                }),
            );

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.licencias.errorAsignar'));
        }
    }

    async function revocar(empleado: InventarioLicenciaResponse) {
        setError(null);
        setExito(null);
        setConfirmandoRevocar(null);

        if (empleado.licenciaId === null) {
            return;
        }

        try {
            await licenciasApi.revocar(empleado.licenciaId);
            setExito(t('admin.licencias.exitoRevocar'));

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.licencias.errorRevocar'));
        }
    }

    const columnas: ColumnaAbm<InventarioLicenciaResponse>[] = [
        {
            encabezado: t('admin.licencias.col.empleado'),
            celda: (fila) => `${fila.apellido}, ${fila.nombre}`,
        },
        { encabezado: t('comun.campo.email'), celda: (fila) => fila.email },
        {
            encabezado: t('admin.licencias.col.departamento'),
            celda: (fila) => fila.departamento ?? t('comun.valor.vacio'),
        },
        {
            encabezado: t('admin.licencias.col.licencia'),
            celda: (fila) =>
                fila.licenciaId === null ? (
                    <span className="rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave">
                        {t('admin.licencias.sinLicencia')}
                    </span>
                ) : (
                    <span className="rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario">
                        {t('comun.estado.activo')}
                    </span>
                ),
        },
        {
            encabezado: t('admin.licencias.col.asignada'),
            celda: (fila) =>
                fila.fechaAsignacion === null
                    ? t('comun.valor.vacio')
                    : formatearFecha(fila.fechaAsignacion),
        },
    ];

    const sinCupo = inventario !== null && inventario.disponibles <= 0;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {alcanceTotal
                    ? t('admin.licencias.titulo')
                    : t('admin.licencias.miEmpresa.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.licencias.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {/* Sin alcance global la empresa no se elige: el backend impone la
                propia, así que el campo solo confundiría. */}
            {alcanceTotal && (
                <div className="mt-6 w-full sm:w-96">
                    <CampoSelect
                        etiqueta={t('comun.campo.empresa')}
                        identificador="licenciaEmpresa"
                        value={empresaId}
                        onChange={(evento) => {
                            setCargando(true);
                            setEmpresaId(evento.target.value);
                        }}
                    >
                        <option value="">{t('comun.campo.empresa')}</option>
                        {empresas.map((empresa) => (
                            <option key={empresa.empresaId} value={empresa.empresaId}>
                                {empresa.razonSocial}
                            </option>
                        ))}
                    </CampoSelect>
                </div>
            )}

            {inventario !== null && (
                <>
                    <div className="mt-6 grid gap-6 sm:grid-cols-3">
                        <Cifra
                            titulo={t('admin.licencias.cupo.contratadas')}
                            valor={formatearNumero(inventario.contratadas)}
                        />
                        <Cifra
                            titulo={t('admin.licencias.cupo.asignadas')}
                            valor={formatearNumero(inventario.asignadas)}
                        />
                        <Cifra
                            titulo={t('admin.licencias.cupo.disponibles')}
                            valor={formatearNumero(inventario.disponibles)}
                            destacada={!sinCupo}
                        />
                    </div>

                    <p className="mt-3 text-sm text-texto-suave">
                        {t('admin.licencias.cupo.plan', { plan: inventario.plan })}
                    </p>
                </>
            )}

            {inventario !== null && (
                <section className="mt-6">
                    <div className="flex flex-wrap items-end justify-end gap-4">
                        <div className="w-full sm:w-72">
                            <CampoTexto
                                etiqueta={t('admin.licencias.buscar')}
                                identificador="busquedaLicencia"
                                type="search"
                                placeholder={t('admin.licencias.buscarPlaceholder')}
                                value={busqueda}
                                onChange={(evento) => setBusqueda(evento.target.value)}
                            />
                        </div>
                    </div>

                    <div className="mt-4">
                        {cargando ? (
                            <p className="text-sm text-texto-suave">
                                {t('admin.licencias.cargando')}
                            </p>
                        ) : (
                            <TablaAbm
                                columnas={columnas}
                                filas={visibles}
                                claveDe={(fila) => fila.usuarioId}
                                inactiva={(fila) => fila.licenciaId === null}
                                mensajeVacio={
                                    busqueda.trim() === ''
                                        ? t('admin.licencias.vacio')
                                        : t('admin.licencias.sinCoincidencias')
                                }
                                acciones={(fila) => (
                                    <div className="flex justify-end gap-2">
                                        {puedeAsignar && fila.licenciaId === null && (
                                            <button
                                                type="button"
                                                onClick={() => void asignar(fila)}
                                                disabled={sinCupo}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-info-fondo disabled:cursor-not-allowed disabled:text-texto-suave disabled:hover:bg-superficie"
                                            >
                                                {t('admin.licencias.asignar')}
                                            </button>
                                        )}

                                        {puedeRevocar &&
                                            fila.licenciaId !== null &&
                                            (confirmandoRevocar === fila.usuarioId ? (
                                                <>
                                                    <button
                                                        type="button"
                                                        onClick={() => void revocar(fila)}
                                                        className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                                    >
                                                        {t('admin.licencias.revocarConfirmar')}
                                                    </button>
                                                    <button
                                                        type="button"
                                                        onClick={() => setConfirmandoRevocar(null)}
                                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                                    >
                                                        {t('comun.boton.no')}
                                                    </button>
                                                </>
                                            ) : (
                                                <button
                                                    type="button"
                                                    onClick={() =>
                                                        setConfirmandoRevocar(fila.usuarioId)
                                                    }
                                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                                >
                                                    {t('admin.licencias.revocar')}
                                                </button>
                                            ))}
                                    </div>
                                )}
                            />
                        )}
                    </div>
                </section>
            )}
        </div>
    );
}
