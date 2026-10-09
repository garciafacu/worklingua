import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { autenticacionApi } from '../api/autenticacionApi';
import { ErrorApi } from '../api/clienteHttp';
import { Alerta } from '../componentes/Alerta';
import { Boton } from '../componentes/Boton';
import { CampoCaptcha } from '../componentes/CampoCaptcha';
import { CampoTexto } from '../componentes/CampoTexto';
import { TarjetaAuth } from '../componentes/TarjetaAuth';
import {
    conPlanPendiente,
    planIdDeParametros,
    recordarPlanAContratar,
} from '../rutas/contratacionPendiente';

interface FormularioRegistro {
    razonSocial: string;
    cuit: string;
    nombre: string;
    apellido: string;
    documento: string;
    email: string;
    clave: string;
    confirmacionClave: string;
}

const FORMULARIO_VACIO: FormularioRegistro = {
    razonSocial: '',
    cuit: '',
    nombre: '',
    apellido: '',
    documento: '',
    email: '',
    clave: '',
    confirmacionClave: '',
};

const LARGO_CUIT = 11;

/**
 * Registro público: da de alta la EMPRESA y su primer usuario.
 *
 * Ya no hay selector de empresa —antes se elegía una existente— porque quien
 * completa este formulario es una empresa que llega desde el sitio, no un
 * empleado de una empresa ya cargada. Los empleados entran por invitación desde
 * el Backoffice.
 *
 * Tampoco hay selector de rol: el rol lo asigna el servidor (Administrador
 * Empresa) y ni siquiera existe como campo del contrato, así que no puede pedirse
 * otro.
 *
 * Con `?contratar=<planId>` (plan elegido en el Catálogo) el plan se recuerda al
 * registrarse: la cuenta se confirma desde el correo y el login retoma la
 * contratación.
 */
export function Registro() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const planAContratar = planIdDeParametros(parametros);

    const [formulario, setFormulario] = useState<FormularioRegistro>(FORMULARIO_VACIO);
    const [erroresCampo, setErroresCampo] = useState<Partial<Record<keyof FormularioRegistro, string>>>({});
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(false);
    const [tokenCaptcha, setTokenCaptcha] = useState('');
    const [errorCaptcha, setErrorCaptcha] = useState<string | undefined>(undefined);
    const [reinicioCaptcha, setReinicioCaptcha] = useState(0);

    function actualizar(campo: keyof FormularioRegistro, valor: string) {
        setFormulario((anterior) => ({ ...anterior, [campo]: valor }));
        setErroresCampo((anterior) => ({ ...anterior, [campo]: undefined }));
    }

    function actualizarCaptcha(token: string) {
        setTokenCaptcha(token);
        setErrorCaptcha(undefined);
    }

    /**
     * Validaciones de UX, no de negocio: acá solo se evita un viaje al servidor
     * por errores evidentes. Las reglas reales (fortaleza de clave, correo o CUIT
     * ya registrados) las aplica la BLL y no se replican.
     */
    function validar(): boolean {
        const errores: Partial<Record<keyof FormularioRegistro, string>> = {};

        if (!formulario.razonSocial.trim()) {
            errores.razonSocial = t('auth.registro.validacion.razonSocial');
        }

        const cuit = formulario.cuit.trim();

        if (cuit.length !== LARGO_CUIT || !/^\d+$/.test(cuit)) {
            errores.cuit = t('auth.registro.validacion.cuit');
        }

        if (!formulario.nombre.trim()) errores.nombre = t('auth.registro.validacion.nombre');
        if (!formulario.apellido.trim()) errores.apellido = t('auth.registro.validacion.apellido');
        if (!formulario.email.trim()) errores.email = t('auth.registro.validacion.email');
        if (!formulario.clave) errores.clave = t('auth.registro.validacion.clave');

        if (formulario.clave && formulario.clave !== formulario.confirmacionClave) {
            errores.confirmacionClave = t('auth.clavesNoCoinciden');
        }

        setErroresCampo(errores);

        const faltaCaptcha = !tokenCaptcha;
        setErrorCaptcha(faltaCaptcha ? t('auth.registro.validacion.captcha') : undefined);

        return Object.keys(errores).length === 0 && !faltaCaptcha;
    }

    async function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);

        if (!validar()) {
            return;
        }

        setCargando(true);

        try {
            const respuesta = await autenticacionApi.registrar({
                razonSocial: formulario.razonSocial.trim(),
                cuit: formulario.cuit.trim(),
                nombre: formulario.nombre.trim(),
                apellido: formulario.apellido.trim(),
                documento: formulario.documento.trim() || undefined,
                email: formulario.email.trim(),
                clave: formulario.clave,
                tokenCaptcha,
            });

            if (planAContratar !== null) {
                recordarPlanAContratar(planAContratar);
            }

            setExito(respuesta.mensaje);
            setFormulario(FORMULARIO_VACIO);
        } catch (excepcion) {
            setError(excepcion instanceof ErrorApi ? excepcion.message : t('auth.registro.error'));
        } finally {
            setCargando(false);
            setReinicioCaptcha((anterior) => anterior + 1);
        }
    }

    return (
        <TarjetaAuth
            titulo={t('auth.registro.titulo')}
            descripcion={t('auth.registro.descripcion')}
            pie={
                <Link to={conPlanPendiente('/login', planAContratar)}>
                    {t('auth.registro.yaTengoCuenta')}
                </Link>
            }
        >
            <form onSubmit={manejarEnvio} noValidate>
                <Alerta
                    tipo="info"
                    mensaje={planAContratar === null ? null : t('auth.registro.contratacionPendiente')}
                />
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />

                <h2 className="mt-2 mb-1 text-sm font-semibold tracking-wide text-texto-suave uppercase">
                    {t('auth.registro.empresa.titulo')}
                </h2>
                <p className="ayuda">{t('auth.registro.empresa.ayuda')}</p>

                <CampoTexto
                    etiqueta={t('auth.registro.campo.razonSocial')}
                    identificador="razonSocial"
                    required
                    maxLength={150}
                    value={formulario.razonSocial}
                    error={erroresCampo.razonSocial}
                    onChange={(evento) => actualizar('razonSocial', evento.target.value)}
                />

                <CampoTexto
                    etiqueta={t('auth.registro.campo.cuit')}
                    identificador="cuit"
                    required
                    inputMode="numeric"
                    maxLength={LARGO_CUIT}
                    value={formulario.cuit}
                    error={erroresCampo.cuit}
                    onChange={(evento) => actualizar('cuit', evento.target.value)}
                />

                <p className="ayuda">{t('auth.registro.ayudaCuit')}</p>

                <h2 className="mt-6 mb-1 text-sm font-semibold tracking-wide text-texto-suave uppercase">
                    {t('auth.registro.persona.titulo')}
                </h2>

                <div className="fila">
                    <CampoTexto
                        etiqueta={t('comun.campo.nombre')}
                        identificador="nombre"
                        required
                        maxLength={80}
                        value={formulario.nombre}
                        error={erroresCampo.nombre}
                        onChange={(evento) => actualizar('nombre', evento.target.value)}
                    />
                    <CampoTexto
                        etiqueta={t('comun.campo.apellido')}
                        identificador="apellido"
                        required
                        maxLength={80}
                        value={formulario.apellido}
                        error={erroresCampo.apellido}
                        onChange={(evento) => actualizar('apellido', evento.target.value)}
                    />
                </div>

                <div className="fila">
                    <CampoTexto
                        etiqueta={t('auth.registro.campo.documento')}
                        identificador="documento"
                        maxLength={20}
                        value={formulario.documento}
                        onChange={(evento) => actualizar('documento', evento.target.value)}
                    />
                </div>

                <CampoTexto
                    etiqueta={t('comun.campo.email')}
                    identificador="email"
                    type="email"
                    autoComplete="username"
                    required
                    maxLength={150}
                    value={formulario.email}
                    error={erroresCampo.email}
                    onChange={(evento) => actualizar('email', evento.target.value)}
                />

                <CampoTexto
                    etiqueta={t('comun.campo.clave')}
                    identificador="clave"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={formulario.clave}
                    error={erroresCampo.clave}
                    onChange={(evento) => actualizar('clave', evento.target.value)}
                />

                <CampoTexto
                    etiqueta={t('auth.campo.repetirClave')}
                    identificador="confirmacionClave"
                    type="password"
                    autoComplete="new-password"
                    required
                    value={formulario.confirmacionClave}
                    error={erroresCampo.confirmacionClave}
                    onChange={(evento) => actualizar('confirmacionClave', evento.target.value)}
                />

                <p className="ayuda">{t('auth.ayudaClave')}</p>

                <CampoCaptcha
                    onCambio={actualizarCaptcha}
                    error={errorCaptcha}
                    reiniciar={reinicioCaptcha}
                />

                <Boton type="submit" cargando={cargando}>
                    {t('auth.registro.crear')}
                </Boton>
            </form>
        </TarjetaAuth>
    );
}
