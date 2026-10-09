import { clienteHttp } from './clienteHttp';
import type {
    EncuestaResponse,
    GuardarEncuestaRequest,
    ResponderEncuestaRequest,
} from '../tipos/encuestas';

export const encuestasApi = {
    /** Encuestas vigentes en un idioma. Requiere Encuesta.Responder. */
    listarVigentes: (idioma: string) =>
        clienteHttp.get<EncuestaResponse[]>(`/encuestas/vigentes?idioma=${encodeURIComponent(idioma)}`),

    /** Registra el voto y devuelve la encuesta con los resultados actualizados. */
    responder: (encuestaId: number, cuerpo: ResponderEncuestaRequest) =>
        clienteHttp.post<EncuestaResponse>(`/encuestas/${encuestaId}/respuestas`, cuerpo),

    /** Backoffice: todas, incluidas las vencidas y las dadas de baja. */
    listarAdministracion: () => clienteHttp.get<EncuestaResponse[]>('/encuestas/administracion'),

    crear: (cuerpo: GuardarEncuestaRequest) =>
        clienteHttp.post<EncuestaResponse>('/encuestas/administracion', cuerpo),

    modificar: (encuestaId: number, cuerpo: GuardarEncuestaRequest) =>
        clienteHttp.put<EncuestaResponse>(`/encuestas/administracion/${encuestaId}`, cuerpo),

    baja: (encuestaId: number) => clienteHttp.borrar<void>(`/encuestas/administracion/${encuestaId}`),
};
