import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import hero from '../../assets/hero.png';
import { Seccion } from '../../componentes/Seccion';

/**
 * Las tres propuestas de valor. Se guardan las CLAVES, no los textos: el
 * componente las resuelve con `t()` en cada render, así que la tarjeta cambia
 * de idioma sin que haya que rearmar el arreglo.
 */
const PROPUESTA = ['sector', 'practica', 'apps'];

export function InicioPublico() {
    const { t } = useTranslation();

    return (
        <>
            <section className="bg-superficie">
                <div className="mx-auto grid max-w-6xl items-center gap-10 px-4 py-16 sm:py-20 lg:grid-cols-2">
                    <div>
                        <h1 className="text-4xl font-semibold tracking-tight text-texto sm:text-5xl">
                            {t('publico.inicio.hero.titulo')}
                        </h1>

                        <p className="mt-5 max-w-xl text-lg text-texto-suave">
                            {t('publico.inicio.hero.detalle')}
                        </p>

                        <div className="mt-8 flex flex-wrap gap-3">
                            <Link
                                to="/catalogo"
                                className="rounded-lg bg-primario px-5 py-3 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                            >
                                {t('publico.inicio.hero.verPlanes')}
                            </Link>

                            <Link
                                to="/registro"
                                className="rounded-lg border border-borde bg-superficie px-5 py-3 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                            >
                                {t('publico.inicio.hero.crearCuenta')}
                            </Link>
                        </div>
                    </div>

                    <img
                        src={hero}
                        alt=""
                        className="mx-auto w-full max-w-md rounded-2xl border border-borde"
                    />
                </div>
            </section>

            <Seccion
                titulo={t('publico.inicio.propuesta.titulo')}
                descripcion={t('publico.inicio.propuesta.descripcion')}
            >
                <div className="grid gap-6 md:grid-cols-3">
                    {PROPUESTA.map((clave) => (
                        <article
                            key={clave}
                            className="rounded-2xl border border-borde bg-superficie p-6"
                        >
                            <h3 className="text-lg font-semibold text-texto">
                                {t(`publico.inicio.propuesta.${clave}.titulo`)}
                            </h3>
                            <p className="mt-2 text-sm text-texto-suave">
                                {t(`publico.inicio.propuesta.${clave}.detalle`)}
                            </p>
                        </article>
                    ))}
                </div>
            </Seccion>

            <Seccion
                titulo={t('publico.inicio.cierre.titulo')}
                descripcion={t('publico.inicio.cierre.descripcion')}
                fondoAlterno
            >
                <div className="flex flex-wrap gap-3">
                    <Link
                        to="/catalogo"
                        className="rounded-lg bg-primario px-5 py-3 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                    >
                        {t('publico.inicio.cierre.conocerPlanes')}
                    </Link>

                    <Link
                        to="/institucional"
                        className="rounded-lg border border-borde bg-superficie px-5 py-3 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                    >
                        {t('publico.inicio.cierre.quienesSomos')}
                    </Link>
                </div>
            </Seccion>
        </>
    );
}
