import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { TarjetaAuth } from '../componentes/TarjetaAuth';
import { conPlanPendiente, planRecordado } from '../rutas/contratacionPendiente';

type EstadoConfirmacion = 'procesando' | 'confirmada' | 'fallida';

export function ConfirmarCuenta() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token');

    const [estado, setEstado] = useState<EstadoConfirmacion>(token ? 'procesando' : 'fallida');
    const [mensaje, setMensaje] = useState<string | null>(
        token ? null : t('auth.confirmar.enlaceInvalido'),
    );

    const yaEnviado = useRef(false);

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
