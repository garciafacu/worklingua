import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi, establecerTokenSesion } from '../api/clienteHttp';
import type { UsuarioSesion } from '../tipos/autenticacion';
import { SesionContexto, type EstadoSesion } from './sesionContexto';

const CLAVE_ALMACENAMIENTO = 'worklingua.sesion';

interface SesionPersistida {
    token: string;
    usuario: UsuarioSesion;
}

function leerSesionGuardada(): SesionPersistida | null {
    try {
        const crudo = sessionStorage.getItem(CLAVE_ALMACENAMIENTO);
        return crudo ? (JSON.parse(crudo) as SesionPersistida) : null;
    } catch {
        // sessionStorage puede fallar (ventana privada, permisos): se sigue sin sesión.
        return null;
    }
}

function guardarSesion(sesion: SesionPersistida): void {
    try {
        sessionStorage.setItem(CLAVE_ALMACENAMIENTO, JSON.stringify(sesion));
    } catch {
        // Sin almacenamiento la sesión igual funciona: se pierde solo al recargar.
    }
}

function borrarSesionGuardada(): void {
    try {
        sessionStorage.removeItem(CLAVE_ALMACENAMIENTO);
    } catch {
        // Nada que limpiar si el almacenamiento no está disponible.
    }
}

/**
 * Mantiene la sesión activa y la comparte con toda la aplicación.
 *
 * Se usa sessionStorage y no localStorage para que la sesión no sobreviva al
 * cierre del navegador. El token es opaco: la validez la decide el backend
 * contra la tabla Sesion, acá solo se transporta.
 */
export function SesionProvider({ children }: { children: ReactNode }) {
    const [usuario, setUsuario] = useState<UsuarioSesion | null>(() => leerSesionGuardada()?.usuario ?? null);

    // Repone el token en el cliente HTTP al montar, para que un F5 no pierda la sesión.
    useEffect(() => {
        establecerTokenSesion(leerSesionGuardada()?.token ?? null);
    }, []);

    const iniciarSesion = useCallback((token: string, usuarioAutenticado: UsuarioSesion) => {
        establecerTokenSesion(token);
        setUsuario(usuarioAutenticado);
        guardarSesion({ token, usuario: usuarioAutenticado });
    }, []);

    const cerrarSesionLocal = useCallback(() => {
        establecerTokenSesion(null);
        setUsuario(null);
        borrarSesionGuardada();
    }, []);

    const cerrarSesion = useCallback(async () => {
        try {
            await autenticacionApi.logout();
        } catch {
            // Si el backend ya invalidó la sesión, el logout local se hace igual.
        } finally {
            cerrarSesionLocal();
        }
    }, [cerrarSesionLocal]);

    const refrescarSesion = useCallback(async () => {
        const guardada = leerSesionGuardada();

        if (!guardada) {
            return;
        }

        // Los efectos de los hijos corren antes que el del provider: el token se
        // repone acá para no pedir la sesión sin él justo después de un F5.
        establecerTokenSesion(guardada.token);

        try {
            const respuesta = await autenticacionApi.sesion();
            const vigente: UsuarioSesion = {
                usuarioId: respuesta.usuarioId,
                nombre: respuesta.nombre,
                apellido: respuesta.apellido,
                email: respuesta.email,
                roles: respuesta.roles,
                permisos: respuesta.permisos,
            };

            // Si nada cambió se conserva el mismo objeto y no se redibuja la app.
            setUsuario((anterior) =>
                JSON.stringify(anterior) === JSON.stringify(vigente) ? anterior : vigente,
            );
            guardarSesion({ token: guardada.token, usuario: vigente });
        } catch (excepcion) {
            if (excepcion instanceof ErrorApi && excepcion.estado === 401) {
                cerrarSesionLocal();
            }
        }
    }, [cerrarSesionLocal]);

    // Una sesión guardada por una versión anterior no trae `permisos`: se trata
    // como sin permisos en vez de romper al leer la propiedad.
    const tienePermiso = useCallback(
        (codigo: string) => (usuario?.permisos ?? []).includes(codigo),
        [usuario],
    );

    const valor = useMemo<EstadoSesion>(
        () => ({
            usuario,
            autenticado: usuario !== null,
            tienePermiso,
            iniciarSesion,
            refrescarSesion,
            cerrarSesion,
        }),
        [usuario, tienePermiso, iniciarSesion, refrescarSesion, cerrarSesion],
    );

    return <SesionContexto.Provider value={valor}>{children}</SesionContexto.Provider>;
}
