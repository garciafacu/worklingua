import { clienteHttp } from './clienteHttp';
import type {
    CursoConProgresoResponse,
    GuardarModuloRequest,
    GuardarProgresoRequest,
    ModuloResponse,
    ProgresoUsuarioResponse,
} from '../tipos/progreso';

export const progresoApi = {
    listarMio: () => clienteHttp.get<CursoConProgresoResponse[]>('/progreso/mio'),

    listarEmpresa: () => clienteHttp.get<ProgresoUsuarioResponse[]>('/progreso/empresa'),

    guardar: (cuerpo: GuardarProgresoRequest) =>
        clienteHttp.post<CursoConProgresoResponse[]>('/progreso', cuerpo),

    listarModulos: (cursoId: number) =>
        clienteHttp.get<ModuloResponse[]>(`/cursos/${cursoId}/modulos`),

    crearModulo: (cursoId: number, cuerpo: GuardarModuloRequest) =>
        clienteHttp.post<ModuloResponse>(`/cursos/${cursoId}/modulos`, cuerpo),

    bajaModulo: (cursoId: number, moduloId: number) =>
        clienteHttp.borrar<void>(`/cursos/${cursoId}/modulos/${moduloId}`),
};
