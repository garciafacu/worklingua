import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { CampoSelect } from './CampoSelect';
import { CampoTexto } from './CampoTexto';
import type { CriteriosOpinion } from '../tipos/opiniones';
import type { PlanResponse } from '../tipos/planes';

interface BuscadorOpinionesProps {
    planes: PlanResponse[];
    buscando: boolean;
    alBuscar: (criterios: CriteriosOpinion) => void;
    alLimpiar: () => void;
}


export function BuscadorOpiniones({
    planes,
    buscando,
    alBuscar,
    alLimpiar,
}: BuscadorOpinionesProps) {
    const { t } = useTranslation();

    const [texto, setTexto] = useState('');
    const [planId, setPlanId] = useState('');
    const [desde, setDesde] = useState('');
    const [hasta, setHasta] = useState('');

    const hayFiltrosAvanzados = planId !== '' || desde !== '' || hasta !== '';

    function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();

        alBuscar({
            texto: texto.trim() || undefined,
            planId: planId ? Number(planId) : undefined,
            desde: desde || undefined,
            hasta: hasta || undefined,
        });
    }

    function limpiar() {
        setTexto('');
        setPlanId('');
        setDesde('');
        setHasta('');
        alLimpiar();
    }

    return (
        <form
            onSubmit={manejarEnvio}
            className="rounded-2xl border border-borde bg-superficie p-6"
            role="search"
        >
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
                <div className="flex-1">
                    <CampoTexto
                        etiqueta={t('privado.opiniones.buscador.texto')}
                        identificador="textoBusquedaOpinion"
                        placeholder={t('privado.opiniones.buscador.placeholder')}
                        value={texto}
                        onChange={(evento) => setTexto(evento.target.value)}
                    />
                </div>

                <button
                    type="submit"
                    disabled={buscando}
                    className="rounded-lg bg-primario px-5 py-2.5 text-sm font-semibold text-white hover:bg-primario-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {buscando ? t('comun.boton.buscando') : t('comun.boton.buscar')}
                </button>
            </div>

            <details className="mt-4" open={hayFiltrosAvanzados}>
                <summary className="cursor-pointer text-sm font-semibold text-primario">
                    {t('publico.catalogo.buscador.avanzada')}
                </summary>

                <div className="mt-4 grid gap-4 sm:grid-cols-3">
                    <CampoSelect
                        etiqueta={t('privado.opiniones.plan')}
                        identificador="planBusqueda"
                        value={planId}
                        onChange={(evento) => setPlanId(evento.target.value)}
                    >
                        <option value="">{t('privado.opiniones.buscador.todosPlanes')}</option>
                        {planes.map((plan) => (
                            <option key={plan.planId} value={plan.planId}>
                                {plan.nombre}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoTexto
                        etiqueta={t('comun.campo.desde')}
                        identificador="desde"
                        type="date"
                        value={desde}
                        onChange={(evento) => setDesde(evento.target.value)}
                    />

                    <CampoTexto
                        etiqueta={t('comun.campo.hasta')}
                        identificador="hasta"
                        type="date"
                        value={hasta}
                        onChange={(evento) => setHasta(evento.target.value)}
                    />
                </div>
            </details>

            <div className="mt-4 flex justify-end">
                <button
                    type="button"
                    onClick={limpiar}
                    className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                >
                    {t('comun.boton.limpiar')}
                </button>
            </div>
        </form>
    );
}
