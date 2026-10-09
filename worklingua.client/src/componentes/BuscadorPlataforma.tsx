import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { CampoSelect } from './CampoSelect';
import { CampoTexto } from './CampoTexto';
import {
    AREAS_BUSQUEDA,
    ORDENES_BUSQUEDA,
    type AreaBusqueda,
    type CriteriosBusqueda,
    type OrdenBusqueda,
    type SeccionBusquedaResponse,
} from '../tipos/busqueda';

interface BuscadorPlataformaProps {
    /** Criterios con los que arranca el formulario, leídos de la URL. */
    criteriosIniciales: CriteriosBusqueda;
    /** Secciones que la sesión puede ver: las devuelve el backend ya filtradas. */
    secciones: SeccionBusquedaResponse[];
    buscando: boolean;
    alBuscar: (criterios: CriteriosBusqueda) => void;
}

interface Formulario {
    texto: string;
    area: AreaBusqueda | '';
    seccion: string;
    ordenarPor: OrdenBusqueda;
    soloTitulo: boolean;
}

function aFormulario(criterios: CriteriosBusqueda): Formulario {
    return {
        texto: criterios.texto ?? '',
        area: criterios.area ?? '',
        seccion: criterios.seccion ?? '',
        ordenarPor: criterios.ordenarPor ?? 'Relevancia',
        soloTitulo: criterios.soloTitulo ?? false,
    };
}

function aCriterios(datos: Formulario): CriteriosBusqueda {
    return {
        texto: datos.texto.trim() || undefined,
        area: datos.area || undefined,
        seccion: datos.seccion || undefined,
        // `Relevancia` es el valor por defecto del backend: no se manda para que
        // la URL de una búsqueda simple quede limpia.
        ordenarPor: datos.ordenarPor === 'Relevancia' ? undefined : datos.ordenarPor,
        soloTitulo: datos.soloTitulo || undefined,
    };
}

/**
 * Formulario del buscador global: búsqueda simple por texto y, plegada, la
 * avanzada con área, sección, criterio de orden y búsqueda solo en títulos.
 * Sigue la maqueta de `BuscadorBitacora`.
 */
export function BuscadorPlataforma({ criteriosIniciales, secciones, buscando, alBuscar }: BuscadorPlataformaProps) {
    const { t } = useTranslation();

    const [formulario, setFormulario] = useState<Formulario>(() => aFormulario(criteriosIniciales));

    const hayFiltrosAvanzados =
        formulario.area !== '' ||
        formulario.seccion !== '' ||
        formulario.ordenarPor !== 'Relevancia' ||
        formulario.soloTitulo;

    const seccionesVisibles = secciones.filter(
        (seccion) => formulario.area === '' || seccion.area === formulario.area,
    );

    function cambiarArea(area: AreaBusqueda | '') {
        setFormulario((actual) => {
            const seccionElegida = secciones.find((seccion) => seccion.clave === actual.seccion);
            const seccionValida = area === '' || seccionElegida === undefined || seccionElegida.area === area;

            return { ...actual, area, seccion: seccionValida ? actual.seccion : '' };
        });
    }

    function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        alBuscar(aCriterios(formulario));
    }

    function limpiar() {
        setFormulario(aFormulario({}));
        alBuscar({});
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
                        etiqueta={t('busqueda.campo.texto')}
                        identificador="textoBusqueda"
                        type="search"
                        maxLength={200}
                        placeholder={t('busqueda.campo.textoPlaceholder')}
                        value={formulario.texto}
                        onChange={(evento) => setFormulario({ ...formulario, texto: evento.target.value })}
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

                <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    <CampoSelect
                        etiqueta={t('busqueda.campo.area')}
                        identificador="areaBusqueda"
                        value={formulario.area}
                        onChange={(evento) => cambiarArea(evento.target.value as AreaBusqueda | '')}
                    >
                        <option value="">{t('busqueda.area.todas')}</option>
                        {AREAS_BUSQUEDA.map((area) => (
                            <option key={area.valor} value={area.valor}>
                                {t(area.etiqueta)}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoSelect
                        etiqueta={t('busqueda.campo.seccion')}
                        identificador="seccionBusqueda"
                        value={formulario.seccion}
                        onChange={(evento) => setFormulario({ ...formulario, seccion: evento.target.value })}
                    >
                        <option value="">{t('busqueda.seccion.todas')}</option>
                        {seccionesVisibles.map((seccion) => (
                            <option key={seccion.clave} value={seccion.clave}>
                                {t(seccion.clave)}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoSelect
                        etiqueta={t('busqueda.campo.ordenarPor')}
                        identificador="ordenarPorBusqueda"
                        value={formulario.ordenarPor}
                        onChange={(evento) =>
                            setFormulario({
                                ...formulario,
                                ordenarPor: evento.target.value as OrdenBusqueda,
                            })
                        }
                    >
                        {ORDENES_BUSQUEDA.map((orden) => (
                            <option key={orden.valor} value={orden.valor}>
                                {t(orden.etiqueta)}
                            </option>
                        ))}
                    </CampoSelect>

                    <label className="flex items-center gap-2 text-sm text-texto sm:col-span-2 lg:col-span-3">
                        <input
                            type="checkbox"
                            className="size-4"
                            checked={formulario.soloTitulo}
                            onChange={(evento) => setFormulario({ ...formulario, soloTitulo: evento.target.checked })}
                        />
                        {t('busqueda.campo.soloTitulo')}
                    </label>
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
