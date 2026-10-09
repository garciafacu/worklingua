import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { bitacoraApi } from '../../../api/bitacoraApi';
import { ErrorApi } from '../../../api/clienteHttp';
import { Alerta } from '../../../componentes/Alerta';
import { BuscadorBitacora } from '../../../componentes/BuscadorBitacora';
import { Paginador } from '../../../componentes/Paginador';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import {
    TAMANIOS_PAGINA,
    type BitacoraEventoResponse,
    type FiltrosBitacora,
    type PaginaBitacoraResponse,
} from '../../../tipos/bitacora';

const SIN_FILTROS: FiltrosBitacora = {};

const CLASES_NIVEL: Record<string, string> = {
    INFO: 'bg-info-fondo text-info',
    WARN: 'bg-alerta-fondo text-alerta',
    ERROR: 'bg-error-fondo text-error',
};

function EtiquetaNivel({ nivel }: { nivel: string | null }) {
    if (!nivel) {
        return <span className="text-texto-suave">—</span>;
    }

    const clases = CLASES_NIVEL[nivel] ?? 'bg-fondo text-texto-suave';

    return (
        <span
            className={`inline-block rounded-md px-2 py-0.5 text-xs font-semibold tracking-wide ${clases}`}
        >
            {nivel}
        </span>
    );
}

/**
 * Consulta de la bitácora de seguridad y auditoría.
 *
 * Los filtros y la paginación los resuelve `sp_Bitacora_Buscar`: la pantalla
 * nunca descarga la bitácora entera. Cambiar un filtro vuelve a la primera
 * página; cambiar de página conserva los filtros.
 *
 * Consultar la bitácora no deja registro en la propia bitácora, por decisión
 * explícita: si lo hiciera, cada búsqueda y cada cambio de página agregarían
 * ruido al listado que se está mirando.
 */
export function AdminBitacora() {
    const { t } = useTranslation();
    // Fecha y hora completas con la cultura activa: en una auditoría el minuto
    // exacto importa, y el formato lo decide la cultura, no un locale fijo.
    const { formatearFecha } = useLocalizacion();

    function mensajeDeError(excepcion: unknown, clave: string): string {
        return excepcion instanceof ErrorApi ? excepcion.message : t(clave);
    }

    const [filtros, setFiltros] = useState<FiltrosBitacora>(SIN_FILTROS);
    const [pagina, setPagina] = useState(1);
    const [tamanioPagina, setTamanioPagina] = useState<number>(TAMANIOS_PAGINA[0]);

    const [resultado, setResultado] = useState<PaginaBitacoraResponse | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [buscando, setBuscando] = useState(false);

    // La consulta la dispara el cambio de criterios, no el envío del formulario:
    // así la carga inicial, cada búsqueda y cada cambio de página pasan por el
    // mismo camino y no hay tres lugares donde manejar el error.
    useEffect(() => {
        bitacoraApi
            .buscar({ ...filtros, pagina, tamanioPagina })
            .then((paginaRecibida) => {
                setResultado(paginaRecibida);
                setError(null);
            })
            .catch((excepcion) => {
                setError(mensajeDeError(excepcion, 'admin.bitacora.errorCargar'));
                setResultado(null);
            })
            .finally(() => {
                setCargando(false);
                setBuscando(false);
            });
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [filtros, pagina, tamanioPagina]);

    function buscar(nuevos: FiltrosBitacora) {
        setBuscando(true);
        setPagina(1);
        setFiltros(nuevos);
    }

    function cambiarTamanio(nuevo: number) {
        setBuscando(true);
        setPagina(1);
        setTamanioPagina(nuevo);
    }

    function cambiarPagina(nueva: number) {
        setBuscando(true);
        setPagina(nueva);
    }

    const hayFiltros = Object.values(filtros).some((valor) => valor !== undefined);

    const columnas: ColumnaAbm<BitacoraEventoResponse>[] = [
        {
            encabezado: t('admin.bitacora.tabla.fecha'),
            celda: (fila) => (
                <span className="whitespace-nowrap tabular-nums">{formatearFecha(fila.fechaEvento, true)}</span>
            ),
        },
        {
            encabezado: t('admin.bitacora.tabla.nivel'),
            celda: (fila) => <EtiquetaNivel nivel={fila.nivel} />,
        },
        {
            encabezado: t('admin.bitacora.tabla.modulo'),
            celda: (fila) => fila.modulo ?? t('comun.valor.vacio'),
        },
        {
            encabezado: t('admin.bitacora.tabla.accion'),
            celda: (fila) => fila.accion ?? t('comun.valor.vacio'),
        },
        {
            encabezado: t('admin.bitacora.tabla.usuario'),
            celda: (fila) =>
                fila.usuario ? (
                    <span title={fila.email ?? undefined}>{fila.usuario}</span>
                ) : (
                    <span className="text-texto-suave">{t('admin.bitacora.sinUsuario')}</span>
                ),
        },
        {
            encabezado: t('comun.campo.descripcion'),
            // La descripción es nvarchar(MAX): sin recorte, una sola fila larga
            // desarma la tabla. El texto completo queda en el title.
            celda: (fila) => (
                <span
                    className="line-clamp-2 block max-w-md"
                    title={fila.descripcion ?? undefined}
                >
                    {fila.descripcion ?? t('comun.valor.vacio')}
                </span>
            ),
        },
    ];

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.bitacora.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">
                {t('admin.bitacora.descripcion')}
            </p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            <section className="mt-6">
                <BuscadorBitacora buscando={buscando} alBuscar={buscar} />
            </section>

            <section className="mt-6">
                {cargando ? (
                    <p className="text-sm text-texto-suave">{t('admin.bitacora.cargando')}</p>
                ) : (
                    <>
                        <TablaAbm
                            columnas={columnas}
                            filas={resultado?.registros ?? []}
                            claveDe={(fila) => fila.bitacoraId}
                            mensajeVacio={
                                hayFiltros
                                    ? t('admin.bitacora.sinCoincidencias')
                                    : t('admin.bitacora.vacio')
                            }
                        />

                        {resultado && (
                            <Paginador
                                pagina={resultado.pagina}
                                totalPaginas={resultado.totalPaginas}
                                totalRegistros={resultado.totalRegistros}
                                tamanioPagina={resultado.tamanioPagina}
                                tamaniosDisponibles={TAMANIOS_PAGINA}
                                ocupado={buscando}
                                alCambiarPagina={cambiarPagina}
                                alCambiarTamanio={cambiarTamanio}
                            />
                        )}
                    </>
                )}
            </section>
        </div>
    );
}
