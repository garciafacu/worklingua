import type { EstadoContratacion } from './contrataciones';

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
    mensajes: MensajeTicketResponse[];
}

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
    codigoIdioma: string;
}

export interface OfertaResponse {
    oferta: {
        ofertaId: number;
        empresaId: number;
        planId: number | null;
        titulo: string;
        descripcion: string;
        fechaDesde: string;
        fechaHasta: string | null;
        activo: boolean;
        usuarioId: number;
        fechaAlta: string;
    };
    empresa: string;
    plan: string | null;
    planActivo: boolean;
}

export interface GuardarOfertaRequest {
    empresaId: number;
    planId: number | null;
    titulo: string;
    descripcion: string;
    fechaDesde: string;
    fechaHasta: string | null;
    activo: boolean;
}
