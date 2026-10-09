import { useTranslation } from 'react-i18next';
import { PaginaEstatica } from '../../componentes/PaginaEstatica';

export function Institucional() {
    const { t } = useTranslation();

    return (
        <PaginaEstatica titulo={t('institucional.titulo')}>
            <p>{t('institucional.intro')}</p>

            <h2>{t('institucional.queHacemos.titulo')}</h2>
            <p>{t('institucional.queHacemos.p1')}</p>

            <h2>{t('institucional.comoFunciona.titulo')}</h2>
            <p>{t('institucional.comoFunciona.p1')}</p>
            <ul>
                <li>{t('institucional.comoFunciona.li1')}</li>
                <li>{t('institucional.comoFunciona.li2')}</li>
                <li>{t('institucional.comoFunciona.li3')}</li>
            </ul>

            <h2>{t('institucional.destinatarios.titulo')}</h2>
            <p>{t('institucional.destinatarios.p1')}</p>

            <h2>{t('institucional.proyecto.titulo')}</h2>
            <p>{t('institucional.proyecto.p1')}</p>
        </PaginaEstatica>
    );
}
