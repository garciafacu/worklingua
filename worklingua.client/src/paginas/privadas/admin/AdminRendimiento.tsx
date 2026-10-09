import { useCallback, useEffect, useState, type FormEvent, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { empresasApi } from '../../../api/empresasApi';
import { rendimientoApi } from '../../../api/rendimientoApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { GraficoBarras, type DatoGrafico } from '../../../componentes/GraficoBarras';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import { descargarCsv, generarCsv } from '../../../servicios/csv';
import type { EmpresaResponse } from '../../../tipos/autenticacion';
import type {
    RendimientoDepartamentoResponse,
    RendimientoEmpleadoResponse,
    RendimientoResponse,
} from '../../../tipos/rendimiento';

interface CifraProps {
    titulo: string;
    valor: ReactNode;
}

function Cifra({ titulo, valor }: CifraProps) {
    return (
        <div className="rounded-2xl border border-borde bg-superficie p-6">
            <p className="m-0 text-sm font-semibold text-texto-suave">{titulo}</p>
            <p className="mt-2 text-2xl font-semibold text-texto">{valor}</p>
        </div>
    );
}

function aTexto(fecha: Date): string {
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${fecha.getFullYear()}-${mes}-${dia}`;
}

function formularioInicial() {
    const hoy = new Date();

    return {
        empresaId: '',
        desde: `${hoy.getFullYear()}-01-01`,
        hasta: aTexto(hoy),
        departamentoId: '',
    };
}

type Formulario = ReturnType<typeof formularioInicial>;

/**
 * Panel de rendimiento formativo (CU-001-004).
 *
 * Tres miradas del mismo período: las cifras de toda la empresa, la comparación
 * entre departamentos y el detalle por empleado. El backend hace las tres
 * cuentas; acá solo se eligen los criterios y se dibuja.
 *
 * Es la misma pantalla para los dos alcances: con Curso.VerTodasLasEmpresas se
 * elige la empresa, y sin ese permiso el backend acota todo a la propia.
 *
 * El CSV se arma con lo que ya está en pantalla: son los mismos datos que
 * muestra la tabla, así que no hace falta un endpoint de exportación.
 */
export function AdminRendimiento() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearFecha, formatearMoneda, formatearNumero } = useLocalizacion();

    const [formulario, setFormulario] = useState<Formulario>(formularioInicial);
    const [criterios, setCriterios] = useState<Formulario>(formularioInicial);
    const [panel, setPanel] = useState<RendimientoResponse | null>(null);
    const [empresas, setEmpresas] = useState<EmpresaResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    const alcanceTotal = tienePermiso(PERMISOS.cursoVerTodasLasEmpresas);

    const mensajeDeError = useCallback(
        (excepcion: unknown) =>
            excepcion instanceof ErrorApi ? excepcion.message : t('admin.rendimiento.error'),
        [t],
    );

    useEffect(() => {
        if (!alcanceTotal) {
            return;
        }

        empresasApi.listar().then(setEmpresas).catch(() => setEmpresas([]));
    }, [alcanceTotal]);

    useEffect(() => {
        // Con alcance total la empresa es obligatoria y el backend la exige. Se
        // evita el viaje para que la pantalla no reciba al operador con un
        // error en rojo: todavía no eligió nada.
        const sinEmpresa = alcanceTotal && criterios.empresaId === '';

        const consulta = sinEmpresa
            ? Promise.resolve(null)
            : rendimientoApi.obtener({
                  empresaId: criterios.empresaId === '' ? undefined : Number(criterios.empresaId),
                  desde: criterios.desde || undefined,
                  hasta: criterios.hasta || undefined,
                  departamentoId:
                      criterios.departamentoId === '' ? undefined : Number(criterios.departamentoId),
              });

        consulta
            .then((datos) => {
                setPanel(datos);
                setError(null);
            })
            .catch((excepcion: unknown) => {
                setPanel(null);
                setError(mensajeDeError(excepcion));
            })
            .finally(() => setCargando(false));
    }, [alcanceTotal, criterios, mensajeDeError]);

    /**
     * Las opciones del selector salen del propio panel: la comparación entre
     * departamentos trae todos los de la empresa, incluso los que no tuvieron
     * actividad, y no la recorta el filtro. Pedirlos aparte sería una consulta
     * de más y, sin alcance total, acá no se sabe de antemano qué empresa
     * impuso el backend.
     */
    const opcionesDepartamento =
        panel === null ? [] : panel.departamentos.filter((fila) => fila.departamentoId !== null);

    function buscar(evento: FormEvent) {
        evento.preventDefault();
        setCargando(true);
        setCriterios({ ...formulario });
    }

    function limpiar() {
        const inicial = formularioInicial();

        setFormulario(inicial);
        setCargando(true);
        setCriterios(inicial);
    }

    function nombreDeDepartamento(nombre: string | null): string {
        return nombre ?? t('admin.rendimiento.sinDepartamento');
    }

    function porcentaje(valor: number): string {
        return `${formatearNumero(valor, 1)}%`;
    }

    function exportar() {
        if (panel === null) {
            return;
        }

        const contenido = generarCsv(
            [
                t('admin.rendimiento.col.empleado'),
                t('comun.campo.email'),
                t('admin.rendimiento.col.departamento'),
                t('admin.rendimiento.col.iniciados'),
                t('admin.rendimiento.col.completados'),
                t('admin.rendimiento.col.avance'),
                t('admin.rendimiento.col.ultimaActividad'),
            ],
            panel.empleados.map((fila) => [
                fila.empleado,
                fila.email,
                nombreDeDepartamento(fila.departamento),
                fila.modulosIniciados,
                fila.modulosCompletados,
                formatearNumero(fila.avancePromedio, 1),
                fila.ultimaActividad === null ? '' : formatearFecha(fila.ultimaActividad),
            ]),
        );

        descargarCsv(`rendimiento-${criterios.desde}-${criterios.hasta}.csv`, contenido);
    }

    const datosDepartamentos: DatoGrafico[] =
        panel === null
            ? []
            : panel.departamentos.map((fila) => ({
                  etiqueta: nombreDeDepartamento(fila.departamento),
                  valor: fila.avancePromedio,
              }));

    const columnasDepartamento: ColumnaAbm<RendimientoDepartamentoResponse>[] = [
        {
            encabezado: t('admin.rendimiento.col.departamento'),
            celda: (fila) => nombreDeDepartamento(fila.departamento),
        },
        {
            encabezado: t('admin.rendimiento.col.empleados'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.empleados),
        },
        {
            encabezado: t('admin.rendimiento.col.conActividad'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.empleadosConActividad),
        },
        {
            encabezado: t('admin.rendimiento.col.completados'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.modulosCompletados),
        },
        {
            encabezado: t('admin.rendimiento.col.avance'),
            numerica: true,
            celda: (fila) => porcentaje(fila.avancePromedio),
        },
    ];

    const columnasEmpleado: ColumnaAbm<RendimientoEmpleadoResponse>[] = [
        { encabezado: t('admin.rendimiento.col.empleado'), celda: (fila) => fila.empleado },
        { encabezado: t('comun.campo.email'), celda: (fila) => fila.email },
        {
            encabezado: t('admin.rendimiento.col.departamento'),
            celda: (fila) => nombreDeDepartamento(fila.departamento),
        },
        {
            encabezado: t('admin.rendimiento.col.iniciados'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.modulosIniciados),
        },
        {
            encabezado: t('admin.rendimiento.col.completados'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.modulosCompletados),
        },
        {
            encabezado: t('admin.rendimiento.col.avance'),
            numerica: true,
            celda: (fila) => porcentaje(fila.avancePromedio),
        },
        {
            encabezado: t('admin.rendimiento.col.ultimaActividad'),
            celda: (fila) =>
                fila.ultimaActividad === null
                    ? t('comun.valor.vacio')
                    : formatearFecha(fila.ultimaActividad),
        },
    ];

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {alcanceTotal ? t('admin.rendimiento.titulo') : t('admin.rendimiento.miEmpresa.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.rendimiento.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            <form
                onSubmit={buscar}
                role="search"
                className="mt-6 rounded-2xl border border-borde bg-superficie p-6"
            >
                {/* Sin alcance global la empresa no se elige: el backend impone
                    la propia, así que el campo solo confundiría. */}
                {alcanceTotal && (
                    <div className="fila">
                        <CampoSelect
                            etiqueta={t('comun.campo.empresa')}
                            identificador="rendimiento-empresa"
                            value={formulario.empresaId}
                            onChange={(evento) =>
                                setFormulario({
                                    ...formulario,
                                    empresaId: evento.target.value,
                                    departamentoId: '',
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
                    </div>
                )}

                <div className={alcanceTotal ? 'fila mt-4' : 'fila'}>
                    <CampoTexto
                        etiqueta={t('admin.rendimiento.fechaDesde')}
                        identificador="rendimiento-desde"
                        type="date"
                        value={formulario.desde}
                        onChange={(evento) => setFormulario({ ...formulario, desde: evento.target.value })}
                    />
                    <CampoTexto
                        etiqueta={t('admin.rendimiento.fechaHasta')}
                        identificador="rendimiento-hasta"
                        type="date"
                        value={formulario.hasta}
                        onChange={(evento) => setFormulario({ ...formulario, hasta: evento.target.value })}
                    />
                </div>

                <div className="fila mt-4">
                    <CampoSelect
                        etiqueta={t('admin.rendimiento.departamento')}
                        identificador="rendimiento-departamento"
                        value={formulario.departamentoId}
                        onChange={(evento) =>
                            setFormulario({ ...formulario, departamentoId: evento.target.value })
                        }
                    >
                        <option value="">{t('admin.rendimiento.departamento.todos')}</option>
                        {opcionesDepartamento.map((fila) => (
                            <option key={fila.departamentoId} value={fila.departamentoId ?? 0}>
                                {fila.departamento}
                            </option>
                        ))}
                    </CampoSelect>
                </div>

                <div className="mt-4 flex flex-wrap gap-3">
                    <Boton type="submit" cargando={cargando}>
                        {t('admin.rendimiento.buscar')}
                    </Boton>
                    <button
                        type="button"
                        onClick={limpiar}
                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                    >
                        {t('admin.rendimiento.limpiar')}
                    </button>
                </div>
            </form>

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('admin.rendimiento.cargando')}</p>}

            {!cargando && panel === null && error === null && (
                <p className="mt-6 text-sm text-texto-suave">{t('admin.rendimiento.elegirEmpresa')}</p>
            )}

            {!cargando && panel !== null && (
                <>
                    <div className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
                        <Cifra
                            titulo={t('admin.rendimiento.cifra.empleados')}
                            valor={formatearNumero(panel.resumen.empleadosConActividad)}
                        />
                        <Cifra
                            titulo={t('admin.rendimiento.cifra.completados')}
                            valor={formatearNumero(panel.resumen.modulosCompletados)}
                        />
                        <Cifra
                            titulo={t('admin.rendimiento.cifra.cursos')}
                            valor={formatearNumero(panel.resumen.cursosCompletados)}
                        />
                        <Cifra
                            titulo={t('admin.rendimiento.cifra.avance')}
                            valor={porcentaje(panel.resumen.avancePromedio)}
                        />
                    </div>

                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <h2 className="m-0 text-lg font-semibold text-texto">
                            {t('admin.rendimiento.costo.titulo')}
                        </h2>

                        {panel.resumen.facturado === null ? (
                            <p className="mt-2 text-sm text-texto-suave">
                                {t('admin.rendimiento.costo.soloEmpresa')}
                            </p>
                        ) : (
                            <div className="mt-4 grid gap-6 sm:grid-cols-3">
                                <Cifra
                                    titulo={t('admin.rendimiento.costo.facturado')}
                                    valor={formatearMoneda(panel.resumen.facturado)}
                                />
                                <Cifra
                                    titulo={t('admin.rendimiento.costo.porModulo')}
                                    valor={formatearMoneda(panel.resumen.costoPorModulo ?? 0)}
                                />
                                <Cifra
                                    titulo={t('admin.rendimiento.costo.porEmpleado')}
                                    valor={formatearMoneda(panel.resumen.costoPorEmpleado ?? 0)}
                                />
                            </div>
                        )}
                    </section>

                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <h2 className="m-0 text-lg font-semibold text-texto">
                            {t('admin.rendimiento.porDepartamento')}
                        </h2>

                        <div className="mt-4">
                            <GraficoBarras
                                datos={datosDepartamentos}
                                etiquetaSerie={t('admin.rendimiento.col.avance')}
                                mensajeVacio={t('admin.rendimiento.sinActividad')}
                                formatearValor={porcentaje}
                            />
                        </div>

                        <div className="mt-4">
                            <TablaAbm
                                columnas={columnasDepartamento}
                                filas={panel.departamentos}
                                claveDe={(fila) => fila.departamentoId ?? 0}
                                mensajeVacio={t('admin.rendimiento.sinActividad')}
                            />
                        </div>
                    </section>

                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <div className="flex flex-wrap items-center justify-between gap-3">
                            <h2 className="m-0 text-lg font-semibold text-texto">
                                {t('admin.rendimiento.detalle')}
                            </h2>
                            <button
                                type="button"
                                onClick={exportar}
                                disabled={panel.empleados.length === 0}
                                className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo disabled:opacity-50"
                            >
                                {t('admin.rendimiento.exportar')}
                            </button>
                        </div>

                        <div className="mt-4">
                            <TablaAbm
                                columnas={columnasEmpleado}
                                filas={panel.empleados}
                                claveDe={(fila) => fila.usuarioId}
                                mensajeVacio={t('admin.rendimiento.sinActividad')}
                            />
                        </div>
                    </section>
                </>
            )}
        </div>
    );
}
