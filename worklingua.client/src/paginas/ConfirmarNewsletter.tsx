import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { ErrorApi } from '../api/clienteHttp';
import { newsletterApi } from '../api/newsletterApi';
import { Alerta } from '../componentes/Alerta';
import { TarjetaAuth } from '../componentes/TarjetaAuth';

type EstadoConfirmacion = 'procesando' | 'confirmada' | 'fallida';

/**
 * Destino del enlace del correo de confirmación del newsletter. Confirma apenas
 * se abre, igual que `ConfirmarCuenta`.
 */
export function ConfirmarNewsletter() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token');

    const [estado, setEstado] = useState<EstadoConfirmacion>(token ? 'procesando' : 'fallida');
    const [error, setError] = useState<string | null>(null);

    // StrictMode ejecuta los efectos dos veces en desarrollo: se confirma una sola vez.
    const yaEnviado = useRef(false);

    useEffect(() => {
        if (!token || yaEnviado.current) {
            return;
        }

        yaEnviado.current = true;

        newsletterApi
            .confirmar(token)
            .then(() => setEstado('confirmada'))
            .catch((excepcion: unknown) => {
                setEstado('fallida');
                setError(excepcion instanceof ErrorApi ? excepcion.message : null);
            });
    }, [token]);

    const mensajeError = !token
        ? t('newsletter.enlaceInvalido')
        : (error ?? t('newsletter.confirmar.error'));

    return (
        <TarjetaAuth
            titulo={t('newsletter.confirmar.titulo')}
            pie={
                estado !== 'procesando' ? (
                    <Link to="/novedades">{t('newsletter.irNovedades')}</Link>
                ) : undefined
            }
        >
            {estado === 'procesando' && (
                <p className="descripcion">{t('newsletter.confirmar.procesando')}</p>
            )}

            <Alerta
                tipo="exito"
                mensaje={estado === 'confirmada' ? t('newsletter.confirmar.exito') : null}
            />
            <Alerta tipo="error" mensaje={estado === 'fallida' ? mensajeError : null} />
        </TarjetaAuth>
    );
}
