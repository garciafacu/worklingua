import type {
    ActivoResponse,
    CursoAdminResponse,
    CursoConProgresoResponse,
    DiferenciaVersionResponse,
    EtiquetaResponse,
    GuardarCursoRequest,
    GuardarModuloRequest,
    GuardarProgresoRequest,
    ModuloResponse,
    ProgresoUsuarioResponse,
    ResultadoRestauracionResponse,
    VersionResponse,
} from '../tipos/cursos';
import { clienteHttp } from './clienteHttp';

export const cursosApi = {
    listarIdiomas: () => clienteHttp.get<string[]>('/cursos/idiomas'),

    listarAdministracion: () => clienteHttp.get<CursoAdminResponse[]>('/cursos/administracion'),

    crear: (cuerpo: GuardarCursoRequest) => clienteHttp.post<CursoAdminResponse>('/cursos', cuerpo),

    modificar: (cursoId: number, cuerpo: GuardarCursoRequest) =>
        clienteHttp.put<CursoAdminResponse>(`/cursos/${cursoId}`, cuerpo),

    baja: (cursoId: number) => clienteHttp.borrar<void>(`/cursos/${cursoId}`),
};

export const etiquetasApi = {
    listar: () => clienteHttp.get<EtiquetaResponse[]>('/etiquetas'),

    crear: (nombre: string) => clienteHttp.post<EtiquetaResponse>('/etiquetas', { nombre }),
};

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

export const activosApi = {
    listar: (cursoId: number) => clienteHttp.get<ActivoResponse[]>(`/activos?cursoId=${cursoId}`),

    crear: (formulario: FormData) => clienteHttp.subir<ActivoResponse>('/activos', formulario),

    cambiarPublicacion: (activoId: number, estado: string) =>
        clienteHttp.patch<ActivoResponse>(`/activos/${activoId}/publicacion`, { estado }),

    cambiarEstado: (activoId: number, activo: boolean) =>
        clienteHttp.patch<ActivoResponse>(`/activos/${activoId}/estado`, { activo }),
};

export const versionesApi = {
    listar: (cursoId: number) => clienteHttp.get<VersionResponse[]>(`/cursos/${cursoId}/versiones`),

    obtener: (versionId: number) => clienteHttp.get<VersionResponse>(`/cursos/versiones/${versionId}`),

    crear: (cursoId: number, observaciones: string | null) =>
        clienteHttp.post<VersionResponse>(`/cursos/${cursoId}/versiones`, { observaciones }),

    restaurar: (versionId: number) =>
        clienteHttp.post<ResultadoRestauracionResponse>(`/cursos/versiones/${versionId}/restaurar`, {}),

    eliminar: (versionId: number) => clienteHttp.borrar<void>(`/cursos/versiones/${versionId}`),

    comparar: (izquierda: number, derecha: number) =>
        clienteHttp.get<DiferenciaVersionResponse[]>(
            `/cursos/versiones/comparar?izquierda=${izquierda}&derecha=${derecha}`,
        ),

    clonar: (cursoId: number, nombre: string) =>
        clienteHttp.post<ResultadoRestauracionResponse>(`/cursos/${cursoId}/clonar`, { nombre }),
};
