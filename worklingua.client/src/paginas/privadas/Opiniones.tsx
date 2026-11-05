import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';
import { opinionesApi } from '../../api/opinionesApi';
import { caracteristicasApi, planesApi } from '../../api/planesApi';
import { Alerta } from '../../componentes/Alerta';
import { BuscadorOpiniones } from '../../componentes/BuscadorOpiniones';
import { DistribucionValoraciones } from '../../componentes/Estrellas';
import { FormularioOpinion } from '../../componentes/FormularioOpinion';
import { ListaOpiniones } from '../../componentes/ListaOpiniones';
import { SelectorPlanOpinion } from '../../componentes/SelectorPlanOpinion';
import { useMensajeDeError } from '../../hooks/useMensajeDeError';
import type { CriteriosOpinion, OpinionResponse } from '../../tipos/opiniones';
import type { CaracteristicaResponse, PlanResponse } from '../../tipos/planes';

type Consulta =
    | { tipo: 'plan' }
    | { tipo: 'busqueda'; criterios: CriteriosOpinion };

const CONSULTA_INICIAL: Consulta = { tipo: 'plan' };

interface ValoracionDePlan {
    planId: number;
    valoracion: OpinionResponse | null;
}

export function Opiniones() {
    const { t } = useTranslation();

    const mensajeDeError = useMensajeDeError();

    const [parametros] = useSearchParams();
    const [planes, setPlanes] = useState<PlanResponse[]>([]);
    const [planesContratados, setPlanesContratados] = useState<number[]>([]);
    const [caracteristicas, setCaracteristicas] = useState<CaracteristicaResponse[]>([]);
    const [planId, setPlanId] = useState<number | null>(null);
    const [opiniones, setOpiniones] = useState<OpinionResponse[]>([]);
    const [consulta, setConsulta] = useState<Consulta>(CONSULTA_INICIAL);
    const [recarga, setRecarga] = useState(0);
    const [confirmando, setConfirmando] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [buscando, setBuscando] = useState(true);
    const [publicando, setPublicando] = useState(false);
    const [eliminando, setEliminando] = useState(false);
    const [valoracionPropia, setValoracionPropia] = useState<ValoracionDePlan | null>(null);

    const valoracionCargada = valoracionPropia !== null && valoracionPropia.planId === planId;
    const valoracionActual = valoracionCargada ? valoracionPropia.valoracion : null;

    useEffect(() => {
        const pedido = Number.parseInt(parametros.get('planId') ?? '', 10);

        Promise.all([
            planesApi.listar(),
            caracteristicasApi.listar(),
            opinionesApi.listarPlanesContratados(),
        ])
            .then(([listado, listaCaracteristicas, contratados]) => {
                setPlanes(listado);
                setCaracteristicas(listaCaracteristicas);
                setPlanesContratados(contratados);

                const valorables = listado.filter((plan) => contratados.includes(plan.planId));
                const elegido = valorables.find((plan) => plan.planId === pedido) ?? valorables[0];
                setPlanId(elegido?.planId ?? null);
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'privado.opiniones.errorPlanes')),
            )
            .finally(() => setCargando(false));
    }, [parametros, mensajeDeError]);

    useEffect(() => {
        if (planId === null) {
            return;
        }

        const pedido =
            consulta.tipo === 'plan'
                ? opinionesApi.listar(planId)
                : opinionesApi.buscar(consulta.criterios);

        pedido
            .then((resultado) => {
                setOpiniones(resultado);
                setError(null);
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'privado.opiniones.errorOpiniones')),
            )
            .finally(() => setBuscando(false));
    }, [consulta, planId, recarga, mensajeDeError]);

    useEffect(() => {
        if (planId === null) {
            return;
        }

        let vigente = true;

        opinionesApi
            .obtenerValoracion(planId)
            .then((valoracion) => {
                if (vigente) {
                    setValoracionPropia({ planId, valoracion: valoracion ?? null });
                }
            })
            .catch((excepcion) => {
                if (vigente) {
                    setError(mensajeDeError(excepcion, 'privado.opiniones.errorValoracion'));
                }
            });

        return () => {
            vigente = false;
        };
    }, [planId, recarga, mensajeDeError]);

    function refrescarPlanes() {
        planesApi
            .listar()
            .then(setPlanes)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'privado.opiniones.errorPlanes')),
            );
    }

    function cambiarPlan(nuevo: number) {
        setBuscando(true);
        setExito(null);
        setConfirmando(null);
        setPlanId(nuevo);
        setConsulta(CONSULTA_INICIAL);
    }

    function buscar(criterios: CriteriosOpinion) {
        setBuscando(true);
        setExito(null);
        setConfirmando(null);
        setConsulta({ tipo: 'busqueda', criterios });
    }

    function volverAlPlan() {
        setBuscando(true);
        setConfirmando(null);
        setConsulta(CONSULTA_INICIAL);
        setRecarga((valor) => valor + 1);
    }

    async function guardar(puntaje: number, texto: string): Promise<boolean> {
        if (planId === null) {
            return false;
        }

        const eraEdicion = valoracionActual !== null;

        setPublicando(true);
        setError(null);
        setExito(null);

        try {
            const guardada = await opinionesApi.guardar({ planId, puntaje, texto });

            setValoracionPropia({ planId, valoracion: guardada });
            setBuscando(true);
            setConsulta(CONSULTA_INICIAL);
            setRecarga((valor) => valor + 1);
            refrescarPlanes();
            setExito(
                t(
                    eraEdicion
                        ? 'privado.opiniones.exitoActualizar'
                        : 'privado.opiniones.exitoPublicar',
                ),
            );

            return true;
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'privado.opiniones.errorPublicar'));

            return false;
        } finally {
            setPublicando(false);
        }
    }

    async function eliminar(comentarioId: number) {
        setEliminando(true);
        setError(null);
        setExito(null);

        try {
            await opinionesApi.eliminar(comentarioId);

            setConfirmando(null);
            setExito(t('privado.opiniones.exitoEliminar'));
            setBuscando(true);
            setRecarga((valor) => valor + 1);
            refrescarPlanes();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'privado.opiniones.errorEliminar'));
        } finally {
            setEliminando(false);
        }
    }

    const planesValorables = planes.filter((plan) => planesContratados.includes(plan.planId));
    const planActual = planesValorables.find((plan) => plan.planId === planId) ?? null;
    const enBusqueda = consulta.tipo === 'busqueda';

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.opiniones.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">
                {t('privado.opiniones.descripcion')}
            </p>

            <div className="mt-6 space-y-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {cargando && (
                <p className="mt-6 text-sm text-texto-suave">
                    {t('privado.opiniones.cargandoPlanes')}
                </p>
            )}

            {!cargando && !error && planes.length === 0 && (
                <p className="mt-6 text-sm text-texto-suave">
                    {t('privado.opiniones.sinPlanes')}
                </p>
            )}

            {!cargando && !error && planes.length > 0 && planesValorables.length === 0 && (
                <p className="mt-6 text-sm text-texto-suave">
                    {t('privado.opiniones.sinContratados')}
                </p>
            )}

            {planActual && (
                <div className="mt-8 space-y-8">
                    <SelectorPlanOpinion
                        planes={planesValorables}
                        caracteristicas={caracteristicas}
                        planId={planActual.planId}
                        alSeleccionar={cambiarPlan}
                    />

                    {valoracionCargada && (
                        <FormularioOpinion
                            key={`${planActual.planId}-${valoracionActual?.comentarioId ?? 'nueva'}`}
                            nombrePlan={planActual.nombre}
                            valoracionPropia={valoracionActual}
                            enviando={publicando}
                            alGuardar={guardar}
                        />
                    )}

                    <BuscadorOpiniones
                        planes={planes}
                        buscando={buscando}
                        alBuscar={buscar}
                        alLimpiar={volverAlPlan}
                    />

                    <section>
                        <h2 className="text-lg font-semibold text-texto">
                            {enBusqueda
                                ? t('privado.opiniones.resultados')
                                : t('privado.opiniones.sobrePlan', {
                                      plan: planActual.nombre,
                                  })}
                        </h2>

                        {!enBusqueda && !buscando && (
                            <div className="mt-4">
                                <DistribucionValoraciones
                                    promedio={planActual.promedioValoracion}
                                    cantidad={planActual.cantidadValoraciones}
                                    opiniones={opiniones}
                                />
                            </div>
                        )}

                        <div className="mt-4">
                            {buscando ? (
                                <p className="text-sm text-texto-suave">
                                    {t('privado.opiniones.cargando')}
                                </p>
                            ) : (
                                <ListaOpiniones
                                    opiniones={opiniones}
                                    mostrarPlan={enBusqueda}
                                    mensajeVacio={
                                        enBusqueda
                                            ? t('privado.opiniones.vacioBusqueda')
                                            : t('privado.opiniones.vacioPlan')
                                    }
                                    confirmando={confirmando}
                                    eliminando={eliminando}
                                    alPedirConfirmacion={setConfirmando}
                                    alEliminar={eliminar}
                                />
                            )}
                        </div>
                    </section>
                </div>
            )}
        </div>
    );
}
