import { clienteHttp } from './clienteHttp';
import type {
    CursoConProgresoResponse,
    GuardarModuloRequest,
    GuardarProgresoRequest,
    ModuloResponse,
    ProgresoUsuarioResponse,
} from '../tipos/progreso';

/** Seguimiento de cursos (tracking) y módulos. */
export const progresoApi = {
    /** Mis cursos con su avance. Requiere Curso.VerProgreso. */
    listarMio: () => clienteHttp.get<CursoConProgresoResponse[]>('/progreso/mio'),

    /** El avance de cada usuario de la empresa. Requiere Curso.VerProgresoEmpresa. */
    listarEmpresa: () => clienteHttp.get<ProgresoUsuarioResponse[]>('/progreso/empresa'),

    /** Inicia o completa un módulo y devuelve los cursos actualizados. */
    guardar: (cuerpo: GuardarProgresoRequest) =>
        clienteHttp.post<CursoConProgresoResponse[]>('/progreso', cuerpo),

    /** Backoffice: módulos activos de un curso. Requiere Curso.Listar. */
    listarModulos: (cursoId: number) =>
        clienteHttp.get<ModuloResponse[]>(`/cursos/${cursoId}/modulos`),

    /** Requiere Curso.Modificar. */
    crearModulo: (cursoId: number, cuerpo: GuardarModuloRequest) =>
        clienteHttp.post<ModuloResponse>(`/cursos/${cursoId}/modulos`, cuerpo),

    /** Baja lógica. Requiere Curso.Modificar. */
    bajaModulo: (cursoId: number, moduloId: number) =>
        clienteHttp.borrar<void>(`/cursos/${cursoId}/modulos/${moduloId}`),
};
