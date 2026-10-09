import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { caracteristicasApi } from '../../api/caracteristicasApi';
import { ErrorApi } from '../../api/clienteHttp';
import { planesApi } from '../../api/planesApi';
import { Alerta } from '../../componentes/Alerta';
import { FiltrosPlanes } from '../../componentes/FiltrosPlanes';
import { TarjetaPlan } from '../../componentes/TarjetaPlan';
import { useSesion } from '../../contexto/useSesion';
import {
    conPlanPendiente,
    recordarPlanAContratar,
    rutaContratacion,
} from '../../rutas/contratacionPendiente';
import { PERMISOS } from '../../rutas/itemsMenu';
import type { CaracteristicaResponse } from '../../tipos/caracteristicas';
import type { PlanResponse } from '../../tipos/planes';

/**
 * Sección pública de planes (Free, Premium y Enterprise). La contratación ocurre
 * en la plataforma (`/inicio/plan`): con sesión y permiso, "Contratar" enlaza
 * ahí; sin sesión, lleva al login con el plan elegido para retomarla después.
 *
 * La comparación (punto 14) ocurre sobre las mismas tarjetas: se filtran los
 * planes y las características a mostrar, y la grilla se rearma al instante. No
 * hay pantalla, tabla ni botón de comparación aparte.
 */
const CLASE_BOTON_CONTRATAR =
    'rounded-lg bg-primario px-3 py-1.5 text-sm font-semibold text-white no-underline hover:bg-primario-hover';

export function Planes() {
    const { t } = useTranslation();

    const [planes, setPlanes] = useState<PlanResponse[]>([]);
    const [caracteristicas, setCaracteristicas] = useState<CaracteristicaResponse[]>([]);
    const [planesVisibles, setPlanesVisibles] = useState<number[]>([]);
    const [idsVisibles, setIdsVisibles] = useState<number[]>([]);
    /**
     * Característica señalada. Vive en la sección y no en cada tarjeta para que
     * al apuntar una fila se resalte la misma en todos los planes: es lo que
     * permite leer la comparación de lado a lado.
     */
    const [idActivo, setIdActivo] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const { autenticado, tienePermiso } = useSesion();

    // Los planes traen qué ofrecen en cada característica; el catálogo trae el
    // nombre y el orden, que son uno solo y no se repiten por plan.
    useEffect(() => {
        Promise.all([planesApi.listar(), caracteristicasApi.listar()])
            .then(([listaPlanes, listaCaracteristicas]) => {
                setPlanes(listaPlanes);
                setCaracteristicas(listaCaracteristicas);
                // Arrancan todos marcados: por defecto la sección muestra las
                // tarjetas completas, sin nada filtrado.
                setPlanesVisibles(listaPlanes.map((plan) => plan.planId));
                setIdsVisibles(listaCaracteristicas.map((fila) => fila.caracteristicaId));
            })
            .catch((excepcion) =>
                setError(
                    excepcion instanceof ErrorApi
                        ? excepcion.message
                        : t('publico.planes.errorCargar'),
                ),
            )
            .finally(() => setCargando(false));
        // `t` entra en las dependencias porque cambia de identidad al cambiar
        // de idioma; volver a pedir los planes es inocuo y evita que el mensaje
        // de error quede en el idioma anterior.
    }, [t]);

    function alternarPlan(planId: number) {
        setPlanesVisibles((actuales) =>
            actuales.includes(planId)
                ? actuales.filter((id) => id !== planId)
                : [...actuales, planId],
        );
    }

    function alternarCaracteristica(caracteristicaId: number) {
        setIdsVisibles((actuales) =>
            actuales.includes(caracteristicaId)
                ? actuales.filter((item) => item !== caracteristicaId)
                : [...actuales, caracteristicaId],
        );
    }

    function restablecer() {
        setPlanesVisibles(planes.map((plan) => plan.planId));
        setIdsVisibles(caracteristicas.map((fila) => fila.caracteristicaId));
    }

    // Se filtra sobre `planes` y no sobre `planesVisibles` para que las tarjetas
    // salgan siempre ordenadas por precio, sin importar en qué orden se marcaron.
    // La grilla no lleva `items-start`: estirar las tarjetas a la misma altura es
    // lo que mantiene alineados los pies y las filas de características.
    const planesAMostrar = planes.filter((plan) => planesVisibles.includes(plan.planId));

    return (
        <div className="mx-auto max-w-6xl px-4 py-12 sm:py-16">
            <h1 className="text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                {t('publico.planes.titulo')}
            </h1>
            <p className="mt-3 max-w-2xl text-base text-texto-suave">
                {t('publico.planes.descripcion')}
            </p>

            <div className="mt-8">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {cargando && (
                <p className="mt-8 text-sm text-texto-suave">{t('publico.planes.cargando')}</p>
            )}

            {!cargando && !error && planes.length === 0 && (
                <p className="mt-8 text-sm text-texto-suave">{t('publico.planes.sinPlanes')}</p>
            )}

            {planes.length > 0 && (
                <>
                    <FiltrosPlanes
                        planes={planes}
                        planesVisibles={planesVisibles}
                        caracteristicas={caracteristicas}
                        idsVisibles={idsVisibles}
                        alAlternarPlan={alternarPlan}
                        alAlternarCaracteristica={alternarCaracteristica}
                        alRestablecer={restablecer}
                        idActivo={idActivo}
                        alSenalarCaracteristica={setIdActivo}
                    />

                    {planesAMostrar.length === 0 ? (
                        <p className="mt-6 rounded-2xl border border-dashed border-borde bg-superficie px-6 py-10 text-center text-sm text-texto-suave">
                            {t('publico.planes.sinSeleccion')}
                        </p>
                    ) : (
                        <div className="mt-6 grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                            {planesAMostrar.map((plan) => (
                                <TarjetaPlan
                                    key={plan.planId}
                                    plan={plan}
                                    caracteristicas={caracteristicas}
                                    idsVisibles={idsVisibles}
                                    idActivo={idActivo}
                                    alSenalarCaracteristica={setIdActivo}
                                    pie={
                                        /* Las opiniones son visibles solo para
                                           personas registradas, así que sin sesión el
                                           enlace lleva al login en lugar de a una
                                           pantalla que devolvería 403. Con sesión pero
                                           sin Comentario.Participar, el enlace no se
                                           ofrece: llevaría a un rebote silencioso. */
                                        autenticado ? (
                                            <div className="flex flex-wrap items-center justify-between gap-3">
                                                {tienePermiso(PERMISOS.comentarioParticipar) && (
                                                    <Link
                                                        to={`/inicio/opiniones?planId=${plan.planId}`}
                                                        className="text-sm font-semibold text-primario"
                                                    >
                                                        {t('publico.planes.valorarOpiniones')}
                                                    </Link>
                                                )}
                                                {tienePermiso(PERMISOS.suscripcionContratar) && (
                                                    <Link
                                                        to={rutaContratacion(plan.planId)}
                                                        className={CLASE_BOTON_CONTRATAR}
                                                    >
                                                        {t('publico.planes.contratar')}
                                                    </Link>
                                                )}
                                            </div>
                                        ) : (
                                            <div className="flex flex-wrap items-center justify-between gap-3">
                                                <Link
                                                    to="/login"
                                                    className="text-sm font-medium text-texto-suave"
                                                >
                                                    {t('publico.planes.loginOpiniones')}
                                                </Link>
                                                {/* Sin sesión, contratar empieza por
                                                    ingresar: el plan viaja al login y se
                                                    recuerda por si la cuenta se crea y
                                                    confirma en otra pestaña. */}
                                                <Link
                                                    to={conPlanPendiente('/login', plan.planId)}
                                                    onClick={() => recordarPlanAContratar(plan.planId)}
                                                    className={CLASE_BOTON_CONTRATAR}
                                                >
                                                    {t('publico.planes.contratar')}
                                                </Link>
                                            </div>
                                        )
                                    }
                                />
                            ))}
                        </div>
                    )}
                </>
            )}

            <div className="mt-12 rounded-2xl border border-borde bg-superficie p-6 sm:p-8">
                <h2 className="text-xl font-semibold text-texto">{t('publico.planes.ayuda.titulo')}</h2>
                <p className="mt-2 text-sm text-texto-suave">
                    {t('publico.planes.ayuda.descripcion')}
                </p>
                <Link
                    to="/contacto"
                    className="mt-4 inline-block rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                >
                    {t('publico.planes.ayuda.boton')}
                </Link>
            </div>
        </div>
    );
}
