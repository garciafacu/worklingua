import type { BackendModule, ReadCallback } from 'i18next';
import { traduccionesApi } from '../api/traduccionesApi';

export const backendTraducciones: BackendModule = {
    type: 'backend',

    init() {},

    read(idioma: string, _espacio: string, devolver: ReadCallback) {
        traduccionesApi
            .obtenerBundle(idioma)
            .then((bundle) => devolver(null, bundle))
            .catch((excepcion: unknown) => {
                devolver(excepcion as Error, true);
            });
    },
};
