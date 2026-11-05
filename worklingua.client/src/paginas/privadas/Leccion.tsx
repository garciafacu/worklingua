import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { progresoApi } from '../../api/cursosApi';
import { Alerta } from '../../componentes/Alerta';
import { EtiquetaEstado, type TonoEstado } from '../../componentes/Basicos';
import { ListaActivos } from '../../componentes/ListaActivos';
import type { CursoConProgresoResponse, EstadoProgreso } from '../../tipos/cursos';

const TONO_POR_ESTADO: Record<EstadoProgreso, TonoEstado> = {
    NO_INICIADO: 'neutro',
    EN_CURSO: 'info',
    COMPLETADO: 'exito',
};

export function Leccion() {
    const { t } = useTranslation();
    const navegar = useNavigate();
    const parametros = useParams();

    const cursoId = Number(parametros.cursoId);
    const moduloId = Number(parametros.moduloId);

    const [cursos, setCursos] = useState<CursoConProgresoResponse[] | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [finalizando, setFinalizando] = useState(false);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        let vigente = true;

        progresoApi
            .listarMio()
            .then((lista) => {
                const empezado = lista
                    .find((item) => item.curso.cursoId === cursoId)
                    ?.modulos.find(({ modulo }) => modulo.moduloId === moduloId)?.progreso;

                if (empezado !== null && empezado !== undefined) {
                    return lista;
                }

                return progresoApi.guardar({ moduloId, accion: 'INICIAR' }).catch(() => lista);
            })
            .then((lista) => {
                if (vigente) {
                    setCursos(lista);
                }
            })
            .catch((excepcion) => {
                if (vigente) {
                    setCursos([]);
                    setError(mensajeDeError(excepcion, 'privado.leccion.error'));
                }
            });

        return () => {
            vigente = false;
        };
    }, [cursoId, moduloId, mensajeDeError]);

    if (cursos === null) {
        return <p className="text-sm text-texto-suave">{t('privado.leccion.cargando')}</p>;
    }

    const curso = cursos.find((item) => item.curso.cursoId === cursoId);
    const posicion = curso?.modulos.findIndex(({ modulo }) => modulo.moduloId === moduloId) ?? -1;

    if (curso === undefined || posicion < 0) {
        return (
            <div className="mx-auto max-w-3xl">
                <Alerta tipo="error" mensaje={error ?? t('privado.leccion.noDisponible')} />
                <Link to="/inicio/cursos" className="text-sm font-semibold text-primario">
                    {t('privado.leccion.volver')}
                </Link>
            </div>
        );
    }

    const { modulo, progreso } = curso.modulos[posicion];
    const estado: EstadoProgreso = progreso?.estado ?? 'EN_CURSO';
    const anterior = posicion > 0 ? curso.modulos[posicion - 1].modulo : null;
    const siguiente = posicion + 1 < curso.modulos.length ? curso.modulos[posicion + 1].modulo : null;

    const material = curso.activos.filter((activo) => activo.moduloId === moduloId);
    const materialCurso = curso.activos.filter((activo) => activo.moduloId === null);

    async function finalizar() {
        setError(null);
        setFinalizando(true);

        try {
            await progresoApi.guardar({ moduloId, accion: 'COMPLETAR' });

            navegar('/inicio/cursos');
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'privado.leccion.errorGuardar'));
            setFinalizando(false);
        }
    }

    function enlaceDeModulo(destino: number) {
        return `/inicio/cursos/${cursoId}/modulos/${destino}`;
    }

    return (
        <article className="mx-auto flex max-w-3xl flex-col gap-6">
            <header className="flex flex-col gap-3">
                <Link to="/inicio/cursos" className="text-sm font-semibold text-primario no-underline">
                    ← {curso.curso.nombre}
                </Link>

                <div className="flex flex-wrap items-start justify-between gap-2">
                    <h1 className="m-0 min-w-0 text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                        {modulo.nombre}
                    </h1>
                    <EtiquetaEstado
                        texto={t(`comun.progreso.estado.${estado}`)}
                        tono={TONO_POR_ESTADO[estado]}
                    />
                </div>

                <p className="m-0 text-sm text-texto-suave">
                    {t('privado.leccion.posicion', {
                        numero: posicion + 1,
                        total: curso.modulos.length,
                    })}
                </p>
            </header>

            <Alerta tipo="error" mensaje={error} />

            {estado === 'COMPLETADO' && (
                <Alerta tipo="exito" mensaje={t('privado.leccion.completado')} />
            )}

            <section className="rounded-2xl border border-borde bg-superficie p-6">
                <h2 className="m-0 text-sm font-semibold text-texto">{t('privado.leccion.objetivo')}</h2>
                <p className="mt-2 mb-0 text-sm text-texto">
                    {modulo.descripcion ?? t('privado.leccion.sinObjetivo')}
                </p>
            </section>

            <section>
                <h2 className="m-0 mb-3 text-sm font-semibold text-texto">
                    {t('privado.leccion.contenido')}
                </h2>

                {modulo.contenido === null ? (
                    <p className="m-0 text-sm text-texto-suave">{t('privado.leccion.sinContenido')}</p>
                ) : (
                    <p className="m-0 text-sm leading-relaxed whitespace-pre-line text-texto">
                        {modulo.contenido}
                    </p>
                )}
            </section>

            <section>
                <h2 className="m-0 mb-3 text-sm font-semibold text-texto">
                    {t('privado.leccion.material')}
                </h2>

                {material.length === 0 ? (
                    <p className="m-0 text-sm text-texto-suave">{t('privado.leccion.contenido.vacio')}</p>
                ) : (
                    <ListaActivos activos={material} />
                )}
            </section>

            {materialCurso.length > 0 && (
                <section>
                    <h2 className="m-0 mb-3 text-sm font-semibold text-texto">
                        {t('privado.leccion.materialCurso')}
                    </h2>
                    <ListaActivos activos={materialCurso} compacta />
                </section>
            )}

            <footer className="flex flex-col gap-3 border-t border-borde pt-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex flex-wrap gap-2">
                    {anterior !== null && (
                        <Link
                            to={enlaceDeModulo(anterior.moduloId)}
                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                        >
                            ← {t('privado.leccion.anterior')}
                        </Link>
                    )}
                    {siguiente !== null && (
                        <Link
                            to={enlaceDeModulo(siguiente.moduloId)}
                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                        >
                            {t('privado.leccion.siguiente')} →
                        </Link>
                    )}
                </div>

                {estado === 'COMPLETADO' ? (
                    <Link
                        to="/inicio/cursos"
                        className="rounded-lg bg-primario px-4 py-2 text-center text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                    >
                        {t('privado.leccion.volver')}
                    </Link>
                ) : (
                    <button
                        type="button"
                        disabled={finalizando}
                        onClick={() => void finalizar()}
                        className="rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white hover:bg-primario-hover disabled:opacity-50"
                    >
                        {finalizando ? t('comun.estado.procesando') : t('privado.leccion.finalizar')}
                    </button>
                )}
            </footer>
        </article>
    );
}
