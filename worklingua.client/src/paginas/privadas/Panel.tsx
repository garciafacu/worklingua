import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { useSesion } from '../../contexto/useSesion';

/** Pantalla posterior al login. Confirma que la sesión y los roles llegaron bien. */
export function Panel() {
    const { t } = useTranslation();
    const { usuario } = useSesion();

    return (
        <div className="mx-auto max-w-4xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.panel.saludo', { nombre: usuario?.nombre ?? '' })}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.panel.sesionActiva')}</p>

            <section className="mt-8 rounded-2xl border border-borde bg-superficie p-6">
                <h2 className="text-lg font-semibold text-texto">{t('privado.panel.datosSesion')}</h2>

                <dl className="mt-4 grid gap-x-6 gap-y-3 text-sm sm:grid-cols-[auto_1fr]">
                    <dt className="font-semibold text-texto-suave">{t('comun.campo.email')}</dt>
                    <dd className="m-0 break-words text-texto">{usuario?.email}</dd>

                    <dt className="font-semibold text-texto-suave">{t('privado.panel.roles')}</dt>
                    <dd className="m-0 break-words text-texto">
                        {usuario?.roles.join(', ') || t('privado.panel.sinRoles')}
                    </dd>

                    <dt className="font-semibold text-texto-suave">{t('privado.panel.permisos')}</dt>
                    <dd className="m-0 break-words text-texto">
                        {usuario?.permisos.join(', ') || t('privado.panel.sinPermisos')}
                    </dd>
                </dl>
            </section>

            <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                <h2 className="text-lg font-semibold text-texto">{t('privado.panel.tuCuenta')}</h2>
                <p className="mt-2 text-sm text-texto-suave">
                    {t('privado.panel.cuentaDetalle')}
                </p>

                <Link
                    to="/inicio/cambiar-clave"
                    className="mt-4 inline-block rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                >
                    {t('privado.panel.cambiarClave')}
                </Link>
            </section>
        </div>
    );
}
