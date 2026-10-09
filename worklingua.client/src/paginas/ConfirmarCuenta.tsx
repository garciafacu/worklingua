import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { TarjetaAuth } from '../componentes/TarjetaAuth';
import { conPlanPendiente, planRecordado } from '../rutas/contratacionPendiente';

type EstadoConfirmacion = 'procesando' | 'confirmada' | 'fallida';

/**
 * Destino del enlace del correo de confirmación: activa la cuenta con el token
 * del query string apenas se abre la página.
 */
export function ConfirmarCuenta() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token');

    // Que falte el token se sabe desde el primer render, así que es estado
    // inicial y no algo que deba resolver un efecto.
    const [estado, setEstado] = useState<EstadoConfirmacion>(token ? 'procesando' : 'fallida');
    // Se guarda la CLAVE, no el texto: así el mensaje propio de la pantalla
    // acompaña el cambio de idioma. Los mensajes que vienen del backend se
    // guardan tal cual y quedan en español, que es la limitación asumida.
    const [mensaje, setMensaje] = useState<string | null>(
        token ? null : t('auth.confirmar.enlaceInvalido'),
    );

    // El token es de un solo uso y StrictMode ejecuta los efectos dos veces en
    // desarrollo: sin este guard, la segunda llamada consumiría un token ya
    // usado y la pantalla mostraría un error sobre una confirmación exitosa.
    const yaEnviado = useRef(false);

    // Si la cuenta se creó para contratar un plan del Catálogo, el enlace al
    // login lo lleva: el enlace del correo abre otra pestaña y ya no hay query.
    const [planAContratar] = useState(planRecordado);

    useEffect(() => {
        if (!token || yaEnviado.current) {
            return;
        }

        yaEnviado.current = true;

        autenticacionApi
            .confirmarCuenta(token)
            .then((respuesta) => {
                setEstado('confirmada');
                setMensaje(respuesta.mensaje);
            })
            .catch((excepcion: unknown) => {
                setEstado('fallida');
                setMensaje(
                    excepcion instanceof ErrorApi
                        ? excepcion.message
                        : t('auth.confirmar.error'),
                );
            });
    }, [token, t]);

    return (
        <TarjetaAuth
            titulo={t('auth.confirmar.titulo')}
            pie={
                estado !== 'procesando' ? (
                    <Link to={conPlanPendiente('/login', planAContratar)}>{t('auth.irLogin')}</Link>
                ) : undefined
            }
        >
            {estado === 'procesando' && (
                <p className="descripcion">{t('auth.confirmar.procesando')}</p>
            )}

            <Alerta tipo="exito" mensaje={estado === 'confirmada' ? mensaje : null} />
            <Alerta tipo="error" mensaje={estado === 'fallida' ? mensaje : null} />
        </TarjetaAuth>
    );
}
