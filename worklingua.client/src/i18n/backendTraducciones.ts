import type { BackendModule, ReadCallback } from 'i18next';
import { traduccionesApi } from '../api/traduccionesApi';

/**
 * Backend de i18next que lee las traducciones de la API.
 *
 * **No hay archivos JSON de traducciones en el repositorio.** La fuente de
 * verdad es SQL Server, y el camino completo es
 * `UI → API → Controller → BLL → MPP → DAL → Stored Procedure → SQL Server`.
 *
 * `GET /api/traducciones/{codigoISO}` devuelve un diccionario plano con TODAS
 * las claves del catálogo: el fallback al español lo resuelve el Stored
 * Procedure, así que el bundle nunca tiene huecos y el navegador no necesita
 * pedir dos idiomas para pintar una pantalla.
 */
export const backendTraducciones: BackendModule = {
    type: 'backend',

    // i18next exige el método, pero no hay nada que configurar: la ruta de la API
    // es fija y el token de sesión no hace falta porque el endpoint es público.
    init() {},

    read(idioma: string, _espacio: string, devolver: ReadCallback) {
        traduccionesApi
            .obtenerBundle(idioma)
            .then((bundle) => devolver(null, bundle))
            .catch((excepcion: unknown) => {
                // El segundo argumento en `true` le dice a i18next que puede
                // reintentar. Si la API está caída, la aplicación renderiza las
                // claves crudas en vez de quedarse en blanco.
                devolver(excepcion as Error, true);
            });
    },
};
