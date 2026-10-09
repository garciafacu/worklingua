import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { reportesApi } from '../../../api/reportesApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { GraficoBarras, type DatoGrafico } from '../../../componentes/GraficoBarras';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { descargarCsv, generarCsv } from '../../../servicios/csv';
import {
    AGRUPACIONES,
    type Agrupacion,
    type GananciaPeriodoResponse,
    type GananciaZonaResponse,
} from '../../../tipos/reportes';

function aTexto(fecha: Date): string {
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${fecha.getFullYear()}-${mes}-${dia}`;
}

function formularioInicial() {
    const hoy = new Date();

    return {
        desde: `${hoy.getFullYear()}-01-01`,
        hasta: aTexto(hoy),
        agrupacion: 'MES' as Agrupacion,
        provincia: '',
    };
}

type Formulario = ReturnType<typeof formularioInicial>;

/**
 * Reportes de ganancias (Formulario, punto 15.b).
 *
 * La ganancia es el cobrado neto: los pagos con tarjeta aprobados menos las
 * notas de crédito. La tabla muestra además lo facturado, que incluye lo que
 * quedó en cuenta corriente y todavía no entró.
 *
 * El bloque por zona siempre abarca todas las zonas: es el desglose del mismo
 * período, y por eso no toma el filtro de zona del formulario.
 */
export function AdminReportes() {
    const { t } = useTranslation();
    const { formatearMoneda, formatearNumero } = useLocalizacion();

    const [formulario, setFormulario] = useState<Formulario>(formularioInicial);
    const [criterios, setCriterios] = useState<Formulario>(formularioInicial);
    const [periodos, setPeriodos] = useState<GananciaPeriodoResponse[]>([]);
    const [zonas, setZonas] = useState<GananciaZonaResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    const mensajeDeError = useCallback(
        (excepcion: unknown) => (excepcion instanceof ErrorApi ? excepcion.message : t('admin.reportes.error')),
        [t],
    );

    useEffect(() => {
        const filtros = {
            desde: criterios.desde || undefined,
            hasta: criterios.hasta || undefined,
            agrupacion: criterios.agrupacion,
            provincia: criterios.provincia || undefined,
        };

        Promise.all([reportesApi.ganancias(filtros), reportesApi.gananciasPorZona(filtros)])
            .then(([listaPeriodos, listaZonas]) => {
                setPeriodos(listaPeriodos);
                setZonas(listaZonas);
                setError(null);
            })
            .catch((excepcion: unknown) => setError(mensajeDeError(excepcion)))
            .finally(() => setCargando(false));
    }, [criterios, mensajeDeError]);

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

    /**
     * Exporta la tabla que se está viendo. Son los mismos datos que ya están en
     * pantalla, así que se arman acá y no hace falta un endpoint aparte.
     */
    function exportar(nombre: string, encabezados: string[], filas: (string | number)[][]) {
        descargarCsv(
            `${nombre}-${criterios.desde}-${criterios.hasta}.csv`,
            generarCsv(encabezados, filas),
        );
    }

    function exportarPeriodos() {
        exportar(
            'ganancias',
            [
                t('admin.reportes.col.periodo'),
                t('admin.reportes.col.facturado'),
                t('admin.reportes.col.cobrado'),
                t('admin.reportes.col.notas'),
                t('admin.reportes.col.neto'),
            ],
            periodos.map((fila) => [fila.clave, fila.facturado, fila.cobrado, fila.notasCredito, fila.neto]),
        );
    }

    function exportarZonas() {
        exportar(
            'ganancias-por-zona',
            [
                t('admin.reportes.col.zona'),
                t('admin.reportes.col.empresas'),
                t('admin.reportes.col.facturado'),
                t('admin.reportes.col.cobrado'),
                t('admin.reportes.col.notas'),
                t('admin.reportes.col.neto'),
            ],
            zonas.map((fila) => [
                fila.zona,
                fila.empresas,
                fila.facturado,
                fila.cobrado,
                fila.notasCredito,
                fila.neto,
            ]),
        );
    }

    const totalNeto = periodos.reduce((suma, fila) => suma + fila.neto, 0);

    const datosPeriodos: DatoGrafico[] = periodos.map((fila) => ({
        etiqueta: fila.clave,
        valor: fila.neto,
    }));

    const datosZonas: DatoGrafico[] = zonas.map((fila) => ({
        etiqueta: fila.zona,
        valor: fila.neto,
    }));

    const columnasPeriodo: ColumnaAbm<GananciaPeriodoResponse>[] = [
        { encabezado: t('admin.reportes.col.periodo'), celda: (fila) => fila.clave },
        {
            encabezado: t('admin.reportes.col.facturado'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.facturado),
        },
        {
            encabezado: t('admin.reportes.col.cobrado'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.cobrado),
        },
        {
            encabezado: t('admin.reportes.col.notas'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.notasCredito),
        },
        {
            encabezado: t('admin.reportes.col.neto'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.neto),
        },
    ];

    const columnasZona: ColumnaAbm<GananciaZonaResponse>[] = [
        { encabezado: t('admin.reportes.col.zona'), celda: (fila) => fila.zona },
        {
            encabezado: t('admin.reportes.col.empresas'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.empresas),
        },
        {
            encabezado: t('admin.reportes.col.facturado'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.facturado),
        },
        {
            encabezado: t('admin.reportes.col.cobrado'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.cobrado),
        },
        {
            encabezado: t('admin.reportes.col.notas'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.notasCredito),
        },
        {
            encabezado: t('admin.reportes.col.neto'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.neto),
        },
    ];

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.reportes.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.reportes.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            <form
                onSubmit={buscar}
                role="search"
                className="mt-6 rounded-2xl border border-borde bg-superficie p-6"
            >
                <div className="fila">
                    <CampoTexto
                        etiqueta={t('admin.reportes.fechaDesde')}
                        identificador="reporte-desde"
                        type="date"
                        value={formulario.desde}
                        onChange={(evento) => setFormulario({ ...formulario, desde: evento.target.value })}
                    />
                    <CampoTexto
                        etiqueta={t('admin.reportes.fechaHasta')}
                        identificador="reporte-hasta"
                        type="date"
                        value={formulario.hasta}
                        onChange={(evento) => setFormulario({ ...formulario, hasta: evento.target.value })}
                    />
                </div>

                <div className="fila mt-4">
                    <CampoSelect
                        etiqueta={t('admin.reportes.agrupacion')}
                        identificador="reporte-agrupacion"
                        value={formulario.agrupacion}
                        onChange={(evento) =>
                            setFormulario({ ...formulario, agrupacion: evento.target.value as Agrupacion })
                        }
                    >
                        {AGRUPACIONES.map((valor) => (
                            <option key={valor} value={valor}>
                                {t(`admin.reportes.agrupacion.${valor}`)}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoSelect
                        etiqueta={t('admin.reportes.provincia')}
                        identificador="reporte-provincia"
                        value={formulario.provincia}
                        onChange={(evento) => setFormulario({ ...formulario, provincia: evento.target.value })}
                    >
                        <option value="">{t('admin.reportes.provincia.todas')}</option>
                        {zonas.map((zona) => (
                            <option key={zona.zona} value={zona.zona}>
                                {zona.zona}
                            </option>
                        ))}
                    </CampoSelect>
                </div>

                <div className="mt-4 flex flex-wrap gap-3">
                    <Boton type="submit" cargando={cargando}>
                        {t('admin.reportes.buscar')}
                    </Boton>
                    <button
                        type="button"
                        onClick={limpiar}
                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                    >
                        {t('admin.reportes.limpiar')}
                    </button>
                </div>
            </form>

            {cargando ? (
                <p className="mt-6 text-sm text-texto-suave">{t('admin.reportes.cargando')}</p>
            ) : (
                <>
                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <div className="flex flex-wrap items-start justify-between gap-3">
                            <div>
                                <h2 className="m-0 text-lg font-semibold text-texto">
                                    {t('admin.reportes.porPeriodo')}
                                </h2>
                                <p className="mt-1 text-sm text-texto-suave">
                                    {t('admin.reportes.totalPeriodo', { importe: formatearMoneda(totalNeto) })}
                                </p>
                            </div>
                            <button
                                type="button"
                                onClick={exportarPeriodos}
                                disabled={periodos.length === 0}
                                className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo disabled:opacity-50"
                            >
                                {t('admin.reportes.exportar')}
                            </button>
                        </div>

                        <div className="mt-4">
                            <GraficoBarras
                                datos={datosPeriodos}
                                etiquetaSerie={t('admin.reportes.col.neto')}
                                mensajeVacio={t('admin.reportes.vacio')}
                                formatearValor={formatearMoneda}
                            />
                        </div>

                        <div className="mt-4">
                            <TablaAbm
                                columnas={columnasPeriodo}
                                filas={periodos}
                                claveDe={(fila) => fila.clave}
                                mensajeVacio={t('admin.reportes.vacio')}
                            />
                        </div>
                    </section>

                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <div className="flex flex-wrap items-center justify-between gap-3">
                            <h2 className="m-0 text-lg font-semibold text-texto">
                                {t('admin.reportes.porZona')}
                            </h2>
                            <button
                                type="button"
                                onClick={exportarZonas}
                                disabled={zonas.length === 0}
                                className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo disabled:opacity-50"
                            >
                                {t('admin.reportes.exportar')}
                            </button>
                        </div>

                        <div className="mt-4">
                            <GraficoBarras
                                datos={datosZonas}
                                etiquetaSerie={t('admin.reportes.col.neto')}
                                mensajeVacio={t('admin.reportes.vacio')}
                                formatearValor={formatearMoneda}
                            />
                        </div>

                        <div className="mt-4">
                            <TablaAbm
                                columnas={columnasZona}
                                filas={zonas}
                                claveDe={(fila) => fila.zona}
                                mensajeVacio={t('admin.reportes.vacio')}
                            />
                        </div>
                    </section>
                </>
            )}
        </div>
    );
}
