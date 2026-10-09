import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Outlet, useLocation } from 'react-router-dom';
import { useSesion } from '../../contexto/useSesion';
import { BarraSuperior } from './BarraSuperior';
import { MenuLateral } from './MenuLateral';

/**
 * Maqueta del área privada: barra horizontal arriba y menú vertical a la
 * izquierda.
 *
 * **Hay un solo scroll: el de la ventana.** Antes el armazón medía `h-screen` y
 * el desplazamiento vivía dentro de `<main>`, así que una pantalla larga —
 * Opiniones o Mi plan— dibujaba su propia barra de scroll metida adentro de la
 * página, además de la del navegador, y al llegar al fondo el menú quedaba
 * cortado. Ahora el armazón crece con el contenido (`min-h-screen`), `<main>` no
 * recorta nada, y lo que se queda quieto lo hace con `sticky`.
 *
 * La barra y el menú se sostienen con `sticky` y no con `fixed` para no tener
 * que reservarles lugar con márgenes: siguen ocupando su espacio en el flujo y
 * el contenido nunca les queda debajo.
 *
 * Desde `lg` el menú es una columna más, pegada bajo la barra y con su propio
 * scroll solo si el listado no entra. Por debajo se convierte en un panel
 * `fixed` que se corre sobre el contenido.
 */
export function LayoutPrivado() {
    const { t } = useTranslation();
    const [menuAbierto, setMenuAbierto] = useState(false);
    const { refrescarSesion } = useSesion();
    const { pathname } = useLocation();

    // El menú se arma con los permisos de la sesión. Se releen al entrar y en
    // cada navegación para reflejar cambios de roles o familias sin volver a
    // iniciar sesión. Quien autoriza sigue siendo el backend.
    useEffect(() => {
        void refrescarSesion();
    }, [pathname, refrescarSesion]);

    function cerrarMenu() {
        setMenuAbierto(false);
    }

    return (
        <div className="flex min-h-screen flex-col bg-fondo">
            {/* `sticky` va acá y no dentro de BarraSuperior: el elemento pegajoso
                se mueve dentro de su contenedor, que tiene que ser el armazón
                entero. Envuelto en un div de alto propio, la barra se despegaría
                apenas ese div sale de pantalla. */}
            <div className="sticky top-0 z-40 h-[var(--alto-barra)] shrink-0">
                <BarraSuperior
                    menuAbierto={menuAbierto}
                    alternarMenu={() => setMenuAbierto((abierto) => !abierto)}
                />
            </div>

            <div className="flex flex-1">
                {/* Fondo oscurecido: cierra el menú al tocar fuera, solo en pantallas chicas. */}
                {menuAbierto && (
                    <button
                        type="button"
                        aria-label={t('layout.menu.cerrar')}
                        className="fixed inset-0 z-30 bg-texto/40 lg:hidden"
                        onClick={cerrarMenu}
                    />
                )}

                {/* Desde lg: pegado bajo la barra, del alto exacto de lo que
                    queda de ventana. El alto explícito es lo que le permite ser
                    `sticky` —un item de flex estirado no tiene margen para
                    moverse— y además mantiene el fondo blanco hasta el borde
                    inferior cuando la página es más larga que la pantalla. */}
                <aside
                    id="menu-lateral"
                    className={`fixed inset-y-0 left-0 z-40 flex w-64 shrink-0 flex-col overflow-y-auto border-r border-borde bg-superficie transition-transform lg:sticky lg:top-[var(--alto-barra)] lg:z-0 lg:h-[calc(100dvh-var(--alto-barra))] lg:translate-x-0 ${
                        menuAbierto ? 'translate-x-0' : '-translate-x-full'
                    }`}
                >
                    <div className="flex justify-end p-3 lg:hidden">
                        <button
                            type="button"
                            onClick={cerrarMenu}
                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                        >
                            {t('comun.boton.cerrar')}
                        </button>
                    </div>

                    <MenuLateral alNavegar={cerrarMenu} />
                </aside>

                {/* Sin `overflow-y-auto`: si recortara, volvería a aparecer la
                    segunda barra de scroll adentro de la página. */}
                <main className="min-w-0 flex-1 px-4 py-6 sm:px-6 lg:px-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
