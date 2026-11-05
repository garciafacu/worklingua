import { type FormEvent, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { alertasApi } from '../../../api/alertasApi';
import { departamentosApi, empresasApi } from '../../../api/empresasApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { EtiquetaEstado } from '../../../componentes/Basicos';
import { Boton, BotonAccion } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { AlertaResponse } from '../../../tipos/alertas';
import type { DepartamentoResponse } from '../../../tipos/empresas';
import type { EmpresaResponse } from '../../../tipos/usuarios';

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
    const [recarga, setRecarga] = useState(0);

    const puedeCrear = tienePermiso(PERMISOS.alertaAlta);
    const puedeEmitir = tienePermiso(PERMISOS.alertaEmitir);
    const puedeDarDeBaja = tienePermiso(PERMISOS.alertaBaja);
    const alcanceTotal = tienePermiso(PERMISOS.alertaVerTodasLasEmpresas);

    const mensajeDeError = useMensajeDeError();

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

            <AlertasResultado error={error} exito={exito} />

            {puedeCrear && !mostrandoFormulario && (
                <div className="mt-6">
                    <Boton type="button" onClick={abrirFormulario}>
                        {t('admin.alertas.nueva')}
                    </Boton>
                </div>
            )}

            {mostrandoFormulario && (
                <form onSubmit={guardar} className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
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
                        <BotonAccion grande onClick={cerrarFormulario}>
                            {t('admin.alertas.cancelar')}
                        </BotonAccion>
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
                                    <BotonAccion onClick={() => void emitir(fila)}>
                                        {t('admin.alertas.emitir')}
                                    </BotonAccion>
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
                                        <BotonAccion onClick={() => setConfirmandoBaja(null)}>
                                            {t('admin.alertas.cancelar')}
                                        </BotonAccion>
                                    </>
                                )}

                                {puedeDarDeBaja && !fila.activo && (
                                    <BotonAccion onClick={() => void cambiarEstado(fila, true)}>
                                        {t('admin.alertas.activar')}
                                    </BotonAccion>
                                )}
                            </div>
                        )}
                    />
                )}
            </div>
        </div>
    );
}
