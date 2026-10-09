import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { idiomasApi } from '../../../api/idiomasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import { IDIOMA_BASE } from '../../../i18n/configuracion';
import type { GuardarIdiomaRequest, IdiomaAdminResponse } from '../../../tipos/idiomas';

const FORMULARIO_VACIO = {
    nombre: '',
    codigoISO: '',
};

type Formulario = typeof FORMULARIO_VACIO;

/**
 * ABM de los idiomas de la PLATAFORMA: aquellos en los que se puede mostrar la
 * interfaz de WorkLingua.
 *
 * Este catálogo no tiene nada que ver con lo que enseña un curso, que es texto
 * libre en el propio curso. Ver `docs/modelo-datos.md` (Idioma).
 *
 * A diferencia del resto del Backoffice, acá **sí se muestran los inactivos**:
 * desactivar un idioma lo saca del selector del sitio pero conserva todas sus
 * traducciones, y tiene que poder reactivarse. Es la excepción documentada a la
 * regla de la Etapa 6.
 *
 * Al crear un idioma nuevo, el backend le genera automáticamente todas las
 * claves de traducción existentes, en blanco. Hasta que alguien las complete
 * desde el ABM de Traducciones, el sitio las muestra en español por fallback.
 */
export function AdminIdiomas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const [idiomas, setIdiomas] = useState<IdiomaAdminResponse[]>([]);
    const [busqueda, setBusqueda] = useState('');
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Fuerza releer el listado despues de guardar, activar o dar de baja. */
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Idioma.Alta');
    const puedeModificar = tienePermiso('Idioma.Modificar');
    const puedeDarDeBaja = tienePermiso('Idioma.Baja');

    // En useCallback para poder ir en las dependencias de los efectos que lo
    // usan: sin eso el linter avisa, y el mensaje de error quedaría en el idioma
    // anterior después de cambiar de idioma.
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        idiomasApi
            .listarAdministracion()
            .then(setIdiomas)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.idiomas.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    // El filtro es en cliente a propósito: el catálogo son decenas de filas y ya
    // están todas en memoria. Un endpoint de búsqueda sería un viaje de más.
    const visibles = useMemo(() => {
        const termino = busqueda.trim().toLowerCase();

        if (termino === '') {
            return idiomas;
        }

        return idiomas.filter(
            (idioma) =>
                idioma.nombre.toLowerCase().includes(termino) ||
                idioma.codigoISO.toLowerCase().includes(termino),
        );
    }, [idiomas, busqueda]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(idioma: IdiomaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(idioma.idiomaId);
        setFormulario({ nombre: idioma.nombre, codigoISO: idioma.codigoISO });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: GuardarIdiomaRequest = {
            nombre: formulario.nombre.trim(),
            codigoISO: formulario.codigoISO.trim().toLowerCase(),
        };

        try {
            if (editandoId === null) {
                await idiomasApi.crear(cuerpo);
                setExito(t('admin.idiomas.exitoAlta', { nombre: cuerpo.nombre }));
            } else {
                await idiomasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.idiomas.exitoModificacion', { nombre: cuerpo.nombre }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.idiomas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function cambiarEstado(idioma: IdiomaAdminResponse) {
        setError(null);
        setExito(null);

        try {
            await idiomasApi.cambiarEstado(idioma.idiomaId, !idioma.activo);
            setExito(
                idioma.activo
                    ? t('admin.idiomas.exitoDesactivar', { nombre: idioma.nombre })
                    : t('admin.idiomas.exitoActivar', { nombre: idioma.nombre }),
            );

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.idiomas.errorEstado'));
        }
    }

    async function darDeBaja(idioma: IdiomaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await idiomasApi.baja(idioma.idiomaId);
            setExito(t('admin.idiomas.exitoBaja', { nombre: idioma.nombre }));

            if (editandoId === idioma.idiomaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.idiomas.errorBaja'));
        }
    }

    /** El idioma base no se puede desactivar ni dar de baja: es el del fallback. */
    function esIdiomaBase(idioma: IdiomaAdminResponse): boolean {
        return idioma.codigoISO.toLowerCase() === IDIOMA_BASE;
    }

    const columnas: ColumnaAbm<IdiomaAdminResponse>[] = [
        { encabezado: t('comun.campo.idioma'), celda: (fila) => fila.nombre },
        { encabezado: t('admin.idiomas.tabla.codigo'), celda: (fila) => fila.codigoISO },
        {
            encabezado: t('comun.campo.estado'),
            celda: (fila) => (
                <span
                    className={
                        fila.activo
                            ? 'rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario'
                            : 'rounded-full bg-fondo px-2.5 py-0.5 text-xs font-semibold text-texto-suave'
                    }
                >
                    {fila.activo ? t('comun.estado.activo') : t('comun.estado.inactivo')}
                </span>
            ),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.idiomas.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.idiomas.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.idiomas.formulario.nuevo')
                            : t('admin.idiomas.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombre"
                                required
                                maxLength={60}
                                value={formulario.nombre}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nombre: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.idiomas.tabla.codigo')}
                                identificador="codigoISO"
                                required
                                maxLength={5}
                                placeholder="es"
                                value={formulario.codigoISO}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, codigoISO: evento.target.value })
                                }
                            />
                        </div>

                        <p className="ayuda">{t('admin.idiomas.formulario.ayudaCodigo')}</p>

                        {editandoId === null && (
                            <p className="ayuda">{t('admin.idiomas.formulario.ayudaAlta')}</p>
                        )}

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.idiomas.formulario.crear')
                                    : t('comun.boton.guardarCambios')}
                            </Boton>

                            {editandoId !== null && (
                                <button
                                    type="button"
                                    onClick={limpiar}
                                    className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                                >
                                    {t('comun.boton.cancelar')}
                                </button>
                            )}
                        </div>
                    </form>
                </section>
            )}

            <section className="mt-6">
                <div className="flex flex-wrap items-end justify-between gap-4">
                    <h2 className="text-lg font-semibold text-texto">
                        {t('admin.idiomas.listado')}
                    </h2>

                    <div className="w-full sm:w-72">
                        <CampoTexto
                            etiqueta={t('admin.idiomas.buscar')}
                            identificador="busquedaIdioma"
                            type="search"
                            placeholder={t('admin.idiomas.buscarPlaceholder')}
                            value={busqueda}
                            onChange={(evento) => setBusqueda(evento.target.value)}
                        />
                    </div>
                </div>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.idiomas.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={visibles}
                            claveDe={(fila) => fila.idiomaId}
                            inactiva={(fila) => !fila.activo}
                            mensajeVacio={
                                busqueda.trim() === ''
                                    ? t('admin.idiomas.vacio')
                                    : t('admin.idiomas.sinCoincidencias')
                            }
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    {puedeModificar && (
                                        <button
                                            type="button"
                                            onClick={() => editar(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                        >
                                            {t('comun.boton.editar')}
                                        </button>
                                    )}

                                    {puedeModificar && !esIdiomaBase(fila) && (
                                        <button
                                            type="button"
                                            onClick={() => void cambiarEstado(fila)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-primario hover:bg-info-fondo"
                                        >
                                            {fila.activo
                                                ? t('comun.boton.desactivar')
                                                : t('comun.boton.activar')}
                                        </button>
                                    )}

                                    {puedeDarDeBaja &&
                                        fila.activo &&
                                        !esIdiomaBase(fila) &&
                                        (confirmandoBaja === fila.idiomaId ? (
                                            <>
                                                <button
                                                    type="button"
                                                    onClick={() => void darDeBaja(fila)}
                                                    className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                                >
                                                    {t('comun.boton.confirmarBaja')}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => setConfirmandoBaja(null)}
                                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                                >
                                                    {t('comun.boton.no')}
                                                </button>
                                            </>
                                        ) : (
                                            <button
                                                type="button"
                                                onClick={() => setConfirmandoBaja(fila.idiomaId)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                            >
                                                {t('comun.boton.darDeBaja')}
                                            </button>
                                        ))}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
