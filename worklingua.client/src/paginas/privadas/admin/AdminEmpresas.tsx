import { type FormEvent, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { empresasApi } from '../../../api/empresasApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { BotonAccion, BotonesFormulario, ConfirmarBaja } from '../../../componentes/Boton';
import { CampoTexto } from '../../../componentes/Campos';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
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

export function AdminEmpresas() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const mensajeDeError = useMensajeDeError();

    const [empresas, setEmpresas] = useState<EmpresaAdminResponse[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);

    const puedeCrear = tienePermiso('Empresa.Alta');
    const puedeModificar = tienePermiso('Empresa.Modificar');
    const puedeDarDeBaja = tienePermiso('Empresa.Baja');
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

            <AlertasResultado error={error} exito={exito} />

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

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.empresas.formulario.crear')}
                            onCancelar={limpiar}
                        />
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
                                        <BotonAccion onClick={() => editar(fila)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {!alcanceTotal ? null : fila.protegido ? (
                                        <span className="px-3 py-1.5 text-sm text-texto-suave">
                                            {t('admin.empresas.protegida')}
                                        </span>
                                    ) : (
                                        puedeDarDeBaja &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === fila.empresaId}
                                            onPedir={() => setConfirmandoBaja(fila.empresaId)}
                                            onConfirmar={() => void darDeBaja(fila)}
                                            onCancelar={() => setConfirmandoBaja(null)}
                                        />)
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
