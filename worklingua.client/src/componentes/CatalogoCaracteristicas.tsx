import { type FormEvent, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { caracteristicasApi } from '../api/planesApi';
import { useSesion } from '../contexto/useSesion';
import { useMensajeDeError } from '../hooks/useMensajeDeError';
import type { CaracteristicaAdminResponse, GuardarCaracteristicaRequest } from '../tipos/planes';
import { Alerta } from './Alerta';
import { BotonAccion, BotonesFormulario, ConfirmarBaja } from './Boton';
import { CampoTexto } from './Campos';
import { type ColumnaAbm, TablaAbm } from './TablaAbm';

const FORMULARIO_VACIO = {
    nombre: '',
    orden: '',
};

type Formulario = typeof FORMULARIO_VACIO;

const PASO_ORDEN = 10;

interface CatalogoCaracteristicasProps {
    alCambiar: () => void;
}

export function CatalogoCaracteristicas({ alCambiar }: CatalogoCaracteristicasProps) {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const mensajeDeError = useMensajeDeError();

    const [caracteristicas, setCaracteristicas] = useState<CaracteristicaAdminResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Caracteristica.Alta');
    const puedeModificar = tienePermiso('Caracteristica.Modificar');
    const puedeDarDeBaja = tienePermiso('Caracteristica.Baja');

    useEffect(() => {
        caracteristicasApi
            .listarAdministracion()
            .then(setCaracteristicas)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.caracteristicas.errorCargar')),
            )
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
        alCambiar();
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        setEditandoId(null);
    }

    function editar(caracteristica: CaracteristicaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(caracteristica.caracteristicaId);
        setFormulario({
            nombre: caracteristica.nombre,
            orden: String(caracteristica.orden),
        });
    }

    function ordenSugerido(): number {
        if (caracteristicas.length === 0) {
            return PASO_ORDEN;
        }

        return Math.max(...caracteristicas.map((fila) => fila.orden)) + PASO_ORDEN;
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: GuardarCaracteristicaRequest = {
            nombre: formulario.nombre.trim(),
            orden: formulario.orden ? Number(formulario.orden) : ordenSugerido(),
        };

        try {
            if (editandoId === null) {
                await caracteristicasApi.crear(cuerpo);
                setExito(t('admin.caracteristicas.exitoAlta', { nombre: cuerpo.nombre }));
            } else {
                await caracteristicasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.caracteristicas.exitoModificacion', { nombre: cuerpo.nombre }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.caracteristicas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function darDeBaja(caracteristica: CaracteristicaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await caracteristicasApi.baja(caracteristica.caracteristicaId);
            setExito(t('admin.caracteristicas.exitoBaja', { nombre: caracteristica.nombre }));

            if (editandoId === caracteristica.caracteristicaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.caracteristicas.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<CaracteristicaAdminResponse>[] = [
        {
            encabezado: t('admin.caracteristicas.tabla.orden'),
            numerica: true,
            celda: (fila) => fila.orden,
        },
        { encabezado: t('comun.campo.nombre'), celda: (fila) => fila.nombre },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div>
            <div className="flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                    <h3 className="text-sm font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.caracteristicas.formulario.nueva')
                            : t('admin.caracteristicas.formulario.editar')}
                    </h3>

                    <div className="fila">
                        <CampoTexto
                            etiqueta={t('comun.campo.nombre')}
                            identificador="caracteristica-nombre"
                            required
                            maxLength={100}
                            value={formulario.nombre}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, nombre: evento.target.value })
                            }
                        />

                        <CampoTexto
                            etiqueta={t('admin.caracteristicas.tabla.orden')}
                            identificador="caracteristica-orden"
                            type="number"
                            min={0}
                            max={100000}
                            step="1"
                            placeholder={String(ordenSugerido())}
                            value={formulario.orden}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, orden: evento.target.value })
                            }
                        />
                    </div>

                    <p className="ayuda">{t('admin.caracteristicas.formulario.ayudaOrden')}</p>

                    <BotonesFormulario
                        editando={editandoId !== null}
                        guardando={guardando}
                        textoCrear={t('admin.caracteristicas.formulario.crear')}
                        onCancelar={limpiar}
                    />
                </form>
            )}

            <div className="mt-6">
                <h3 className="text-sm font-semibold text-texto">
                    {t('admin.caracteristicas.listado')}
                </h3>

                <div className="mt-3">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">
                            {t('admin.caracteristicas.cargando')}
                        </p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={caracteristicas}
                            claveDe={(fila) => fila.caracteristicaId}
                            inactiva={(fila) => !fila.activo}
                            mensajeVacio={t('admin.caracteristicas.vacio')}
                            acciones={(fila) => (
                                <div className="flex justify-end gap-2">
                                    {puedeModificar && (
                                        <BotonAccion onClick={() => editar(fila)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {puedeDarDeBaja &&
                                        fila.activo &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === fila.caracteristicaId}
                                            onPedir={() =>
                                                    setConfirmandoBaja(fila.caracteristicaId)}
                                            onConfirmar={() => void darDeBaja(fila)}
                                            onCancelar={() => setConfirmandoBaja(null)}
                                        />)}
                                </div>
                            )}
                        />
                    )}
                </div>
            </div>
        </div>
    );
}
