import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../api/clienteHttp';
import { progresoApi } from '../api/progresoApi';
import type { ModuloResponse } from '../tipos/progreso';
import { Alerta } from './Alerta';
import { Boton } from './Boton';
import { CampoTexto } from './CampoTexto';

interface PanelModulosProps {
    cursoId: number;
    puedeModificar: boolean;
}

/**
 * Módulos de un curso, para el ABM de Cursos. Son las unidades sobre las que
 * cada usuario registra su avance (Mis cursos). Un módulo no se edita: se da de
 * baja y se vuelve a cargar, así el avance ya registrado no cambia de sentido.
 */
export function PanelModulos({ cursoId, puedeModificar }: PanelModulosProps) {
    const { t } = useTranslation();

    const [modulos, setModulos] = useState<ModuloResponse[]>([]);
    const [nombre, setNombre] = useState('');
    /** El objetivo del módulo. */
    const [descripcion, setDescripcion] = useState('');
    /** El texto de la lección. */
    const [contenido, setContenido] = useState('');
    const [orden, setOrden] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [guardando, setGuardando] = useState(false);
    const [recarga, setRecarga] = useState(0);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        progresoApi
            .listarModulos(cursoId)
            .then(setModulos)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.cursos.modulos.error')))
            .finally(() => setCargando(false));
    }, [cursoId, recarga, mensajeDeError]);

    const siguienteOrden = modulos.reduce((maximo, modulo) => Math.max(maximo, modulo.ordenModulo), 0) + 1;

    async function agregar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setGuardando(true);

        try {
            await progresoApi.crearModulo(cursoId, {
                nombre: nombre.trim(),
                descripcion: descripcion.trim() || null,
                contenido: contenido.trim() || null,
                ordenModulo: orden ? Number(orden) : siguienteOrden,
            });
            setNombre('');
            setDescripcion('');
            setContenido('');
            setOrden('');
            setRecarga((numero) => numero + 1);
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.modulos.error'));
        } finally {
            setGuardando(false);
        }
    }

    async function quitar(moduloId: number) {
        setError(null);

        try {
            await progresoApi.bajaModulo(cursoId, moduloId);
            setRecarga((numero) => numero + 1);
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.modulos.error'));
        }
    }

    return (
        <div className="flex flex-col gap-4">
            <p className="m-0 text-sm text-texto-suave">{t('admin.cursos.modulos.descripcion')}</p>

            <Alerta tipo="error" mensaje={error} />

            {cargando ? (
                <p className="m-0 text-sm text-texto-suave">{t('admin.cursos.modulos.cargando')}</p>
            ) : modulos.length === 0 ? (
                <p className="m-0 text-sm text-texto-suave">{t('admin.cursos.modulos.vacio')}</p>
            ) : (
                <ol className="m-0 flex list-none flex-col divide-y divide-borde rounded-xl border border-borde p-0">
                    {modulos.map((modulo) => (
                        <li key={modulo.moduloId} className="flex items-center justify-between gap-3 px-4 py-2.5">
                            <div className="min-w-0">
                                <p className="m-0 text-sm font-medium text-texto">
                                    {modulo.ordenModulo}. {modulo.nombre}
                                </p>
                                {modulo.descripcion && (
                                    <p className="m-0 text-xs text-texto-suave">{modulo.descripcion}</p>
                                )}
                            </div>
                            {puedeModificar && (
                                <button
                                    type="button"
                                    onClick={() => void quitar(modulo.moduloId)}
                                    className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                >
                                    {t('comun.boton.quitar')}
                                </button>
                            )}
                        </li>
                    ))}
                </ol>
            )}

            {puedeModificar && (
                <form onSubmit={agregar} noValidate className="flex flex-col gap-3 border-t border-borde pt-4">
                    <h3 className="m-0 text-sm font-semibold text-texto">{t('admin.cursos.modulos.nuevo')}</h3>
                    <CampoTexto
                        etiqueta={t('admin.cursos.modulos.nombre')}
                        identificador="modulo-nombre"
                        required
                        maxLength={150}
                        value={nombre}
                        onChange={(evento) => setNombre(evento.target.value)}
                    />
                    <div className="fila">
                        <CampoTexto
                            etiqueta={t('admin.cursos.modulos.objetivo')}
                            identificador="modulo-objetivo"
                            maxLength={500}
                            value={descripcion}
                            onChange={(evento) => setDescripcion(evento.target.value)}
                        />
                        <CampoTexto
                            etiqueta={t('admin.cursos.modulos.orden')}
                            identificador="modulo-orden"
                            type="number"
                            min={1}
                            placeholder={String(siguienteOrden)}
                            value={orden}
                            onChange={(evento) => setOrden(evento.target.value)}
                        />
                    </div>

                    {/* El contenido es la lección: lo que el empleado lee al
                        abrir el módulo, con o sin archivos adjuntos. */}
                    <div>
                        <label
                            htmlFor="modulo-contenido"
                            className="block text-sm font-semibold text-texto"
                        >
                            {t('admin.cursos.modulos.contenido')}
                        </label>
                        <textarea
                            id="modulo-contenido"
                            rows={6}
                            maxLength={8000}
                            value={contenido}
                            onChange={(evento) => setContenido(evento.target.value)}
                            className="mt-1 w-full rounded-lg border border-borde bg-superficie px-3 py-2 text-sm text-texto"
                        />
                        <p className="mt-1 mb-0 text-xs text-texto-suave">
                            {t('admin.cursos.modulos.contenido.ayuda')}
                        </p>
                    </div>
                    <div>
                        <Boton type="submit" cargando={guardando}>
                            {t('admin.cursos.modulos.agregar')}
                        </Boton>
                    </div>
                </form>
            )}
        </div>
    );
}
