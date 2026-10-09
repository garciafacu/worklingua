import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { culturasApi } from '../../../api/culturasApi';
import { idiomasApi } from '../../../api/idiomasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import type { CulturaAdminResponse, GuardarCulturaRequest } from '../../../tipos/culturas';
import type { IdiomaAdminResponse } from '../../../tipos/idiomas';

const FORMULARIO_VACIO = {
    codigo: '',
    nombre: '',
    idiomaId: '',
    moneda: '',
    simboloMoneda: '',
    formatoFecha: 'dd/MM/yyyy',
    separadorDecimal: ',',
    separadorMiles: '.',
    tasaConversion: '1',
    esPredeterminada: false,
};

type Formulario = typeof FORMULARIO_VACIO;

/**
 * ABM de las culturas: la configuración regional de cada idioma de plataforma.
 *
 * Define con qué locale, moneda, formato de fecha y separadores numéricos se
 * muestra el sitio cuando alguien elige ese idioma.
 *
 * **Sobre la tasa de conversión.** Los precios se guardan una sola vez, en la
 * moneda base (pesos argentinos). Cada cultura los multiplica por su tasa antes
 * de mostrarlos. La tasa se carga a mano acá: no hay integración con ninguna API
 * de cotización, así que si nadie la actualiza envejece y los importes en otra
 * moneda dejan de ser fieles.
 */
export function AdminCulturas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const [culturas, setCulturas] = useState<CulturaAdminResponse[]>([]);
    const [idiomas, setIdiomas] = useState<IdiomaAdminResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Cultura.Alta');
    const puedeModificar = tienePermiso('Cultura.Modificar');
    const puedeDarDeBaja = tienePermiso('Cultura.Baja');

    // En useCallback para poder ir en las dependencias de los efectos que lo
    // usan: sin eso el linter avisa, y el mensaje de error quedaría en el idioma
    // anterior después de cambiar de idioma.
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        idiomasApi.listarAdministracion().then(setIdiomas).catch(() => setIdiomas([]));
    }, []);

    useEffect(() => {
        culturasApi
            .listarAdministracion()
            .then(setCulturas)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.culturas.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(cultura: CulturaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(cultura.culturaId);
        setFormulario({
            codigo: cultura.codigo,
            nombre: cultura.nombre,
            idiomaId: String(cultura.idiomaId),
            moneda: cultura.moneda,
            simboloMoneda: cultura.simboloMoneda,
            formatoFecha: cultura.formatoFecha,
            separadorDecimal: cultura.separadorDecimal,
            separadorMiles: cultura.separadorMiles,
            tasaConversion: String(cultura.tasaConversion),
            esPredeterminada: cultura.esPredeterminada,
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: GuardarCulturaRequest = {
            codigo: formulario.codigo.trim(),
            nombre: formulario.nombre.trim(),
            idiomaId: Number(formulario.idiomaId),
            moneda: formulario.moneda.trim().toUpperCase(),
            simboloMoneda: formulario.simboloMoneda.trim(),
            formatoFecha: formulario.formatoFecha.trim(),
            separadorDecimal: formulario.separadorDecimal,
            separadorMiles: formulario.separadorMiles,
            tasaConversion: Number(formulario.tasaConversion),
            esPredeterminada: formulario.esPredeterminada,
        };

        try {
            if (editandoId === null) {
                await culturasApi.crear(cuerpo);
                setExito(t('admin.culturas.exitoAlta', { codigo: cuerpo.codigo }));
            } else {
                await culturasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.culturas.exitoModificacion', { codigo: cuerpo.codigo }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.culturas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function cambiarEstado(cultura: CulturaAdminResponse) {
        setError(null);
        setExito(null);

        try {
            await culturasApi.cambiarEstado(cultura.culturaId, !cultura.activo);
            setExito(
                cultura.activo
                    ? t('admin.culturas.exitoDesactivar', { codigo: cultura.codigo })
                    : t('admin.culturas.exitoActivar', { codigo: cultura.codigo }),
            );

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.culturas.errorEstado'));
        }
    }

    async function darDeBaja(cultura: CulturaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await culturasApi.baja(cultura.culturaId);
            setExito(t('admin.culturas.exitoBaja', { codigo: cultura.codigo }));

            if (editandoId === cultura.culturaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.culturas.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<CulturaAdminResponse>[] = [
        { encabezado: t('admin.culturas.tabla.codigo'), celda: (fila) => fila.codigo },
        { encabezado: t('comun.campo.nombre'), celda: (fila) => fila.nombre },
        { encabezado: t('comun.campo.idioma'), celda: (fila) => fila.idioma },
        {
            encabezado: t('admin.culturas.tabla.moneda'),
            celda: (fila) => `${fila.simboloMoneda} ${fila.moneda}`,
        },
        { encabezado: t('admin.culturas.tabla.fecha'), celda: (fila) => fila.formatoFecha },
        {
            encabezado: t('admin.culturas.tabla.tasa'),
            numerica: true,
            celda: (fila) => fila.tasaConversion,
        },
        {
            encabezado: t('comun.campo.estado'),
            celda: (fila) => (fila.activo ? t('comun.estado.activo') : t('comun.estado.inactivo')),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.culturas.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.culturas.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.culturas.formulario.nueva')
                            : t('admin.culturas.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.culturas.tabla.codigo')}
                                identificador="codigo"
                                required
                                maxLength={10}
                                placeholder="es-AR"
                                value={formulario.codigo}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, codigo: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('comun.campo.nombre')}
                                identificador="nombreCultura"
                                required
                                maxLength={100}
                                value={formulario.nombre}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nombre: evento.target.value })
                                }
                            />

                            <CampoSelect
                                etiqueta={t('admin.culturas.formulario.idioma')}
                                identificador="idiomaCultura"
                                required
                                value={formulario.idiomaId}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, idiomaId: evento.target.value })
                                }
                            >
                                <option value="">
                                    {t('admin.culturas.formulario.elegirIdioma')}
                                </option>
                                {idiomas.map((idioma) => (
                                    <option key={idioma.idiomaId} value={idioma.idiomaId}>
                                        {idioma.nombre}
                                    </option>
                                ))}
                            </CampoSelect>
                        </div>

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.culturas.formulario.moneda')}
                                identificador="moneda"
                                required
                                maxLength={50}
                                placeholder="ARS"
                                value={formulario.moneda}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, moneda: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.culturas.formulario.simbolo')}
                                identificador="simboloMoneda"
                                required
                                maxLength={10}
                                placeholder="$"
                                value={formulario.simboloMoneda}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        simboloMoneda: evento.target.value,
                                    })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.culturas.formulario.formatoFecha')}
                                identificador="formatoFecha"
                                required
                                maxLength={30}
                                placeholder="dd/MM/yyyy"
                                value={formulario.formatoFecha}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        formatoFecha: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.culturas.formulario.separadorDecimal')}
                                identificador="separadorDecimal"
                                required
                                maxLength={1}
                                value={formulario.separadorDecimal}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        separadorDecimal: evento.target.value,
                                    })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.culturas.formulario.separadorMiles')}
                                identificador="separadorMiles"
                                required
                                maxLength={1}
                                value={formulario.separadorMiles}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        separadorMiles: evento.target.value,
                                    })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.culturas.formulario.tasa')}
                                identificador="tasaConversion"
                                type="number"
                                min={0.000001}
                                step="0.000001"
                                required
                                value={formulario.tasaConversion}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        tasaConversion: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        <p className="ayuda">{t('admin.culturas.formulario.ayudaTasa')}</p>

                        <label className="flex items-center gap-2 text-sm text-texto">
                            <input
                                type="checkbox"
                                checked={formulario.esPredeterminada}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        esPredeterminada: evento.target.checked,
                                    })
                                }
                                className="h-4 w-4"
                            />
                            {t('admin.culturas.formulario.predeterminada')}
                        </label>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.culturas.formulario.crear')
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
                <h2 className="text-lg font-semibold text-texto">{t('admin.culturas.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.culturas.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={culturas}
                            claveDe={(fila) => fila.culturaId}
                            inactiva={(fila) => !fila.activo}
                            mensajeVacio={t('admin.culturas.vacio')}
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

                                    {puedeModificar && (
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
                                        (confirmandoBaja === fila.culturaId ? (
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
                                                onClick={() => setConfirmandoBaja(fila.culturaId)}
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
