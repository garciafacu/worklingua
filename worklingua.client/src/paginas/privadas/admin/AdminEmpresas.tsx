import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../../api/clienteHttp';
import { empresasApi } from '../../../api/empresasApi';
import { Alerta } from '../../../componentes/Alerta';
import { Boton } from '../../../componentes/Boton';
import { CampoTexto } from '../../../componentes/CampoTexto';
import { TablaAbm, type ColumnaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import type { EmpresaAdminResponse, GuardarEmpresaRequest } from '../../../tipos/empresas';

const FORMULARIO_VACIO = {
    razonSocial: '',
    cuit: '',
    email: '',
    telefono: '',
    direccion: '',
    ciudad: '',
    provincia: '',
    pais: '',
};

type Formulario = typeof FORMULARIO_VACIO;

function opcional(valor: string): string | null {
    return valor.trim() || null;
}

/**
 * ABM de las empresas cliente.
 *
 * Dos empresas están protegidas y no se pueden dar de baja: la interna de
 * plataforma, que aloja al Super Admin, y la que reciben por defecto los
 * registros públicos. Tampoco se puede dar de baja una empresa que todavía
 * tenga usuarios activos; eso lo valida el backend.
 */
export function AdminEmpresas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    // En useCallback para poder ir en las dependencias de los efectos que lo
    // usan: sin eso el linter avisa, y el mensaje de error quedaría en el idioma
    // anterior después de cambiar de idioma.
    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) =>
            excepcion instanceof ErrorApi ? excepcion.message : t(clave),
        [t],
    );

    const [empresas, setEmpresas] = useState<EmpresaAdminResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    /** Fuerza releer el listado despues de guardar o dar de baja. */
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Empresa.Alta');
    const puedeModificar = tienePermiso('Empresa.Modificar');
    const puedeDarDeBaja = tienePermiso('Empresa.Baja');
    // Sin alcance total la pantalla es "Mi empresa": el backend devuelve y deja
    // modificar solo la empresa del usuario.
    const alcanceTotal = tienePermiso('Empresa.VerTodasLasEmpresas');

    useEffect(() => {
        empresasApi
            .listarAdministracion()
            .then(setEmpresas)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.empresas.errorCargar')),
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

    function editar(empresa: EmpresaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(empresa.empresaId);
        setFormulario({
            razonSocial: empresa.razonSocial,
            cuit: empresa.cuit,
            email: empresa.email,
            telefono: empresa.telefono ?? '',
            direccion: empresa.direccion ?? '',
            ciudad: empresa.ciudad ?? '',
            provincia: empresa.provincia ?? '',
            pais: empresa.pais ?? '',
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: GuardarEmpresaRequest = {
            razonSocial: formulario.razonSocial.trim(),
            cuit: formulario.cuit.trim(),
            email: formulario.email.trim(),
            telefono: opcional(formulario.telefono),
            direccion: opcional(formulario.direccion),
            ciudad: opcional(formulario.ciudad),
            provincia: opcional(formulario.provincia),
            pais: opcional(formulario.pais),
        };

        try {
            if (editandoId === null) {
                await empresasApi.crear(cuerpo);
                setExito(t('admin.empresas.exitoAlta', { nombre: cuerpo.razonSocial }));
            } else {
                await empresasApi.modificar(editandoId, cuerpo);
                setExito(t('admin.empresas.exitoModificacion', { nombre: cuerpo.razonSocial }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.empresas.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function darDeBaja(empresa: EmpresaAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await empresasApi.baja(empresa.empresaId);
            setExito(t('admin.empresas.exitoBaja', { nombre: empresa.razonSocial }));

            if (editandoId === empresa.empresaId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.empresas.errorBaja'));
        }
    }

    const columnas: ColumnaAbm<EmpresaAdminResponse>[] = [
        { encabezado: t('admin.empresas.tabla.razonSocial'), celda: (fila) => fila.razonSocial },
        { encabezado: t('admin.empresas.tabla.cuit'), celda: (fila) => fila.cuit },
        { encabezado: t('comun.campo.email'), celda: (fila) => fila.email },
        {
            encabezado: t('admin.empresas.tabla.ciudad'),
            celda: (fila) => fila.ciudad ?? t('comun.valor.vacio'),
        },
    ];

    if (alcanceTotal) {
        columnas.push({
            encabezado: t('admin.empresas.tabla.tipo'),
            celda: (fila) =>
                !fila.seleccionable
                    ? t('admin.empresas.tipo.interna')
                    : fila.protegido
                      ? t('admin.empresas.tipo.porDefecto')
                      : t('admin.empresas.tipo.cliente'),
        });
    }

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {alcanceTotal ? t('admin.empresas.titulo') : t('admin.empresas.miEmpresa.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">
                {alcanceTotal
                    ? t('admin.empresas.descripcion')
                    : t('admin.empresas.miEmpresa.descripcion')}
            </p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.empresas.formulario.nueva')
                            : t('admin.empresas.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.empresas.tabla.razonSocial')}
                                identificador="razonSocial"
                                required
                                maxLength={150}
                                value={formulario.razonSocial}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, razonSocial: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.empresas.tabla.cuit')}
                                identificador="cuit"
                                required
                                maxLength={11}
                                placeholder="30712345678"
                                disabled={editandoId !== null}
                                value={formulario.cuit}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, cuit: evento.target.value })
                                }
                            />
                        </div>

                        <p className="ayuda">
                            {t('admin.empresas.formulario.ayudaCuit')}
                        </p>

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('comun.campo.email')}
                                identificador="email"
                                type="email"
                                required
                                maxLength={120}
                                value={formulario.email}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, email: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.empresas.formulario.telefono')}
                                identificador="telefono"
                                maxLength={30}
                                value={formulario.telefono}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, telefono: evento.target.value })
                                }
                            />
                        </div>

                        <CampoTexto
                            etiqueta={t('admin.empresas.formulario.direccion')}
                            identificador="direccion"
                            maxLength={250}
                            value={formulario.direccion}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, direccion: evento.target.value })
                            }
                        />

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.empresas.formulario.ciudad')}
                                identificador="ciudad"
                                maxLength={80}
                                value={formulario.ciudad}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, ciudad: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.empresas.formulario.provincia')}
                                identificador="provincia"
                                maxLength={80}
                                value={formulario.provincia}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, provincia: evento.target.value })
                                }
                            />

                            <CampoTexto
                                etiqueta={t('admin.empresas.formulario.pais')}
                                identificador="pais"
                                maxLength={80}
                                value={formulario.pais}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, pais: evento.target.value })
                                }
                            />
                        </div>

                        <div className="flex flex-wrap gap-3">
                            <Boton type="submit" cargando={guardando}>
                                {editandoId === null
                                    ? t('admin.empresas.formulario.crear')
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
                <h2 className="text-lg font-semibold text-texto">{t('admin.empresas.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.empresas.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={empresas}
                            claveDe={(fila) => fila.empresaId}
                            mensajeVacio={t('admin.empresas.vacio')}
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

                                    {!alcanceTotal ? null : fila.protegido ? (
                                        <span className="px-3 py-1.5 text-sm text-texto-suave">
                                            {t('admin.empresas.protegida')}
                                        </span>
                                    ) : (
                                        puedeDarDeBaja &&
                                        (confirmandoBaja === fila.empresaId ? (
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
                                                onClick={() => setConfirmandoBaja(fila.empresaId)}
                                                className="rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm font-semibold text-error hover:bg-error-fondo"
                                            >
                                                {t('comun.boton.darDeBaja')}
                                            </button>
                                        ))
                                    )}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>
        </div>
    );
}
