import { useTranslation } from 'react-i18next';
import type { ActivoResponse } from '../tipos/activos';

interface ListaActivosProps {
    activos: ActivoResponse[];
    /**
     * Compacta: una fila con miniatura y enlace, para los índices.
     * Completa: el recurso se consume en la página —la imagen se ve y el audio
     * se escucha—, que es lo que hace que una lección sea una lección.
     */
    compacta?: boolean;
}

/**
 * Material pedagógico (CU-004-001) tal como lo consume el empleado. Lo usan
 * Mis cursos, para el material general, y la lección, para el del módulo.
 *
 * Quien la usa ya recibió solo los activos publicados y vigentes: el backend
 * los filtra en `BLLActivoPedagogico.ListarPublicados`.
 */
export function ListaActivos({ activos, compacta = false }: ListaActivosProps) {
    const { t } = useTranslation();

    if (compacta) {
        return (
            <ul className="m-0 grid list-none grid-cols-1 gap-2 p-0 sm:grid-cols-2">
                {activos.map((activo) => (
                    <li key={activo.activoPedagogicoId}>
                        <a
                            href={activo.urlArchivo ?? '#'}
                            target="_blank"
                            rel="noreferrer"
                            className="flex h-full items-center gap-3 rounded-lg border border-borde bg-superficie p-2 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                        >
                            {activo.tipoContenido === 'IMAGEN' && activo.urlArchivo ? (
                                <img
                                    src={activo.urlArchivo}
                                    alt={activo.nombre}
                                    loading="lazy"
                                    className="h-12 w-12 shrink-0 rounded object-cover"
                                />
                            ) : (
                                <span
                                    aria-hidden="true"
                                    className="flex h-12 w-12 shrink-0 items-center justify-center rounded bg-fondo text-lg"
                                >
                                    {activo.tipoContenido === 'AUDIO' ? '♪' : '▤'}
                                </span>
                            )}

                            <span className="flex min-w-0 flex-col">
                                <span className="truncate">{activo.nombre}</span>
                                <span className="text-xs font-normal text-texto-suave">
                                    {t(`admin.cursos.activos.tipo.${activo.tipoContenido}`)}
                                    {' · '}
                                    {t('privado.cursos.activos.abrir')}
                                </span>
                            </span>
                        </a>
                    </li>
                ))}
            </ul>
        );
    }

    return (
        <ul className="m-0 grid list-none grid-cols-1 gap-4 p-0 md:grid-cols-2">
            {activos.map((activo) => (
                <li
                    key={activo.activoPedagogicoId}
                    className="flex flex-col gap-3 rounded-xl border border-borde bg-superficie p-4"
                >
                    <div className="min-w-0">
                        <p className="m-0 text-sm font-semibold text-texto">{activo.nombre}</p>
                        <p className="m-0 text-xs text-texto-suave">
                            {t(`admin.cursos.activos.tipo.${activo.tipoContenido}`)}
                        </p>
                    </div>

                    {activo.descripcion && (
                        <p className="m-0 text-sm text-texto">{activo.descripcion}</p>
                    )}

                    {activo.urlArchivo !== null && activo.tipoContenido === 'IMAGEN' && (
                        <img
                            src={activo.urlArchivo}
                            alt={activo.nombre}
                            loading="lazy"
                            className="max-h-72 w-full rounded-lg bg-fondo object-contain"
                        />
                    )}

                    {activo.urlArchivo !== null && activo.tipoContenido === 'AUDIO' && (
                        <audio controls preload="none" src={activo.urlArchivo} className="w-full">
                            {t('privado.leccion.audio.sinSoporte')}
                        </audio>
                    )}

                    {activo.urlArchivo !== null && (
                        <a
                            href={activo.urlArchivo}
                            target="_blank"
                            rel="noreferrer"
                            className="self-start rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                        >
                            {t('privado.cursos.activos.abrir')}
                        </a>
                    )}
                </li>
            ))}
        </ul>
    );
}
