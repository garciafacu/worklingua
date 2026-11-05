import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { planesApi } from '../../../api/planesApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { BotonAccion, ConfirmarBaja } from '../../../componentes/Boton';
import { CatalogoCaracteristicas } from '../../../componentes/CatalogoCaracteristicas';
import { FormularioPlan } from '../../../componentes/FormularioPlan';
import { Modal } from '../../../componentes/Modal';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import type { PlanAdminResponse } from '../../../tipos/planes';

export function AdminPlanes() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearMoneda } = useLocalizacion();

    const [planes, setPlanes] = useState<PlanAdminResponse[]>([]);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [planEnEdicion, setPlanEnEdicion] = useState<PlanAdminResponse | null>(null);
    const [catalogoAbierto, setCatalogoAbierto] = useState(false);
    const [versionCatalogo, setVersionCatalogo] = useState(0);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);

    const mensajeDeError = useMensajeDeError();

    const puedeCrear = tienePermiso('Plan.Alta');
    const puedeModificar = tienePermiso('Plan.Modificar');
    const puedeDarDeBaja = tienePermiso('Plan.Baja');
    const puedeVerCatalogo = tienePermiso('Caracteristica.Listar');

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
                    <BotonAccion grande onClick={() => setCatalogoAbierto(true)}>
                        {t('admin.planes.catalogo')}
                    </BotonAccion>
                )}
            </div>

            <AlertasResultado error={error} exito={exito} />

            {mostrarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {planEnEdicion === null
                            ? t('admin.planes.formulario.nuevo')
                            : t('admin.planes.formulario.editar')}

                        {planEnEdicion !== null && (
                            <span className="ml-2 font-normal text-texto-suave">
                                {planEnEdicion.nombre}
                            </span>
                        )}
                    </h2>

                    <div className="mt-4">
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
                                    <BotonAccion onClick={() => editar(plan)}>
                                        {puedeModificar
                                            ? t('comun.boton.editar')
                                            : t('comun.boton.ver')}
                                    </BotonAccion>

                                    {plan.protegido ? (
                                        <span className="px-3 py-1.5 text-sm text-texto-suave">
                                            {t('admin.planes.protegido')}
                                        </span>
                                    ) : (
                                        puedeDarDeBaja &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === plan.planId}
                                            onPedir={() => setConfirmandoBaja(plan.planId)}
                                            onConfirmar={() => void darDeBaja(plan)}
                                            onCancelar={() => setConfirmandoBaja(null)}
                                        />)
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
