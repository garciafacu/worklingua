import { useState, type FormEvent, type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { useLocalizacion } from '../contexto/useLocalizacion';
import type { EstadoTicket, TicketResponse } from '../tipos/tickets';
import { Alerta } from './Alerta';
import { Boton } from './Boton';
import { EtiquetaEstado, type TonoEstado } from './EtiquetaEstado';

const TONO_POR_ESTADO: Record<EstadoTicket, TonoEstado> = {
    PENDIENTE: 'alerta',
    EN_PROCESO: 'info',
    RESPONDIDO: 'exito',
    CERRADO: 'neutro',
};

/** Pastilla con el estado traducido de una consulta. */
export function EstadoTicketEtiqueta({ estado }: { estado: EstadoTicket }) {
    const { t } = useTranslation();

    return <EtiquetaEstado texto={t(`comun.ticket.estado.${estado}`)} tono={TONO_POR_ESTADO[estado]} />;
}

interface HiloTicketProps {
    ticket: TicketResponse;
    /** Desde qué lado se mira: define de qué lado se alinean los mensajes propios. */
    vista: 'cliente' | 'operador';
    /** Envía el mensaje y devuelve la consulta actualizada, o lanza el error de la API. */
    alEnviar: (texto: string) => Promise<void>;
    /** Acciones extra del encabezado (cerrar, tomar). */
    acciones?: ReactNode;
}

/**
 * Detalle de una consulta de soporte: datos, historial de mensajes y cuadro para
 * responder. Lo comparten la pantalla del cliente y la bandeja del Backoffice;
 * cada una resuelve qué endpoint llama al enviar.
 */
export function HiloTicket({ ticket, vista, alEnviar, acciones }: HiloTicketProps) {
    const { t } = useTranslation();
    const { formatearFecha } = useLocalizacion();

    const [texto, setTexto] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [enviando, setEnviando] = useState(false);

    const cerrado = ticket.ticket.estado === 'CERRADO';

    async function enviar(evento: FormEvent) {
        evento.preventDefault();

        if (!texto.trim()) {
            setError(t('soporte.hilo.mensajeVacio'));

            return;
        }

        setError(null);
        setEnviando(true);

        try {
            await alEnviar(texto.trim());
            setTexto('');
        } catch (excepcion) {
            setError(excepcion instanceof Error ? excepcion.message : t('soporte.hilo.errorEnviar'));
        } finally {
            setEnviando(false);
        }
    }

    return (
        <div className="flex flex-col gap-5">
            <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                    <p className="m-0 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                        #{ticket.ticket.ticketId}
                        {vista === 'operador' && ` · ${ticket.empresa}`}
                    </p>
                    <h3 className="mt-1 mb-0 text-lg font-semibold text-texto">{ticket.ticket.asunto}</h3>
                    <p className="mt-1 mb-0 text-sm text-texto-suave">
                        {t('soporte.hilo.contexto', {
                            plan: ticket.plan,
                            estado: t(`comun.contratacion.estado.${ticket.estadoSuscripcion}`),
                        })}
                        {ticket.curso && ` · ${t('soporte.hilo.curso', { curso: ticket.curso })}`}
                    </p>
                    <p className="mt-1 mb-0 text-xs text-texto-suave">
                        {t('soporte.hilo.abierta', {
                            autor: ticket.autor,
                            fecha: formatearFecha(ticket.ticket.fechaAlta, true),
                        })}
                        {ticket.ticket.fechaCierre &&
                            ` · ${t('soporte.hilo.cerrada', { fecha: formatearFecha(ticket.ticket.fechaCierre, true) })}`}
                    </p>
                </div>

                <div className="flex flex-wrap items-center gap-2">
                    <EstadoTicketEtiqueta estado={ticket.ticket.estado} />
                    {acciones}
                </div>
            </div>

            <ol className="m-0 flex list-none flex-col gap-3 p-0">
                {ticket.mensajes.map(({ mensaje, autor }) => {
                    const deOperador = mensaje.esOperador;
                    const propio = vista === 'operador' ? deOperador : !deOperador;

                    return (
                        <li key={mensaje.mensajeId} className={`flex ${propio ? 'justify-end' : 'justify-start'}`}>
                            <div
                                className={`max-w-[85%] rounded-2xl border px-4 py-3 ${
                                    deOperador ? 'border-info-fondo bg-info-fondo' : 'border-borde bg-superficie'
                                }`}
                            >
                                <p className="m-0 text-xs font-semibold text-texto-suave">
                                    {deOperador ? t('soporte.hilo.soporte', { autor }) : autor}
                                    {' · '}
                                    {formatearFecha(mensaje.fechaEnvio, true)}
                                </p>
                                <p className="mt-1 mb-0 text-sm whitespace-pre-line text-texto">{mensaje.texto}</p>
                            </div>
                        </li>
                    );
                })}
            </ol>

            {cerrado ? (
                <p className="m-0 text-sm text-texto-suave">{t('soporte.hilo.estaCerrada')}</p>
            ) : (
                <form onSubmit={enviar} noValidate className="flex flex-col gap-3">
                    <Alerta tipo="error" mensaje={error} />
                    <div className="campo">
                        <label htmlFor={`respuesta-${ticket.ticket.ticketId}`}>
                            {vista === 'operador' ? t('soporte.hilo.responder') : t('soporte.hilo.escribir')}
                        </label>
                        <textarea
                            id={`respuesta-${ticket.ticket.ticketId}`}
                            rows={4}
                            maxLength={2000}
                            value={texto}
                            onChange={(evento) => setTexto(evento.target.value)}
                            className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                        />
                    </div>
                    <div>
                        <Boton type="submit" cargando={enviando}>
                            {t('soporte.hilo.enviar')}
                        </Boton>
                    </div>
                </form>
            )}
        </div>
    );
}
