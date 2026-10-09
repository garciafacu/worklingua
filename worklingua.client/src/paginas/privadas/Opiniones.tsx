import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';
import { caracteristicasApi } from '../../api/caracteristicasApi';
import { ErrorApi } from '../../api/clienteHttp';
import { opinionesApi } from '../../api/opinionesApi';
import { planesApi } from '../../api/planesApi';
import { Alerta } from '../../componentes/Alerta';
import { BuscadorOpiniones } from '../../componentes/BuscadorOpiniones';
import { DistribucionValoraciones } from '../../componentes/DistribucionValoraciones';
import { FormularioOpinion } from '../../componentes/FormularioOpinion';
import { ListaOpiniones } from '../../componentes/ListaOpiniones';
import { SelectorPlanOpinion } from '../../componentes/SelectorPlanOpinion';
import type { CaracteristicaResponse } from '../../tipos/caracteristicas';
import type { CriteriosOpinion, OpinionResponse } from '../../tipos/opiniones';
import type { PlanResponse } from '../../tipos/planes';

/**
 * Qué está mostrando la lista.
 *
 * `plan` usa `GET /api/comentarios` (las opiniones del plan elegido, punto 20)
 * y `busqueda` usa `GET /api/comentarios/buscar` (punto 15).
 */
type Consulta =
    | { tipo: 'plan' }
    | { tipo: 'busqueda'; criterios: CriteriosOpinion };

const CONSULTA_INICIAL: Consulta = { tipo: 'plan' };

/**
 * La valoración propia junto con el plan al que corresponde. Guardar el plan
 * evita mostrar, mientras llega la respuesta, la valoración del plan anterior.
 */
interface ValoracionDePlan {
    planId: number;
    valoracion: OpinionResponse | null;
}

/**
 * Opiniones sobre los planes (punto 20) y búsqueda privada (punto 15).
 *
 * Vive en el área privada y requiere el permiso Comentario.Participar.
 * `RutaConPermiso` evita entrar sin ese permiso, pero quien autoriza de verdad
 * es el backend: los cuatro endpoints exigen el permiso en cada pedido.
 *
 * La sección se llama Opiniones de cara al usuario; la entidad persistida sigue
 * siendo `Comentario`, y por eso la API y los ids no cambiaron de nombre.
 */
export function Opiniones() {
    const { t } = useTranslation();

    // Declarado antes de los efectos que lo usan: si viviera mas abajo, el
    // efecto lo capturaria antes de existir y no se actualizaria al cambiar de
    // idioma (regla react-hooks del linter).
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    const [parametros] = useSearchParams();
    const [planes, setPlanes] = useState<PlanResponse[]>([]);
    const [caracteristicas, setCaracteristicas] = useState<CaracteristicaResponse[]>([]);
    const [planId, setPlanId] = useState<number | null>(null);
    const [opiniones, setOpiniones] = useState<OpinionResponse[]>([]);
    const [consulta, setConsulta] = useState<Consulta>(CONSULTA_INICIAL);
    /** Fuerza releer la lista después de publicar o eliminar, sin cambiar la consulta. */
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

    // El plan puede venir del enlace del catálogo (?planId=N); si no, se toma el
    // primero del listado, que llega ordenado por precio. Las características se
    // piden junto con los planes porque las tarjetas del selector las necesitan.
    useEffect(() => {
        const pedido = Number.parseInt(parametros.get('planId') ?? '', 10);

        Promise.all([planesApi.listar(), caracteristicasApi.listar()])
            .then(([listado, listaCaracteristicas]) => {
                setPlanes(listado);
                setCaracteristicas(listaCaracteristicas);

                const elegido = listado.find((plan) => plan.planId === pedido) ?? listado[0];
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

    // La valoración propia se pide aparte de la lista: con una búsqueda activa la
    // lista no tiene por qué incluirla, y el formulario la necesita igual.
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

    /** Relee los planes para que las tarjetas muestren el promedio actualizado. */
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
        // Si ya se estaba viendo el plan, `consulta` no cambia y el efecto no
        // volvería a correr, dejando el indicador de carga encendido para
        // siempre. El contador fuerza la relectura.
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
            // Se vuelve al listado del plan para que la valoración recién guardada
            // quede visible aunque se estuviera viendo una búsqueda.
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
            // Se relee en vez de sacar la fila en memoria: la baja es lógica y el
            // servidor es el que decide qué sigue visible.
            setBuscando(true);
            setRecarga((valor) => valor + 1);
            refrescarPlanes();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'privado.opiniones.errorEliminar'));
        } finally {
            setEliminando(false);
        }
    }

    const planActual = planes.find((plan) => plan.planId === planId) ?? null;
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

            {planActual && (
                <div className="mt-8 space-y-8">
                    <SelectorPlanOpinion
                        planes={planes}
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
