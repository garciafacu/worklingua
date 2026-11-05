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
    alNavegar?: () => void;
}

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
