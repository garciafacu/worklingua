import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, NavLink, useLocation } from 'react-router-dom';
import { ITEMS_PUBLICOS } from '../../rutas/itemsMenu';
import { BuscadorBarra } from '../BuscadorBarra';
import { Marca } from '../Marca';
import { SelectorIdioma } from './SelectorIdioma';

const CLASE_ENLACE = 'whitespace-nowrap rounded-lg px-2 py-2 text-sm font-medium no-underline transition-colors';

function clasesEnlace({ isActive }: { isActive: boolean }) {
    return isActive
        ? `${CLASE_ENLACE} bg-info-fondo text-primario`
        : `${CLASE_ENLACE} text-texto-suave hover:bg-fondo hover:text-texto`;
}

interface EnlacesProps {
    className: string;
    claseBotonIngreso: string;
    alNavegar?: () => void;
}

/** Los mismos enlaces sirven al menú horizontal y al desplegable; cambia el envoltorio. */
function EnlacesPublicos({ className, claseBotonIngreso, alNavegar }: EnlacesProps) {
    const { t } = useTranslation();
    const { pathname } = useLocation();
    const enLogin = pathname === '/login';

    return (
        <nav aria-label={t('layout.publico.navegacion')} className={className}>
            {ITEMS_PUBLICOS.map((item) => (
                <NavLink
                    key={item.ruta}
                    to={item.ruta}
                    end={item.ruta === '/'}
                    className={clasesEnlace}
                    onClick={alNavegar}
                >
                    {t(item.etiqueta)}
                </NavLink>
            ))}

            {enLogin ? (
                // Ya estamos en /login: el botón se muestra activo pero
                // deshabilitado, no tiene sentido navegar a la misma pantalla.
                <span
                    className={`${claseBotonIngreso} cursor-not-allowed opacity-60`}
                    aria-disabled="true"
                    aria-current="page"
                >
                    {t('layout.publico.iniciarSesion')}
                </span>
            ) : (
                <Link to="/login" className={claseBotonIngreso} onClick={alNavegar}>
                    {t('layout.publico.iniciarSesion')}
                </Link>
            )}
        </nav>
    );
}

/**
 * Navegación horizontal del sitio público. En pantallas chicas los enlaces se
 * pliegan detrás de un botón; el estado vive acá porque no le interesa a nadie más.
 */
export function EncabezadoPublico() {
    const { t } = useTranslation();
    const [desplegado, setDesplegado] = useState(false);

    return (
        <header className="sticky top-0 z-20 border-b border-borde bg-superficie">
            <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-3">
                <Link
                    to="/"
                    className="text-lg font-bold tracking-tight text-marca no-underline"
                    onClick={() => setDesplegado(false)}
                >
                    <Marca claseAcento="text-primario" />
                </Link>

                <EnlacesPublicos
                    className="hidden items-center gap-1 xl:flex"
                    claseBotonIngreso="ml-2 whitespace-nowrap rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                />

                <div className="flex items-center gap-2">
                    <BuscadorBarra
                        rutaResultados="/buscar"
                        identificador="buscarPublico"
                        className="hidden w-44 md:block"
                    />

                    <SelectorIdioma />

                    <button
                        type="button"
                        className="rounded-lg border border-borde bg-superficie px-3 py-2 text-sm font-semibold text-texto hover:bg-fondo xl:hidden"
                        aria-expanded={desplegado}
                        aria-controls="menu-publico"
                        onClick={() => setDesplegado((abierto) => !abierto)}
                    >
                        {desplegado ? t('comun.boton.cerrar') : t('layout.publico.menu')}
                    </button>
                </div>
            </div>

            {desplegado && (
                <div id="menu-publico" className="border-t border-borde px-4 py-3 xl:hidden">
                    <BuscadorBarra
                        rutaResultados="/buscar"
                        identificador="buscarPublicoMenu"
                        className="mb-3 md:hidden"
                        alBuscar={() => setDesplegado(false)}
                    />

                    <EnlacesPublicos
                        className="flex flex-col gap-1"
                        claseBotonIngreso="mt-2 rounded-lg bg-primario px-4 py-2 text-center text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                        alNavegar={() => setDesplegado(false)}
                    />
                </div>
            )}
        </header>
    );
}
