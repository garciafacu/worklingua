import { clienteHttp } from './clienteHttp';
import type { ActivoResponse } from '../tipos/activos';

export const activosApi = {
    listar: (cursoId: number) => clienteHttp.get<ActivoResponse[]>(`/activos?cursoId=${cursoId}`),

    crear: (formulario: FormData) => clienteHttp.subir<ActivoResponse>('/activos', formulario),

    cambiarPublicacion: (activoId: number, estado: string) =>
        clienteHttp.patch<ActivoResponse>(`/activos/${activoId}/publicacion`, { estado }),

    cambiarEstado: (activoId: number, activo: boolean) =>
        clienteHttp.patch<ActivoResponse>(`/activos/${activoId}/estado`, { activo }),
};
