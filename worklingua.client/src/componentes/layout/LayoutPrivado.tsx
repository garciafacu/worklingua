import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Outlet, useLocation } from 'react-router-dom';
import { useSesion } from '../../contexto/useSesion';
import { BarraSuperior } from './BarraSuperior';
import { MenuLateral } from './MenuLateral';
import { BotonAccion } from '../Boton';

export function LayoutPrivado() {
    const { t } = useTranslation();
    const [menuAbierto, setMenuAbierto] = useState(false);
    const { refrescarSesion } = useSesion();
    const { pathname } = useLocation();

    useEffect(() => {
        void refrescarSesion();
    }, [pathname, refrescarSesion]);

    function cerrarMenu() {
        setMenuAbierto(false);
    }

    return (
        <div className="flex min-h-screen flex-col bg-fondo">
            <div className="sticky top-0 z-40 h-[var(--alto-barra)] shrink-0">
                <BarraSuperior
                    menuAbierto={menuAbierto}
                    alternarMenu={() => setMenuAbierto((abierto) => !abierto)}
                />
            </div>

            <div className="flex flex-1">
                {menuAbierto && (
                    <button
                        type="button"
                        aria-label={t('layout.menu.cerrar')}
                        className="fixed inset-0 z-30 bg-texto/40 lg:hidden"
                        onClick={cerrarMenu}
                    />
                )}

                <aside
                    id="menu-lateral"
                    className={`fixed inset-y-0 left-0 z-40 flex w-64 shrink-0 flex-col overflow-y-auto border-r border-borde bg-superficie transition-transform lg:sticky lg:top-[var(--alto-barra)] lg:z-0 lg:h-[calc(100dvh-var(--alto-barra))] lg:translate-x-0 ${
                        menuAbierto ? 'translate-x-0' : '-translate-x-full'
                    }`}
                >
                    <div className="flex justify-end p-3 lg:hidden">
                        <BotonAccion onClick={cerrarMenu}>
                            {t('comun.boton.cerrar')}
                        </BotonAccion>
                    </div>

                    <MenuLateral alNavegar={cerrarMenu} />
                </aside>

                <main className="min-w-0 flex-1 px-4 py-6 sm:px-6 lg:px-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
