import type {
    CrearTicketRequest,
    EstadoTicket,
    GuardarOfertaRequest,
    OfertaResponse,
    OpcionesTicketResponse,
    TicketResponse,
} from '../tipos/tickets';
import { clienteHttp } from './clienteHttp';

function conEstado(ruta: string, estado?: EstadoTicket | ''): string {
    return estado ? `${ruta}?estado=${estado}` : ruta;
}

export const ticketsApi = {
    opciones: () => clienteHttp.get<OpcionesTicketResponse>('/tickets/opciones'),

    listar: (estado?: EstadoTicket | '') => clienteHttp.get<TicketResponse[]>(conEstado('/tickets', estado)),

    obtener: (ticketId: number) => clienteHttp.get<TicketResponse>(`/tickets/${ticketId}`),

    crear: (cuerpo: CrearTicketRequest) => clienteHttp.post<TicketResponse>('/tickets', cuerpo),

    enviarMensaje: (ticketId: number, texto: string) =>
        clienteHttp.post<TicketResponse>(`/tickets/${ticketId}/mensajes`, { texto }),

    cerrar: (ticketId: number) => clienteHttp.put<TicketResponse>(`/tickets/${ticketId}/cerrar`),

    listarAdministracion: (estado?: EstadoTicket | '') =>
        clienteHttp.get<TicketResponse[]>(conEstado('/tickets/administracion', estado)),

    obtenerAdministracion: (ticketId: number) =>
        clienteHttp.get<TicketResponse>(`/tickets/administracion/${ticketId}`),

    responder: (ticketId: number, texto: string) =>
        clienteHttp.post<TicketResponse>(`/tickets/administracion/${ticketId}/respuesta`, { texto }),

    cambiarEstado: (ticketId: number, estado: EstadoTicket) =>
        clienteHttp.put<TicketResponse>(`/tickets/administracion/${ticketId}/estado`, { estado }),
};

export const ofertasApi = {
    listarMias: () => clienteHttp.get<OfertaResponse[]>('/ofertas/mias'),

    listarAdministracion: () => clienteHttp.get<OfertaResponse[]>('/ofertas/administracion'),

    crear: (cuerpo: GuardarOfertaRequest) =>
        clienteHttp.post<OfertaResponse>('/ofertas/administracion', cuerpo),

    modificar: (ofertaId: number, cuerpo: GuardarOfertaRequest) =>
        clienteHttp.put<OfertaResponse>(`/ofertas/administracion/${ofertaId}`, cuerpo),

    baja: (ofertaId: number) => clienteHttp.borrar<void>(`/ofertas/administracion/${ofertaId}`),
};
