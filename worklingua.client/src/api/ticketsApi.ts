import { clienteHttp } from './clienteHttp';
import type {
    CrearTicketRequest,
    EstadoTicket,
    OpcionesTicketResponse,
    TicketResponse,
} from '../tipos/tickets';

function conEstado(ruta: string, estado?: EstadoTicket | ''): string {
    return estado ? `${ruta}?estado=${estado}` : ruta;
}

/**
 * HelpDesk. Del lado del cliente exige Ticket.Crear (el alcance lo resuelve el
 * backend: los propios, o los de la empresa con Ticket.VerEmpresa). Las rutas de
 * `administracion` exigen Ticket.Atender.
 */
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
