import { useState, type FormEvent } from 'react';
import { Trans, useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { contactoApi } from '../../api/contactoApi';
import { ErrorApi } from '../../api/clienteHttp';
import { Alerta } from '../../componentes/Alerta';
import { Boton } from '../../componentes/Boton';
import { CampoTexto } from '../../componentes/CampoTexto';
import type { EnviarConsultaContactoRequest } from '../../tipos/contacto';

/**
 * Los datos de contacto: la clave de la etiqueta se traduce, el valor no.
 *
 * Un correo, un teléfono y una dirección son los mismos en cualquier idioma.
 * Solo el horario lleva clave propia, porque "Lunes a viernes, de 9 a 18 h" sí
 * es una frase.
 */
const DATOS = [
    { etiqueta: 'publico.contacto.dato.email', valor: 'facundogarcia077@gmail.com' },
    { etiqueta: 'publico.contacto.dato.telefono', valor: '+54 11 2655-8986' },
    {
        etiqueta: 'publico.contacto.dato.direccion',
        valor: 'Av. Corrientes 1234, Ciudad Autónoma de Buenos Aires',
    },
    { etiqueta: 'publico.contacto.dato.horario', valorClave: 'publico.contacto.dato.horarioValor' },
];

const FORMULARIO_VACIO: EnviarConsultaContactoRequest = {
    nombre: '',
    email: '',
    asunto: '',
    mensaje: '',
};

export function Contacto() {
    const { t } = useTranslation();

    const [formulario, setFormulario] = useState<EnviarConsultaContactoRequest>(FORMULARIO_VACIO);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(false);

    function actualizar(campo: keyof EnviarConsultaContactoRequest, valor: string) {
        setFormulario((anterior) => ({ ...anterior, [campo]: valor }));
    }

    async function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setCargando(true);

        try {
            const respuesta = await contactoApi.enviar(formulario);
            setExito(respuesta.mensaje);
            setFormulario(FORMULARIO_VACIO);
        } catch (excepcion) {
            setError(
                excepcion instanceof ErrorApi
                    ? excepcion.message
                    : t('publico.contacto.errorEnviar'),
            );
        } finally {
            setCargando(false);
        }
    }

    return (
        <div className="mx-auto max-w-3xl px-4 py-12 sm:py-16">
            <h1 className="text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                {t('publico.contacto.titulo')}
            </h1>
            <p className="mt-3 text-base text-texto-suave">{t('publico.contacto.descripcion')}</p>
            <p className="mt-2 text-sm text-texto-suave">
                {/* `components` ata el marcador <1> de la traducción al enlace por
                    nombre, sin depender de la posición de los hijos. */}
                <Trans
                    i18nKey="publico.contacto.faq"
                    components={{
                        1: <Link to="/preguntas-frecuentes" className="text-primario hover:underline" />,
                    }}
                />
            </p>

            <dl className="mt-10 grid gap-6 sm:grid-cols-2">
                {DATOS.map((dato) => (
                    <div
                        key={dato.etiqueta}
                        className="rounded-2xl border border-borde bg-superficie p-5"
                    >
                        <dt className="text-xs font-semibold tracking-wide text-texto-suave uppercase">
                            {t(dato.etiqueta)}
                        </dt>
                        <dd className="mt-2 text-base text-texto">
                            {dato.valorClave ? t(dato.valorClave) : dato.valor}
                        </dd>
                    </div>
                ))}
            </dl>

            <form
                onSubmit={manejarEnvio}
                className="mt-10 rounded-2xl border border-borde bg-superficie p-6 sm:p-8"
                noValidate
            >
                <h2 className="text-xl font-semibold text-texto">
                    {t('publico.contacto.formulario.titulo')}
                </h2>
                <p className="mt-2 text-sm text-texto-suave">
                    {t('publico.contacto.formulario.descripcion')}
                </p>

                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />

                <div className="mt-6 grid gap-4 sm:grid-cols-2">
                    <CampoTexto
                        etiqueta={t('comun.campo.nombre')}
                        identificador="nombre"
                        required
                        value={formulario.nombre}
                        onChange={(evento) => actualizar('nombre', evento.target.value)}
                    />

                    <CampoTexto
                        etiqueta={t('comun.campo.email')}
                        identificador="email"
                        type="email"
                        required
                        value={formulario.email}
                        onChange={(evento) => actualizar('email', evento.target.value)}
                    />
                </div>

                <div className="mt-4">
                    <CampoTexto
                        etiqueta={t('publico.contacto.formulario.asunto')}
                        identificador="asunto"
                        required
                        value={formulario.asunto}
                        onChange={(evento) => actualizar('asunto', evento.target.value)}
                    />
                </div>

                <div className="campo mt-4">
                    <label htmlFor="mensaje">{t('publico.contacto.formulario.mensaje')}</label>
                    <textarea
                        id="mensaje"
                        name="mensaje"
                        rows={5}
                        required
                        value={formulario.mensaje}
                        onChange={(evento) => actualizar('mensaje', evento.target.value)}
                        className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                    />
                </div>

                <Boton
                    type="submit"
                    cargando={cargando}
                    className="mt-6 rounded-lg bg-primario px-5 py-2.5 text-sm font-semibold text-white hover:bg-primario-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {t('publico.contacto.formulario.enviar')}
                </Boton>
            </form>
        </div>
    );
}
