import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { ErrorApi } from '../api/clienteHttp';
import { newsletterApi } from '../api/newsletterApi';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { TarjetaAuth } from '../componentes/TarjetaAuth';

/**
 * Destino del enlace de baja que trae cada newsletter.
 *
 * A diferencia de la confirmación, la baja pide un clic: algunos filtros de
 * correo abren los enlaces para analizarlos y darían de baja a la persona sin
 * que lo haya pedido.
 */
export function BajaNewsletter() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token');

    const [hecha, setHecha] = useState(false);
    const [procesando, setProcesando] = useState(false);
    const [error, setError] = useState<string | null>(token ? null : t('newsletter.enlaceInvalido'));

    async function darDeBaja() {
        if (!token) {
            return;
        }

        setError(null);
        setProcesando(true);

        try {
            await newsletterApi.darDeBaja(token);
            setHecha(true);
        } catch (excepcion) {
            setError(excepcion instanceof ErrorApi ? excepcion.message : t('newsletter.baja.error'));
        } finally {
            setProcesando(false);
        }
    }

    return (
        <TarjetaAuth
            titulo={t('newsletter.baja.titulo')}
            pie={<Link to="/novedades">{t('newsletter.irNovedades')}</Link>}
        >
            {!hecha && token && <p className="descripcion">{t('newsletter.baja.pregunta')}</p>}

            <Alerta tipo="exito" mensaje={hecha ? t('newsletter.baja.exito') : null} />
            <Alerta tipo="error" mensaje={error} />

            {!hecha && token && (
                <Boton type="button" cargando={procesando} onClick={() => void darDeBaja()}>
                    {t('newsletter.baja.confirmar')}
                </Boton>
            )}
        </TarjetaAuth>
    );
}
