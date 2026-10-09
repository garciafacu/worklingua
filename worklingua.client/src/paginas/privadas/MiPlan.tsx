import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { caracteristicasApi } from '../../api/caracteristicasApi';
import { ErrorApi } from '../../api/clienteHttp';
import { contratacionesApi } from '../../api/contratacionesApi';
import { planesApi } from '../../api/planesApi';
import { suscripcionesApi } from '../../api/suscripcionesApi';
import { Alerta } from '../../componentes/Alerta';
import { Boton } from '../../componentes/Boton';
import { FormularioContratacion } from '../../componentes/FormularioContratacion';
import { Modal } from '../../componentes/Modal';
import { TarjetaPlan } from '../../componentes/TarjetaPlan';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import { useSesion } from '../../contexto/useSesion';
import { PERMISOS } from '../../rutas/itemsMenu';
import type { CaracteristicaResponse } from '../../tipos/caracteristicas';
import type { ContratacionResponse } from '../../tipos/contrataciones';
import type { PlanResponse } from '../../tipos/planes';
import type { SuscripcionResponse } from '../../tipos/suscripciones';

/**
 * El plan de la empresa y el catálogo completo, con la contratación.
 *
 * Cada plan que no es el vigente ofrece "Contratar", que abre el formulario de
 * medios de pago. La contratación vigente paga se puede cancelar: el backend
 * anula la factura, emite una NC total y devuelve la empresa al plan Free.
 * Contratar y cancelar exigen `Suscripcion.Contratar`; sin ese permiso la
 * pantalla queda en modo consulta.
 *
 * `?contratar=<planId>` abre el formulario directamente: es el enlace que usa el
 * Catálogo público.
 */
export function MiPlan() {
    const { t } = useTranslation();
    const { cultura, formatearFecha } = useLocalizacion();
    const { tienePermiso } = useSesion();
    const [parametros, setParametros] = useSearchParams();

    const [suscripcion, setSuscripcion] = useState<SuscripcionResponse | null>(null);
    const [planes, setPlanes] = useState<PlanResponse[]>([]);
    const [caracteristicas, setCaracteristicas] = useState<CaracteristicaResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [confirmandoCancelacion, setConfirmandoCancelacion] = useState(false);
    const [cancelando, setCancelando] = useState(false);
    const [errorCancelacion, setErrorCancelacion] = useState<string | null>(null);

    const puedeContratar = tienePermiso(PERMISOS.suscripcionContratar);
    const planIdAContratar = Number(parametros.get('contratar')) || null;

    // La suscripción se pide aparte de los catálogos porque su 404 no es un
    // error de pantalla: significa "esta empresa todavía no tiene plan", que es
    // el caso de las creadas antes del auto-registro.
    const pedirSuscripcion = useCallback(
        () =>
            suscripcionesApi.obtenerLaMia().catch((excepcion: unknown) => {
                if (excepcion instanceof ErrorApi && excepcion.estado === 404) {
                    return null;
                }

                throw excepcion;
            }),
        [],
    );

    useEffect(() => {
        Promise.all([pedirSuscripcion(), planesApi.listar(), caracteristicasApi.listar()])
            .then(([laMia, listaPlanes, listaCaracteristicas]) => {
                setSuscripcion(laMia);
                setPlanes(listaPlanes);
                setCaracteristicas(listaCaracteristicas);
            })
            .catch((excepcion) =>
                setError(
                    excepcion instanceof ErrorApi
                        ? excepcion.message
                        : t('privado.miPlan.error'),
                ),
            )
            .finally(() => setCargando(false));
    }, [t, pedirSuscripcion]);

    function recargarSuscripcion() {
        pedirSuscripcion()
            .then(setSuscripcion)
            .catch((excepcion) =>
                setError(
                    excepcion instanceof ErrorApi ? excepcion.message : t('privado.miPlan.error'),
                ),
            );
    }

    function abrirContratacion(planId: number) {
        setExito(null);
        setParametros({ contratar: String(planId) }, { replace: true });
    }

    function cerrarContratacion() {
        setParametros({}, { replace: true });
    }

    function alContratado(resultado: ContratacionResponse) {
        cerrarContratacion();
        setExito(
            t('privado.contratacion.resultado.ACTIVA', { plan: resultado.plan }) +
                (resultado.numeroFactura
                    ? ' ' + t('privado.contratacion.resultado.factura', { numero: resultado.numeroFactura })
                    : ''),
        );
        recargarSuscripcion();
    }

    async function cancelarContratacion() {
        if (!suscripcion) {
            return;
        }

        setErrorCancelacion(null);
        setCancelando(true);

        try {
            const resultado = await contratacionesApi.cancelar(
                suscripcion.suscripcionId,
                cultura?.codigo ?? '',
            );
            const notaCredito = resultado.notas.find((nota) => nota.tipo === 'NC');

            setConfirmandoCancelacion(false);
            setExito(
                t('privado.miPlan.exitoCancelacion', { plan: resultado.plan }) +
                    (notaCredito
                        ? ' ' + t('privado.miPlan.notaCreditoEmitida', { numero: notaCredito.numero })
                        : ''),
            );
            recargarSuscripcion();
        } catch (excepcion) {
            setErrorCancelacion(
                excepcion instanceof ErrorApi ? excepcion.message : t('privado.miPlan.errorCancelar'),
            );
        } finally {
            setCancelando(false);
        }
    }

    const idsVisibles = caracteristicas.map((fila) => fila.caracteristicaId);
    const planAContratar = puedeContratar
        ? (planes.find((plan) => plan.planId === planIdAContratar && plan.planId !== suscripcion?.planId) ?? null)
        : null;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.miPlan.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.miPlan.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {cargando && (
                <p className="mt-6 text-sm text-texto-suave">{t('privado.miPlan.cargando')}</p>
            )}

            {!cargando && !error && (
                <>
                    <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                        <h2 className="text-lg font-semibold text-texto">
                            {t('privado.miPlan.planActual')}
                        </h2>

                        {suscripcion ? (
                            <div className="mt-3 flex flex-wrap items-center justify-between gap-4">
                                <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
                                    <p className="m-0 text-2xl font-bold tracking-tight text-texto">
                                        {suscripcion.plan}
                                    </p>
                                    <span className="rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario">
                                        {t(`comun.contratacion.estado.${suscripcion.estado}`)}
                                    </span>
                                    <p className="m-0 text-sm text-texto-suave">
                                        {t('privado.miPlan.vigenteDesde', {
                                            fecha: formatearFecha(suscripcion.fechaInicio),
                                        })}
                                        {suscripcion.fechaFin &&
                                            ' · ' +
                                                t('privado.miPlan.vigenteHasta', {
                                                    fecha: formatearFecha(suscripcion.fechaFin),
                                                })}
                                    </p>
                                </div>

                                {puedeContratar && suscripcion.cancelable && (
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setErrorCancelacion(null);
                                            setConfirmandoCancelacion(true);
                                        }}
                                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {t('privado.miPlan.cancelar')}
                                    </button>
                                )}
                            </div>
                        ) : (
                            <p className="mt-3 mb-0 text-sm text-texto-suave">
                                {t('privado.miPlan.sinSuscripcion')}
                            </p>
                        )}

                        {!puedeContratar && (
                            <p className="ayuda mt-4 mb-0">{t('privado.miPlan.sinPermiso')}</p>
                        )}
                    </section>

                    {planes.length > 0 && (
                        <section className="mt-8">
                            <h2 className="text-lg font-semibold text-texto">
                                {t('privado.miPlan.todosLosPlanes')}
                            </h2>

                            <div className="mt-4 grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                                {planes.map((plan) => {
                                    const esElSuyo = suscripcion?.planId === plan.planId;

                                    return (
                                        <TarjetaPlan
                                            key={plan.planId}
                                            plan={plan}
                                            caracteristicas={caracteristicas}
                                            idsVisibles={idsVisibles}
                                            idActivo={null}
                                            alSenalarCaracteristica={() => {}}
                                            seleccionada={esElSuyo}
                                            pie={
                                                esElSuyo ? (
                                                    <p className="m-0 text-sm font-semibold text-primario">
                                                        {t('privado.miPlan.esTuPlan')}
                                                    </p>
                                                ) : puedeContratar ? (
                                                    <Boton onClick={() => abrirContratacion(plan.planId)}>
                                                        {t('privado.miPlan.contratar')}
                                                    </Boton>
                                                ) : undefined
                                            }
                                        />
                                    );
                                })}
                            </div>
                        </section>
                    )}

                    <section className="mt-8 rounded-2xl border border-borde bg-superficie p-6">
                        <p className="m-0 text-sm text-texto-suave">
                            {t('privado.miPlan.avisoContratacion')}
                        </p>
                        <Link
                            to="/contacto"
                            className="mt-4 inline-block rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                        >
                            {t('privado.miPlan.contactar')}
                        </Link>
                    </section>
                </>
            )}

            <Modal
                titulo={planAContratar ? t('privado.contratacion.titulo', { plan: planAContratar.nombre }) : ''}
                descripcion={t('privado.contratacion.descripcion')}
                abierto={planAContratar !== null}
                alCerrar={cerrarContratacion}
            >
                {planAContratar && (
                    <FormularioContratacion
                        key={planAContratar.planId}
                        plan={planAContratar}
                        alContratado={alContratado}
                        alCancelar={cerrarContratacion}
                    />
                )}
            </Modal>

            <Modal
                titulo={t('privado.miPlan.confirmarCancelacion.titulo', { plan: suscripcion?.plan ?? '' })}
                abierto={confirmandoCancelacion}
                alCerrar={() => setConfirmandoCancelacion(false)}
            >
                <div className="flex flex-col gap-4">
                    <p className="m-0 text-sm text-texto">
                        {t('privado.miPlan.confirmarCancelacion.descripcion')}
                    </p>
                    <Alerta tipo="error" mensaje={errorCancelacion} />
                    <div className="flex flex-wrap gap-3">
                        <Boton cargando={cancelando} onClick={() => void cancelarContratacion()}>
                            {t('privado.miPlan.confirmarCancelacion.confirmar')}
                        </Boton>
                        <button
                            type="button"
                            onClick={() => setConfirmandoCancelacion(false)}
                            className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                        >
                            {t('privado.miPlan.confirmarCancelacion.volver')}
                        </button>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
