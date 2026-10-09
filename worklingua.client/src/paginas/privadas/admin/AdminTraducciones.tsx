import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { idiomasApi } from '../../../api/idiomasApi';
import { traduccionesApi } from '../../../api/traduccionesApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { IDIOMA_BASE } from '../../../i18n/configuracion';
import type { IdiomaAdminResponse } from '../../../tipos/idiomas';
import type { TraduccionAdminResponse } from '../../../tipos/traducciones';

/**
 * ABM de las traducciones de la interfaz.
 *
 * Al elegir un idioma se listan **todas** las claves del catálogo, no solo las
 * que ese idioma ya tiene: el Stored Procedure recorre las del español con un
 * LEFT JOIN, así que una clave sin traducir aparece igual, marcada como
 * pendiente.
 *
 * La columna en español está siempre a la vista y es de solo lectura: es el
 * original contra el que se traduce.
 *
 * Un texto vacío es un valor válido y significa "vuelve a estar pendiente": el
 * sitio la muestra otra vez en español por fallback.
 */
export function AdminTraducciones() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { idiomaActual } = useLocalizacion();

    const [idiomas, setIdiomas] = useState<IdiomaAdminResponse[]>([]);
    const [idiomaId, setIdiomaId] = useState<number | null>(null);
    const [filas, setFilas] = useState<TraduccionAdminResponse[]>([]);
    /** Textos editados que todavía no se guardaron, indexados por clave. */
    const [editadas, setEditadas] = useState<Record<string, string>>({});
    const [busqueda, setBusqueda] = useState('');
    const [soloPendientes, setSoloPendientes] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [recarga, setRecarga] = useState(0);

    const puedeModificar = tienePermiso('Traduccion.Modificar');

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    // El catálogo de idiomas se pide una sola vez. Arranca elegido el idioma que
    // la persona está usando, salvo que sea el español: traducir el español al
    // español no tiene sentido, así que en ese caso se ofrece el primer idioma
    // que no sea el base.
    useEffect(() => {
        idiomasApi
            .listarAdministracion()
            .then((lista) => {
                setIdiomas(lista);

                const preferido =
                    lista.find(
                        (idioma) =>
                            idioma.codigoISO === idiomaActual &&
                            idioma.codigoISO !== IDIOMA_BASE,
                    ) ?? lista.find((idioma) => idioma.codigoISO !== IDIOMA_BASE);

                setIdiomaId(preferido ? preferido.idiomaId : null);
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.traducciones.errorIdiomas')),
            );
        // Solo al montar: cambiar de idioma la interfaz no debe reordenar la
        // pantalla que se está usando para traducir.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // La grilla se pide al servidor con los filtros aplicados, no se filtra en
    // memoria: son cientos de claves y el SP ya sabe resolver "solo pendientes".
    //
    // Sin idioma elegido no se llama a la API y la grilla queda vacía. Eso se
    // resuelve al renderizar, con `filasVisibles`, y no con un setState en el
    // cuerpo del efecto: escribir estado ahí dispara un render en cascada
    // (react-hooks/set-state-in-effect).
    useEffect(() => {
        if (idiomaId === null) {
            return;
        }

        traduccionesApi
            .listarAdministracion({
                idiomaId,
                texto: busqueda.trim() || undefined,
                clave: undefined,
                soloPendientes,
            })
            .then((resultado) => {
                setFilas(resultado);
                setError(null);
            })
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.traducciones.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [idiomaId, busqueda, soloPendientes, recarga, mensajeDeError]);

    // `cargando` lo encienden los manejadores, no el efecto: escribir estado en
    // el cuerpo de un efecto dispara un render en cascada. Es el mismo criterio
    // que ya usa AdminBitacora con su bandera `buscando`.
    function filtrar(aplicar: () => void) {
        setCargando(true);
        aplicar();
    }

    // Cambiar de idioma descarta lo editado: guardarlo contra otro idioma sería
    // escribir en el lugar equivocado.
    function elegirIdioma(nuevo: number) {
        filtrar(() => {
            setIdiomaId(nuevo);
            setEditadas({});
            setExito(null);
        });
    }

    function editarTexto(clave: string, valor: string) {
        setEditadas((actuales) => ({ ...actuales, [clave]: valor }));
    }

    /** El texto que se muestra en el campo: lo editado si lo hay, si no lo guardado. */
    function textoDe(fila: TraduccionAdminResponse): string {
        return editadas[fila.clave] ?? fila.texto;
    }

    function estaSucia(fila: TraduccionAdminResponse): boolean {
        const editado = editadas[fila.clave];

        return editado !== undefined && editado !== fila.texto;
    }

    // Sin idioma elegido no hay nada que mostrar, aunque `filas` conserve lo del
    // idioma anterior.
    const filasVisibles = idiomaId === null ? [] : filas;

    const sucias = filasVisibles.filter(estaSucia);
    const pendientes = filasVisibles.filter((fila) => fila.pendiente).length;

    async function guardar() {
        if (idiomaId === null || sucias.length === 0) {
            return;
        }

        setError(null);
        setExito(null);
        setGuardando(true);

        try {
            const respuesta = await traduccionesApi.guardar({
                idiomaId,
                traducciones: sucias.map((fila) => ({
                    clave: fila.clave,
                    texto: textoDe(fila),
                })),
            });

            setExito(respuesta.mensaje);
            setEditadas({});
            setCargando(true);
            setRecarga((numero) => numero + 1);
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.traducciones.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    const idiomaElegido = idiomas.find((idioma) => idioma.idiomaId === idiomaId);

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.traducciones.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.traducciones.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    <CampoSelect
                        etiqueta={t('admin.traducciones.filtro.idioma')}
                        identificador="idiomaTraduccion"
                        value={idiomaId === null ? '' : String(idiomaId)}
                        onChange={(evento) => elegirIdioma(Number(evento.target.value))}
                    >
                        <option value="">{t('admin.traducciones.filtro.elegirIdioma')}</option>
                        {idiomas
                            // El español es el original: no se traduce a sí mismo.
                            .filter((idioma) => idioma.codigoISO !== IDIOMA_BASE)
                            .map((idioma) => (
                                <option key={idioma.idiomaId} value={idioma.idiomaId}>
                                    {idioma.nombre}
                                    {idioma.activo ? '' : ` (${t('comun.estado.inactivo')})`}
                                </option>
                            ))}
                    </CampoSelect>

                    <CampoTexto
                        etiqueta={t('admin.traducciones.filtro.buscar')}
                        identificador="busquedaTraduccion"
                        type="search"
                        placeholder={t('admin.traducciones.filtro.buscarPlaceholder')}
                        value={busqueda}
                        onChange={(evento) => filtrar(() => setBusqueda(evento.target.value))}
                    />

                    <label className="campo flex-row items-center gap-2 self-end pb-2">
                        <input
                            type="checkbox"
                            checked={soloPendientes}
                            onChange={(evento) =>
                                filtrar(() => setSoloPendientes(evento.target.checked))
                            }
                            className="h-4 w-4"
                        />
                        <span className="text-sm text-texto">
                            {t('admin.traducciones.filtro.soloPendientes')}
                        </span>
                    </label>
                </div>

                {idiomaElegido && !cargando && (
                    <p className="mt-4 text-sm text-texto-suave" role="status">
                        {t('admin.traducciones.resumen', {
                            idioma: idiomaElegido.nombre,
                            total: filasVisibles.length,
                            pendientes,
                        })}
                    </p>
                )}
            </section>

            <section className="mt-6">
                {idiomaId === null ? (
                    <p className="rounded-2xl border border-dashed border-borde bg-superficie px-6 py-10 text-center text-sm text-texto-suave">
                        {t('admin.traducciones.sinIdioma')}
                    </p>
                ) : cargando ? (
                    <p className="text-sm text-texto-suave">{t('admin.traducciones.cargando')}</p>
                ) : filasVisibles.length === 0 ? (
                    <p className="text-sm text-texto-suave">{t('admin.traducciones.vacio')}</p>
                ) : (
                    <div className="overflow-x-auto rounded-2xl border border-borde bg-superficie">
                        <table className="w-full border-collapse text-sm">
                            <thead>
                                <tr className="border-b border-borde text-left">
                                    <th
                                        scope="col"
                                        className="px-3 py-2 text-xs font-semibold tracking-wide text-texto-suave uppercase"
                                    >
                                        {t('admin.traducciones.tabla.clave')}
                                    </th>
                                    <th
                                        scope="col"
                                        className="px-3 py-2 text-xs font-semibold tracking-wide text-texto-suave uppercase"
                                    >
                                        {t('admin.traducciones.tabla.espanol')}
                                    </th>
                                    <th
                                        scope="col"
                                        className="px-3 py-2 text-xs font-semibold tracking-wide text-texto-suave uppercase"
                                    >
                                        {idiomaElegido
                                            ? idiomaElegido.nombre
                                            : t('admin.traducciones.tabla.traduccion')}
                                    </th>
                                </tr>
                            </thead>

                            <tbody>
                                {filasVisibles.map((fila) => {
                                    const sucia = estaSucia(fila);

                                    return (
                                        <tr
                                            key={fila.clave}
                                            className="border-b border-borde align-top last:border-b-0"
                                        >
                                            <td className="px-3 py-2 font-mono text-xs break-all text-texto-suave">
                                                {fila.clave}
                                                {fila.pendiente && (
                                                    <span className="mt-1 block font-sans text-[11px] font-semibold text-error">
                                                        {t('admin.traducciones.pendiente')}
                                                    </span>
                                                )}
                                            </td>

                                            <td className="px-3 py-2 text-texto-suave">
                                                {fila.textoEspanol}
                                            </td>

                                            <td className="px-3 py-2">
                                                <textarea
                                                    rows={2}
                                                    maxLength={1000}
                                                    disabled={!puedeModificar}
                                                    aria-label={t(
                                                        'admin.traducciones.tabla.editar',
                                                        { clave: fila.clave },
                                                    )}
                                                    value={textoDe(fila)}
                                                    onChange={(evento) =>
                                                        editarTexto(fila.clave, evento.target.value)
                                                    }
                                                    className={`w-full rounded-lg border bg-superficie p-2 text-sm text-texto outline-none focus:border-borde-foco disabled:cursor-not-allowed disabled:opacity-60 ${
                                                        sucia ? 'border-primario' : 'border-borde'
                                                    }`}
                                                />
                                            </td>
                                        </tr>
                                    );
                                })}
                            </tbody>
                        </table>
                    </div>
                )}

                {puedeModificar && filasVisibles.length > 0 && (
                    <div className="mt-4 flex flex-wrap items-center gap-3">
                        <Boton
                            type="button"
                            cargando={guardando}
                            disabled={sucias.length === 0}
                            onClick={() => void guardar()}
                        >
                            {sucias.length === 1
                                ? t('admin.traducciones.guardarUno')
                                : t('admin.traducciones.guardar', { cantidad: sucias.length })}
                        </Boton>

                        {sucias.length > 0 && (
                            <button
                                type="button"
                                onClick={() => setEditadas({})}
                                className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                            >
                                {t('admin.traducciones.descartar')}
                            </button>
                        )}
                    </div>
                )}
            </section>
        </div>
    );
}
