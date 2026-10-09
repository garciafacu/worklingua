import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useSearchParams } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { ticketsApi } from '../../api/ticketsApi';
import { Alerta } from '../../componentes/Alerta';
import { Boton } from '../../componentes/Boton';
import { CampoSelect } from '../../componentes/CampoSelect';
import { CampoTexto } from '../../componentes/CampoTexto';
import { EstadoTicketEtiqueta, HiloTicket } from '../../componentes/HiloTicket';
import { Modal } from '../../componentes/Modal';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import { ESTADOS_TICKET, type EstadoTicket, type OpcionesTicketResponse, type TicketResponse } from '../../tipos/tickets';

const FORMULARIO_VACIO = { suscripcionId: '', cursoId: '', asunto: '', texto: '' };

/**
 * HelpDesk del cliente (puntos 6.c y 16): consultas asincrónicas sobre lo
 * contratado, con su historial de mensajes.
 *
 * Cada consulta se asocia a una contratación de la empresa y, opcionalmente, a un
 * curso. El usuario ve las suyas; con `Ticket.VerEmpresa`, las de toda la
 * empresa (eso lo resuelve el backend). `?ticket=<id>` abre una directamente: es
 * el enlace de los avisos por correo.
 */
export function Soporte() {
    const { t } = useTranslation();
    const { formatearFecha, idiomaActual } = useLocalizacion();
    const [parametros, setParametros] = useSearchParams();
    const ticketElegido = Number(parametros.get('ticket')) || null;

    const [tickets, setTickets] = useState<TicketResponse[]>([]);
    const [detalle, setDetalle] = useState<TicketResponse | null>(null);
    const [filtro, setFiltro] = useState<EstadoTicket | ''>('');
    const [opciones, setOpciones] = useState<OpcionesTicketResponse | null>(null);
    const [creando, setCreando] = useState(false);
    const [formulario, setFormulario] = useState(FORMULARIO_VACIO);
    const [error, setError] = useState<string | null>(null);
    const [errorFormulario, setErrorFormulario] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [recarga, setRecarga] = useState(0);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        ticketsApi
            .listar(filtro)
            .then(setTickets)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'soporte.error')))
            .finally(() => setCargando(false));
    }, [filtro, recarga, mensajeDeError]);

    useEffect(() => {
        if (ticketElegido === null) {
            return;
        }

        ticketsApi
            .obtener(ticketElegido)
            .then(setDetalle)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'soporte.error')));
    }, [ticketElegido, mensajeDeError]);

    function elegir(ticketId: number) {
        setExito(null);
        setParametros({ ticket: String(ticketId) }, { replace: true });
    }

    function actualizar(ticket: TicketResponse) {
        setDetalle(ticket);
        setRecarga((numero) => numero + 1);
    }

    function abrirNueva() {
        setErrorFormulario(null);
        setFormulario(FORMULARIO_VACIO);
        setCreando(true);

        if (opciones === null) {
            ticketsApi
                .opciones()
                .then((resultado) => {
                    setOpciones(resultado);
                    const activa = resultado.contrataciones.find((item) => item.suscripcion.estado === 'ACTIVA');

                    if (activa) {
                        setFormulario((actual) => ({ ...actual, suscripcionId: String(activa.suscripcion.suscripcionId) }));
                    }
                })
                .catch((excepcion) => setErrorFormulario(mensajeDeError(excepcion, 'soporte.error')));
        }
    }

    async function crear(evento: FormEvent) {
        evento.preventDefault();

        if (!formulario.suscripcionId || !formulario.asunto.trim() || !formulario.texto.trim()) {
            setErrorFormulario(t('soporte.nueva.incompleta'));

            return;
        }

        setErrorFormulario(null);
        setGuardando(true);

        try {
            const creado = await ticketsApi.crear({
                suscripcionId: Number(formulario.suscripcionId),
                cursoId: formulario.cursoId ? Number(formulario.cursoId) : null,
                asunto: formulario.asunto.trim(),
                texto: formulario.texto.trim(),
                codigoIdioma: idiomaActual,
            });

            setCreando(false);
            setExito(t('soporte.nueva.exito', { numero: creado.ticket.ticketId }));
            setDetalle(creado);
            setParametros({ ticket: String(creado.ticket.ticketId) }, { replace: true });
            setRecarga((numero) => numero + 1);
        } catch (excepcion) {
            setErrorFormulario(mensajeDeError(excepcion, 'soporte.nueva.error'));
        } finally {
            setGuardando(false);
        }
    }

    async function cerrar(ticketId: number) {
        setError(null);

        try {
            actualizar(await ticketsApi.cerrar(ticketId));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'soporte.error'));
        }
    }

    const detalleVisible = detalle !== null && detalle.ticket.ticketId === ticketElegido ? detalle : null;

    return (
        <div className="mx-auto max-w-6xl">
            <div className="flex flex-wrap items-start justify-between gap-4">
                <div>
                    <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">{t('soporte.titulo')}</h1>
                    <p className="mt-2 text-sm text-texto-suave">{t('soporte.descripcion')}</p>
                </div>
                <Boton onClick={abrirNueva}>{t('soporte.nueva.boton')}</Boton>
            </div>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            <div className="mt-6 grid items-start gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(0,3fr)]">
                <section className="flex flex-col gap-3">
                    <div className="sm:w-64">
                        <CampoSelect
                            etiqueta={t('soporte.filtro')}
                            identificador="filtroEstadoTicket"
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

                    {cargando && <p className="text-sm text-texto-suave">{t('soporte.cargando')}</p>}

                    {!cargando && tickets.length === 0 && (
                        <p className="rounded-2xl border border-dashed border-borde bg-superficie px-6 py-8 text-center text-sm text-texto-suave">
                            {t('soporte.vacio')}
                        </p>
                    )}

                    <ul className="m-0 flex list-none flex-col gap-2 p-0">
                        {tickets.map((item) => (
                            <li key={item.ticket.ticketId}>
                                <button
                                    type="button"
                                    onClick={() => elegir(item.ticket.ticketId)}
                                    aria-current={item.ticket.ticketId === ticketElegido}
                                    className={`w-full rounded-2xl border bg-superficie p-4 text-left hover:bg-fondo ${
                                        item.ticket.ticketId === ticketElegido ? 'border-primario' : 'border-borde'
                                    }`}
                                >
                                    <span className="flex items-start justify-between gap-2">
                                        <span className="min-w-0 text-sm font-semibold text-texto">
                                            #{item.ticket.ticketId} · {item.ticket.asunto}
                                        </span>
                                        <EstadoTicketEtiqueta estado={item.ticket.estado} />
                                    </span>
                                    <span className="mt-1 block text-xs text-texto-suave">
                                        {item.plan}
                                        {item.curso && ` · ${item.curso}`} · {item.autor}
                                    </span>
                                    <span className="mt-1 block text-xs text-texto-suave">
                                        {t('soporte.ultimoMovimiento', {
                                            fecha: formatearFecha(item.ticket.fechaUltimoMovimiento, true),
                                        })}
                                    </span>
                                </button>
                            </li>
                        ))}
                    </ul>
                </section>

                <section className="rounded-2xl border border-borde bg-superficie p-6">
                    {detalleVisible ? (
                        <HiloTicket
                            key={detalleVisible.ticket.ticketId}
                            ticket={detalleVisible}
                            vista="cliente"
                            alEnviar={async (texto) =>
                                actualizar(await ticketsApi.enviarMensaje(detalleVisible.ticket.ticketId, texto))
                            }
                            acciones={
                                detalleVisible.ticket.estado !== 'CERRADO' && (
                                    <button
                                        type="button"
                                        onClick={() => void cerrar(detalleVisible.ticket.ticketId)}
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {t('soporte.cerrar')}
                                    </button>
                                )
                            }
                        />
                    ) : (
                        <p className="m-0 text-sm text-texto-suave">{t('soporte.elegir')}</p>
                    )}
                </section>
            </div>

            <Modal
                titulo={t('soporte.nueva.titulo')}
                descripcion={t('soporte.nueva.descripcion')}
                abierto={creando}
                alCerrar={() => setCreando(false)}
            >
                <form onSubmit={crear} noValidate className="flex flex-col gap-4">
                    <Alerta tipo="error" mensaje={errorFormulario} />

                    <CampoSelect
                        etiqueta={t('soporte.nueva.contratacion')}
                        identificador="ticket-suscripcion"
                        required
                        value={formulario.suscripcionId}
                        onChange={(evento) => setFormulario({ ...formulario, suscripcionId: evento.target.value })}
                    >
                        <option value="" disabled>
                            —
                        </option>
                        {(opciones?.contrataciones ?? []).map((item) => (
                            <option key={item.suscripcion.suscripcionId} value={item.suscripcion.suscripcionId}>
                                {t('soporte.nueva.opcionContratacion', {
                                    plan: item.plan,
                                    fecha: formatearFecha(item.suscripcion.fechaInicio),
                                    estado: t(`comun.contratacion.estado.${item.suscripcion.estado}`),
                                })}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoSelect
                        etiqueta={t('soporte.nueva.curso')}
                        identificador="ticket-curso"
                        value={formulario.cursoId}
                        onChange={(evento) => setFormulario({ ...formulario, cursoId: evento.target.value })}
                    >
                        <option value="">{t('soporte.nueva.sinCurso')}</option>
                        {(opciones?.cursos ?? []).map((curso) => (
                            <option key={curso.cursoId} value={curso.cursoId}>
                                {curso.nombre}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoTexto
                        etiqueta={t('soporte.nueva.asunto')}
                        identificador="ticket-asunto"
                        required
                        maxLength={150}
                        value={formulario.asunto}
                        onChange={(evento) => setFormulario({ ...formulario, asunto: evento.target.value })}
                    />

                    <div className="campo">
                        <label htmlFor="ticket-texto">{t('soporte.nueva.mensaje')}</label>
                        <textarea
                            id="ticket-texto"
                            rows={6}
                            required
                            maxLength={2000}
                            value={formulario.texto}
                            onChange={(evento) => setFormulario({ ...formulario, texto: evento.target.value })}
                            className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                        />
                    </div>

                    <div className="flex flex-wrap gap-3">
                        <Boton type="submit" cargando={guardando}>
                            {t('soporte.nueva.enviar')}
                        </Boton>
                        <button
                            type="button"
                            onClick={() => setCreando(false)}
                            className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                        >
                            {t('comun.boton.cancelar')}
                        </button>
                    </div>
                </form>
            </Modal>
        </div>
    );
}
