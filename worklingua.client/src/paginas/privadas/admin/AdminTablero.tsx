import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ErrorApi } from '../../../api/clienteHttp';
import { reportesApi } from '../../../api/reportesApi';
import { Alerta } from '../../../componentes/Alerta';
import { GraficoBarras, type DatoGrafico } from '../../../componentes/GraficoBarras';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import type { TableroResponse } from '../../../tipos/reportes';

interface TarjetaProps {
    titulo: string;
    valor: string;
    detalle: ReactNode;
}

function Tarjeta({ titulo, valor, detalle }: TarjetaProps) {
    return (
        <article className="rounded-2xl border border-borde bg-superficie p-6">
            <h2 className="m-0 text-sm font-semibold text-texto-suave">{titulo}</h2>
            <p className="mt-2 text-2xl font-semibold text-texto">{valor}</p>
            <p className="mt-1 text-sm text-texto-suave">{detalle}</p>
        </article>
    );
}

/**
 * Tablero del negocio (Formulario, punto 15.c): lo cobrado en el mes, la deuda
 * de las empresas clientes y las contrataciones vigentes.
 *
 * Los ingresos son el cobrado neto, el mismo criterio que los reportes: pagos
 * con tarjeta aprobados menos notas de crédito.
 */
export function AdminTablero() {
    const { t } = useTranslation();
    const { formatearMoneda, formatearNumero } = useLocalizacion();

    const [tablero, setTablero] = useState<TableroResponse | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    const mensajeDeError = useCallback(
        (excepcion: unknown) => (excepcion instanceof ErrorApi ? excepcion.message : t('admin.tablero.error')),
        [t],
    );

    useEffect(() => {
        reportesApi
            .tablero()
            .then(setTablero)
            .catch((excepcion: unknown) => setError(mensajeDeError(excepcion)))
            .finally(() => setCargando(false));
    }, [mensajeDeError]);

    /**
     * Comparación con el mes anterior. Sin base contra la cual comparar (mes
     * anterior en cero o negativo) se muestra el importe en vez del porcentaje,
     * que sería engañoso.
     */
    function comparacion(datos: TableroResponse): ReactNode {
        if (datos.ingresosMesAnterior <= 0) {
            return t('admin.tablero.mesAnterior', { importe: formatearMoneda(datos.ingresosMesAnterior) });
        }

        const variacion =
            ((datos.ingresosMes - datos.ingresosMesAnterior) / datos.ingresosMesAnterior) * 100;
        const porcentaje = formatearNumero(Math.abs(variacion), 1);

        if (Math.round(variacion) === 0) {
            return t('admin.tablero.ingresosIgual');
        }

        return variacion > 0
            ? t('admin.tablero.ingresosSuba', { porcentaje })
            : t('admin.tablero.ingresosBaja', { porcentaje });
    }

    const datosPlanes: DatoGrafico[] =
        tablero === null
            ? []
            : tablero.planes.map((fila) => ({ etiqueta: fila.plan, valor: fila.cantidad }));

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.tablero.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.tablero.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('admin.tablero.cargando')}</p>}

            {tablero !== null && (
                <>
                    <div className="mt-6 grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                        <Tarjeta
                            titulo={t('admin.tablero.ingresos')}
                            valor={formatearMoneda(tablero.ingresosMes)}
                            detalle={comparacion(tablero)}
                        />
                        <Tarjeta
                            titulo={t('admin.tablero.deuda')}
                            valor={formatearMoneda(tablero.deudaTotal)}
                            detalle={t('admin.tablero.deudaDetalle', {
                                cantidad: formatearNumero(tablero.empresasConDeuda),
                            })}
                        />
                        <Tarjeta
                            titulo={t('admin.tablero.empresas')}
                            valor={formatearNumero(tablero.empresasActivas)}
                            detalle={`${t('admin.tablero.contrataciones')}: ${formatearNumero(
                                tablero.contratacionesActivas,
                            )}`}
                        />
                        {/* Señal de upselling: cuántas licencias de las vendidas
                            están realmente en uso, y quiénes ya no tienen cupo. */}
                        <Tarjeta
                            titulo={t('admin.tablero.licencias')}
                            valor={`${formatearNumero(
                                tablero.licenciasAsignadas,
                            )} / ${formatearNumero(tablero.licenciasContratadas)}`}
                            detalle={t('admin.tablero.licenciasDetalle', {
                                cantidad: formatearNumero(tablero.empresasSinCupo),
                            })}
                        />
                    </div>

                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <h2 className="m-0 text-lg font-semibold text-texto">
                            {t('admin.tablero.contratacionesPorPlan')}
                        </h2>

                        <div className="mt-4">
                            <GraficoBarras
                                datos={datosPlanes}
                                etiquetaSerie={t('admin.tablero.contrataciones')}
                                mensajeVacio={t('comun.grafico.sinDatos')}
                                formatearValor={(valor) => formatearNumero(valor)}
                            />
                        </div>

                        <p className="mt-4 text-sm">
                            <Link to="/inicio/admin/reportes" className="font-semibold text-primario">
                                {t('admin.tablero.verReportes')}
                            </Link>
                        </p>
                    </section>
                </>
            )}
        </div>
    );
}
