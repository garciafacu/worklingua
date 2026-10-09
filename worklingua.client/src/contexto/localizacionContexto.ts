import { createContext } from 'react';
import type { CulturaResponse } from '../tipos/culturas';
import type { IdiomaResponse } from '../tipos/idiomas';

export interface EstadoLocalizacion {
    /** Los idiomas de plataforma activos, tal como los devuelve la API. */
    idiomas: IdiomaResponse[];

    /** Código ISO del idioma en uso. */
    idiomaActual: string;

    cambiarIdioma: (codigoISO: string) => Promise<void>;

    /**
     * La cultura que corresponde al idioma en uso, o `null` si todavía no se
     * cargó ninguna. Los formateadores de abajo ya la contemplan: no hace falta
     * chequearla en las pantallas.
     */
    cultura: CulturaResponse | null;

    /**
     * Formatea un importe guardado en la moneda base (ARS) aplicando la tasa de
     * conversión de la cultura.
     */
    formatearMoneda: (importe: number) => string;

    /** Formatea una fecha ISO. Con `conHora` agrega la hora. */
    formatearFecha: (valor: string | Date | null, conHora?: boolean) => string;

    /** Con `decimales` fija la cantidad de decimales (por ejemplo, un promedio "4,5"). */
    formatearNumero: (valor: number, decimales?: number) => string;
}

// El contexto y el hook viven en archivos aparte del provider para no exportar
// nada que no sea un componente desde un .tsx (regla de react-refresh), igual
// que en sesionContexto / SesionProvider / useSesion.
export const LocalizacionContexto = createContext<EstadoLocalizacion | undefined>(undefined);
