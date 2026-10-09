import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { planesApi } from '../../../api/planesApi';
import { Alerta } from '../../../componentes/Alerta';
import { CatalogoCaracteristicas } from '../../../componentes/CatalogoCaracteristicas';
import { FormularioPlan } from '../../../componentes/FormularioPlan';
import { Modal } from '../../../componentes/Modal';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import type { PlanAdminResponse } from '../../../tipos/planes';

/**
 * Sección Planes: el ABM de los planes de suscripción y, dentro, todo lo de sus
 * características.
 *
 * Las tres superficies que antes estaban separadas —el ABM de planes, el ABM del
 * catálogo de características y el panel que las asociaba— viven acá: el
 * formulario del plan está arriba del listado con sus características en una
 * pestaña, y el catálogo se administra en un modal que se abre encima sin perder
 * lo que se estaba editando.
 *
 * Los botones se muestran según los permisos de la sesión, pero eso es
 * presentación: cada endpoint vuelve a exigir el permiso y responde 403 si
 * falta. Los planes del catálogo público están protegidos: el backend rechaza su
 * baja y la fila no dibuja el botón.
 */
export function AdminPlanes() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    // El precio se muestra con la cultura activa, no con un Intl fijo en es-AR.
    const { formatearMoneda } = useLocalizacion();

    const [planes, setPlanes] = useState<PlanAdminResponse[]>([]);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    /** Plan que se está editando; `null` es un alta. */
    const [planEnEdicion, setPlanEnEdicion] = useState<PlanAdminResponse | null>(null);
    const [catalogoAbierto, setCatalogoAbierto] = useState(false);
    /** Sube cada vez que el catálogo cambia, para que el panel lo relea. */
    const [versionCatalogo, setVersionCatalogo] = useState(0);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Fuerza releer el listado despues de guardar o dar de baja. */
    const [recarga, setRecarga] = useState(0);

    // En useCallback para poder ir en las dependencias de los efectos que lo
    // usan: sin eso el linter avisa, y el mensaje de error quedaría en el idioma
    // anterior después de cambiar de idioma.
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    const puedeCrear = tienePermiso('Plan.Alta');
    const puedeModificar = tienePermiso('Plan.Modificar');
    const puedeDarDeBaja = tienePermiso('Plan.Baja');
    const puedeVerCatalogo = tienePermiso('Caracteristica.Listar');

    // El listado se relee cambiando este contador. El pedido vive dentro del
    // efecto y no en una función que el efecto llame, para no disparar setState
    // de forma sincrónica en el cuerpo del efecto (react-hooks/set-state-in-effect).
    useEffect(() => {
        planesApi
            .listarAdministracion()
            .then(setPlanes)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.planes.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function editar(plan: PlanAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setPlanEnEdicion(plan);
    }

    async function darDeBaja(plan: PlanAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await planesApi.baja(plan.planId);
            setExito(t('admin.planes.exitoBaja', { nombre: plan.nombre }));

            if (planEnEdicion?.planId === plan.planId) {
                setPlanEnEdicion(null);
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.planes.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<PlanAdminResponse>[] = [
        { encabezado: t('comun.campo.nombre'), celda: (plan) => plan.nombre },
        {
            encabezado: t('comun.campo.descripcion'),
            celda: (plan) => (
                <span className="block max-w-md">
                    {plan.descripcion ?? t('comun.valor.vacio')}
                </span>
            ),
        },
        {
            encabezado: t('admin.planes.tabla.precio'),
            numerica: true,
            celda: (plan) => formatearMoneda(plan.precioMensual),
        },
        {
            encabezado: t('admin.planes.tabla.licencias'),
            numerica: true,
            celda: (plan) => plan.cantidadLicencias,
        },
        {
            encabezado: t('admin.planes.tabla.destacado'),
            celda: (plan) =>
                plan.destacado ? t('comun.estado.si') : t('comun.valor.vacio'),
        },
    ];

    /**
     * El alta solo aparece con permiso de crear; la edición, en cambio, se abre
     * siempre: sin `Plan.Modificar` el formulario queda de solo lectura y sirve
     * para consultar qué características tiene el plan.
     */
    const mostrarFormulario = planEnEdicion !== null || puedeCrear;

    return (
        <div className="mx-auto max-w-6xl">
            <div className="flex flex-wrap items-start justify-between gap-x-6 gap-y-3">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                        {t('admin.planes.titulo')}
                    </h1>
                    <p className="mt-2 max-w-2xl text-sm text-texto-suave">
                        {t('admin.planes.descripcion')}
                    </p>
                </div>

                {puedeVerCatalogo && (
                    <button
                        type="button"
                        onClick={() => setCatalogoAbierto(true)}
                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                    >
                        {t('admin.planes.catalogo')}
                    </button>
                )}
            </div>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {mostrarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {planEnEdicion === null
                            ? t('admin.planes.formulario.nuevo')
                            : t('admin.planes.formulario.editar')}

                        {/* El nombre es dato, no texto de interfaz: no lleva
                            clave de traducción. Con el formulario arriba del
                            listado, dice de un vistazo qué fila se está tocando. */}
                        {planEnEdicion !== null && (
                            <span className="ml-2 font-normal text-texto-suave">
                                {planEnEdicion.nombre}
                            </span>
                        )}
                    </h2>

                    <div className="mt-4">
                        {/* La `key` remonta el formulario al cambiar de plan: su
                            estado arranca de cero y vuelve a pedir las características. */}
                        <FormularioPlan
                            key={planEnEdicion?.planId ?? 'nuevo'}
                            plan={planEnEdicion}
                            puedeCrear={puedeCrear}
                            puedeModificar={puedeModificar}
                            versionCatalogo={versionCatalogo}
                            alAdministrarCatalogo={
                                puedeVerCatalogo ? () => setCatalogoAbierto(true) : undefined
                            }
                            alRecargarListado={recargar}
                            alCancelar={() => setPlanEnEdicion(null)}
                            alGuardado={(mensaje) => {
                                setExito(mensaje);
                                setPlanEnEdicion(null);
                                recargar();
                            }}
                        />
                    </div>
                </section>
            )}

            <section className="mt-6">
                <h2 className="text-lg font-semibold text-texto">{t('admin.planes.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.planes.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={planes}
                            claveDe={(plan) => plan.planId}
                            inactiva={(plan) => !plan.activo}
                            mensajeVacio={t('admin.planes.vacio')}
                            acciones={(plan) => (
                                <div className="flex justify-end gap-2">
                                    <button
                                        type="button"
                                        onClick={() => editar(plan)}
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {puedeModificar
                                            ? t('comun.boton.editar')
                                            : t('comun.boton.ver')}
                                    </button>

                                    {plan.protegido ? (
                                        <span className="px-3 py-1.5 text-sm text-texto-suave">
                                            {t('admin.planes.protegido')}
                                        </span>
                                    ) : (
                                        puedeDarDeBaja &&
                                        (confirmandoBaja === plan.planId ? (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => void darDeBaja(plan)}
                                                    className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                                >
                                                    {t('comun.boton.confirmarBaja')}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => setConfirmandoBaja(null)}
                                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                                >
                                                    {t('comun.boton.no')}
                                                </button>
                                            </>
                                        ) : (
                                            <button
                                                type="button"
                                                onClick={() => setConfirmandoBaja(plan.planId)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                            >
                                                {t('comun.boton.darDeBaja')}
                                            </button>
                                        ))
                                    )}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>

            <Modal
                titulo={t('admin.caracteristicas.titulo')}
                descripcion={t('admin.caracteristicas.descripcion')}
                abierto={catalogoAbierto}
                ancho="ancho"
                alCerrar={() => setCatalogoAbierto(false)}
            >
                {catalogoAbierto && (
                    <CatalogoCaracteristicas
                        alCambiar={() => setVersionCatalogo((numero) => numero + 1)}
                    />
                )}
            </Modal>
        </div>
    );
}
