import { Trans, useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { PaginaEstatica } from '../../componentes/PaginaEstatica';

export function PoliticaPrivacidad() {
    const { t } = useTranslation();

    return (
        <PaginaEstatica
            titulo={t('legal.privacidad.titulo')}
            actualizado={t('legal.privacidad.actualizado')}
        >
            <p>{t('legal.privacidad.intro')}</p>

            <h2>{t('legal.privacidad.datos.titulo')}</h2>
            <ul>
                {/*
                    Cada ítem abre con un rótulo en negrita. Va con <Trans> y una
                    sola clave para que la traducción decida dónde termina el
                    rótulo: en inglés el corte no cae en el mismo lugar.
                */}
                <li>
                    <Trans i18nKey="legal.privacidad.datos.li1">
                        <strong>De la cuenta:</strong> nombre, apellido, documento, correo
                        electrónico y empresa a la que pertenece la persona usuaria.
                    </Trans>
                </li>
                <li>
                    <Trans i18nKey="legal.privacidad.datos.li2">
                        <strong>De la actividad:</strong> avance en los cursos, resultados de
                        evaluaciones y prácticas de pronunciación.
                    </Trans>
                </li>
                <li>
                    <Trans i18nKey="legal.privacidad.datos.li3">
                        <strong>Técnicos:</strong> fecha y hora de cada inicio de sesión.
                    </Trans>
                </li>
            </ul>

            <h2>{t('legal.privacidad.usos.titulo')}</h2>
            <ul>
                <li>{t('legal.privacidad.usos.li1')}</li>
                <li>{t('legal.privacidad.usos.li2')}</li>
                <li>{t('legal.privacidad.usos.li3')}</li>
                <li>{t('legal.privacidad.usos.li4')}</li>
            </ul>

            <h2>{t('legal.privacidad.claves.titulo')}</h2>
            <p>{t('legal.privacidad.claves.p1')}</p>

            <h2>{t('legal.privacidad.comparten.titulo')}</h2>
            <p>{t('legal.privacidad.comparten.p1')}</p>

            <h2>{t('legal.privacidad.conservacion.titulo')}</h2>
            <p>{t('legal.privacidad.conservacion.p1')}</p>

            <h2>{t('legal.privacidad.correos.titulo')}</h2>
            <p>{t('legal.privacidad.correos.p1')}</p>

            <h2>{t('legal.privacidad.derechos.titulo')}</h2>
            <p>
                <Trans i18nKey="legal.privacidad.derechos.p1">
                    Toda persona puede solicitar el acceso, la rectificación o la supresión de sus
                    datos personales, así como oponerse a determinados tratamientos. Los pedidos se
                    canalizan a través de la{' '}
                    <Link to="/contacto" className="text-primario hover:underline">
                        página de contacto
                    </Link>
                    .
                </Trans>
            </p>

            <h2>{t('legal.privacidad.cambios.titulo')}</h2>
            <p>{t('legal.privacidad.cambios.p1')}</p>
        </PaginaEstatica>
    );
}
