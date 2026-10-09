import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { alertasApi } from '../../../api/alertasApi';
import { ErrorApi } from '../../../api/clienteHttp';
import { departamentosApi } from '../../../api/departamentosApi';
import { empresasApi } from '../../../api/empresasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoSelect } from '../../../componentes/CampoSelect';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { EtiquetaEstado } from '../../../componentes/EtiquetaEstado';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { EmpresaResponse } from '../../../tipos/autenticacion';
import type { DepartamentoResponse } from '../../../tipos/departamentos';
import type { AlertaResponse } from '../../../tipos/alertas';

const DIAS_POR_DEFECTO = 15;

function formularioInicial() {
    return {
        empresaId: '',
        departamentoId: '',
        titulo: '',
        mensaje: '',
        diasInactividad: String(DIAS_POR_DEFECTO),
    };
}

type Formulario = ReturnType<typeof formularioInicial>;

/**
 * Alertas preventivas de deserción (CU-001-010).
 *
 * Una alerta es una regla: título, mensaje y una condición de inactividad sobre
 * un grupo de empleados. Guardarla la emite en el acto, y queda en el panel
 * para volver a emitirla o desactivarla.
 *
 * Es la misma pantalla para los dos alcances: con Alerta.VerTodasLasEmpresas se
 * elige la empresa, y sin ese permiso el backend acota todo a la propia.
 *
 * El formulario muestra a cuántos empleados alcanzaría la condición antes de
 * emitir. Es una consulta de solo lectura: quien decide si hay destinatarios
 * sigue siendo el backend, que vuelve a verificarlo al guardar.
 */
export function AdminAlertas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearFecha, formatearNumero } = useLocalizacion();

    const [alertas, setAlertas] = useState<AlertaResponse[]>([]);
    const [empresas, setEmpresas] = useState<EmpresaResponse[]>([]);
    const [departamentos, setDepartamentos] = useState<DepartamentoResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(formularioInicial);
    const [mostrandoFormulario, setMostrandoFormulario] = useState(false);
    const [alcance, setAlcance] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    /** Fuerza releer el panel después de emitir o cambiar de estado. */
    const [recarga, setRecarga] = useState(0);

    const puedeCrear = tienePermiso(PERMISOS.alertaAlta);
    const puedeEmitir = tienePermiso(PERMISOS.alertaEmitir);
    const puedeDarDeBaja = tienePermiso(PERMISOS.alertaBaja);
    const alcanceTotal = tienePermiso(PERMISOS.alertaVerTodasLasEmpresas);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    useEffect(() => {
        if (!alcanceTotal) {
            return;
        }

        empresasApi.listar().then(setEmpresas).catch(() => setEmpresas([]));
    }, [alcanceTotal]);

    useEffect(() => {
        alertasApi
            .listar()
            .then((datos) => {
                setAlertas(datos);
                setError(null);
            })
            .catch((excepcion: unknown) => setError(mensajeDeError(excepcion, 'admin.alertas.errorCargar')))
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    // Los departamentos del selector siguen a la empresa elegida. Sin alcance
    // total el backend ya devuelve los de la empresa propia e ignora el id.
    const empresaElegida = formulario.empresaId === '' ? 0 : Number(formulario.empresaId);

    useEffect(() => {
        if (!mostrandoFormulario || (alcanceTotal && empresaElegida === 0)) {
            return;
        }

        departamentosApi
            .listarAsignables(empresaElegida)
            .then(setDepartamentos)
            .catch(() => setDepartamentos([]));
    }, [mostrandoFormulario, alcanceTotal, empresaElegida]);

    // El alcance se recalcula cuando cambian los criterios, no al tipear el
    // mensaje: es lo único que lo afecta.
    const diasElegidos = Number(formulario.diasInactividad);
    const departamentoElegido = formulario.departamentoId === '' ? 0 : Number(formulario.departamentoId);

    useEffect(() => {
        if (!mostrandoFormulario || diasElegidos < 1 || (alcanceTotal && empresaElegida === 0)) {
            return;
        }

        alertasApi
            .alcance(empresaElegida, departamentoElegido, diasElegidos)
            .then((empleados) => setAlcance(empleados.length))
            .catch(() => setAlcance(null));
    }, [mostrandoFormulario, alcanceTotal, empresaElegida, departamentoElegido, diasElegidos]);

    function recargar() {
        setRecarga((numero) => numero + 1);
    }

    function abrirFormulario() {
        setFormulario(formularioInicial());
        setAlcance(null);
        setError(null);
        setExito(null);
        setMostrandoFormulario(true);
    }

    function cerrarFormulario() {
        setMostrandoFormulario(false);
        setFormulario(formularioInicial());
        setAlcance(null);
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        try {
            const resultado = await alertasApi.crear({
                empresaId: empresaElegida,
                departamentoId: departamentoElegido === 0 ? null : departamentoElegido,
                titulo: formulario.titulo,
                mensaje: formulario.mensaje,
                diasInactividad: diasElegidos,
            });

            setExito(
                t('admin.alertas.exitoGuardar', { destinatarios: resultado.destinatarios }),
            );
            cerrarFormulario();
            recargar();
        } catch (excepcion: unknown) {
            setError(mensajeDeError(excepcion, 'admin.alertas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function emitir(alerta: AlertaResponse) {
        setError(null);
        setExito(null);

        try {
            const resultado = await alertasApi.emitir(alerta.alertaId);

            setExito(t('admin.alertas.exitoEmitir', { destinatarios: resultado.destinatarios }));
            recargar();
        } catch (excepcion: unknown) {
            setError(mensajeDeError(excepcion, 'admin.alertas.errorEmitir'));
        }
    }

    async function cambiarEstado(alerta: AlertaResponse, activo: boolean) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await alertasApi.cambiarEstado(alerta.alertaId, activo);

            setExito(
                activo ? t('admin.alertas.exitoActivar') : t('admin.alertas.exitoDesactivar'),
            );
            recargar();
        } catch (excepcion: unknown) {
            setError(mensajeDeError(excepcion, 'admin.alertas.errorEstado'));
        }
    }

    function destinoDe(alerta: AlertaResponse): string {
        return alerta.departamento ?? t('admin.alertas.departamento.todos');
    }

    const columnas: ColumnaAbm<AlertaResponse>[] = [
        { encabezado: t('admin.alertas.col.titulo'), celda: (fila) => fila.titulo },
        { encabezado: t('admin.alertas.col.destino'), celda: (fila) => destinoDe(fila) },
        {
            encabezado: t('admin.alertas.col.dias'),
            numerica: true,
            celda: (fila) => t('admin.alertas.dias.sufijo', { dias: fila.diasInactividad }),
        },
        {
            encabezado: t('admin.alertas.col.ultimaEmision'),
            celda: (fila) =>
                fila.ultimaEmision === null
                    ? t('admin.alertas.nunca')
                    : formatearFecha(fila.ultimaEmision, true),
        },
        {
            encabezado: t('admin.alertas.col.destinatarios'),
            numerica: true,
            celda: (fila) => formatearNumero(fila.destinatarios),
        },
        {
            encabezado: t('admin.alertas.col.estado'),
            celda: (fila) => (
                <EtiquetaEstado
                    tono={fila.activo ? 'exito' : 'neutro'}
                    texto={
                        fila.activo
                            ? t('admin.alertas.estado.activa')
                            : t('admin.alertas.estado.inactiva')
                    }
                />
            ),
        },
    ];

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {alcanceTotal ? t('admin.alertas.titulo') : t('admin.alertas.miEmpresa.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.alertas.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeCrear && !mostrandoFormulario && (
                <div className="mt-6">
                    <Boton type="button" onClick={abrirFormulario}>
                        {t('admin.alertas.nueva')}
                    </Boton>
                </div>
            )}

            {mostrandoFormulario && (
                <form onSubmit={guardar} className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    {/* Sin alcance global la empresa no se elige: el backend
                        impone la propia, así que el campo solo confundiría. */}
                    {alcanceTotal && (
                        <div className="fila">
                            <CampoSelect
                                etiqueta={t('comun.campo.empresa')}
                                identificador="alerta-empresa"
                                value={formulario.empresaId}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        empresaId: evento.target.value,
                                        departamentoId: '',
                                    })
                                }
                            >
                                <option value="">{t('comun.campo.empresa')}</option>
                                {empresas.map((empresa) => (
                                    <option key={empresa.empresaId} value={empresa.empresaId}>
                                        {empresa.razonSocial}
                                    </option>
                                ))}
                            </CampoSelect>
                        </div>
                    )}

                    <div className={alcanceTotal ? 'mt-4' : ''}>
                        <CampoTexto
                            etiqueta={t('admin.alertas.campo.titulo')}
                            identificador="alerta-titulo"
                            value={formulario.titulo}
                            maxLength={150}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, titulo: evento.target.value })
                            }
                        />
                    </div>

                    <div className="mt-4">
                        <label
                            htmlFor="alerta-mensaje"
                            className="block text-sm font-semibold text-texto"
                        >
                            {t('admin.alertas.campo.mensaje')}
                        </label>
                        <textarea
                            id="alerta-mensaje"
                            rows={3}
                            maxLength={2000}
                            value={formulario.mensaje}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, mensaje: evento.target.value })
                            }
                            className="mt-1 w-full rounded-lg border border-borde bg-superficie px-3 py-2 text-sm text-texto"
                        />
                    </div>

                    <div className="fila mt-4">
                        <CampoSelect
                            etiqueta={t('admin.alertas.departamento')}
                            identificador="alerta-departamento"
                            value={formulario.departamentoId}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, departamentoId: evento.target.value })
                            }
                        >
                            <option value="">{t('admin.alertas.departamento.todos')}</option>
                            {departamentos.map((departamento) => (
                                <option
                                    key={departamento.departamentoId}
                                    value={departamento.departamentoId}
                                >
                                    {departamento.nombre}
                                </option>
                            ))}
                        </CampoSelect>

                        <CampoTexto
                            etiqueta={t('admin.alertas.campo.dias')}
                            identificador="alerta-dias"
                            type="number"
                            min={1}
                            max={365}
                            value={formulario.diasInactividad}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, diasInactividad: evento.target.value })
                            }
                        />
                    </div>

                    {alcance !== null && (
                        <p className="mt-3 text-sm text-texto-suave" aria-live="polite">
                            {t('admin.alertas.alcance', { cantidad: alcance })}
                        </p>
                    )}

                    <div className="mt-4 flex flex-wrap gap-3">
                        <Boton type="submit" cargando={guardando}>
                            {t('admin.alertas.guardar')}
                        </Boton>
                        <button
                            type="button"
                            onClick={cerrarFormulario}
                            className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                        >
                            {t('admin.alertas.cancelar')}
                        </button>
                    </div>
                </form>
            )}

            <div className="mt-6">
                {cargando ? (
                    <p className="text-sm text-texto-suave">{t('admin.alertas.cargando')}</p>
                ) : (
                    <TablaAbm
                        columnas={columnas}
                        filas={alertas}
                        claveDe={(fila) => fila.alertaId}
                        inactiva={(fila) => !fila.activo}
                        mensajeVacio={t('admin.alertas.vacio')}
                        acciones={(fila) => (
                            <div className="flex flex-wrap justify-end gap-2">
                                {puedeEmitir && fila.activo && (
                                    <button
                                        type="button"
                                        onClick={() => void emitir(fila)}
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {t('admin.alertas.emitir')}
                                    </button>
                                )}

                                {puedeDarDeBaja && fila.activo && confirmandoBaja !== fila.alertaId && (
                                    <button
                                        type="button"
                                        onClick={() => setConfirmandoBaja(fila.alertaId)}
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-fondo"
                                    >
                                        {t('admin.alertas.desactivar')}
                                    </button>
                                )}

                                {puedeDarDeBaja && fila.activo && confirmandoBaja === fila.alertaId && (
                                    <>
                                        <span className="self-center text-sm text-texto-suave">
                                            {t('admin.alertas.confirmarDesactivar')}
                                        </span>
                                        <button
                                            type="button"
                                            onClick={() => void cambiarEstado(fila, false)}
                                            className="rounded-lg border border-error bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-fondo"
                                        >
                                            {t('admin.alertas.desactivar')}
                                        </button>
                                        <button
                                            type="button"
                                            onClick={() => setConfirmandoBaja(null)}
                                            className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                        >
                                            {t('admin.alertas.cancelar')}
                                        </button>
                                    </>
                                )}

                                {puedeDarDeBaja && !fila.activo && (
                                    <button
                                        type="button"
                                        onClick={() => void cambiarEstado(fila, true)}
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo"
                                    >
                                        {t('admin.alertas.activar')}
                                    </button>
                                )}
                            </div>
                        )}
                    />
                )}
            </div>
        </div>
    );
}
