import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { notificacionesApi } from '../../api/notificacionesApi';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import type { BandejaResponse } from '../../tipos/notificaciones';

/**
 * La campana de notificaciones (CU-001-010).
 *
 * El caso de uso manda las alertas a "dispositivos móviles"; esta plataforma es
 * web, así que llegan acá. La bandeja no pide permiso: cualquier sesión puede
 * recibir notificaciones.
 *
 * Se lee al montar y cada vez que se abre el panel, sin polling: una alerta la
 * emite un administrador a mano, no es un flujo en tiempo real que justifique
 * un intervalo corriendo en cada pestaña.
 *
 * Si la bandeja falla, la campana simplemente no se muestra: es accesoria y no
 * tiene sentido romper la barra superior por ella.
 */
export function CampanaNotificaciones() {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();

    const [bandeja, setBandeja] = useState<BandejaResponse | null>(null);
    const [abierta, setAbierta] = useState(false);
    const contenedor = useRef<HTMLDivElement>(null);

    useEffect(() => {
        notificacionesApi
            .bandeja()
            .then(setBandeja)
            .catch(() => setBandeja(null));
    }, []);

    // Cerrar al hacer clic afuera o con Escape: el panel tapa contenido y
    // quedarse abierto al navegar es molesto.
    useEffect(() => {
        if (!abierta) {
            return;
        }

        function alClicAfuera(evento: MouseEvent) {
            if (contenedor.current && !contenedor.current.contains(evento.target as Node)) {
                setAbierta(false);
            }
        }

        function alEscape(evento: KeyboardEvent) {
            if (evento.key === 'Escape') {
                setAbierta(false);
            }
        }

        document.addEventListener('mousedown', alClicAfuera);
        document.addEventListener('keydown', alEscape);

        return () => {
            document.removeEventListener('mousedown', alClicAfuera);
            document.removeEventListener('keydown', alEscape);
        };
    }, [abierta]);

    function alternar() {
        const abriendo = !abierta;

        setAbierta(abriendo);

        if (abriendo) {
            notificacionesApi
                .bandeja()
                .then(setBandeja)
                .catch(() => undefined);
        }
    }

    function marcarTodas() {
        notificacionesApi
            .marcarTodas()
            .then(setBandeja)
            .catch(() => undefined);
    }

    if (bandeja === null) {
        return null;
    }

    return (
        <div ref={contenedor} className="relative">
            <button
                type="button"
                onClick={alternar}
                aria-expanded={abierta}
                aria-controls="panel-notificaciones"
                aria-label={
                    bandeja.noLeidas > 0
                        ? `${t('layout.notificaciones.abrir')}: ${t('layout.notificaciones.noLeidas', {
                              cantidad: bandeja.noLeidas,
                          })}`
                        : t('layout.notificaciones.abrir')
                }
                className="relative rounded-lg border border-borde bg-superficie px-3 py-2 text-sm font-semibold text-texto hover:bg-fondo"
            >
                <span aria-hidden="true">🔔</span>
                {bandeja.noLeidas > 0 && (
                    <span
                        aria-hidden="true"
                        className="absolute -right-1 -top-1 min-w-5 rounded-full bg-error px-1 text-xs font-bold leading-5 text-white"
                    >
                        {bandeja.noLeidas}
                    </span>
                )}
            </button>

            {abierta && (
                <div
                    id="panel-notificaciones"
                    className="absolute right-0 z-20 mt-2 w-80 rounded-2xl border border-borde bg-superficie p-4 shadow-lg"
                >
                    <div className="flex items-center justify-between gap-3">
                        <h2 className="m-0 text-sm font-semibold text-texto">
                            {t('layout.notificaciones.titulo')}
                        </h2>

                        {bandeja.noLeidas > 0 && (
                            <button
                                type="button"
                                onClick={marcarTodas}
                                className="rounded-lg border border-borde bg-superficie px-2 py-1 text-xs font-semibold text-texto hover:bg-fondo"
                            >
                                {t('layout.notificaciones.marcarTodas')}
                            </button>
                        )}
                    </div>

                    {bandeja.notificaciones.length === 0 ? (
                        <p className="mt-3 text-sm text-texto-suave">
                            {t('layout.notificaciones.vacio')}
                        </p>
                    ) : (
                        <ul className="mt-3 max-h-80 list-none overflow-y-auto p-0">
                            {bandeja.notificaciones.map((notificacion) => (
                                <li
                                    key={notificacion.notificacionId}
                                    className={
                                        notificacion.leida
                                            ? 'border-t border-borde py-3 first:border-t-0 first:pt-0'
                                            : 'border-t border-borde py-3 first:border-t-0 first:pt-0 font-semibold'
                                    }
                                >
                                    <p className="m-0 text-sm text-texto">{notificacion.titulo}</p>
                                    <p className="mt-1 text-sm font-normal text-texto-suave">
                                        {notificacion.mensaje}
                                    </p>
                                    <p className="mt-1 text-xs font-normal text-texto-suave">
                                        {formatearFecha(notificacion.fechaEnvio, true)}
                                    </p>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            )}
        </div>
    );
}
