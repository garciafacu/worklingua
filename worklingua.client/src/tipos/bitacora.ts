export interface BitacoraEventoResponse {
    bitacoraId: number;
    usuarioId: number | null;
    fechaEvento: string;
    modulo: string | null;
    accion: string | null;
    descripcion: string | null;
    nivel: string | null;
    usuario: string;
    email: string | null;
}

export interface PaginaBitacoraResponse {
    registros: BitacoraEventoResponse[];
    totalRegistros: number;
    pagina: number;
    tamanioPagina: number;
    totalPaginas: number;
}

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

export type FiltrosBitacora = Omit<CriteriosBitacora, 'pagina' | 'tamanioPagina'>;

export const NIVELES_BITACORA = [
    { valor: 'INFO', etiqueta: 'admin.bitacora.nivel.info' },
    { valor: 'WARN', etiqueta: 'admin.bitacora.nivel.warn' },
    { valor: 'ERROR', etiqueta: 'admin.bitacora.nivel.error' },
] as const;

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

export const TAMANIOS_PAGINA = [25, 50, 100] as const;
