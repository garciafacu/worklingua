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
        return null;
    }
}

function guardarSesion(sesion: SesionPersistida): void {
    try {
        sessionStorage.setItem(CLAVE_ALMACENAMIENTO, JSON.stringify(sesion));
    } catch {
    }
}

function borrarSesionGuardada(): void {
    try {
        sessionStorage.removeItem(CLAVE_ALMACENAMIENTO);
    } catch {
    }
}

export function SesionProvider({ children }: { children: ReactNode }) {
    const [usuario, setUsuario] = useState<UsuarioSesion | null>(() => leerSesionGuardada()?.usuario ?? null);

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
        } finally {
            cerrarSesionLocal();
        }
    }, [cerrarSesionLocal]);

    const refrescarSesion = useCallback(async () => {
        const guardada = leerSesionGuardada();

        if (!guardada) {
            return;
        }

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
