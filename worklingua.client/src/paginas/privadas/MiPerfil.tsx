import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { usuariosApi } from '../../api/usuariosApi';
import { Alerta } from '../../componentes/Alerta';
import { Boton } from '../../componentes/Boton';
import { CampoTexto } from '../../componentes/CampoTexto';
import { useSesion } from '../../contexto/useSesion';
import type { PerfilResponse } from '../../tipos/perfil';

const CLASE_SECCION = 'mt-6 rounded-2xl border border-borde bg-superficie p-6';

/**
 * Fecha local de hoy en `yyyy-MM-dd`, el formato de `<input type="date">`. No
 * se usa `toISOString` porque es UTC: en Argentina, de noche, daría mañana.
 */
function hoy(): string {
    const fecha = new Date();
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${fecha.getFullYear()}-${mes}-${dia}`;
}

/**
 * Datos que el usuario no puede cambiar desde acá. El correo es su usuario de
 * ingreso y el documento es único: los modifica un administrador.
 */
function DatosCuenta({ perfil }: { perfil: PerfilResponse }) {
    const { t } = useTranslation();
    const filas: [string, string][] = [
        [t('comun.campo.email'), perfil.email],
        [t('privado.perfil.documento'), perfil.documento ?? t('privado.perfil.sinDato')],
        [t('comun.campo.empresa'), perfil.empresa],
        [t('privado.perfil.departamento'), perfil.departamento ?? t('privado.perfil.sinDato')],
        [t('comun.campo.rol'), perfil.roles.join(', ') || t('privado.perfil.sinDato')],
    ];

    return (
        <section className={CLASE_SECCION}>
            <h2 className="text-lg font-semibold text-texto">{t('privado.perfil.datosCuenta')}</h2>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.perfil.soloLectura')}</p>

            <dl className="mt-4 grid gap-x-6 gap-y-3 text-sm sm:grid-cols-[auto_1fr]">
                {filas.map(([etiqueta, valor]) => (
                    <div key={etiqueta} className="contents">
                        <dt className="font-semibold text-texto-suave">{etiqueta}</dt>
                        <dd className="m-0 break-words text-texto">{valor}</dd>
                    </div>
                ))}
            </dl>
        </section>
    );
}

/**
 * Mi perfil: autogestión de los datos personales (CU-001-003).
 *
 * Solo se editan nombre, apellido y fecha de nacimiento. Al guardar se relee la
 * sesión para que el nombre de la barra superior quede al día.
 */
export function MiPerfil() {
    const { t } = useTranslation();
    const { refrescarSesion } = useSesion();

    const [perfil, setPerfil] = useState<PerfilResponse | null>(null);
    const [nombre, setNombre] = useState('');
    const [apellido, setApellido] = useState('');
    const [fechaNacimiento, setFechaNacimiento] = useState('');
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    const aplicarPerfil = useCallback((datos: PerfilResponse) => {
        setPerfil(datos);
        setNombre(datos.nombre);
        setApellido(datos.apellido);
        setFechaNacimiento(datos.fechaNacimiento ?? '');
    }, []);

    useEffect(() => {
        usuariosApi
            .obtenerPerfil()
            .then(aplicarPerfil)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'privado.perfil.errorCargar')))
            .finally(() => setCargando(false));
    }, [aplicarPerfil, mensajeDeError]);

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        try {
            const respuesta = await usuariosApi.modificarPerfil({
                nombre,
                apellido,
                fechaNacimiento: fechaNacimiento || null,
            });

            aplicarPerfil(await usuariosApi.obtenerPerfil());
            setExito(respuesta.mensaje);
            await refrescarSesion();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'privado.perfil.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    return (
        <div className="mx-auto max-w-4xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('privado.perfil.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('privado.perfil.descripcion')}</p>

            <div className="mt-6 space-y-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {cargando && (
                <p className="mt-6 text-sm text-texto-suave">{t('privado.perfil.cargando')}</p>
            )}

            {perfil && (
                <>
                    <section className={CLASE_SECCION}>
                        <h2 className="text-lg font-semibold text-texto">
                            {t('privado.perfil.datosPersonales')}
                        </h2>

                        <form onSubmit={guardar} noValidate className="mt-4">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombre"
                                autoComplete="given-name"
                                required
                                maxLength={80}
                                value={nombre}
                                onChange={(evento) => setNombre(evento.target.value)}
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.apellido')}
                                identificador="apellido"
                                autoComplete="family-name"
                                required
                                maxLength={80}
                                value={apellido}
                                onChange={(evento) => setApellido(evento.target.value)}
                            />

                            <CampoTexto
                                etiqueta={t('privado.perfil.fechaNacimiento')}
                                identificador="fechaNacimiento"
                                type="date"
                                autoComplete="bday"
                                min="1900-01-01"
                                max={hoy()}
                                value={fechaNacimiento}
                                onChange={(evento) => setFechaNacimiento(evento.target.value)}
                            />

                            <Boton
                                type="submit"
                                cargando={guardando}
                                disabled={!nombre.trim() || !apellido.trim()}
                            >
                                {t('privado.perfil.guardar')}
                            </Boton>
                        </form>
                    </section>

                    <DatosCuenta perfil={perfil} />

                    <section className={CLASE_SECCION}>
                        <h2 className="text-lg font-semibold text-texto">
                            {t('privado.perfil.seguridad')}
                        </h2>
                        <p className="mt-2 text-sm text-texto-suave">
                            {t('privado.panel.cuentaDetalle')}
                        </p>

                        <Link
                            to="/inicio/cambiar-clave"
                            className="mt-4 inline-block rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white no-underline hover:bg-primario-hover"
                        >
                            {t('privado.panel.cambiarClave')}
                        </Link>
                    </section>
                </>
            )}
        </div>
    );
}
