import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { progresoApi } from '../../api/progresoApi';
import { Alerta } from '../../componentes/Alerta';
import { BarraProgreso } from '../../componentes/BarraProgreso';
import { CampoSelect } from '../../componentes/CampoSelect';
import { EtiquetaEstado, type TonoEstado } from '../../componentes/EtiquetaEstado';
import { TablaAbm, type ColumnaAbm } from '../../componentes/TablaAbm';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import { useSesion } from '../../contexto/useSesion';
import { PERMISOS } from '../../rutas/itemsMenu';
import type {
    CursoConProgresoResponse,
    EstadoProgreso,
    ProgresoUsuarioResponse,
} from '../../tipos/progreso';

const TONO_POR_ESTADO: Record<EstadoProgreso, TonoEstado> = {
    NO_INICIADO: 'neutro',
    EN_CURSO: 'info',
    COMPLETADO: 'exito',
};

type Pestana = 'mios' | 'equipo';

interface FilaEquipo {
    clave: string;
    usuario: string;
    email: string;
    curso: CursoConProgresoResponse;
}

function EstadoCurso({ estado }: { estado: EstadoProgreso }) {
    const { t } = useTranslation();

    return <EtiquetaEstado texto={t(`comun.progreso.estado.${estado}`)} tono={TONO_POR_ESTADO[estado]} />;
}

/**
 * Seguimiento de lo contratado (punto 6.a): los cursos del usuario con su avance.
 *
 * El avance es el porcentaje de módulos completados, y el estado del curso lo
 * calcula el backend a partir de sus módulos. Esta pantalla es el índice: el
 * módulo se recorre en su lección (`Leccion.tsx`), que es la que registra el
 * progreso. Con `Curso.VerProgresoEmpresa`, la pestaña "Mi equipo" muestra el
 * avance de todos los usuarios de la empresa.
 */
export function MisCursos() {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();
    const { tienePermiso } = useSesion();
    const veEquipo = tienePermiso(PERMISOS.cursoVerProgresoEmpresa);

    const [pestana, setPestana] = useState<Pestana>('mios');
    const [cursos, setCursos] = useState<CursoConProgresoResponse[]>([]);
    const [equipo, setEquipo] = useState<ProgresoUsuarioResponse[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Filtros de la clasificación (CU-004-005); vacío es "todos". */
    const [filtroSector, setFiltroSector] = useState('');
    const [filtroEtiqueta, setFiltroEtiqueta] = useState('');

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        const pedidoEquipo = veEquipo ? progresoApi.listarEmpresa() : Promise.resolve([]);

        Promise.all([progresoApi.listarMio(), pedidoEquipo])
            .then(([mios, deLaEmpresa]) => {
                setCursos(mios);
                setEquipo(deLaEmpresa);
            })
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'privado.cursos.error')))
            .finally(() => setCargando(false));
    }, [veEquipo, mensajeDeError]);

    /**
     * Los sectores y las etiquetas salen de los cursos que ya se trajeron: el
     * alumno no tiene permiso para leer el catálogo ni el diccionario, y
     * ofrecerle filtrar por algo que no tiene no serviría de nada.
     */
    const sectores = [
        ...new Set(cursos.map((item) => item.curso.sector).filter((sector) => sector !== null)),
    ].sort();

    const etiquetas = [
        ...new Map(
            cursos
                .flatMap((item) => item.curso.etiquetas)
                .map((etiqueta) => [etiqueta.etiquetaId, etiqueta]),
        ).values(),
    ].sort((uno, otro) => uno.nombre.localeCompare(otro.nombre));

    const cursosVisibles = cursos.filter((item) => {
        const porSector = filtroSector === '' || item.curso.sector === filtroSector;
        const porEtiqueta =
            filtroEtiqueta === '' ||
            item.curso.etiquetas.some((etiqueta) => String(etiqueta.etiquetaId) === filtroEtiqueta);

        return porSector && porEtiqueta;
    });

    const hayFiltros = filtroSector !== '' || filtroEtiqueta !== '';

    /**
     * Un curso que salió de su ventana de publicación (CU-004-006). El backend
     * lo sigue mandando porque el usuario ya registró avance; acá se avisa para
     * que entienda por qué no lo encuentra en el catálogo.
     */
    function fueraDePublicacion(curso: CursoConProgresoResponse['curso']) {
        const hoy = new Date().toISOString().slice(0, 10);

        return (
            (curso.fechaPublicacion !== null && curso.fechaPublicacion > hoy) ||
            (curso.fechaFin !== null && curso.fechaFin < hoy)
        );
    }

    function modulosCompletados(item: CursoConProgresoResponse) {
        return item.modulos.filter(({ progreso }) => progreso?.estado === 'COMPLETADO').length;
    }

    /** Cuántos archivos acompañan a ese módulo. Los abre la lección. */
    function archivosDelModulo(item: CursoConProgresoResponse, moduloId: number) {
        return item.activos.filter((activo) => activo.moduloId === moduloId).length;
    }

    function limpiarFiltros() {
        setFiltroSector('');
        setFiltroEtiqueta('');
    }

    const filasEquipo: FilaEquipo[] = equipo.flatMap((persona) =>
        persona.cursos.map((curso) => ({
            clave: `${persona.usuarioId}-${curso.curso.cursoId}`,
            usuario: persona.usuario,
            email: persona.email,
            curso,
        })),
    );

    const columnasEquipo: ColumnaAbm<FilaEquipo>[] = [
        {
            encabezado: t('privado.cursos.equipo.usuario'),
            celda: (fila) => (
                <>
                    <span className="font-medium text-texto">{fila.usuario}</span>
                    <br />
                    <span className="text-xs text-texto-suave">{fila.email}</span>
                </>
            ),
        },
        { encabezado: t('privado.cursos.equipo.curso'), celda: (fila) => fila.curso.curso.nombre },
        { encabezado: t('privado.cursos.equipo.estado'), celda: (fila) => <EstadoCurso estado={fila.curso.estado} /> },
        {
            encabezado: t('privado.cursos.equipo.avance'),
            numerica: true,
            celda: (fila) => `${fila.curso.porcentajeAvance}%`,
        },
        {
            encabezado: t('privado.cursos.equipo.ultimaActividad'),
            celda: (fila) => formatearFecha(fila.curso.ultimaActividad, true),
        },
    ];

    const pestanas: { clave: Pestana; etiqueta: string }[] = [
        { clave: 'mios', etiqueta: t('privado.cursos.pestana.mios') },
        { clave: 'equipo', etiqueta: t('privado.cursos.pestana.equipo') },
    ];

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.cursos.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.cursos.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {veEquipo && (
                <div role="tablist" className="mt-6 flex gap-1 border-b border-borde">
                    {pestanas.map((item) => (
                        <button
                            key={item.clave}
                            type="button"
                            role="tab"
                            aria-selected={pestana === item.clave}
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
            )}

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('privado.cursos.cargando')}</p>}

            {/* Filtros de clasificación. Solo aparecen si hay por qué filtrar:
                con un curso o sin etiquetas cargadas serían ruido. */}
            {!cargando && pestana === 'mios' && cursos.length > 1 &&
                (sectores.length > 1 || etiquetas.length > 0) && (
                    <div className="mt-6 flex flex-col gap-3 rounded-2xl border border-borde bg-superficie p-4 sm:flex-row sm:flex-wrap sm:items-end">
                        {sectores.length > 1 && (
                            <div className="w-full sm:w-52">
                                <CampoSelect
                                    etiqueta={t('privado.cursos.filtro.sector')}
                                    identificador="filtroSector"
                                    value={filtroSector}
                                    onChange={(evento) => setFiltroSector(evento.target.value)}
                                >
                                    <option value="">{t('privado.cursos.filtro.todos')}</option>
                                    {sectores.map((sector) => (
                                        <option key={sector} value={sector}>
                                            {t(`admin.cursos.sector.${sector}`)}
                                        </option>
                                    ))}
                                </CampoSelect>
                            </div>
                        )}

                        {etiquetas.length > 0 && (
                            <div className="w-full sm:w-52">
                                <CampoSelect
                                    etiqueta={t('privado.cursos.filtro.etiqueta')}
                                    identificador="filtroEtiqueta"
                                    value={filtroEtiqueta}
                                    onChange={(evento) => setFiltroEtiqueta(evento.target.value)}
                                >
                                    <option value="">{t('privado.cursos.filtro.todas')}</option>
                                    {etiquetas.map((etiqueta) => (
                                        <option key={etiqueta.etiquetaId} value={etiqueta.etiquetaId}>
                                            {etiqueta.nombre}
                                        </option>
                                    ))}
                                </CampoSelect>
                            </div>
                        )}

                        {hayFiltros && (
                            <button
                                type="button"
                                onClick={limpiarFiltros}
                                className="w-full rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo sm:w-auto"
                            >
                                {t('privado.cursos.filtro.limpiar')}
                            </button>
                        )}

                        <p className="m-0 text-sm text-texto-suave sm:ml-auto">
                            {t('privado.cursos.resumen', { cantidad: cursosVisibles.length })}
                        </p>
                    </div>
                )}

            {!cargando && pestana === 'mios' && (
                <div className="mt-6 grid gap-6 lg:grid-cols-2">
                    {cursos.length === 0 && (
                        <p className="text-sm text-texto-suave">{t('privado.cursos.vacio')}</p>
                    )}

                    {cursos.length > 0 && cursosVisibles.length === 0 && (
                        <p className="text-sm text-texto-suave">{t('privado.cursos.sinResultados')}</p>
                    )}

                    {cursosVisibles.map((item) => (
                        <article
                            key={item.curso.cursoId}
                            className="flex flex-col gap-4 rounded-2xl border border-borde bg-superficie p-6"
                        >
                            <header className="flex flex-col gap-3">
                                <div className="flex flex-wrap items-start justify-between gap-2">
                                    <h2 className="m-0 min-w-0 text-lg font-semibold text-texto">
                                        {item.curso.nombre}
                                    </h2>
                                    <EstadoCurso estado={item.estado} />
                                </div>

                                <p className="m-0 text-sm text-texto-suave">
                                    {t('privado.cursos.detalle', {
                                        idioma: item.curso.idioma,
                                        nivel: item.curso.nivel,
                                    })}
                                    {item.curso.duracionHoras !== null &&
                                        ` · ${t('admin.cursos.tabla.horas')}: ${item.curso.duracionHoras}`}
                                </p>

                                {item.curso.descripcion && (
                                    <p className="m-0 text-sm text-texto">{item.curso.descripcion}</p>
                                )}

                                {/* Clasificación (CU-004-005) y aviso de ventana
                                    de publicación (CU-004-006). */}
                                {(item.curso.sector !== null ||
                                    item.curso.etiquetas.length > 0 ||
                                    fueraDePublicacion(item.curso)) && (
                                    <ul className="m-0 flex list-none flex-wrap gap-2 p-0">
                                        {item.curso.sector !== null && (
                                            <li>
                                                <EtiquetaEstado
                                                    tono="info"
                                                    texto={t(`admin.cursos.sector.${item.curso.sector}`)}
                                                />
                                            </li>
                                        )}

                                        {item.curso.etiquetas.map((etiqueta) => (
                                            <li key={etiqueta.etiquetaId}>
                                                <span className="inline-block rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave">
                                                    {etiqueta.nombre}
                                                </span>
                                            </li>
                                        ))}

                                        {fueraDePublicacion(item.curso) && (
                                            <li>
                                                <span title={t('privado.cursos.fueraDePublicacion.ayuda')}>
                                                    <EtiquetaEstado
                                                        tono="alerta"
                                                        texto={t('privado.cursos.fueraDePublicacion')}
                                                    />
                                                </span>
                                            </li>
                                        )}
                                    </ul>
                                )}
                            </header>

                            <div>
                                <BarraProgreso
                                    porcentaje={item.porcentajeAvance}
                                    etiqueta={t('privado.cursos.avance', { porcentaje: item.porcentajeAvance })}
                                />
                                <p className="mt-2 mb-0 flex flex-wrap gap-x-2 text-xs text-texto-suave">
                                    <span>
                                        {t('privado.cursos.avance', { porcentaje: item.porcentajeAvance })}
                                    </span>
                                    {item.modulos.length > 0 && (
                                        <span>
                                            ·{' '}
                                            {t('privado.cursos.modulosCompletados', {
                                                completados: modulosCompletados(item),
                                                total: item.modulos.length,
                                            })}
                                        </span>
                                    )}
                                    {item.ultimaActividad && (
                                        <span>
                                            ·{' '}
                                            {t('privado.cursos.ultimaActividad', {
                                                fecha: formatearFecha(item.ultimaActividad, true),
                                            })}
                                        </span>
                                    )}
                                </p>
                            </div>

                            <section className="border-t border-borde pt-4">
                                <h3 className="m-0 text-sm font-semibold text-texto">
                                    {t('privado.cursos.modulos')}
                                </h3>

                                {item.modulos.length === 0 ? (
                                    <p className="mt-2 mb-0 text-sm text-texto-suave">
                                        {t('privado.cursos.sinModulos')}
                                    </p>
                                ) : (
                                    <ol className="m-0 mt-2 flex list-none flex-col divide-y divide-borde p-0">
                                        {/* La fila es un enlace a la lección: el
                                            progreso se registra ahí, al entrar y
                                            al terminarla, no con botones acá. */}
                                        {item.modulos.map(({ modulo, progreso }) => {
                                            const estado: EstadoProgreso = progreso?.estado ?? 'NO_INICIADO';
                                            const archivos = archivosDelModulo(item, modulo.moduloId);

                                            return (
                                                <li key={modulo.moduloId}>
                                                    <Link
                                                        to={`/inicio/cursos/${item.curso.cursoId}/modulos/${modulo.moduloId}`}
                                                        className="-mx-2 flex flex-col gap-2 rounded-lg px-2 py-3 text-texto no-underline hover:bg-fondo sm:flex-row sm:items-center sm:justify-between sm:gap-3"
                                                    >
                                                        <span className="min-w-0">
                                                            <span className="block text-sm font-medium text-texto">
                                                                {modulo.ordenModulo}. {modulo.nombre}
                                                            </span>
                                                            {modulo.descripcion && (
                                                                <span className="block text-xs text-texto-suave">
                                                                    {modulo.descripcion}
                                                                </span>
                                                            )}
                                                            {archivos > 0 && (
                                                                <span className="mt-1 block text-xs text-texto-suave">
                                                                    {t('privado.cursos.modulo.material', {
                                                                        cantidad: archivos,
                                                                    })}
                                                                </span>
                                                            )}
                                                        </span>

                                                        <span className="flex shrink-0 flex-wrap items-center gap-2">
                                                            <EstadoCurso estado={estado} />
                                                            <span className="text-sm font-semibold text-primario">
                                                                {t('privado.cursos.abrirModulo')} →
                                                            </span>
                                                        </span>
                                                    </Link>
                                                </li>
                                            );
                                        })}
                                    </ol>
                                )}
                            </section>

                        </article>
                    ))}
                </div>
            )}

            {!cargando && pestana === 'equipo' && (
                <div className="mt-6">
                    <TablaAbm
                        columnas={columnasEquipo}
                        filas={filasEquipo}
                        claveDe={(fila) => fila.clave}
                        mensajeVacio={t('privado.cursos.equipo.vacio')}
                    />
                </div>
            )}
        </div>
    );
}
