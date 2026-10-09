import { clienteHttp } from './clienteHttp';
import type { ActivoResponse } from '../tipos/activos';

export const activosApi = {
    /** Activos del curso, incluidos los dados de baja. Requiere Curso.Listar. */
    listar: (cursoId: number) => clienteHttp.get<ActivoResponse[]>(`/activos?cursoId=${cursoId}`),

    /**
     * Sube el archivo y registra el activo como borrador. Requiere
     * Curso.Modificar.
     *
     * Con `confirmarNombre` en true se acepta la nomenclatura que sugirió el
     * backend al detectar un nombre duplicado.
     */
    crear: (formulario: FormData) => clienteHttp.subir<ActivoResponse>('/activos', formulario),

    /** Publicar o volver a borrador. */
    cambiarPublicacion: (activoId: number, estado: string) =>
        clienteHttp.patch<ActivoResponse>(`/activos/${activoId}/publicacion`, { estado }),

    /** Baja lógica y reactivación. */
    cambiarEstado: (activoId: number, activo: boolean) =>
        clienteHttp.patch<ActivoResponse>(`/activos/${activoId}/estado`, { activo }),
};
