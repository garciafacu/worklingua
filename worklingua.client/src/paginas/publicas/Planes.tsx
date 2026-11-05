import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { caracteristicasApi, planesApi } from '../../api/planesApi';
import { Alerta } from '../../componentes/Alerta';
import { CampoSelect } from '../../componentes/Campos';
import { FiltrosPlanes } from '../../componentes/FiltrosPlanes';
import { TarjetaPlan } from '../../componentes/TarjetaPlan';
import { useSesion } from '../../contexto/useSesion';
import {
    conPlanPendiente,
    recordarPlanAContratar,
    rutaContratacion,
} from '../../rutas/contratacionPendiente';
import { PERMISOS } from '../../rutas/itemsMenu';
import type { CaracteristicaResponse, PlanResponse } from '../../tipos/planes';

const CLASE_BOTON_CONTRATAR =
    'rounded-lg bg-primario px-3 py-1.5 text-sm font-semibold text-white no-underline hover:bg-primario-hover';

type OrdenPlanes = 'precio' | 'valoracion';

export function Planes() {
    const { t } = useTranslation();

    const [planes, setPlanes] = useState<PlanResponse[]>([]);
    const [caracteristicas, setCaracteristicas] = useState<CaracteristicaResponse[]>([]);
    const [planesVisibles, setPlanesVisibles] = useState<number[]>([]);
    const [orden, setOrden] = useState<OrdenPlanes>('precio');
    const [idsVisibles, setIdsVisibles] = useState<number[]>([]);
    const [idActivo, setIdActivo] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const { autenticado, tienePermiso } = useSesion();

    useEffect(() => {
        Promise.all([planesApi.listar(), caracteristicasApi.listar()])
            .then(([listaPlanes, listaCaracteristicas]) => {
                setPlanes(listaPlanes);
                setCaracteristicas(listaCaracteristicas);
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

    const planesAMostrar = ordenar(planes.filter((plan) => planesVisibles.includes(plan.planId)));

    function ordenar(lista: PlanResponse[]) {
        if (orden !== 'valoracion') {
            return lista;
        }

        return [...lista].sort(
            (uno, otro) =>
                otro.promedioValoracion - uno.promedioValoracion ||
                otro.cantidadValoraciones - uno.cantidadValoraciones,
        );
    }

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

                    <div className="mt-6 sm:w-64">
                        <CampoSelect
                            etiqueta={t('publico.planes.orden')}
                            identificador="ordenPlanes"
                            value={orden}
                            onChange={(evento) => setOrden(evento.target.value as OrdenPlanes)}
                        >
                            <option value="precio">{t('publico.planes.orden.precio')}</option>
                            <option value="valoracion">{t('publico.planes.orden.valoracion')}</option>
                        </CampoSelect>
                    </div>

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
