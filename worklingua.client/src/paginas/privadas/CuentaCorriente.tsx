import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../api/clienteHttp';
import { cuentaCorrienteApi } from '../../api/cuentaCorrienteApi';
import { Alerta } from '../../componentes/Alerta';
import { EtiquetaEstado, type TonoEstado } from '../../componentes/EtiquetaEstado';
import { TablaAbm, type ColumnaAbm } from '../../componentes/TablaAbm';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import type {
    EstadoCuentaCorrienteResponse,
    FacturaConPlanResponse,
    MovimientoResponse,
    NotaConFacturaResponse,
    PagoConFacturaResponse,
} from '../../tipos/cuentaCorriente';

type Pestana = 'movimientos' | 'facturas' | 'pagos' | 'notas';

const TONO_FACTURA: Record<string, TonoEstado> = {
    PAGADA: 'exito',
    A_CUENTA: 'alerta',
    ANULADA: 'neutro',
};

/**
 * Estado de cuenta corriente de la empresa (punto 6.b).
 *
 * Lee lo que graba la contratación: facturas, pagos, notas de crédito/débito y
 * los movimientos con su saldo acumulado. Los importes se guardan en ARS y se
 * muestran en la moneda de la cultura activa; cada comprobante conserva además la
 * moneda con la que se operó.
 */
export function CuentaCorriente() {
    const { t } = useTranslation();
    const { formatearMoneda, formatearFecha } = useLocalizacion();

    const [estado, setEstado] = useState<EstadoCuentaCorrienteResponse | null>(null);
    const [pestana, setPestana] = useState<Pestana>('movimientos');
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    useEffect(() => {
        cuentaCorrienteApi
            .obtener()
            .then(setEstado)
            .catch((excepcion: unknown) =>
                setError(excepcion instanceof ErrorApi ? excepcion.message : t('privado.cuentaCorriente.error')),
            )
            .finally(() => setCargando(false));
    }, [t]);

    const columnasMovimientos: ColumnaAbm<MovimientoResponse>[] = [
        { encabezado: t('privado.cuentaCorriente.col.fecha'), celda: (fila) => formatearFecha(fila.movimiento.fechaMovimiento, true) },
        {
            encabezado: t('privado.cuentaCorriente.col.tipo'),
            celda: (fila) => t(`privado.cuentaCorriente.tipo.${fila.movimiento.tipo}`),
        },
        {
            encabezado: t('privado.cuentaCorriente.col.concepto'),
            celda: (fila) => (
                <>
                    <span className="text-texto">{fila.movimiento.concepto}</span>
                    {fila.comprobante && (
                        <>
                            <br />
                            <span className="text-xs text-texto-suave">{fila.comprobante}</span>
                        </>
                    )}
                </>
            ),
        },
        {
            encabezado: t('privado.cuentaCorriente.col.debe'),
            numerica: true,
            celda: (fila) => (fila.movimiento.debe > 0 ? formatearMoneda(fila.movimiento.debe) : '—'),
        },
        {
            encabezado: t('privado.cuentaCorriente.col.haber'),
            numerica: true,
            celda: (fila) => (fila.movimiento.haber > 0 ? formatearMoneda(fila.movimiento.haber) : '—'),
        },
        {
            encabezado: t('privado.cuentaCorriente.col.saldo'),
            numerica: true,
            celda: (fila) => formatearMoneda(fila.saldoAcumulado),
        },
    ];

    const columnasFacturas: ColumnaAbm<FacturaConPlanResponse>[] = [
        { encabezado: t('privado.cuentaCorriente.col.numero'), celda: (fila) => fila.factura.numeroFactura },
        { encabezado: t('privado.cuentaCorriente.col.plan'), celda: (fila) => fila.plan },
        { encabezado: t('privado.cuentaCorriente.col.emision'), celda: (fila) => formatearFecha(fila.factura.fechaEmision) },
        { encabezado: t('privado.cuentaCorriente.col.vencimiento'), celda: (fila) => formatearFecha(fila.factura.fechaVencimiento) },
        {
            encabezado: t('privado.cuentaCorriente.col.estado'),
            celda: (fila) => (
                <EtiquetaEstado
                    texto={t(`privado.cuentaCorriente.estadoFactura.${fila.factura.estado}`)}
                    tono={TONO_FACTURA[fila.factura.estado] ?? 'neutro'}
                />
            ),
        },
        { encabezado: t('privado.cuentaCorriente.col.moneda'), celda: (fila) => fila.factura.moneda },
        { encabezado: t('privado.cuentaCorriente.col.importe'), numerica: true, celda: (fila) => formatearMoneda(fila.factura.importe) },
    ];

    const columnasPagos: ColumnaAbm<PagoConFacturaResponse>[] = [
        { encabezado: t('privado.cuentaCorriente.col.fecha'), celda: (fila) => formatearFecha(fila.pago.fechaPago, true) },
        { encabezado: t('privado.cuentaCorriente.col.factura'), celda: (fila) => fila.numeroFactura },
        { encabezado: t('privado.cuentaCorriente.col.medio'), celda: (fila) => t(`comun.contratacion.medio.${fila.pago.medioPago}`) },
        { encabezado: t('privado.cuentaCorriente.col.referencia'), celda: (fila) => fila.pago.numeroOperacion ?? '—' },
        {
            encabezado: t('privado.cuentaCorriente.col.estado'),
            celda: (fila) => t(`privado.cuentaCorriente.estadoPago.${fila.pago.estado ?? 'APROBADO'}`),
        },
        { encabezado: t('privado.cuentaCorriente.col.importe'), numerica: true, celda: (fila) => formatearMoneda(fila.pago.importe ?? 0) },
    ];

    const columnasNotas: ColumnaAbm<NotaConFacturaResponse>[] = [
        { encabezado: t('privado.cuentaCorriente.col.fecha'), celda: (fila) => formatearFecha(fila.nota.fechaEmision, true) },
        {
            encabezado: t('privado.cuentaCorriente.col.numero'),
            celda: (fila) => (
                <>
                    <span className="text-texto">{fila.nota.numero}</span>
                    <br />
                    <span className="text-xs text-texto-suave">{t(`comun.contratacion.nota.${fila.nota.tipo}`)}</span>
                </>
            ),
        },
        { encabezado: t('privado.cuentaCorriente.col.factura'), celda: (fila) => fila.numeroFactura },
        { encabezado: t('privado.cuentaCorriente.col.motivo'), celda: (fila) => fila.nota.motivo },
        {
            encabezado: t('privado.cuentaCorriente.col.estado'),
            celda: (fila) => t(`privado.cuentaCorriente.estadoNota.${fila.nota.estado}`),
        },
        { encabezado: t('privado.cuentaCorriente.col.importe'), numerica: true, celda: (fila) => formatearMoneda(fila.nota.importe) },
        {
            encabezado: t('privado.cuentaCorriente.col.disponible'),
            numerica: true,
            celda: (fila) => (fila.nota.tipo === 'NC' ? formatearMoneda(fila.nota.saldoDisponible) : '—'),
        },
    ];

    const pestanas: { clave: Pestana; etiqueta: string }[] = [
        { clave: 'movimientos', etiqueta: t('privado.cuentaCorriente.pestana.movimientos') },
        { clave: 'facturas', etiqueta: t('privado.cuentaCorriente.pestana.facturas') },
        { clave: 'pagos', etiqueta: t('privado.cuentaCorriente.pestana.pagos') },
        { clave: 'notas', etiqueta: t('privado.cuentaCorriente.pestana.notas') },
    ];

    const deudor = (estado?.saldo ?? 0) > 0;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.cuentaCorriente.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.cuentaCorriente.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('privado.cuentaCorriente.cargando')}</p>}

            {estado && (
                <>
                    <section className="mt-6 grid gap-4 sm:grid-cols-3">
                        <div className="rounded-2xl border border-borde bg-superficie p-5">
                            <p className="m-0 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                                {t('privado.cuentaCorriente.saldo')}
                            </p>
                            <p className={`mt-2 mb-0 text-2xl font-bold ${deudor ? 'text-error' : 'text-exito'}`}>
                                {formatearMoneda(Math.abs(estado.saldo))}
                            </p>
                            <p className="mt-1 mb-0 text-xs text-texto-suave">
                                {estado.saldo === 0
                                    ? t('privado.cuentaCorriente.saldoCero')
                                    : deudor
                                      ? t('privado.cuentaCorriente.saldoDeudor')
                                      : t('privado.cuentaCorriente.saldoFavor')}
                            </p>
                        </div>
                        <div className="rounded-2xl border border-borde bg-superficie p-5">
                            <p className="m-0 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                                {t('privado.cuentaCorriente.notasDisponibles')}
                            </p>
                            <p className="mt-2 mb-0 text-2xl font-bold text-texto">
                                {formatearMoneda(estado.saldoNotasCredito)}
                            </p>
                        </div>
                        <div className="rounded-2xl border border-borde bg-superficie p-5">
                            <p className="m-0 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                                {t('privado.cuentaCorriente.limite')}
                            </p>
                            <p className="mt-2 mb-0 text-2xl font-bold text-texto">
                                {formatearMoneda(estado.limiteCuentaCorriente)}
                            </p>
                        </div>
                    </section>

                    <div role="tablist" className="mt-8 flex flex-wrap gap-1 border-b border-borde">
                        {pestanas.map((item) => (
                            <button
                                key={item.clave}
                                type="button"
                                role="tab"
                                aria-selected={pestana === item.clave}
                                onClick={() => setPestana(item.clave)}
                                className={`-mb-px rounded-none border-0 border-b-2 bg-transparent px-4 py-2 text-sm font-semibold ${
                                    pestana === item.clave
                                        ? 'border-primario text-primario'
                                        : 'border-transparent text-texto-suave hover:text-texto'
                                }`}
                            >
                                {item.etiqueta}
                            </button>
                        ))}
                    </div>

                    <div className="mt-4">
                        {pestana === 'movimientos' && (
                            <TablaAbm
                                columnas={columnasMovimientos}
                                filas={estado.movimientos}
                                claveDe={(fila) => fila.movimiento.movimientoId}
                                mensajeVacio={t('privado.cuentaCorriente.vacio')}
                            />
                        )}
                        {pestana === 'facturas' && (
                            <TablaAbm
                                columnas={columnasFacturas}
                                filas={estado.facturas}
                                claveDe={(fila) => fila.factura.facturaId}
                                inactiva={(fila) => fila.factura.estado === 'ANULADA'}
                                mensajeVacio={t('privado.cuentaCorriente.vacio')}
                            />
                        )}
                        {pestana === 'pagos' && (
                            <TablaAbm
                                columnas={columnasPagos}
                                filas={estado.pagos}
                                claveDe={(fila) => fila.pago.pagoId}
                                mensajeVacio={t('privado.cuentaCorriente.vacio')}
                            />
                        )}
                        {pestana === 'notas' && (
                            <TablaAbm
                                columnas={columnasNotas}
                                filas={estado.notas}
                                claveDe={(fila) => fila.nota.notaId}
                                mensajeVacio={t('privado.cuentaCorriente.vacio')}
                            />
                        )}
                    </div>
                </>
            )}
        </div>
    );
}
