import { useEffect, useId, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../api/clienteHttp';
import { contratacionesApi } from '../api/contratacionesApi';
import { useLocalizacion } from '../contexto/useLocalizacion';
import type {
    ContratacionResponse,
    CotizacionContratacionResponse,
} from '../tipos/contrataciones';
import type { PlanResponse } from '../tipos/planes';
import { Alerta } from './Alerta';
import { Boton } from './Boton';
import { CampoTexto } from './CampoTexto';

interface FormularioContratacionProps {
    plan: PlanResponse;
    /** Se llama solo con una contratación aprobada; un rechazo se muestra acá. */
    alContratado: (resultado: ContratacionResponse) => void;
    alCancelar: () => void;
}

const TARJETA_VACIA = { numero: '', titular: '', vencimiento: '', codigoSeguridad: '' };

/** Diferencia que se tolera al comparar importes convertidos y redondeados a centavos. */
const TOLERANCIA = 0.005;

function aCentavos(valor: number): number {
    return Math.round(valor * 100) / 100;
}

function leerImporte(texto: string): number {
    const valor = Number(texto.replace(',', '.'));

    return Number.isFinite(valor) ? valor : Number.NaN;
}

/**
 * Contratación de un plan: resumen, medios de pago y datos de la tarjeta.
 *
 * Los medios se combinan así: lo que se marca de saldo a favor (NC) y de cuenta
 * corriente se escribe en la moneda de la operación, y la tarjeta paga el resto.
 * El backend recalcula todo en ARS y es quien decide: los topes de acá son solo
 * para avisar antes de enviar.
 */
export function FormularioContratacion({ plan, alContratado, alCancelar }: FormularioContratacionProps) {
    const { t } = useTranslation();
    const { cultura, formatearMoneda } = useLocalizacion();
    const idFormulario = useId();
    const codigoCultura = cultura?.codigo ?? '';

    const [cotizacion, setCotizacion] = useState<CotizacionContratacionResponse | null>(null);
    const [usarNotaCredito, setUsarNotaCredito] = useState(false);
    const [usarCuentaCorriente, setUsarCuentaCorriente] = useState(false);
    const [importeNotaCredito, setImporteNotaCredito] = useState('');
    const [importeCuentaCorriente, setImporteCuentaCorriente] = useState('');
    const [tarjeta, setTarjeta] = useState(TARJETA_VACIA);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [enviando, setEnviando] = useState(false);

    useEffect(() => {
        contratacionesApi
            .cotizar(plan.planId, codigoCultura)
            .then(setCotizacion)
            .catch((excepcion: unknown) =>
                setError(
                    excepcion instanceof ErrorApi
                        ? excepcion.message
                        : t('privado.contratacion.errorCotizar'),
                ),
            )
            .finally(() => setCargando(false));
    }, [plan.planId, codigoCultura, t]);

    if (cargando) {
        return <p className="text-sm text-texto-suave">{t('privado.contratacion.cargando')}</p>;
    }

    if (!cotizacion) {
        return <Alerta tipo="error" mensaje={error} />;
    }

    const tasa = cotizacion.tasaConversion;
    const aMoneda = (base: number) => aCentavos(base * tasa);
    const aBase = (enMoneda: number) => aCentavos(enMoneda / tasa);

    const esGratuito = cotizacion.importe === 0;
    const notaCredito = usarNotaCredito ? leerImporte(importeNotaCredito) : 0;
    const cuentaCorriente = usarCuentaCorriente ? leerImporte(importeCuentaCorriente) : 0;
    const totalMoneda = aMoneda(cotizacion.importe);
    const restoTarjeta = Math.max(
        0,
        aCentavos(cotizacion.importe - aBase(notaCredito || 0) - aBase(cuentaCorriente || 0)),
    );
    const recargo = aCentavos(
        (aBase(cuentaCorriente || 0) * cotizacion.porcentajeRecargoCuentaCorriente) / 100,
    );
    const pideTarjeta = !esGratuito && restoTarjeta > TOLERANCIA;

    function alternarNotaCredito(marcado: boolean) {
        setUsarNotaCredito(marcado);

        if (marcado && cotizacion) {
            const tope = Math.min(cotizacion.notaCreditoUtilizable, cotizacion.importe);
            setImporteNotaCredito(String(aMoneda(tope)));
        }
    }

    function alternarCuentaCorriente(marcado: boolean) {
        setUsarCuentaCorriente(marcado);

        if (marcado && cotizacion) {
            const restante = Math.max(0, cotizacion.importe - aBase(notaCredito || 0));
            const tope = Math.min(cotizacion.disponibleCuentaCorriente, restante);
            setImporteCuentaCorriente(String(aMoneda(tope)));
        }
    }

    function validar(): string | null {
        if (!cotizacion) {
            return null;
        }

        if (Number.isNaN(notaCredito) || Number.isNaN(cuentaCorriente) || notaCredito < 0 || cuentaCorriente < 0) {
            return t('privado.contratacion.validacion.importeInvalido');
        }

        if (notaCredito + cuentaCorriente > totalMoneda + TOLERANCIA * 2) {
            return t('privado.contratacion.validacion.superaTotal');
        }

        if (notaCredito > aMoneda(cotizacion.notaCreditoUtilizable) + TOLERANCIA * 2) {
            return t('privado.contratacion.validacion.superaSaldoFavor');
        }

        if (cuentaCorriente > aMoneda(cotizacion.disponibleCuentaCorriente) + TOLERANCIA * 2) {
            return t('privado.contratacion.validacion.superaCuentaCorriente');
        }

        const tarjetaIncompleta = Object.values(tarjeta).some((valor) => valor.trim() === '');

        if (pideTarjeta && tarjetaIncompleta) {
            return t('privado.contratacion.validacion.tarjetaIncompleta');
        }

        return null;
    }

    async function confirmar(evento: FormEvent) {
        evento.preventDefault();

        const problema = validar();
        setError(problema);

        if (problema) {
            return;
        }

        setEnviando(true);

        try {
            const resultado = await contratacionesApi.contratar({
                planId: plan.planId,
                codigoCultura,
                importeNotaCredito: notaCredito,
                importeCuentaCorriente: cuentaCorriente,
                tarjeta: pideTarjeta ? tarjeta : null,
            });

            if (resultado.estado === 'ACTIVA') {
                alContratado(resultado);

                return;
            }

            setError(
                t('privado.contratacion.resultado.RECHAZADA', { motivo: resultado.motivo ?? '' }),
            );
        } catch (excepcion) {
            setError(
                excepcion instanceof ErrorApi
                    ? excepcion.message
                    : t('privado.contratacion.errorContratar'),
            );
        } finally {
            setEnviando(false);
        }
    }

    const filasResumen: { etiqueta: string; valor: string }[] = [
        { etiqueta: t('privado.contratacion.resumen.importe'), valor: formatearMoneda(cotizacion.importe) },
        {
            etiqueta: t('privado.contratacion.resumen.moneda'),
            valor: `${cotizacion.moneda} (${cotizacion.simboloMoneda})`,
        },
    ];

    if (cotizacion.planActual) {
        filasResumen.unshift({
            etiqueta: t('privado.contratacion.resumen.planActual'),
            valor: cotizacion.planActual,
        });
    }

    if (cotizacion.creditoCambioPlan > 0) {
        filasResumen.push({
            etiqueta: t('privado.contratacion.resumen.credito'),
            valor: formatearMoneda(cotizacion.creditoCambioPlan),
        });
    }

    if (!esGratuito) {
        filasResumen.push(
            {
                etiqueta: t('privado.contratacion.resumen.saldoFavor'),
                valor: formatearMoneda(cotizacion.notaCreditoUtilizable),
            },
            {
                etiqueta: t('privado.contratacion.resumen.cuentaCorriente'),
                valor: formatearMoneda(cotizacion.disponibleCuentaCorriente),
            },
        );
    }

    return (
        <form id={idFormulario} onSubmit={confirmar} noValidate className="flex flex-col gap-5">
            <dl className="m-0 divide-y divide-borde rounded-xl border border-borde text-sm">
                {filasResumen.map((fila) => (
                    <div key={fila.etiqueta} className="flex items-baseline justify-between gap-4 px-4 py-2.5">
                        <dt className="text-texto-suave">{fila.etiqueta}</dt>
                        <dd className="m-0 text-right font-semibold text-texto">{fila.valor}</dd>
                    </div>
                ))}
            </dl>

            <p className="ayuda m-0">{t('privado.contratacion.resumen.vigencia')}</p>

            {esGratuito ? (
                <p className="m-0 text-sm text-texto-suave">{t('privado.contratacion.medios.sinCargo')}</p>
            ) : (
                <fieldset className="m-0 flex flex-col gap-4 border-0 p-0">
                    <legend className="mb-2 text-sm font-semibold text-texto">
                        {t('privado.contratacion.medios.titulo')}
                    </legend>

                    <label className="flex items-center gap-2.5 text-sm text-texto">
                        <input
                            type="checkbox"
                            className="size-4"
                            disabled={cotizacion.notaCreditoUtilizable <= 0}
                            checked={usarNotaCredito}
                            onChange={(evento) => alternarNotaCredito(evento.target.checked)}
                        />
                        {t('privado.contratacion.medios.notaCredito')}
                    </label>

                    {usarNotaCredito && (
                        <CampoTexto
                            etiqueta={t('privado.contratacion.medios.importe', { moneda: cotizacion.moneda })}
                            identificador="contratacion-importeNotaCredito"
                            type="number"
                            inputMode="decimal"
                            min={0}
                            step="0.01"
                            value={importeNotaCredito}
                            onChange={(evento) => setImporteNotaCredito(evento.target.value)}
                        />
                    )}

                    <label className="flex items-center gap-2.5 text-sm text-texto">
                        <input
                            type="checkbox"
                            className="size-4"
                            disabled={cotizacion.disponibleCuentaCorriente <= 0}
                            checked={usarCuentaCorriente}
                            onChange={(evento) => alternarCuentaCorriente(evento.target.checked)}
                        />
                        {t('privado.contratacion.medios.cuentaCorriente')}
                    </label>

                    {usarCuentaCorriente && (
                        <>
                            <CampoTexto
                                etiqueta={t('privado.contratacion.medios.importe', { moneda: cotizacion.moneda })}
                                identificador="contratacion-importeCuentaCorriente"
                                type="number"
                                inputMode="decimal"
                                min={0}
                                step="0.01"
                                value={importeCuentaCorriente}
                                onChange={(evento) => setImporteCuentaCorriente(evento.target.value)}
                            />
                            {recargo > 0 && (
                                <p className="ayuda m-0">
                                    {t('privado.contratacion.medios.recargo', {
                                        porcentaje: cotizacion.porcentajeRecargoCuentaCorriente,
                                        importe: formatearMoneda(recargo),
                                    })}
                                </p>
                            )}
                        </>
                    )}

                    <p className="m-0 text-sm font-semibold text-texto">
                        {t('privado.contratacion.medios.tarjeta', { importe: formatearMoneda(restoTarjeta) })}
                    </p>

                    {pideTarjeta && (
                        <div className="flex flex-col gap-4">
                            <CampoTexto
                                etiqueta={t('privado.contratacion.tarjeta.numero')}
                                identificador="contratacion-numeroTarjeta"
                                inputMode="numeric"
                                autoComplete="cc-number"
                                maxLength={23}
                                value={tarjeta.numero}
                                onChange={(evento) => setTarjeta({ ...tarjeta, numero: evento.target.value })}
                            />
                            <CampoTexto
                                etiqueta={t('privado.contratacion.tarjeta.titular')}
                                identificador="contratacion-titular"
                                autoComplete="cc-name"
                                maxLength={100}
                                value={tarjeta.titular}
                                onChange={(evento) => setTarjeta({ ...tarjeta, titular: evento.target.value })}
                            />
                            <div className="fila">
                                <CampoTexto
                                    etiqueta={t('privado.contratacion.tarjeta.vencimiento')}
                                    identificador="contratacion-vencimiento"
                                    autoComplete="cc-exp"
                                    placeholder="MM/AA"
                                    maxLength={7}
                                    value={tarjeta.vencimiento}
                                    onChange={(evento) =>
                                        setTarjeta({ ...tarjeta, vencimiento: evento.target.value })
                                    }
                                />
                                <CampoTexto
                                    etiqueta={t('privado.contratacion.tarjeta.codigo')}
                                    identificador="contratacion-codigo"
                                    type="password"
                                    inputMode="numeric"
                                    autoComplete="cc-csc"
                                    maxLength={4}
                                    value={tarjeta.codigoSeguridad}
                                    onChange={(evento) =>
                                        setTarjeta({ ...tarjeta, codigoSeguridad: evento.target.value })
                                    }
                                />
                            </div>
                            <p className="ayuda m-0">{t('privado.contratacion.tarjeta.ayuda')}</p>
                        </div>
                    )}
                </fieldset>
            )}

            <Alerta tipo="error" mensaje={error} />

            <div className="flex flex-wrap gap-3">
                <Boton type="submit" cargando={enviando}>
                    {t('privado.contratacion.confirmar', { importe: formatearMoneda(cotizacion.importe) })}
                </Boton>
                <button
                    type="button"
                    onClick={alCancelar}
                    className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                >
                    {t('comun.boton.cancelar')}
                </button>
            </div>
        </form>
    );
}
