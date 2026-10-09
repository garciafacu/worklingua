import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { CampoTexto } from '../componentes/CampoTexto';
import { TarjetaAuth } from '../componentes/TarjetaAuth';

/** Cambio de clave con la sesión abierta. Requiere la clave actual. */
export function CambiarClave() {
    const { t } = useTranslation();

    const [claveActual, setClaveActual] = useState('');
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

        setCargando(true);

        try {
            const respuesta = await autenticacionApi.cambiarClave({ claveActual, claveNueva });
            setMensaje(respuesta.mensaje);
            setClaveActual('');
            setClaveNueva('');
            setConfirmacion('');
        } catch (excepcion) {
            setError(excepcion instanceof ErrorApi ? excepcion.message : t('auth.cambiarClave.error'));
        } finally {
            setCargando(false);
        }
    }

    return (
        <div className="mx-auto max-w-4xl">
            <TarjetaAuth
                titulo={t('auth.cambiarClave.titulo')}
                descripcion={t('auth.cambiarClave.descripcion')}
                contenedor="panel"
                pie={<Link to="/inicio/perfil">{t('auth.cambiarClave.volver')}</Link>}
            >
                <form onSubmit={manejarEnvio} noValidate>
                    <Alerta tipo="error" mensaje={error} />
                    <Alerta tipo="exito" mensaje={mensaje} />

                    <CampoTexto
                        etiqueta={t('auth.campo.claveActual')}
                        identificador="claveActual"
                        type="password"
                        autoComplete="current-password"
                        required
                        value={claveActual}
                        onChange={(evento) => setClaveActual(evento.target.value)}
                    />

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
                        {t('auth.cambiarClave.guardar')}
                    </Boton>
                </form>
            </TarjetaAuth>
        </div>
    );
}
