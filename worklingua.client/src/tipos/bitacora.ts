/** Un evento de la bitácora, espejo de `Contratos/BitacoraEventoResponse.cs`. */
export interface BitacoraEventoResponse {
    bitacoraId: number;
    /** Null en los eventos anónimos: consultas de contacto, logins con correo inexistente. */
    usuarioId: number | null;
    fechaEvento: string;
    modulo: string | null;
    accion: string | null;
    descripcion: string | null;
    nivel: string | null;
    /** Vacío si el evento no tiene usuario o si la cuenta ya no existe. */
    usuario: string;
    email: string | null;
}

/** Una página de resultados, espejo de `Contratos/PaginaBitacoraResponse.cs`. */
export interface PaginaBitacoraResponse {
    registros: BitacoraEventoResponse[];
    totalRegistros: number;
    pagina: number;
    tamanioPagina: number;
    totalPaginas: number;
}

/**
 * Criterios de la búsqueda de bitácora.
 *
 * La búsqueda simple usa solo `texto`; la avanzada suma el resto. Todos se
 * combinan, y la búsqueda la resuelve la base: nunca se filtra acá.
 */
export interface CriteriosBitacora {
    texto?: string;
    usuario?: string;
    modulo?: string;
    accion?: string;
    nivel?: string;
    desde?: string;
    hasta?: string;
    pagina?: number;
    tamanioPagina?: number;
}

/**
 * Los criterios sin la paginación: lo que emite el buscador.
 *
 * La página no es un filtro y no la elige el formulario, la lleva el contenedor:
 * cambiar un filtro vuelve a la primera página, cambiar de página conserva los
 * filtros.
 */
export type FiltrosBitacora = Omit<CriteriosBitacora, 'pagina' | 'tamanioPagina'>;

/**
 * Niveles que registra la bitácora.
 *
 * Espejo de las constantes `Nivel*` de `worklingua.Server/BLL/BLLBitacora.cs`.
 * No hay una columna "resultado" en `BitacoraEvento`: el nivel es lo que
 * distingue una operación normal de una que falló.
 */
/**
 * Niveles de la bitácora. La etiqueta es una CLAVE de traducción: el buscador
 * la resuelve con `t()`, así el combo acompaña el cambio de idioma. El `valor`
 * es lo que viaja al backend y no se traduce.
 */
export const NIVELES_BITACORA = [
    { valor: 'INFO', etiqueta: 'admin.bitacora.nivel.info' },
    { valor: 'WARN', etiqueta: 'admin.bitacora.nivel.warn' },
    { valor: 'ERROR', etiqueta: 'admin.bitacora.nivel.error' },
] as const;

/**
 * Módulos que escriben en la bitácora.
 *
 * Espejo de las constantes `Modulo*` de `worklingua.Server/BLL/BLLBitacora.cs`,
 * que es la fuente de verdad. Se listan acá en vez de pedirlos por API para no
 * agregar un endpoint de catálogo que devolvería siempre lo mismo.
 */
export const MODULOS_BITACORA = [
    'Autenticacion',
    'Caracteristica',
    'Comentario',
    'Contacto',
    'Curso',
    'Empresa',
    'Idioma',
    'Instalacion',
    'Operador',
    'Permiso',
    'Plan',
    'Rol',
    'Seguridad',
    'Usuario',
] as const;

/** Opciones de tamaño de página que ofrece el paginador. El backend topea en 100. */
export const TAMANIOS_PAGINA = [25, 50, 100] as const;
