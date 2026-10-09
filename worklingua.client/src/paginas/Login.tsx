import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { CampoTexto } from '../componentes/CampoTexto';
import { TarjetaAuth } from '../componentes/TarjetaAuth';
import { useSesion } from '../contexto/useSesion';
import {
    conPlanPendiente,
    olvidarPlanAContratar,
    planIdDeParametros,
    rutaContratacion,
} from '../rutas/contratacionPendiente';

/**
 * Inicio de sesión. Con `?contratar=<planId>` (el visitante eligió un plan en el
 * Catálogo) avisa que la contratación sigue después de ingresar y, al entrar,
 * lleva directo al formulario de ese plan en lugar del Panel.
 */
export function Login() {
    const { t } = useTranslation();
    const navegar = useNavigate();
    const { iniciarSesion } = useSesion();
    const [parametros] = useSearchParams();
    const planAContratar = planIdDeParametros(parametros);

    const [email, setEmail] = useState('');
    const [clave, setClave] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(false);

    async function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setCargando(true);

        try {
            const respuesta = await autenticacionApi.login({ email, clave });

            iniciarSesion(respuesta.token, {
                usuarioId: respuesta.usuarioId,
                nombre: respuesta.nombre,
                apellido: respuesta.apellido,
                email: respuesta.email,
                roles: respuesta.roles,
                permisos: respuesta.permisos ?? [],
            });

            olvidarPlanAContratar();
            navegar(planAContratar === null ? '/inicio' : rutaContratacion(planAContratar), {
                replace: true,
            });
        } catch (excepcion) {
            // El backend ya devuelve un mensaje apto para el usuario, incluido el
            // caso de la cuenta sin confirmar (403).
            setError(excepcion instanceof ErrorApi ? excepcion.message : t('auth.login.error'));
        } finally {
            setCargando(false);
        }
    }

    return (
        <TarjetaAuth
            titulo={t('auth.login.titulo')}
            descripcion={t('auth.login.descripcion')}
            pie={
                <>
                    <Link to="/recuperar-clave">{t('auth.login.olvide')}</Link>
                    <Link to={conPlanPendiente('/registro', planAContratar)}>
                        {t('auth.login.crearCuenta')}
                    </Link>
                </>
            }
        >
            <form onSubmit={manejarEnvio} noValidate>
                <Alerta
                    tipo="info"
                    mensaje={planAContratar === null ? null : t('auth.login.contratacionPendiente')}
                />
                <Alerta tipo="error" mensaje={error} />

                <CampoTexto
                    etiqueta={t('comun.campo.email')}
                    identificador="email"
                    type="email"
                    autoComplete="username"
                    required
                    value={email}
                    onChange={(evento) => setEmail(evento.target.value)}
                />

                <CampoTexto
                    etiqueta={t('comun.campo.clave')}
                    identificador="clave"
                    type="password"
                    autoComplete="current-password"
                    required
                    value={clave}
                    onChange={(evento) => setClave(evento.target.value)}
                />

                <Boton type="submit" cargando={cargando}>
                    {t('auth.login.ingresar')}
                </Boton>
            </form>
        </TarjetaAuth>
    );
}
