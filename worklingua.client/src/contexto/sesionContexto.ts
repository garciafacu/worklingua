import { createContext } from 'react';
import type { UsuarioSesion } from '../tipos/autenticacion';

export interface EstadoSesion {
    usuario: UsuarioSesion | null;
    autenticado: boolean;
    /**
     * Comodidad de navegación para mostrar u ocultar opciones. Quien autoriza
     * de verdad es el backend, que resuelve los permisos contra la base en cada
     * pedido: acá solo se leen los que vinieron en el login.
     */
    tienePermiso: (codigo: string) => boolean;
    iniciarSesion: (token: string, usuario: UsuarioSesion) => void;
    /**
     * Relee roles y permisos desde el backend para que el menú refleje los
     * cambios hechos después del login. Si la sesión ya no es válida, la cierra.
     */
    refrescarSesion: () => Promise<void>;
    cerrarSesion: () => Promise<void>;
}

// El contexto y el hook viven en archivos aparte del provider para no exportar
// nada que no sea un componente desde un .tsx (regla de react-refresh).
export const SesionContexto = createContext<EstadoSesion | undefined>(undefined);
