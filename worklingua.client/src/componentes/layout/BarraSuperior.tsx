import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate } from 'react-router-dom';
import { useSesion } from '../../contexto/useSesion';
import { BuscadorBarra } from '../BuscadorBarra';
import { Marca } from '../Marca';
import { CampanaNotificaciones } from './CampanaNotificaciones';
import { SelectorIdioma } from './SelectorIdioma';

interface BarraSuperiorProps {
    /** Abre y cierra el menú lateral en pantallas chicas. */
    alternarMenu: () => void;
    menuAbierto: boolean;
}

/**
 * Barra horizontal fija del área privada. Es estática: siempre muestra lo mismo
 * (marca, selector de idioma, usuario y salida), sin depender de la ruta ni de
 * los permisos.
 *
 * Ocupa todo el alto que le da `LayoutPrivado` (`--alto-barra`) en vez de
 * medirse por su propio padding: el menú lateral se cuelga de esa misma medida
 * para quedar pegado justo debajo, y con un alto variable quedarían desalineados.
 */
export function BarraSuperior({ alternarMenu, menuAbierto }: BarraSuperiorProps) {
    const { t } = useTranslation();
    const { usuario, cerrarSesion } = useSesion();
    const navegar = useNavigate();
    const [cerrando, setCerrando] = useState(false);

    async function manejarLogout() {
        setCerrando(true);
        await cerrarSesion();
        navegar('/login', { replace: true });
    }

    return (
        <header className="h-full border-b border-borde bg-superficie">
            <div className="flex h-full items-center gap-3 px-4">
                <button
                    type="button"
                    className="rounded-lg border border-borde bg-superficie px-3 py-2 text-sm font-semibold text-texto hover:bg-fondo lg:hidden"
                    aria-expanded={menuAbierto}
                    aria-controls="menu-lateral"
                    aria-label={menuAbierto ? t('layout.menu.cerrar') : t('layout.menu.abrir')}
                    onClick={alternarMenu}
                >
                    ☰
                </button>

                <Link to="/inicio" className="text-lg font-bold tracking-tight text-marca no-underline">
                    <Marca claseAcento="text-primario" />
                </Link>

                <div className="ml-auto flex items-center gap-3">
                    <BuscadorBarra
                        rutaResultados="/inicio/buscar"
                        identificador="buscarPrivado"
                        className="hidden w-56 md:block"
                    />

                    <CampanaNotificaciones />

                    <SelectorIdioma />

                    <span className="hidden text-sm text-texto-suave sm:inline">
                        {usuario?.nombre} {usuario?.apellido}
                    </span>

                    <button
                        type="button"
                        onClick={manejarLogout}
                        disabled={cerrando}
                        className="rounded-lg border border-borde bg-superficie px-3 py-2 text-sm font-semibold text-texto hover:bg-fondo disabled:cursor-not-allowed disabled:opacity-60"
                    >
                        {cerrando ? t('layout.barra.saliendo') : t('layout.barra.cerrarSesion')}
                    </button>
                </div>
            </div>
        </header>
    );
}
