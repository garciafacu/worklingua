import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { CampoTexto } from '../componentes/CampoTexto';
import { TarjetaAuth } from '../componentes/TarjetaAuth';

/** Destino del enlace del correo de recupero de clave. */
export function RestablecerClave() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token');

    const [claveNueva, setClaveNueva] = useState('');
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

        if (claveNueva !== confirmacion) {
            setErrorConfirmacion(t('auth.clavesNoCoinciden'));
            return;
        }

        if (!token) {
            setError(t('auth.restablecer.enlaceInvalido'));
            return;
        }

        setCargando(true);

        try {
            const respuesta = await autenticacionApi.restablecerClave({ token, claveNueva });
            setMensaje(respuesta.mensaje);
            setClaveNueva('');
            setConfirmacion('');
        } catch (excepcion) {
            setError(excepcion instanceof ErrorApi ? excepcion.message : t('auth.restablecer.error'));
        } finally {
            setCargando(false);
        }
    }

    return (
        <TarjetaAuth
            titulo={t('auth.restablecer.titulo')}
            descripcion={t('auth.restablecer.descripcion')}
            pie={<Link to="/login">{t('auth.irLogin')}</Link>}
        >
            <form onSubmit={manejarEnvio} noValidate>
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={mensaje} />

                <CampoTexto
                    etiqueta={t('auth.campo.claveNueva')}
                    identificador="claveNueva"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={claveNueva}
                    onChange={(evento) => setClaveNueva(evento.target.value)}
                />

                <CampoTexto
                    etiqueta={t('auth.campo.repetirClaveNueva')}
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
                    {t('auth.restablecer.guardar')}
                </Boton>
            </form>
        </TarjetaAuth>
    );
}
