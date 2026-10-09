import { clienteHttp } from './clienteHttp';
import type { CursoAdminResponse, GuardarCursoRequest } from '../tipos/cursos';

/**
 * Cursos del Backoffice. No hay consulta pública: cada empresa ve los cursos
 * globales y los propios, y todos los endpoints exigen sesión con el permiso
 * `Curso.*` que corresponda. Un curso de otra empresa responde 404.
 */
export const cursosApi = {
    /**
     * Los idiomas que dictan los cursos activos. Requiere Curso.Listar.
     *
     * Alimenta el autocomplete del ABM. Reemplaza a `idiomasApi.listar()`, que
     * desde la Etapa 8 devuelve los idiomas de la plataforma y no los de los cursos.
     */
    listarIdiomas: () => clienteHttp.get<string[]>('/cursos/idiomas'),

    /** Incluye los cursos dados de baja. Requiere el permiso Curso.Listar. */
    listarAdministracion: () => clienteHttp.get<CursoAdminResponse[]>('/cursos/administracion'),

    crear: (cuerpo: GuardarCursoRequest) => clienteHttp.post<CursoAdminResponse>('/cursos', cuerpo),

    modificar: (cursoId: number, cuerpo: GuardarCursoRequest) =>
        clienteHttp.put<CursoAdminResponse>(`/cursos/${cursoId}`, cuerpo),

    baja: (cursoId: number) => clienteHttp.borrar<void>(`/cursos/${cursoId}`),
};
