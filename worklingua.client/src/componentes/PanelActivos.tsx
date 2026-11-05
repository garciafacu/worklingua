import { type FormEvent, useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../api/clienteHttp';
import { activosApi, progresoApi } from '../api/cursosApi';
import { useMensajeDeError } from '../hooks/useMensajeDeError';
import {
    type ActivoResponse,
    EXTENSIONES_POR_TIPO,
    MEGABYTES_MAXIMOS,
    type TipoActivo,
    TIPOS_ACTIVO,
} from '../tipos/cursos';
import type { ModuloResponse } from '../tipos/cursos';
import { Alerta } from './Alerta';
import { EtiquetaEstado } from './Basicos';
import { Boton, BotonAccion } from './Boton';
import { CampoArchivo, CampoSelect, CampoTexto } from './Campos';

const ESTADO_BORRADOR = 'BORRADOR';
const ESTADO_PUBLICADO = 'PUBLICADO';

interface PanelActivosProps {
    cursoId: number;
    puedeModificar: boolean;
}

export function PanelActivos({ cursoId, puedeModificar }: PanelActivosProps) {
    const { t } = useTranslation();

    const [activos, setActivos] = useState<ActivoResponse[]>([]);
    const [modulos, setModulos] = useState<ModuloResponse[]>([]);
    const [nombre, setNombre] = useState('');
    const [descripcion, setDescripcion] = useState('');
    const [moduloId, setModuloId] = useState('');
    const [tipo, setTipo] = useState<TipoActivo>('IMAGEN');
    const [archivo, setArchivo] = useState<File | null>(null);
    const [sugerencia, setSugerencia] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [recarga, setRecarga] = useState(0);
    const campoArchivo = useRef<HTMLInputElement>(null);

    const mensajeDeError = useMensajeDeError();

    useEffect(() => {
        Promise.all([activosApi.listar(cursoId), progresoApi.listarModulos(cursoId)])
            .then(([listaActivos, listaModulos]) => {
                setActivos(listaActivos);
                setModulos(listaModulos);
            })
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.cursos.activos.error')))
            .finally(() => setCargando(false));
    }, [cursoId, recarga, mensajeDeError]);

    function recargar() {
        setRecarga((numero) => numero + 1);
    }

    function limpiarFormulario() {
        setNombre('');
        setDescripcion('');
        setModuloId('');
        setArchivo(null);
        setSugerencia(null);

        if (campoArchivo.current) {
            campoArchivo.current.value = '';
        }
    }

    async function subir(confirmarNombre: boolean) {
        if (archivo === null) {
            setError(t('admin.cursos.activos.sinArchivo'));

            return;
        }

        setError(null);
        setExito(null);
        setGuardando(true);

        const formulario = new FormData();

        formulario.append('cursoId', String(cursoId));
        formulario.append('nombre', nombre);
        formulario.append('tipoContenido', tipo);
        formulario.append('descripcion', descripcion);
        formulario.append('confirmarNombre', String(confirmarNombre));
        formulario.append('archivo', archivo);

        if (moduloId !== '') {
            formulario.append('moduloId', moduloId);
        }

        try {
            await activosApi.crear(formulario);

            setExito(t('admin.cursos.activos.exito'));
            limpiarFormulario();
            recargar();
        } catch (excepcion: unknown) {
            if (excepcion instanceof ErrorApi && excepcion.sugerencia) {
                setSugerencia(excepcion.sugerencia);
                setError(null);
            } else {
                setSugerencia(null);
                setError(mensajeDeError(excepcion, 'admin.cursos.activos.errorGuardar'));
            }
        } finally {
            setGuardando(false);
        }
    }

    function agregar(evento: FormEvent) {
        evento.preventDefault();
        void subir(false);
    }

    async function cambiarPublicacion(activo: ActivoResponse) {
        setError(null);
        setExito(null);

        const destino = activo.estado === ESTADO_PUBLICADO ? ESTADO_BORRADOR : ESTADO_PUBLICADO;

        try {
            await activosApi.cambiarPublicacion(activo.activoPedagogicoId, destino);
            recargar();
        } catch (excepcion: unknown) {
            setError(mensajeDeError(excepcion, 'admin.cursos.activos.errorEstado'));
        }
    }

    async function cambiarEstado(activo: ActivoResponse, vigente: boolean) {
        setError(null);
        setExito(null);

        try {
            await activosApi.cambiarEstado(activo.activoPedagogicoId, vigente);
            recargar();
        } catch (excepcion: unknown) {
            setError(mensajeDeError(excepcion, 'admin.cursos.activos.errorEstado'));
        }
    }

    function etiquetaDeEstado(activo: ActivoResponse) {
        if (!activo.activo) {
            return <EtiquetaEstado tono="neutro" texto={t('admin.cursos.activos.estado.baja')} />;
        }

        return activo.estado === ESTADO_PUBLICADO ? (
            <EtiquetaEstado tono="exito" texto={t('admin.cursos.activos.estado.publicado')} />
        ) : (
            <EtiquetaEstado tono="alerta" texto={t('admin.cursos.activos.estado.borrador')} />
        );
    }

    return (
        <div className="flex flex-col gap-4">
            <p className="m-0 text-sm text-texto-suave">{t('admin.cursos.activos.descripcion')}</p>

            <Alerta tipo="error" mensaje={error} />
            <Alerta tipo="exito" mensaje={exito} />

            {cargando ? (
                <p className="text-sm text-texto-suave">{t('admin.cursos.activos.cargando')}</p>
            ) : activos.length === 0 ? (
                <p className="text-sm text-texto-suave">{t('admin.cursos.activos.vacio')}</p>
            ) : (
                <ul className="m-0 flex list-none flex-col gap-3 p-0">
                    {activos.map((activo) => (
                        <li
                            key={activo.activoPedagogicoId}
                            className={
                                activo.activo
                                    ? 'flex flex-wrap items-center justify-between gap-3 rounded-lg border border-borde p-3'
                                    : 'flex flex-wrap items-center justify-between gap-3 rounded-lg border border-borde p-3 opacity-60'
                            }
                        >
                            <div className="min-w-0">
                                <p className="m-0 text-sm font-semibold text-texto">{activo.nombre}</p>
                                <p className="mt-1 text-xs text-texto-suave">
                                    {t(`admin.cursos.activos.tipo.${activo.tipoContenido}`)}
                                    {' · '}
                                    {activo.modulo ?? t('admin.cursos.activos.modulo.curso')}
                                    {activo.descripcion ? ` · ${activo.descripcion}` : ''}
                                </p>
                            </div>

                            <div className="flex flex-wrap items-center gap-2">
                                {etiquetaDeEstado(activo)}

                                {activo.urlArchivo && (
                                    <a
                                        href={activo.urlArchivo}
                                        target="_blank"
                                        rel="noreferrer"
                                        className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto no-underline hover:bg-fondo"
                                    >
                                        {t('admin.cursos.activos.verArchivo')}
                                    </a>
                                )}

                                {puedeModificar && activo.activo && (
                                    <BotonAccion onClick={() => void cambiarPublicacion(activo)}>
                                        {activo.estado === ESTADO_PUBLICADO
                                            ? t('admin.cursos.activos.volverABorrador')
                                            : t('admin.cursos.activos.publicar')}
                                    </BotonAccion>
                                )}

                                {puedeModificar && activo.activo && (
                                    <BotonAccion
                                        variante="peligro"
                                        onClick={() => void cambiarEstado(activo, false)}
                                    >
                                        {t('admin.cursos.activos.darDeBaja')}
                                    </BotonAccion>
                                )}

                                {puedeModificar && !activo.activo && (
                                    <BotonAccion onClick={() => void cambiarEstado(activo, true)}>
                                        {t('admin.cursos.activos.reactivar')}
                                    </BotonAccion>
                                )}
                            </div>
                        </li>
                    ))}
                </ul>
            )}

            {puedeModificar && (
                <form onSubmit={agregar} noValidate className="flex flex-col gap-3 border-t border-borde pt-4">
                    <CampoTexto
                        etiqueta={t('admin.cursos.activos.nombre')}
                        identificador="activo-nombre"
                        required
                        maxLength={150}
                        value={nombre}
                        onChange={(evento) => {
                            setNombre(evento.target.value);
                            setSugerencia(null);
                        }}
                    />

                    <div className="fila">
                        <CampoSelect
                            etiqueta={t('admin.cursos.activos.tipo')}
                            identificador="activo-tipo"
                            value={tipo}
                            onChange={(evento) => {
                                setTipo(evento.target.value as TipoActivo);
                                setArchivo(null);

                                if (campoArchivo.current) {
                                    campoArchivo.current.value = '';
                                }
                            }}
                        >
                            {TIPOS_ACTIVO.map((valor) => (
                                <option key={valor} value={valor}>
                                    {t(`admin.cursos.activos.tipo.${valor}`)}
                                </option>
                            ))}
                        </CampoSelect>

                        <CampoTexto
                            etiqueta={t('admin.cursos.activos.descripcionCampo')}
                            identificador="activo-descripcion"
                            maxLength={500}
                            value={descripcion}
                            onChange={(evento) => setDescripcion(evento.target.value)}
                        />
                    </div>

                    {modulos.length > 0 && (
                        <CampoSelect
                            etiqueta={t('admin.cursos.activos.modulo')}
                            identificador="activo-modulo"
                            value={moduloId}
                            onChange={(evento) => setModuloId(evento.target.value)}
                        >
                            <option value="">{t('admin.cursos.activos.modulo.curso')}</option>
                            {modulos.map((modulo) => (
                                <option key={modulo.moduloId} value={modulo.moduloId}>
                                    {modulo.ordenModulo}. {modulo.nombre}
                                </option>
                            ))}
                        </CampoSelect>
                    )}

                    <CampoArchivo
                        etiqueta={t('admin.cursos.activos.archivo')}
                        identificador="activo-archivo"
                        ref={campoArchivo}
                        accept={EXTENSIONES_POR_TIPO[tipo]}
                        archivo={archivo?.name ?? null}
                        onChange={(evento) => {
                            setArchivo(evento.target.files?.[0] ?? null);
                            setSugerencia(null);
                        }}
                    />

                    <p className="m-0 text-xs text-texto-suave">
                        {t('admin.cursos.activos.archivo.ayuda', {
                            mb: MEGABYTES_MAXIMOS,
                            extensiones: EXTENSIONES_POR_TIPO[tipo],
                        })}
                    </p>

                    {sugerencia && (
                        <div className="rounded-lg border border-alerta bg-alerta-fondo p-3">
                            <p className="m-0 text-sm text-texto">
                                {t('admin.cursos.activos.sugerencia', { nombre: sugerencia })}
                            </p>
                            <div className="mt-3 flex flex-wrap gap-2">
                                <button
                                    type="button"
                                    onClick={() => void subir(true)}
                                    disabled={guardando}
                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-texto hover:bg-fondo disabled:opacity-50"
                                >
                                    {t('admin.cursos.activos.sugerencia.aceptar')}
                                </button>
                                <BotonAccion onClick={() => setSugerencia(null)}>
                                    {t('admin.cursos.activos.cancelarSugerencia')}
                                </BotonAccion>
                            </div>
                        </div>
                    )}

                    <div>
                        <Boton type="submit" cargando={guardando}>
                            {t('admin.cursos.activos.guardar')}
                        </Boton>
                    </div>
                </form>
            )}
        </div>
    );
}
