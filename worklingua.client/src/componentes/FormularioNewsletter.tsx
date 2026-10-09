import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../api/clienteHttp';
import { newsletterApi } from '../api/newsletterApi';
import { useLocalizacion } from '../contexto/useLocalizacion';
import { Alerta } from './Alerta';
import { Boton } from './Boton';

/**
 * Suscripción al newsletter con doble opt-in.
 *
 * Manda el idioma activo de la interfaz: es el idioma en el que la persona va a
 * recibir la confirmación y los envíos. El mensaje de éxito es propio de la
 * pantalla (traducido) porque la API responde lo mismo exista o no el correo.
 */
export function FormularioNewsletter() {
    const { t } = useTranslation();
    const { idiomaActual } = useLocalizacion();

    const [email, setEmail] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [enviando, setEnviando] = useState(false);

    async function suscribir(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);

        const limpio = email.trim();

        if (!limpio.includes('@')) {
            setError(t('newsletter.formulario.emailInvalido'));

            return;
        }

        setEnviando(true);

        try {
            await newsletterApi.suscribir({ email: limpio, idioma: idiomaActual });
            setExito(t('newsletter.formulario.exito'));
            setEmail('');
        } catch (excepcion) {
            setError(
                excepcion instanceof ErrorApi ? excepcion.message : t('newsletter.formulario.error'),
            );
        } finally {
            setEnviando(false);
        }
    }

    return (
        <section aria-labelledby="titulo-newsletter">
            <h2 id="titulo-newsletter" className="text-base font-semibold text-texto">
                {t('newsletter.formulario.titulo')}
            </h2>
            <p className="mt-1 text-sm text-texto-suave">{t('newsletter.formulario.descripcion')}</p>

            <form onSubmit={suscribir} noValidate className="mt-3 flex flex-col gap-2 sm:flex-row">
                <label htmlFor="email-newsletter" className="sr-only">
                    {t('newsletter.formulario.email')}
                </label>
                <input
                    id="email-newsletter"
                    name="email"
                    type="email"
                    autoComplete="email"
                    maxLength={150}
                    required
                    placeholder={t('newsletter.formulario.placeholder')}
                    value={email}
                    onChange={(evento) => setEmail(evento.target.value)}
                    className="min-w-0 flex-1 rounded-lg border border-borde bg-superficie px-3 py-2 text-sm text-texto outline-none focus:border-borde-foco"
                />
                <Boton
                    type="submit"
                    cargando={enviando}
                    className="rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white hover:bg-primario-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {t('newsletter.formulario.suscribirme')}
                </Boton>
            </form>

            <div className="mt-2">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>
        </section>
    );
}
