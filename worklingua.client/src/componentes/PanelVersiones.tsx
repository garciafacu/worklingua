import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../api/clienteHttp';
import { versionesApi } from '../api/versionesApi';
import { useLocalizacion } from '../contexto/useLocalizacion';
import { descargarTexto } from '../servicios/descarga';
import type { DiferenciaVersionResponse, VersionResponse } from '../tipos/versiones';
import { Alerta } from './Alerta';
import { Boton } from './Boton';
import { CampoTexto } from './CampoTexto';

interface PanelVersionesProps {
    cursoId: number;
    puedeModificar: boolean;
    /** Avisa que el curso cambió, para que el ABM relea el listado. */
    alRestaurar: () => void;
}

/**
 * Historial de versiones de un curso (CU-004-004).
 *
 * Cada punto de restauración es el curso entero serializado a XML: datos,
 * clasificación, ventana, etiquetas, módulos y activos. Restaurar lee ese
 * documento y lo vuelve a aplicar, dejando antes una versión automática con lo
 * que había, así que nunca es un movimiento irreversible.
 *
 * El XML se puede ver tal cual: es el único lugar del proyecto donde el
 * formato del punto 23 del Formulario queda a la vista.
 */
export function PanelVersiones({ cursoId, puedeModificar, alRestaurar }: PanelVersionesProps) {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();

    const [versiones, setVersiones] = useState<VersionResponse[]>([]);
    const [observaciones, setObservaciones] = useState('');
    const [seleccionadas, setSeleccionadas] = useState<number[]>([]);
    const [diferencias, setDiferencias] = useState<DiferenciaVersionResponse[] | null>(null);
    const [xmlVisible, setXmlVisible] = useState<VersionResponse | null>(null);
    /** Id de la versión que está esperando confirmación de borrado. */
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [recarga, setRecarga] = useState(0);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        versionesApi
            .listar(cursoId)
            .then(setVersiones)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.cursos.versiones.error')))
            .finally(() => setCargando(false));
    }, [cursoId, recarga, mensajeDeError]);

    function recargar() {
        setRecarga((numero) => numero + 1);
    }

    async function crear() {
        setError(null);
        setExito(null);
        setGuardando(true);

        try {
            await versionesApi.crear(cursoId, observaciones.trim() || null);

            setExito(t('admin.cursos.versiones.exitoCrear'));
            setObservaciones('');
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.versiones.errorCrear'));
        } finally {
            setGuardando(false);
        }
    }

    async function restaurar(version: VersionResponse) {
        setError(null);
        setExito(null);

        try {
            const resultado = await versionesApi.restaurar(version.versionContenidoId);

            const aviso = t('admin.cursos.versiones.exitoRestaurar');

            setExito(
                resultado.omitidos === 0
                    ? aviso
                    : `${aviso} ${t('admin.cursos.versiones.omitidos', { cantidad: resultado.omitidos })}`,
            );
            setDiferencias(null);
            setSeleccionadas([]);
            recargar();
            alRestaurar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.versiones.errorRestaurar'));
        }
    }

    /** Dos a la vez: elegir una tercera reemplaza a la más vieja. */
    function alternarSeleccion(versionId: number) {
        setDiferencias(null);

        if (seleccionadas.includes(versionId)) {
            setSeleccionadas(seleccionadas.filter((id) => id !== versionId));

            return;
        }

        setSeleccionadas(
            seleccionadas.length < 2 ? [...seleccionadas, versionId] : [seleccionadas[1], versionId],
        );
    }

    async function comparar() {
        if (seleccionadas.length !== 2) {
            return;
        }

        setError(null);

        try {
            setDiferencias(await versionesApi.comparar(seleccionadas[0], seleccionadas[1]));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.versiones.error'));
        }
    }

    async function verXml(version: VersionResponse) {
        setError(null);

        try {
            setXmlVisible(await versionesApi.obtener(version.versionContenidoId));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.versiones.error'));
        }
    }

    /**
     * El XML se pide completo recién acá: el historial viaja sin él, así que
     * descargar es la única forma de llevárselo sin abrirlo en pantalla.
     */
    async function descargarXml(version: VersionResponse) {
        setError(null);

        try {
            const completa = await versionesApi.obtener(version.versionContenidoId);

            descargarTexto(
                `curso-${completa.cursoId}-v${completa.numeroVersion}.xml`,
                completa.contenidoXml ?? '',
                'application/xml;charset=utf-8',
            );
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.versiones.error'));
        }
    }

    async function eliminar(version: VersionResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await versionesApi.eliminar(version.versionContenidoId);

            setExito(t('admin.cursos.versiones.exitoEliminar'));
            // Si estaba elegida para comparar o abierta, dejarla afuera.
            setSeleccionadas(seleccionadas.filter((id) => id !== version.versionContenidoId));
            setDiferencias(null);

            if (xmlVisible?.versionContenidoId === version.versionContenidoId) {
                setXmlVisible(null);
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.versiones.errorEliminar'));
        }
    }

    function numeroDe(versionId: number) {
        const version = versiones.find((item) => item.versionContenidoId === versionId);

        return version === undefined ? '' : version.numeroVersion;
    }

    return (
        <div className="flex flex-col gap-4">
            <p className="m-0 text-sm text-texto-suave">{t('admin.cursos.versiones.descripcion')}</p>

            <Alerta tipo="error" mensaje={error} />
            <Alerta tipo="exito" mensaje={exito} />

            {puedeModificar && (
                <div className="flex flex-wrap items-end gap-3 border-b border-borde pb-4">
                    <div className="w-72">
                        <CampoTexto
                            etiqueta={t('admin.cursos.versiones.crear.observaciones')}
                            identificador="versionObservaciones"
                            maxLength={500}
                            value={observaciones}
                            onChange={(evento) => setObservaciones(evento.target.value)}
                        />
                    </div>
                    <Boton type="button" cargando={guardando} onClick={() => void crear()}>
                        {t('admin.cursos.versiones.crear')}
                    </Boton>
                </div>
            )}

            {cargando ? (
                <p className="text-sm text-texto-suave">{t('admin.cursos.versiones.cargando')}</p>
            ) : versiones.length === 0 ? (
                <p className="text-sm text-texto-suave">{t('admin.cursos.versiones.vacio')}</p>
            ) : (
                <>
                    <p className="m-0 text-xs text-texto-suave">
                        {t('admin.cursos.versiones.comparar.ayuda')}
                    </p>

                    <ul className="m-0 flex list-none flex-col gap-2 p-0">
                        {versiones.map((version) => (
                            <li
                                key={version.versionContenidoId}
                                className={
                                    seleccionadas.includes(version.versionContenidoId)
                                        ? 'flex flex-wrap items-center justify-between gap-3 rounded-lg border border-primario p-3'
                                        : 'flex flex-wrap items-center justify-between gap-3 rounded-lg border border-borde p-3'
                                }
                            >
                                <label className="flex items-start gap-3">
                                    <input
                                        type="checkbox"
                                        className="mt-1"
                                        checked={seleccionadas.includes(version.versionContenidoId)}
                                        onChange={() => alternarSeleccion(version.versionContenidoId)}
                                    />
                                    <span>
                                        <span className="block text-sm font-semibold text-texto">
                                            v{version.numeroVersion} ·{' '}
                                            {formatearFecha(version.fechaVersion, true)}
                                        </span>
                                        <span className="block text-xs text-texto-suave">
                                            {version.autor ?? t('comun.valor.vacio')}
                                            {version.observaciones
                                                ? ` · ${version.observaciones}`
                                                : ''}
                                        </span>
                                    </span>
                                </label>

                                <div className="flex flex-wrap gap-2">
                                    {confirmandoBaja === version.versionContenidoId ? (
                                        <>
                                            <span className="self-center text-sm text-texto-suave">
                                                {t('admin.cursos.versiones.eliminar.confirmar', {
                                                    version: version.numeroVersion,
                                                })}
                                            </span>
                                            <button
                                                type="button"
                                                onClick={() => void eliminar(version)}
                                                className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                            >
                                                {t('admin.cursos.versiones.eliminar')}
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => setConfirmandoBaja(null)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                            >
                                                {t('admin.cursos.versiones.eliminar.cancelar')}
                                            </button>
                                        </>
                                    ) : (
                                        <>
                                            <button
                                                type="button"
                                                onClick={() => void verXml(version)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                            >
                                                {t('admin.cursos.versiones.verXml')}
                                            </button>

                                            <button
                                                type="button"
                                                onClick={() => void descargarXml(version)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                            >
                                                {t('admin.cursos.versiones.descargar')}
                                            </button>

                                            {puedeModificar && (
                                                <button
                                                    type="button"
                                                    onClick={() => void restaurar(version)}
                                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                                >
                                                    {t('admin.cursos.versiones.restaurar')}
                                                </button>
                                            )}

                                            {puedeModificar && (
                                                <button
                                                    type="button"
                                                    onClick={() =>
                                                        setConfirmandoBaja(version.versionContenidoId)
                                                    }
                                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                                >
                                                    {t('admin.cursos.versiones.eliminar')}
                                                </button>
                                            )}
                                        </>
                                    )}
                                </div>
                            </li>
                        ))}
                    </ul>

                    {seleccionadas.length === 2 && (
                        <div>
                            <button
                                type="button"
                                onClick={() => void comparar()}
                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                            >
                                {t('admin.cursos.versiones.comparar')}
                            </button>
                        </div>
                    )}

                    {/* Camino alternativo 3: qué cambió entre las dos. */}
                    {diferencias !== null && (
                        <div className="rounded-lg border border-borde p-3">
                            <h3 className="m-0 text-sm font-semibold text-texto">
                                {t('admin.cursos.versiones.comparar.titulo', {
                                    izquierda: numeroDe(seleccionadas[0]),
                                    derecha: numeroDe(seleccionadas[1]),
                                })}
                            </h3>

                            {diferencias.length === 0 ? (
                                <p className="mt-2 text-sm text-texto-suave">
                                    {t('admin.cursos.versiones.comparar.iguales')}
                                </p>
                            ) : (
                                <table className="mt-3 w-full text-left text-sm">
                                    <thead>
                                        <tr className="text-xs uppercase text-texto-suave">
                                            <th className="py-1">
                                                {t('admin.cursos.versiones.comparar.campo')}
                                            </th>
                                            <th className="py-1">v{numeroDe(seleccionadas[0])}</th>
                                            <th className="py-1">v{numeroDe(seleccionadas[1])}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {diferencias.map((diferencia) => (
                                            <tr key={diferencia.campo} className="border-t border-borde">
                                                <th scope="row" className="py-2 font-semibold text-texto">
                                                    {t(
                                                        `admin.cursos.versiones.campo.${diferencia.campo}`,
                                                    )}
                                                </th>
                                                <td className="py-2 text-texto-suave">
                                                    {diferencia.izquierda || t('comun.valor.vacio')}
                                                </td>
                                                <td className="py-2 text-texto">
                                                    {diferencia.derecha || t('comun.valor.vacio')}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            )}

                            <button
                                type="button"
                                onClick={() => setDiferencias(null)}
                                className="mt-3 rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                            >
                                {t('admin.cursos.versiones.comparar.cerrar')}
                            </button>
                        </div>
                    )}

                    {/* El documento XML del punto 23, tal cual se guarda. */}
                    {xmlVisible !== null && (
                        <div className="rounded-lg border border-borde p-3">
                            <h3 className="m-0 text-sm font-semibold text-texto">
                                {t('admin.cursos.versiones.xml.titulo', {
                                    version: xmlVisible.numeroVersion,
                                })}
                            </h3>
                            <pre className="mt-2 max-h-80 overflow-auto rounded bg-fondo p-3 text-xs text-texto">
                                {xmlVisible.contenidoXml}
                            </pre>
                            <button
                                type="button"
                                onClick={() => setXmlVisible(null)}
                                className="mt-2 rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                            >
                                {t('admin.cursos.versiones.xml.cerrar')}
                            </button>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}
