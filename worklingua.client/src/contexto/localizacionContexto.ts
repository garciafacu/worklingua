import { createContext } from 'react';
import type { CulturaResponse } from '../tipos/culturas';
import type { IdiomaResponse } from '../tipos/idiomas';

export interface EstadoLocalizacion {
    idiomas: IdiomaResponse[];

    idiomaActual: string;

    cambiarIdioma: (codigoISO: string) => Promise<void>;

    cultura: CulturaResponse | null;

    formatearMoneda: (importe: number) => string;

    formatearFecha: (valor: string | Date | null, conHora?: boolean) => string;

    formatearNumero: (valor: number, decimales?: number) => string;
}

export const LocalizacionContexto = createContext<EstadoLocalizacion | undefined>(undefined);
