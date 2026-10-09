import { Trans, useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { PaginaEstatica } from '../../componentes/PaginaEstatica';

export function TerminosCondiciones() {
    const { t } = useTranslation();

    return (
        <PaginaEstatica
            titulo={t('legal.terminos.titulo')}
            actualizado={t('legal.terminos.actualizado')}
        >
            <p>{t('legal.terminos.intro')}</p>

            <h2>{t('legal.terminos.objeto.titulo')}</h2>
            <p>{t('legal.terminos.objeto.p1')}</p>

            <h2>{t('legal.terminos.cuentas.titulo')}</h2>
            <ul>
                <li>{t('legal.terminos.cuentas.li1')}</li>
                <li>{t('legal.terminos.cuentas.li2')}</li>
                <li>{t('legal.terminos.cuentas.li3')}</li>
            </ul>

            <h2>{t('legal.terminos.planes.titulo')}</h2>
            <p>{t('legal.terminos.planes.p1')}</p>

            <h2>{t('legal.terminos.usoAceptable.titulo')}</h2>
            <ul>
                <li>{t('legal.terminos.usoAceptable.li1')}</li>
                <li>{t('legal.terminos.usoAceptable.li2')}</li>
                <li>{t('legal.terminos.usoAceptable.li3')}</li>
            </ul>

            <h2>{t('legal.terminos.contenido.titulo')}</h2>
            <p>{t('legal.terminos.contenido.p1')}</p>

            <h2>{t('legal.terminos.disponibilidad.titulo')}</h2>
            <p>{t('legal.terminos.disponibilidad.p1')}</p>

            <h2>{t('legal.terminos.baja.titulo')}</h2>
            <p>{t('legal.terminos.baja.p1')}</p>

            <h2>{t('legal.terminos.datos.titulo')}</h2>
            {/*
                El párrafo lleva un enlace en el medio. Con <Trans> la traducción
                es UNA sola clave con el marcador <1>...</1>, en vez de tres
                fragmentos que cada idioma tendría que ordenar igual que el
                español. El índice 1 es el segundo hijo: el 0 es el texto suelto.
            */}
            <p>
                <Trans i18nKey="legal.terminos.datos.p1">
                    El tratamiento de los datos personales se describe en la{' '}
                    <Link to="/privacidad" className="text-primario hover:underline">
                        política de privacidad
                    </Link>
                    , que forma parte de estos términos.
                </Trans>
            </p>

            <h2>{t('legal.terminos.cambios.titulo')}</h2>
            <p>{t('legal.terminos.cambios.p1')}</p>

            <h2>{t('legal.terminos.contacto.titulo')}</h2>
            <p>
                <Trans i18nKey="legal.terminos.contacto.p1">
                    Ante cualquier consulta sobre estos términos, se puede escribir a través de la{' '}
                    <Link to="/contacto" className="text-primario hover:underline">
                        página de contacto
                    </Link>
                    .
                </Trans>
            </p>
        </PaginaEstatica>
    );
}
