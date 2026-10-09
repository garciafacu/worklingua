import { useTranslation } from 'react-i18next';

interface PaginadorProps {
    pagina: number;
    totalPaginas: number;
    totalRegistros: number;
    tamanioPagina: number;
    tamaniosDisponibles: readonly number[];
    /** Deshabilita los controles mientras hay una consulta en vuelo. */
    ocupado?: boolean;
    alCambiarPagina: (pagina: number) => void;
    alCambiarTamanio: (tamanio: number) => void;
}

const CLASE_BOTON =
    'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo disabled:cursor-not-allowed disabled:opacity-50';

/**
 * Navegación entre páginas de un listado que pagina en el servidor.
 *
 * No sabe qué se está paginando: recibe la posición y avisa hacia dónde ir. El
 * contenedor es el que consulta.
 *
 * No renderiza nada si no hay resultados: el listado ya muestra su propio
 * mensaje de vacío y un paginador de cero páginas no dice nada.
 */
export function Paginador({
    pagina,
    totalPaginas,
    totalRegistros,
    tamanioPagina,
    tamaniosDisponibles,
    ocupado = false,
    alCambiarPagina,
    alCambiarTamanio,
}: PaginadorProps) {
    const { t } = useTranslation();

    if (totalRegistros === 0) {
        return null;
    }

    const primero = (pagina - 1) * tamanioPagina + 1;
    const ultimo = Math.min(pagina * tamanioPagina, totalRegistros);

    return (
        <nav
            aria-label={t('comun.paginador.etiqueta')}
            className="mt-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between"
        >
            <p className="text-sm text-texto-suave" role="status">
                {t('comun.paginador.resumen', {
                    primero,
                    ultimo,
                    total: totalRegistros,
                    pagina,
                    totalPaginas,
                })}
            </p>

            <div className="flex items-center gap-3">
                <label className="flex items-center gap-2 text-sm text-texto-suave">
                    {t('comun.paginador.porPagina')}
                    <select
                        value={tamanioPagina}
                        disabled={ocupado}
                        onChange={(evento) => alCambiarTamanio(Number(evento.target.value))}
                        className="rounded-lg border border-borde bg-superficie px-2 py-1.5 text-sm text-texto"
                    >
                        {tamaniosDisponibles.map((tamanio) => (
                            <option key={tamanio} value={tamanio}>
                                {tamanio}
                            </option>
                        ))}
                    </select>
                </label>

                <div className="flex gap-2">
                    <button
                        type="button"
                        className={CLASE_BOTON}
                        disabled={ocupado || pagina <= 1}
                        onClick={() => alCambiarPagina(pagina - 1)}
                    >
                        {t('comun.paginador.anterior')}
                    </button>

                    <button
                        type="button"
                        className={CLASE_BOTON}
                        disabled={ocupado || pagina >= totalPaginas}
                        onClick={() => alCambiarPagina(pagina + 1)}
                    >
                        {t('comun.paginador.siguiente')}
                    </button>
                </div>
            </div>
        </nav>
    );
}
