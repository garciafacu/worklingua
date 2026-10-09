import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { ErrorApi } from '../api/clienteHttp';
import { usuariosApi } from '../api/usuariosApi';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { CampoTexto } from '../componentes/CampoTexto';
import { TarjetaAuth } from '../componentes/TarjetaAuth';

/**
 * Destino del enlace del correo de invitación.
 *
 * A diferencia de restablecer la clave, acá la cuenta todavía no está activa: el
 * mismo pedido define la primera clave y la activa. Por eso el token es de tipo
 * invitación y no de recupero.
 */
export function CompletarInvitacion() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token');

    const [clave, setClave] = useState('');
    const [confirmacion, setConfirmacion] = useState('');
    const [errorConfirmacion, setErrorConfirmacion] = useState<string | undefined>();
    const [mensaje, setMensaje] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(false);

    async function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setMensaje(null);
        setErrorConfirmacion(undefined);

        if (clave !== confirmacion) {
            setErrorConfirmacion(t('auth.clavesNoCoinciden'));
            return;
        }

        if (!token) {
            setError(t('auth.invitacion.enlaceInvalido'));
            return;
        }

        setCargando(true);

        try {
            const respuesta = await usuariosApi.completarInvitacion({ token, clave });
            setMensaje(respuesta.mensaje);
            setClave('');
            setConfirmacion('');
        } catch (excepcion) {
            setError(
                excepcion instanceof ErrorApi ? excepcion.message : t('auth.invitacion.error'),
            );
        } finally {
            setCargando(false);
        }
    }

    return (
        <TarjetaAuth
            titulo={t('auth.invitacion.titulo')}
            descripcion={t('auth.invitacion.descripcion')}
            pie={<Link to="/login">{t('auth.irLogin')}</Link>}
        >
            <form onSubmit={manejarEnvio} noValidate>
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={mensaje} />

                <CampoTexto
                    etiqueta={t('comun.campo.clave')}
                    identificador="clave"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={clave}
                    onChange={(evento) => setClave(evento.target.value)}
                />

                <CampoTexto
                    etiqueta={t('auth.campo.repetirClave')}
                    identificador="confirmacion"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={confirmacion}
                    error={errorConfirmacion}
                    onChange={(evento) => setConfirmacion(evento.target.value)}
                />

                <p className="ayuda">{t('auth.ayudaClave')}</p>

                <Boton type="submit" cargando={cargando}>
                    {t('auth.invitacion.activar')}
                </Boton>
            </form>
        </TarjetaAuth>
    );
}
