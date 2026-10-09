import { useTranslation } from 'react-i18next';
import { NavLink } from 'react-router-dom';
import { useSesion } from '../../contexto/useSesion';
import { GRUPOS_MENU } from '../../rutas/itemsMenu';

const CLASE_ITEM = 'block rounded-lg px-3 py-2 text-sm font-medium no-underline transition-colors';

function clasesItem({ isActive }: { isActive: boolean }) {
    return isActive
        ? `${CLASE_ITEM} bg-info-fondo text-primario`
        : `${CLASE_ITEM} text-texto-suave hover:bg-fondo hover:text-texto`;
}

interface MenuLateralProps {
    /** Cierra el menú al navegar cuando está en modo panel lateral. */
    alNavegar?: () => void;
}

/**
 * Menú vertical del área privada, separado en bloques.
 *
 * Cada ítem se filtra por dos cosas: el permiso que declara —los que no declaran
 * ninguno se muestran siempre— y la marca `oculto`, que saca del menú una
 * pantalla terminada que todavía no se quiere ofrecer.
 *
 * Un bloque sin ítems visibles no se dibuja: Administración desaparece sola para
 * quien no administra nada, en lugar de dejar un título suelto.
 *
 * Nada de esto es seguridad, es presentación. Entrar a la ruta a mano igual
 * termina en un 403 del backend, que resuelve los permisos contra la base en
 * cada pedido.
 */
export function MenuLateral({ alNavegar }: MenuLateralProps) {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const grupos = GRUPOS_MENU.map((grupo) => ({
        ...grupo,
        items: grupo.items.filter(
            (item) =>
                !item.oculto &&
                (!item.permiso || tienePermiso(item.permiso)) &&
                (!item.ocultarConPermiso || !tienePermiso(item.ocultarConPermiso)),
        ),
    })).filter((grupo) => grupo.items.length > 0);

    return (
        // `grow` para que el nav llene el alto del sidebar y `mt-auto` del último
        // bloque tenga espacio libre que absorber. Si el menú no entra, el
        // `mt-auto` colapsa y `overflow-y-auto` del <aside> lo hace scrollear.
        <nav
            aria-label={t('layout.menu.titulo')}
            className="flex grow flex-col gap-7 p-3 pb-6"
        >
            {grupos.map((grupo) => (
                <section
                    key={grupo.etiqueta}
                    className={`flex flex-col gap-1 ${
                        grupo.fijarAbajo ? 'mt-auto border-t border-borde pt-6' : ''
                    }`}
                >
                    <h2 className="px-3 pb-1 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                        {t(grupo.etiqueta)}
                    </h2>

                    {grupo.items.map((item) => (
                        <NavLink
                            key={item.ruta}
                            to={item.ruta}
                            // Sin `end`, /inicio quedaría activo en todas las
                            // pantallas del área privada, porque todas cuelgan de él.
                            end={item.ruta === '/inicio'}
                            className={clasesItem}
                            onClick={alNavegar}
                        >
                            {t(item.etiqueta)}
                        </NavLink>
                    ))}
                </section>
            ))}
        </nav>
    );
}
