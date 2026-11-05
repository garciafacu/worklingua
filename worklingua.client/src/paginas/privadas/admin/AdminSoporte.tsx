import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';
import { ErrorApi } from '../../../api/clienteHttp';
import { ticketsApi } from '../../../api/ticketsApi';
import { Alerta } from '../../../componentes/Alerta';
import { CampoSelect } from '../../../componentes/Campos';
import { EstadoTicketEtiqueta, HiloTicket } from '../../../componentes/HiloTicket';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { ESTADOS_TICKET, type EstadoTicket, type TicketResponse } from '../../../tipos/tickets';

export function AdminSoporte() {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();
    const [parametros, setParametros] = useSearchParams();
    const ticketElegido = Number(parametros.get('ticket')) || null;

    const [tickets, setTickets] = useState<TicketResponse[]>([]);
    const [detalle, setDetalle] = useState<TicketResponse | null>(null);
    const [filtro, setFiltro] = useState<EstadoTicket | ''>('PENDIENTE');
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        ticketsApi
            .listarAdministracion(filtro)
            .then(setTickets)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'soporte.error')))
            .finally(() => setCargando(false));
    }, [filtro, recarga, mensajeDeError]);

    useEffect(() => {
        if (ticketElegido === null) {
            return;
        }

        ticketsApi
            .obtenerAdministracion(ticketElegido)
            .then(setDetalle)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'soporte.error')));
    }, [ticketElegido, mensajeDeError]);

    function actualizar(ticket: TicketResponse) {
        setDetalle(ticket);
        setRecarga((numero) => numero + 1);
    }

    async function cambiarEstado(ticketId: number, estado: EstadoTicket) {
        setError(null);

        try {
            actualizar(await ticketsApi.cambiarEstado(ticketId, estado));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'soporte.error'));
        }
    }

    const columnas: ColumnaAbm<TicketResponse>[] = [
        { encabezado: '#', celda: (fila) => fila.ticket.ticketId },
        {
            encabezado: t('admin.soporte.col.asunto'),
            celda: (fila) => (
                <>
                    <span className="font-medium text-texto">{fila.ticket.asunto}</span>
                    <br />
                    <span className="text-xs text-texto-suave">
                        {fila.plan ?? t('soporte.hilo.contextoPublica')}
                        {fila.curso && ` · ${fila.curso}`}
                    </span>
                </>
            ),
        },
        {
            encabezado: t('admin.soporte.col.empresa'),
            celda: (fila) => (
                <>
                    {fila.empresa === null ? (
                        <span
                            className="rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave"
                            title={t('admin.soporte.publica.ayuda')}
                        >
                            {t('admin.soporte.publica')}
                        </span>
                    ) : (
                        <span className="text-texto">{fila.empresa}</span>
                    )}
                    <br />
                    <span className="text-xs text-texto-suave">
                        {fila.autor}
                        {fila.empresa === null && ` · ${fila.autorEmail}`}
                    </span>
                </>
            ),
        },
        { encabezado: t('admin.soporte.col.estado'), celda: (fila) => <EstadoTicketEtiqueta estado={fila.ticket.estado} /> },
        {
            encabezado: t('admin.soporte.col.ultimoMovimiento'),
            celda: (fila) => formatearFecha(fila.ticket.fechaUltimoMovimiento, true),
        },
        { encabezado: t('admin.soporte.col.mensajes'), numerica: true, celda: (fila) => fila.cantidadMensajes },
    ];

    const detalleVisible = detalle !== null && detalle.ticket.ticketId === ticketElegido ? detalle : null;
    const claseBoton =
        'rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo';

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">{t('admin.soporte.titulo')}</h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.soporte.descripcion')}</p>

            <div className="mt-6">
                <Alerta tipo="error" mensaje={error} />
            </div>

            {detalleVisible && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <HiloTicket
                        key={detalleVisible.ticket.ticketId}
                        ticket={detalleVisible}
                        vista="operador"
                        alEnviar={async (texto) =>
                            actualizar(await ticketsApi.responder(detalleVisible.ticket.ticketId, texto))
                        }
                        acciones={
                            <>
                                {detalleVisible.ticket.estado !== 'EN_PROCESO' && (
                                    <button
                                        type="button"
                                        onClick={() => void cambiarEstado(detalleVisible.ticket.ticketId, 'EN_PROCESO')}
                                        className={claseBoton}
                                    >
                                        {detalleVisible.ticket.estado === 'CERRADO'
                                            ? t('admin.soporte.reabrir')
                                            : t('admin.soporte.tomar')}
                                    </button>
                                )}
                                {detalleVisible.ticket.estado !== 'CERRADO' && (
                                    <button
                                        type="button"
                                        onClick={() => void cambiarEstado(detalleVisible.ticket.ticketId, 'CERRADO')}
                                        className={claseBoton}
                                    >
                                        {t('admin.soporte.cerrar')}
                                    </button>
                                )}
                                <button
                                    type="button"
                                    onClick={() => setParametros({}, { replace: true })}
                                    className={claseBoton}
                                >
                                    {t('comun.boton.cerrar')}
                                </button>
                            </>
                        }
                    />
                </section>
            )}

            <section className="mt-6">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                    <h2 className="text-lg font-semibold text-texto">{t('admin.soporte.bandeja')}</h2>
                    <div className="sm:w-64">
                        <CampoSelect
                            etiqueta={t('soporte.filtro')}
                            identificador="filtroEstadoAdmin"
                            value={filtro}
                            onChange={(evento) => {
                                setCargando(true);
                                setFiltro(evento.target.value as EstadoTicket | '');
                            }}
                        >
                            <option value="">{t('soporte.todos')}</option>
                            {ESTADOS_TICKET.map((estado) => (
                                <option key={estado} value={estado}>
                                    {t(`comun.ticket.estado.${estado}`)}
                                </option>
                            ))}
                        </CampoSelect>
                    </div>
                </div>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('soporte.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={tickets}
                            claveDe={(fila) => fila.ticket.ticketId}
                            inactiva={(fila) => fila.ticket.estado === 'CERRADO'}
                            mensajeVacio={t('soporte.vacio')}
                            acciones={(fila) => (
                                <div className="flex justify-end">
                                    <button
                                        type="button"
                                        onClick={() => setParametros({ ticket: String(fila.ticket.ticketId) }, { replace: true })}
                                        className={claseBoton}
                                    >
                                        {t('comun.boton.ver')}
                                    </button>
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
