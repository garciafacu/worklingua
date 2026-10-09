import type { EstadoContratacion } from './contrataciones';

/** Estados de una consulta de soporte, espejo de `BLL/BLLTicket.cs`. */
export const ESTADOS_TICKET = ['PENDIENTE', 'EN_PROCESO', 'RESPONDIDO', 'CERRADO'] as const;

export type EstadoTicket = (typeof ESTADOS_TICKET)[number];

export interface MensajeTicketResponse {
    mensaje: {
        mensajeId: number;
        ticketId: number;
        usuarioId: number;
        esOperador: boolean;
        texto: string;
        fechaEnvio: string;
    };
    autor: string;
}

/** Una consulta con su historial, espejo de `BE/BETicketConDetalle.cs`. */
export interface TicketResponse {
    ticket: {
        ticketId: number;
        empresaId: number;
        usuarioId: number;
        suscripcionId: number;
        cursoId: number | null;
        asunto: string;
        estado: EstadoTicket;
        fechaAlta: string;
        fechaUltimoMovimiento: string;
        fechaCierre: string | null;
    };
    empresa: string;
    autor: string;
    autorEmail: string;
    plan: string;
    estadoSuscripcion: EstadoContratacion;
    curso: string | null;
    cantidadMensajes: number;
    /** Vacío en los listados; completo en el detalle. */
    mensajes: MensajeTicketResponse[];
}

/** Lo que el cliente puede elegir al abrir una consulta, espejo de `BE/BEOpcionesTicket.cs`. */
export interface OpcionesTicketResponse {
    contrataciones: {
        suscripcion: {
            suscripcionId: number;
            planId: number;
            fechaInicio: string;
            fechaFin: string | null;
            estado: EstadoContratacion;
        };
        plan: string;
    }[];
    cursos: { cursoId: number; nombre: string }[];
}

export interface CrearTicketRequest {
    suscripcionId: number;
    cursoId: number | null;
    asunto: string;
    texto: string;
    /** Idioma de la interfaz: define en qué idioma llegan los avisos por correo. */
    codigoIdioma: string;
}
