import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { CampoTexto } from '../componentes/CampoTexto';
import { TarjetaAuth } from '../componentes/TarjetaAuth';

export function RecuperarClave() {
    const { t } = useTranslation();

    const [email, setEmail] = useState('');
    const [mensaje, setMensaje] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(false);

    async function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setMensaje(null);
        setCargando(true);

        try {
            // El backend responde lo mismo exista o no la cuenta, asi que la
            // pantalla tampoco distingue los dos casos.
            const respuesta = await autenticacionApi.recuperarClave(email.trim());
            setMensaje(respuesta.mensaje);
            setEmail('');
        } catch (excepcion) {
            setError(excepcion instanceof ErrorApi ? excepcion.message : t('auth.recuperar.error'));
        } finally {
            setCargando(false);
        }
    }

    return (
        <TarjetaAuth
            titulo={t('auth.recuperar.titulo')}
            descripcion={t('auth.recuperar.descripcion')}
            pie={<Link to="/login">{t('auth.volverLogin')}</Link>}
        >
            <form onSubmit={manejarEnvio} noValidate>
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={mensaje} />

                <CampoTexto
                    etiqueta={t('comun.campo.email')}
                    identificador="email"
                    type="email"
                    autoComplete="username"
                    required
                    value={email}
                    onChange={(evento) => setEmail(evento.target.value)}
                />

                <Boton type="submit" cargando={cargando}>
                    {t('auth.recuperar.enviar')}
                </Boton>
            </form>
        </TarjetaAuth>
    );
}
