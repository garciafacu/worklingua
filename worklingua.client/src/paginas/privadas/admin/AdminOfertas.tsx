import { type FormEvent, useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { empresasApi } from '../../../api/empresasApi';
import { planesApi } from '../../../api/planesApi';
import { ofertasApi } from '../../../api/ticketsApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { BotonAccion, BotonesFormulario, ConfirmarBaja } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { PERMISOS } from '../../../rutas/itemsMenu';
import type { EmpresaAdminResponse } from '../../../tipos/empresas';
import type { PlanAdminResponse } from '../../../tipos/planes';
import type { GuardarOfertaRequest, OfertaResponse } from '../../../tipos/tickets';

function hoy(): string {
    const fecha = new Date();
    const mes = String(fecha.getMonth() + 1).padStart(2, '0');
    const dia = String(fecha.getDate()).padStart(2, '0');

    return `${fecha.getFullYear()}-${mes}-${dia}`;
}

const FORMULARIO_VACIO = {
    empresaId: '',
    planId: '',
    titulo: '',
    descripcion: '',
    fechaDesde: '',
    fechaHasta: '',
    activo: true,
};

type Formulario = typeof FORMULARIO_VACIO;

export function AdminOfertas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearFecha } = useLocalizacion();

    const [ofertas, setOfertas] = useState<OfertaResponse[]>([]);
    const [empresas, setEmpresas] = useState<EmpresaAdminResponse[]>([]);
    const [planes, setPlanes] = useState<PlanAdminResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>({ ...FORMULARIO_VACIO, fechaDesde: hoy() });
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso(PERMISOS.ofertaAlta);
    const puedeModificar = tienePermiso(PERMISOS.ofertaModificar);
    const puedeDarDeBaja = tienePermiso(PERMISOS.ofertaBaja);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        Promise.all([empresasApi.listarAdministracion(), planesApi.listarAdministracion()])
            .then(([listaEmpresas, listaPlanes]) => {
                setEmpresas(listaEmpresas.filter((empresa) => empresa.activo && !empresa.protegido));
                setPlanes(listaPlanes.filter((plan) => plan.activo));
            })
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.ofertas.errorCargar')));
    }, [mensajeDeError]);

    useEffect(() => {
        ofertasApi
            .listarAdministracion()
            .then(setOfertas)
            .catch((excepcion) => setError(mensajeDeError(excepcion, 'admin.ofertas.errorCargar')))
            .finally(() => setCargando(false));
    }, [recarga, mensajeDeError]);

    function recargar() {
        setCargando(true);
        setRecarga((numero) => numero + 1);
    }

    function limpiar() {
        setFormulario({ ...FORMULARIO_VACIO, fechaDesde: hoy() });
        setEditandoId(null);
    }

    function editar(item: OfertaResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(item.oferta.ofertaId);
        setFormulario({
            empresaId: String(item.oferta.empresaId),
            planId: item.oferta.planId === null ? '' : String(item.oferta.planId),
            titulo: item.oferta.titulo,
            descripcion: item.oferta.descripcion,
            fechaDesde: item.oferta.fechaDesde,
            fechaHasta: item.oferta.fechaHasta ?? '',
            activo: item.oferta.activo,
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);

        if (!formulario.empresaId || !formulario.titulo.trim() || !formulario.descripcion.trim() || !formulario.fechaDesde) {
            setError(t('admin.ofertas.incompleto'));

            return;
        }

        setGuardando(true);

        const cuerpo: GuardarOfertaRequest = {
            empresaId: Number(formulario.empresaId),
            planId: formulario.planId ? Number(formulario.planId) : null,
            titulo: formulario.titulo.trim(),
            descripcion: formulario.descripcion.trim(),
            fechaDesde: formulario.fechaDesde,
            fechaHasta: formulario.fechaHasta || null,
            activo: formulario.activo,
        };

        try {
            if (editandoId === null) {
                await ofertasApi.crear(cuerpo);
                setExito(t('admin.ofertas.exitoAlta', { titulo: cuerpo.titulo }));
            } else {
                await ofertasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.ofertas.exitoModificacion', { titulo: cuerpo.titulo }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.ofertas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function darDeBaja(item: OfertaResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await ofertasApi.baja(item.oferta.ofertaId);
            setExito(t('admin.ofertas.exitoBaja', { titulo: item.oferta.titulo }));

            if (editandoId === item.oferta.ofertaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.ofertas.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<OfertaResponse>[] = [
        { encabezado: t('admin.ofertas.col.titulo'), celda: (item) => item.oferta.titulo },
        { encabezado: t('admin.ofertas.col.empresa'), celda: (item) => item.empresa },
        { encabezado: t('admin.ofertas.col.plan'), celda: (item) => item.plan ?? '—' },
        {
            encabezado: t('admin.ofertas.col.vigencia'),
            celda: (item) =>
                item.oferta.fechaHasta
                    ? `${formatearFecha(item.oferta.fechaDesde)} – ${formatearFecha(item.oferta.fechaHasta)}`
                    : t('admin.ofertas.desde', { fecha: formatearFecha(item.oferta.fechaDesde) }),
        },
        {
            encabezado: t('admin.ofertas.col.estado'),
            celda: (item) => (item.oferta.activo ? t('admin.ofertas.activa') : t('admin.ofertas.inactiva')),
        },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">{t('admin.ofertas.titulo')}</h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.ofertas.descripcion')}</p>

            <AlertasResultado error={error} exito={exito} />

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null ? t('admin.ofertas.nueva') : t('admin.ofertas.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoSelect
                                etiqueta={t('admin.ofertas.col.empresa')}
                                identificador="oferta-empresa"
                                required
                                value={formulario.empresaId}
                                onChange={(evento) => setFormulario({ ...formulario, empresaId: evento.target.value })}
                            >
                                <option value="" disabled>
                                    —
                                </option>
                                {empresas.map((empresa) => (
                                    <option key={empresa.empresaId} value={empresa.empresaId}>
                                        {empresa.razonSocial}
                                    </option>
                                ))}
                            </CampoSelect>

                            <CampoSelect
                                etiqueta={t('admin.ofertas.planSugerido')}
                                identificador="oferta-plan"
                                value={formulario.planId}
                                onChange={(evento) => setFormulario({ ...formulario, planId: evento.target.value })}
                            >
                                <option value="">{t('admin.ofertas.sinPlan')}</option>
                                {planes.map((plan) => (
                                    <option key={plan.planId} value={plan.planId}>
                                        {plan.nombre}
                                    </option>
                                ))}
                            </CampoSelect>
                        </div>

                        <CampoTexto
                            etiqueta={t('admin.ofertas.col.titulo')}
                            identificador="oferta-titulo"
                            required
                            maxLength={150}
                            value={formulario.titulo}
                            onChange={(evento) => setFormulario({ ...formulario, titulo: evento.target.value })}
                        />

                        <div className="campo">
                            <label htmlFor="oferta-descripcion">{t('admin.ofertas.descripcionCampo')}</label>
                            <textarea
                                id="oferta-descripcion"
                                rows={4}
                                required
                                maxLength={1000}
                                value={formulario.descripcion}
                                onChange={(evento) => setFormulario({ ...formulario, descripcion: evento.target.value })}
                                className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                            />
                        </div>

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.ofertas.fechaDesde')}
                                identificador="oferta-desde"
                                type="date"
                                required
                                value={formulario.fechaDesde}
                                onChange={(evento) => setFormulario({ ...formulario, fechaDesde: evento.target.value })}
                            />
                            <CampoTexto
                                etiqueta={t('admin.ofertas.fechaHasta')}
                                identificador="oferta-hasta"
                                type="date"
                                value={formulario.fechaHasta}
                                onChange={(evento) => setFormulario({ ...formulario, fechaHasta: evento.target.value })}
                            />
                        </div>

                        <label className="flex items-center gap-2.5 text-sm text-texto">
                            <input
                                type="checkbox"
                                className="size-4"
                                checked={formulario.activo}
                                onChange={(evento) => setFormulario({ ...formulario, activo: evento.target.checked })}
                            />
                            {t('admin.ofertas.activa')}
                        </label>

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.ofertas.crear')}
                            onCancelar={limpiar}
                        />
                    </form>
                </section>
            )}

            <section className="mt-6">
                <h2 className="text-lg font-semibold text-texto">{t('admin.ofertas.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.ofertas.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={ofertas}
                            claveDe={(item) => item.oferta.ofertaId}
                            inactiva={(item) => !item.oferta.activo}
                            mensajeVacio={t('admin.ofertas.vacio')}
                            acciones={(item) => (
                                <div className="flex justify-end gap-2">
                                    {puedeModificar && (
                                        <BotonAccion onClick={() => editar(item)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {puedeDarDeBaja &&
                                        item.oferta.activo &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === item.oferta.ofertaId}
                                            onPedir={() => setConfirmandoBaja(item.oferta.ofertaId)}
                                            onConfirmar={() => void darDeBaja(item)}
                                            onCancelar={() => setConfirmandoBaja(null)}
                                        />)}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
