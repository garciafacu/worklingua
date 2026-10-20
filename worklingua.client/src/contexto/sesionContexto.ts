import { createContext } from 'react';
import type { UsuarioSesion } from '../tipos/autenticacion';

export interface EstadoSesion {
    usuario: UsuarioSesion | null;
    autenticado: boolean;
    tienePermiso: (codigo: string) => boolean;
    iniciarSesion: (token: string, usuario: UsuarioSesion) => void;
    refrescarSesion: () => Promise<void>;
    cerrarSesion: () => Promise<void>;
}

export const SesionContexto = createContext<EstadoSesion | undefined>(undefined);
