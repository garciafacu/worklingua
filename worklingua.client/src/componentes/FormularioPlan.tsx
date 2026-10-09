import { useCallback, useEffect, useId, useRef, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { caracteristicasApi } from '../api/caracteristicasApi';
import { ErrorApi } from '../api/clienteHttp';
import { planesApi } from '../api/planesApi';
import { Alerta } from './Alerta';
import { Boton } from './Boton';
import { CampoTexto } from './CampoTexto';
import {
    SelectorCaracteristicasPlan,
    type FilaCaracteristicaPlan,
} from './SelectorCaracteristicasPlan';
import type {
    GuardarPlanRequest,
    ItemPlanCaracteristica,
    PlanAdminResponse,
} from '../tipos/planes';

const FORMULARIO_VACIO = {
    nombre: '',
    descripcion: '',
    precioMensual: '',
    cantidadLicencias: '',
    destacado: false,
};

type Formulario = typeof FORMULARIO_VACIO;

type Pestana = 'datos' | 'caracteristicas';

interface FormularioPlanProps {
    /** `null` es un alta. */
    plan: PlanAdminResponse | null;
    puedeCrear: boolean;
    puedeModificar: boolean;
    /**
     * Cambia cada vez que el catálogo se modifica desde el modal. Al cambiar, el
     * formulario vuelve a pedirlo conservando lo que ya se marcó.
     */
    versionCatalogo: number;
    /** Ausente cuando la sesión no puede ver el catálogo. */
    alAdministrarCatalogo?: () => void;
    /** Relee el listado de planes sin perder lo que se está editando. */
    alRecargarListado: () => void;
    /** Vuelve el formulario al modo alta, sin guardar. */
    alCancelar: () => void;
    alGuardado: (mensaje: string) => void;
}

function aFormulario(plan: PlanAdminResponse | null): Formulario {
    if (plan === null) {
        return FORMULARIO_VACIO;
    }

    return {
        nombre: plan.nombre,
        descripcion: plan.descripcion ?? '',
        precioMensual: String(plan.precioMensual),
        cantidadLicencias: String(plan.cantidadLicencias),
        destacado: plan.destacado,
    };
}

/**
 * Alta y edición de un plan, con sus características, en una sola operación.
 *
 * Antes eran dos pantallas: el formulario guardaba el plan y un panel aparte
 * guardaba las características. Acá el botón es uno solo, así que un plan nuevo
 * ya nace con lo que ofrece en vez de nacer vacío.
 *
 * Son dos requests igual, porque `PUT /planes/{id}/caracteristicas` necesita el
 * id que devuelve el alta. Si el segundo falla, el plan ya quedó creado: el
 * formulario avisa, se para en la pestaña Características y pasa a modo edición
 * sobre ese id, para que reintentar no cree un duplicado.
 */
export function FormularioPlan({
    plan,
    puedeCrear,
    puedeModificar,
    versionCatalogo,
    alAdministrarCatalogo,
    alRecargarListado,
    alCancelar,
    alGuardado,
}: FormularioPlanProps) {
    const { t } = useTranslation();
    const idFormulario = useId();
    const versionAlMontar = useRef(versionCatalogo);

    // El id inicial no cambia mientras el formulario vive: el componente se
    // vuelve a montar por cada plan. `planId` sí cambia, cuando un alta termina.
    const planIdInicial = plan === null ? null : plan.planId;

    const [formulario, setFormulario] = useState<Formulario>(() => aFormulario(plan));
    const [filas, setFilas] = useState<FilaCaracteristicaPlan[]>([]);
    const [planId, setPlanId] = useState<number | null>(planIdInicial);
    const [pestana, setPestana] = useState<Pestana>('datos');
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);

    /** Sin el permiso que corresponde al modo, el formulario queda de solo lectura. */
    const puedeEditarCampos = planId === null ? puedeCrear : puedeModificar;

    // En useCallback para poder ir en las dependencias de los efectos que lo
    // usan: sin eso el linter avisa, y el mensaje de error quedaría en el idioma
    // anterior después de cambiar de idioma.
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    // El pedido vive dentro del efecto y no en una función que el efecto llame,
    // para no disparar setState de forma sincrónica en su cuerpo
    // (react-hooks/set-state-in-effect).
    useEffect(() => {
        Promise.all([
            caracteristicasApi.listar(),
            planIdInicial === null
                ? Promise.resolve([])
                : planesApi.listarCaracteristicas(planIdInicial),
        ])
            .then(([catalogo, asociadas]) => {
                setFilas(
                    catalogo.map((caracteristica) => {
                        const valor = asociadas.find(
                            (item) => item.caracteristicaId === caracteristica.caracteristicaId,
                        );

                        return {
                            caracteristica,
                            asociada: valor !== undefined,
                            incluido: valor?.incluido ?? true,
                            detalle: valor?.detalle ?? '',
                        };
                    }),
                );
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.planes.panel.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [planIdInicial, mensajeDeError]);

    // Relectura del catálogo después de tocarlo en el modal. Conserva lo que ya
    // estaba marcado: solo agrega las nuevas y actualiza nombre y orden.
    //
    // La versión del montaje se guarda en un ref para saltear la primera
    // ejecución: el catálogo ya lo trajo el efecto de arriba.
    useEffect(() => {
        if (versionCatalogo === versionAlMontar.current) {
            return;
        }

        caracteristicasApi
            .listar()
            .then((catalogo) => {
                setFilas((actuales) =>
                    catalogo.map((caracteristica) => {
                        const previa = actuales.find(
                            (fila) =>
                                fila.caracteristica.caracteristicaId ===
                                caracteristica.caracteristicaId,
                        );

                        return previa === undefined
                            ? { caracteristica, asociada: false, incluido: true, detalle: '' }
                            : { ...previa, caracteristica };
                    }),
                );
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.planes.panel.errorCargar')),
            );
    }, [versionCatalogo, mensajeDeError]);

    function actualizarFila(
        caracteristicaId: number,
        cambios: Partial<FilaCaracteristicaPlan>,
    ) {
        setFilas((actuales) =>
            actuales.map((fila) =>
                fila.caracteristica.caracteristicaId === caracteristicaId
                    ? { ...fila, ...cambios }
                    : fila,
            ),
        );
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setGuardando(true);

        const esAlta = planId === null;

        const cuerpo: GuardarPlanRequest = {
            nombre: formulario.nombre.trim(),
            descripcion: formulario.descripcion.trim() || null,
            precioMensual: Number(formulario.precioMensual),
            cantidadLicencias: Number(formulario.cantidadLicencias),
            destacado: formulario.destacado,
        };

        const caracteristicas: ItemPlanCaracteristica[] = filas
            .filter((fila) => fila.asociada)
            .map((fila) => ({
                caracteristicaId: fila.caracteristica.caracteristicaId,
                incluido: fila.incluido,
                detalle: fila.detalle.trim() || null,
            }));

        let idGuardado: number;

        try {
            if (planId === null) {
                const creado = await planesApi.crear(cuerpo);
                idGuardado = creado.planId;
                setPlanId(idGuardado);
            } else {
                await planesApi.modificar(planId, cuerpo);
                idGuardado = planId;
            }
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.planes.errorGuardar'));
            setGuardando(false);

            return;
        }

        // En edición se manda siempre, incluso vacío: es la única forma de
        // desasociar lo que se desmarcó, porque el PUT reemplaza el conjunto.
        if (puedeModificar) {
            try {
                await planesApi.guardarCaracteristicas(idGuardado, { caracteristicas });
            } catch (excepcion) {
                const detalle = excepcion instanceof ErrorApi ? ' ' + excepcion.message : '';

                setError(t('admin.planes.errorCaracteristicas') + detalle);
                setPestana('caracteristicas');
                setGuardando(false);
                alRecargarListado();

                return;
            }
        }

        setGuardando(false);
        alGuardado(
            esAlta
                ? t('admin.planes.exitoAlta', { nombre: cuerpo.nombre })
                : t('admin.planes.exitoModificacion', { nombre: cuerpo.nombre }),
        );
    }

    const pestanas: { clave: Pestana; etiqueta: string }[] = [
        { clave: 'datos', etiqueta: t('admin.planes.formulario.datos') },
        { clave: 'caracteristicas', etiqueta: t('admin.planes.caracteristicas') },
    ];

    return (
        <div className="flex flex-col gap-5">
            <div role="tablist" className="flex gap-1 border-b border-borde">
                {pestanas.map((item) => (
                    <button
                        key={item.clave}
                        type="button"
                        role="tab"
                        id={`${idFormulario}-tab-${item.clave}`}
                        aria-selected={pestana === item.clave}
                        aria-controls={`${idFormulario}-panel-${item.clave}`}
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

            <Alerta tipo="error" mensaje={error} />

            <form id={idFormulario} onSubmit={guardar} noValidate>
                {/* El contenedor con `hidden` no lleva clases de display: una
                    utilidad como `flex` le ganaría al `[hidden]` del navegador y
                    la pestaña oculta se seguiría viendo. */}
                <div
                    role="tabpanel"
                    id={`${idFormulario}-panel-datos`}
                    aria-labelledby={`${idFormulario}-tab-datos`}
                    hidden={pestana !== 'datos'}
                >
                    <div className="flex flex-col gap-4">
                        <CampoTexto
                            etiqueta={t('comun.campo.nombre')}
                            identificador="plan-nombre"
                            required
                            maxLength={100}
                            disabled={!puedeEditarCampos}
                            value={formulario.nombre}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, nombre: evento.target.value })
                            }
                        />

                        <CampoTexto
                            etiqueta={t('comun.campo.descripcion')}
                            identificador="plan-descripcion"
                            maxLength={500}
                            disabled={!puedeEditarCampos}
                            value={formulario.descripcion}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, descripcion: evento.target.value })
                            }
                        />

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.planes.tabla.precio')}
                                identificador="plan-precioMensual"
                                type="number"
                                min={0}
                                step="0.01"
                                required
                                disabled={!puedeEditarCampos}
                                value={formulario.precioMensual}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, precioMensual: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.planes.formulario.licencias')}
                                identificador="plan-cantidadLicencias"
                                type="number"
                                min={1}
                                step="1"
                                required
                                disabled={!puedeEditarCampos}
                                value={formulario.cantidadLicencias}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        cantidadLicencias: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        {/* La banda "Recomendado" del catálogo público sale de acá:
                            antes era una constante con el nombre del plan. */}
                        <label className="flex items-center gap-2.5 text-sm text-texto">
                            <input
                                type="checkbox"
                                className="size-4"
                                disabled={!puedeEditarCampos}
                                checked={formulario.destacado}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, destacado: evento.target.checked })
                                }
                            />
                            {t('admin.planes.formulario.destacar')}
                        </label>

                        {planId === null && (
                            <p className="ayuda">{t('admin.planes.formulario.ayudaAlta')}</p>
                        )}
                    </div>
                </div>

                <div
                    role="tabpanel"
                    id={`${idFormulario}-panel-caracteristicas`}
                    aria-labelledby={`${idFormulario}-tab-caracteristicas`}
                    hidden={pestana !== 'caracteristicas'}
                >
                    <SelectorCaracteristicasPlan
                        filas={filas}
                        cargando={cargando}
                        puedeGuardar={puedeModificar}
                        alAdministrarCatalogo={alAdministrarCatalogo}
                        alCambiar={actualizarFila}
                    />
                </div>
            </form>

            <div className="flex flex-wrap gap-3">
                {puedeEditarCampos && (
                    <Boton type="submit" form={idFormulario} cargando={guardando}>
                        {planId === null
                            ? t('admin.planes.formulario.crear')
                            : t('comun.boton.guardarCambios')}
                    </Boton>
                )}

                {planId !== null && (
                    <button
                        type="button"
                        onClick={alCancelar}
                        className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                    >
                        {t('comun.boton.cancelar')}
                    </button>
                )}
            </div>
        </div>
    );
}
