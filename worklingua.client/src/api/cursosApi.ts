import { clienteHttp } from './clienteHttp';
import type { CursoAdminResponse, GuardarCursoRequest } from '../tipos/cursos';

export const cursosApi = {
    listarIdiomas: () => clienteHttp.get<string[]>('/cursos/idiomas'),

    listarAdministracion: () => clienteHttp.get<CursoAdminResponse[]>('/cursos/administracion'),

    crear: (cuerpo: GuardarCursoRequest) => clienteHttp.post<CursoAdminResponse>('/cursos', cuerpo),

    modificar: (cursoId: number, cuerpo: GuardarCursoRequest) =>
        clienteHttp.put<CursoAdminResponse>(`/cursos/${cursoId}`, cuerpo),

    baja: (cursoId: number) => clienteHttp.borrar<void>(`/cursos/${cursoId}`),
};
