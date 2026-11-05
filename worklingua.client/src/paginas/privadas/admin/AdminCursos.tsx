import { type FormEvent, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { cursosApi, etiquetasApi, versionesApi } from '../../../api/cursosApi';
import { AlertasResultado } from '../../../componentes/Alerta';
import { Boton, BotonAccion, BotonesFormulario, ConfirmarBaja } from '../../../componentes/Boton';
import { CampoSelect, CampoTexto } from '../../../componentes/Campos';
import { Modal } from '../../../componentes/Modal';
import { PanelActivos } from '../../../componentes/PanelActivos';
import { PanelModulos } from '../../../componentes/PanelModulos';
import { PanelVersiones } from '../../../componentes/PanelVersiones';
import { type ColumnaAbm, TablaAbm } from '../../../componentes/TablaAbm';
import { useLocalizacion } from '../../../contexto/useLocalizacion';
import { useSesion } from '../../../contexto/useSesion';
import { useMensajeDeError } from '../../../hooks/useMensajeDeError';
import type {
    CursoAdminResponse,
    EtiquetaResponse,
    GuardarCursoRequest,
    SectorCurso,
} from '../../../tipos/cursos';
import { NIVELES_CURSO, SECTORES_CURSO } from '../../../tipos/cursos';

const FORMULARIO_VACIO = {
    idioma: '',
    nombre: '',
    descripcion: '',
    nivel: NIVELES_CURSO[0] as string,
    duracionHoras: '',
    sector: '',
    fechaPublicacion: '',
    fechaFin: '',
};

type Formulario = typeof FORMULARIO_VACIO;

const ID_SUGERENCIAS_IDIOMA = 'sugerenciasIdiomaCurso';

const ID_SUGERENCIAS_ETIQUETA = 'sugerenciasEtiquetaCurso';

export function AdminCursos() {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();
    const { formatearFecha } = useLocalizacion();

    const [cursos, setCursos] = useState<CursoAdminResponse[]>([]);
    const [idiomas, setIdiomas] = useState<string[]>([]);
    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);
    const [editandoId, setEditandoId] = useState<number | null>(null);
    const [confirmandoBaja, setConfirmandoBaja] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);
    const [recarga, setRecarga] = useState(0);
    const [guardando, setGuardando] = useState(false);
    const [cursoModulos, setCursoModulos] = useState<CursoAdminResponse | null>(null);
    const [cursoActivos, setCursoActivos] = useState<CursoAdminResponse | null>(null);
    const [cursoVersiones, setCursoVersiones] = useState<CursoAdminResponse | null>(null);
    const [cursoAClonar, setCursoAClonar] = useState<CursoAdminResponse | null>(null);
    const [nombreCopia, setNombreCopia] = useState('');
    const [clonando, setClonando] = useState(false);
    const [diccionario, setDiccionario] = useState<EtiquetaResponse[]>([]);
    const [etiquetas, setEtiquetas] = useState<EtiquetaResponse[]>([]);
    const [etiquetaEscrita, setEtiquetaEscrita] = useState('');
    const [etiquetaPorCrear, setEtiquetaPorCrear] = useState<string | null>(null);

    const puedeCrear = tienePermiso('Curso.Alta');
    const puedeModificar = tienePermiso('Curso.Modificar');
    const puedeDarDeBaja = tienePermiso('Curso.Baja');
    const alcanceTotal = tienePermiso('Curso.VerTodasLasEmpresas');
    const esGestionable = (curso: CursoAdminResponse) => alcanceTotal || curso.empresaId !== null;

    const mensajeDeError = useMensajeDeError();

    useEffect(() => {
        cursosApi.listarIdiomas().then(setIdiomas).catch(() => setIdiomas([]));
        etiquetasApi.listar().then(setDiccionario).catch(() => setDiccionario([]));
    }, [recarga]);

    useEffect(() => {
        cursosApi
            .listarAdministracion()
            .then(setCursos)
            .catch((excepcion) =>
                setError(mensajeDeError(excepcion, 'admin.cursos.errorCargar')),
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
        setEtiquetas([]);
        setEtiquetaEscrita('');
        setEtiquetaPorCrear(null);
    }

    function editar(curso: CursoAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);
        setEditandoId(curso.cursoId);
        setEtiquetas(curso.etiquetas);
        setEtiquetaEscrita('');
        setEtiquetaPorCrear(null);
        setFormulario({
            idioma: curso.idioma,
            nombre: curso.nombre,
            descripcion: curso.descripcion ?? '',
            nivel: curso.nivel,
            duracionHoras: curso.duracionHoras === null ? '' : String(curso.duracionHoras),
            sector: curso.sector ?? '',
            fechaPublicacion: curso.fechaPublicacion ?? '',
            fechaFin: curso.fechaFin ?? '',
        });
    }

    async function guardar(evento: FormEvent) {
        evento.preventDefault();
        setError(null);
        setExito(null);
        setGuardando(true);

        const cuerpo: GuardarCursoRequest = {
            idioma: formulario.idioma.trim(),
            nombre: formulario.nombre.trim(),
            descripcion: formulario.descripcion.trim() || null,
            nivel: formulario.nivel,
            duracionHoras: formulario.duracionHoras ? Number(formulario.duracionHoras) : null,
            sector: formulario.sector === '' ? null : (formulario.sector as SectorCurso),
            fechaPublicacion: formulario.fechaPublicacion || null,
            fechaFin: formulario.fechaFin || null,
            etiquetaIds: etiquetas.map((etiqueta) => etiqueta.etiquetaId),
        };

        try {
            if (editandoId === null) {
                await cursosApi.crear(cuerpo);
                setExito(t('admin.cursos.exitoAlta', { nombre: cuerpo.nombre }));
            } else {
                await cursosApi.modificar(editandoId, cuerpo);
                setExito(t('admin.cursos.exitoModificacion', { nombre: cuerpo.nombre }));
            }

            limpiar();
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.errorGuardar'));
        } finally {
            setGuardando(false);
        }
    }

    async function darDeBaja(curso: CursoAdminResponse) {
        setError(null);
        setExito(null);
        setConfirmandoBaja(null);

        try {
            await cursosApi.baja(curso.cursoId);
            setExito(t('admin.cursos.exitoBaja', { nombre: curso.nombre }));

            if (editandoId === curso.cursoId) {
                limpiar();
            }

            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.errorBaja'));
        }
    }

    function claveDeEtiqueta(nombre: string) {
        return nombre
            .trim()
            .toUpperCase()
            .normalize('NFD')
            .replace(/[̀-ͯ]/g, '');
    }

    function agregarEtiqueta() {
        const nombre = etiquetaEscrita.trim();

        if (nombre === '') {
            return;
        }

        const clave = claveDeEtiqueta(nombre);
        const yaPuesta = etiquetas.some((etiqueta) => claveDeEtiqueta(etiqueta.nombre) === clave);

        if (yaPuesta) {
            setEtiquetaEscrita('');

            return;
        }

        const delDiccionario = diccionario.find(
            (etiqueta) => claveDeEtiqueta(etiqueta.nombre) === clave,
        );

        if (delDiccionario) {
            setEtiquetas([...etiquetas, delDiccionario]);
            setEtiquetaEscrita('');
            setEtiquetaPorCrear(null);

            return;
        }

        setEtiquetaPorCrear(nombre);
    }

    async function crearEtiqueta() {
        if (etiquetaPorCrear === null) {
            return;
        }

        setError(null);

        try {
            const creada = await etiquetasApi.crear(etiquetaPorCrear);

            setDiccionario([...diccionario, creada]);
            setEtiquetas([...etiquetas, creada]);
            setEtiquetaEscrita('');
            setEtiquetaPorCrear(null);
        } catch (excepcion) {
            setEtiquetaPorCrear(null);
            setError(mensajeDeError(excepcion, 'admin.cursos.etiquetas.invalida'));
        }
    }

    function quitarEtiqueta(etiquetaId: number) {
        setEtiquetas(etiquetas.filter((etiqueta) => etiqueta.etiquetaId !== etiquetaId));
    }

    function abrirClonado(curso: CursoAdminResponse) {
        setError(null);
        setExito(null);
        setCursoAClonar(curso);
        setNombreCopia('');
    }

    async function clonar() {
        if (cursoAClonar === null) {
            return;
        }

        setError(null);
        setExito(null);
        setClonando(true);

        try {
            const resultado = await versionesApi.clonar(cursoAClonar.cursoId, nombreCopia.trim());

            const aviso = t('admin.cursos.clonar.exito');

            setExito(
                resultado.omitidos === 0
                    ? aviso
                    : `${aviso} ${t('admin.cursos.clonar.omitidos', { cantidad: resultado.omitidos })}`,
            );
            setCursoAClonar(null);
            setNombreCopia('');
            recargar();
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'admin.cursos.clonar.error'));
        } finally {
            setClonando(false);
        }
    }

    function ventanaDe(curso: CursoAdminResponse) {
        if (curso.fechaPublicacion === null && curso.fechaFin === null) {
            return t('admin.cursos.ventana.siempre');
        }

        if (curso.fechaPublicacion !== null && curso.fechaFin !== null) {
            return t('admin.cursos.ventana.entre', {
                desde: formatearFecha(curso.fechaPublicacion),
                hasta: formatearFecha(curso.fechaFin),
            });
        }

        return curso.fechaPublicacion !== null
            ? t('admin.cursos.ventana.desde', { fecha: formatearFecha(curso.fechaPublicacion) })
            : t('admin.cursos.ventana.hasta', { fecha: formatearFecha(curso.fechaFin) });
    }

    function origenDe(curso: CursoAdminResponse) {
        if (curso.empresaId === null) {
            return t('admin.cursos.origen.global');
        }

        return alcanceTotal
            ? t('admin.cursos.origen.empresa', { empresaId: curso.empresaId })
            : t('admin.cursos.origen.propio');
    }

    const columnas: ColumnaAbm<CursoAdminResponse>[] = [
        { encabezado: t('admin.cursos.tabla.nombre'), celda: (curso) => curso.nombre },
        { encabezado: t('admin.cursos.tabla.origen'), celda: (curso) => origenDe(curso) },
        { encabezado: t('comun.campo.idioma'), celda: (curso) => curso.idioma },
        {
            encabezado: t('comun.campo.nivel'),
            celda: (curso) => t(`comun.nivel.${curso.nivel.toLowerCase()}`),
        },
        {
            encabezado: t('admin.cursos.tabla.horas'),
            numerica: true,
            celda: (curso) =>
                curso.duracionHoras === null ? t('comun.valor.vacio') : curso.duracionHoras,
        },
        {
            encabezado: t('admin.cursos.sector'),
            celda: (curso) =>
                curso.sector === null
                    ? t('comun.valor.vacio')
                    : t(`admin.cursos.sector.${curso.sector}`),
        },
        {
            encabezado: t('admin.cursos.etiquetas'),
            celda: (curso) =>
                curso.etiquetas.length === 0
                    ? t('admin.cursos.etiquetas.vacio')
                    : curso.etiquetas.map((etiqueta) => etiqueta.nombre).join(', '),
        },
        { encabezado: t('admin.cursos.ventana'), celda: (curso) => ventanaDe(curso) },
    ];

    const puedeUsarFormulario = editandoId === null ? puedeCrear : puedeModificar;

    return (
        <div className="mx-auto max-w-6xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">
                {t('admin.cursos.titulo')}
            </h1>
            <p className="mt-2 text-sm text-texto-suave">{t('admin.cursos.descripcion')}</p>

            <AlertasResultado error={error} exito={exito} />

            {puedeUsarFormulario && (
                <section className="mt-6 rounded-2xl border border-borde bg-superficie p-6">
                    <h2 className="text-lg font-semibold text-texto">
                        {editandoId === null
                            ? t('admin.cursos.formulario.nuevo')
                            : t('admin.cursos.formulario.editar')}
                    </h2>

                    <form onSubmit={guardar} noValidate className="mt-4 flex flex-col gap-4">
                        <CampoTexto
                            etiqueta={t('admin.cursos.tabla.nombre')}
                            identificador="nombre"
                            required
                            maxLength={150}
                            value={formulario.nombre}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, nombre: evento.target.value })
                            }
                        />

                        <CampoTexto
                            etiqueta={t('comun.campo.descripcion')}
                            identificador="descripcion"
                            maxLength={500}
                            value={formulario.descripcion}
                            onChange={(evento) =>
                                setFormulario({ ...formulario, descripcion: evento.target.value })
                            }
                        />

                        <div className="fila">
                            <CampoTexto
                                etiqueta={t('admin.cursos.formulario.idioma')}
                                identificador="idioma"
                                required
                                maxLength={50}
                                list={ID_SUGERENCIAS_IDIOMA}
                                placeholder={t('admin.cursos.formulario.idiomaPlaceholder')}
                                value={formulario.idioma}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, idioma: evento.target.value })
                                }
                            />

                            <datalist id={ID_SUGERENCIAS_IDIOMA}>
                                {idiomas.map((opcion) => (
                                    <option key={opcion} value={opcion} />
                                ))}
                            </datalist>

                            <CampoSelect
                                etiqueta={t('comun.campo.nivel')}
                                identificador="nivel"
                                required
                                value={formulario.nivel}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, nivel: evento.target.value })
                                }
                            >
                                {NIVELES_CURSO.map((nivel) => (
                                    <option key={nivel} value={nivel}>
                                        {t(`comun.nivel.${nivel.toLowerCase()}`)}
                                    </option>
                                ))}
                            </CampoSelect>

                            <CampoTexto
                                etiqueta={t('admin.cursos.formulario.duracion')}
                                identificador="duracionHoras"
                                type="number"
                                min={1}
                                max={10000}
                                step="1"
                                value={formulario.duracionHoras}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        duracionHoras: evento.target.value,
                                    })
                                }
                            />
                        </div>

                        <div className="fila mt-4">
                            <CampoSelect
                                etiqueta={t('admin.cursos.sector')}
                                identificador="sector"
                                value={formulario.sector}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, sector: evento.target.value })
                                }
                            >
                                <option value="">{t('admin.cursos.sector.sin')}</option>
                                {SECTORES_CURSO.map((sector) => (
                                    <option key={sector} value={sector}>
                                        {t(`admin.cursos.sector.${sector}`)}
                                    </option>
                                ))}
                            </CampoSelect>
                        </div>

                        <div className="mt-4">
                            <label htmlFor="etiquetaNueva" className="block text-sm font-semibold text-texto">
                                {t('admin.cursos.etiquetas')}
                            </label>

                            {etiquetas.length > 0 && (
                                <ul className="mt-2 m-0 flex list-none flex-wrap gap-2 p-0">
                                    {etiquetas.map((etiqueta) => (
                                        <li key={etiqueta.etiquetaId}>
                                            <span className="inline-flex items-center gap-2 rounded-full bg-fondo px-3 py-1 text-sm text-texto">
                                                {etiqueta.nombre}
                                                <button
                                                    type="button"
                                                    onClick={() => quitarEtiqueta(etiqueta.etiquetaId)}
                                                    aria-label={t('admin.cursos.etiquetas.quitar', {
                                                        nombre: etiqueta.nombre,
                                                    })}
                                                    className="text-texto-suave hover:text-error"
                                                >
                                                    ×
                                                </button>
                                            </span>
                                        </li>
                                    ))}
                                </ul>
                            )}

                            <div className="mt-2 flex flex-wrap items-center gap-2">
                                <input
                                    id="etiquetaNueva"
                                    list={ID_SUGERENCIAS_ETIQUETA}
                                    maxLength={50}
                                    value={etiquetaEscrita}
                                    onChange={(evento) => {
                                        setEtiquetaEscrita(evento.target.value);
                                        setEtiquetaPorCrear(null);
                                    }}
                                    onKeyDown={(evento) => {
                                        if (evento.key === 'Enter') {
                                            evento.preventDefault();
                                            agregarEtiqueta();
                                        }
                                    }}
                                    className="w-56 rounded-lg border border-borde bg-superficie px-3 py-2 text-sm text-texto"
                                />
                                <datalist id={ID_SUGERENCIAS_ETIQUETA}>
                                    {diccionario.map((etiqueta) => (
                                        <option key={etiqueta.etiquetaId} value={etiqueta.nombre} />
                                    ))}
                                </datalist>

                                <button
                                    type="button"
                                    onClick={agregarEtiqueta}
                                    className="rounded-lg border border-borde bg-superficie px-3 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                                >
                                    {t('admin.cursos.etiquetas.agregar')}
                                </button>
                            </div>

                            <p className="ayuda">{t('admin.cursos.etiquetas.ayuda')}</p>

                            {etiquetaPorCrear !== null && (
                                <div className="mt-2 rounded-lg border border-alerta bg-alerta-fondo p-3">
                                    <p className="m-0 text-sm text-texto">
                                        {t('admin.cursos.etiquetas.crear', { nombre: etiquetaPorCrear })}
                                    </p>
                                    <div className="mt-3 flex flex-wrap gap-2">
                                        <BotonAccion onClick={() => void crearEtiqueta()}>
                                            {t('admin.cursos.etiquetas.crear.aceptar')}
                                        </BotonAccion>
                                        <BotonAccion onClick={() => setEtiquetaPorCrear(null)}>
                                            {t('admin.cursos.etiquetas.crear.cancelar')}
                                        </BotonAccion>
                                    </div>
                                </div>
                            )}
                        </div>

                        <div className="fila mt-4">
                            <CampoTexto
                                etiqueta={t('admin.cursos.fechaPublicacion')}
                                identificador="fechaPublicacion"
                                type="date"
                                value={formulario.fechaPublicacion}
                                onChange={(evento) =>
                                    setFormulario({
                                        ...formulario,
                                        fechaPublicacion: evento.target.value,
                                    })
                                }
                            />
                            <CampoTexto
                                etiqueta={t('admin.cursos.fechaFin')}
                                identificador="fechaFin"
                                type="date"
                                value={formulario.fechaFin}
                                onChange={(evento) =>
                                    setFormulario({ ...formulario, fechaFin: evento.target.value })
                                }
                            />
                        </div>

                        <p className="ayuda">{t('admin.cursos.ventana.ayuda')}</p>

                        {(formulario.fechaPublicacion !== '' || formulario.fechaFin !== '') && (
                            <div className="mt-2">
                                <BotonAccion
                                    onClick={() =>
                                        setFormulario({
                                            ...formulario,
                                            fechaPublicacion: '',
                                            fechaFin: '',
                                        })
                                    }
                                >
                                    {t('admin.cursos.ventana.cancelar')}
                                </BotonAccion>
                            </div>
                        )}

                        <p className="ayuda">{t('admin.cursos.formulario.ayudaIdioma')}</p>

                        <BotonesFormulario
                            editando={editandoId !== null}
                            guardando={guardando}
                            textoCrear={t('admin.cursos.formulario.crear')}
                            onCancelar={limpiar}
                        />
                    </form>
                </section>
            )}

            <section className="mt-6">
                <h2 className="text-lg font-semibold text-texto">{t('admin.cursos.listado')}</h2>

                <div className="mt-4">
                    {cargando ? (
                        <p className="text-sm text-texto-suave">{t('admin.cursos.cargando')}</p>
                    ) : (
                        <TablaAbm
                            columnas={columnas}
                            filas={cursos}
                            claveDe={(curso) => curso.cursoId}
                            inactiva={(curso) => !curso.activo}
                            mensajeVacio={t('admin.cursos.vacio')}
                            acciones={(curso) => (
                                <div className="flex justify-end gap-2">
                                    <BotonAccion onClick={() => setCursoModulos(curso)}>
                                        {t('admin.cursos.modulos.boton')}
                                    </BotonAccion>

                                    <BotonAccion onClick={() => setCursoActivos(curso)}>
                                        {t('admin.cursos.activos.boton')}
                                    </BotonAccion>

                                    <BotonAccion onClick={() => setCursoVersiones(curso)}>
                                        {t('admin.cursos.versiones')}
                                    </BotonAccion>

                                    {puedeCrear && esGestionable(curso) && (
                                        <BotonAccion onClick={() => abrirClonado(curso)}>
                                            {t('admin.cursos.clonar')}
                                        </BotonAccion>
                                    )}

                                    {puedeModificar && esGestionable(curso) && (
                                        <BotonAccion onClick={() => editar(curso)}>
                                            {t('comun.boton.editar')}
                                        </BotonAccion>
                                    )}

                                    {puedeDarDeBaja &&
                                        esGestionable(curso) &&
                                        curso.activo &&
                                        (<ConfirmarBaja
                                            confirmando={confirmandoBaja === curso.cursoId}
                                            onPedir={() => setConfirmandoBaja(curso.cursoId)}
                                            onConfirmar={() => void darDeBaja(curso)}
                                            onCancelar={() => setConfirmandoBaja(null)}
                                        />)}
                                </div>
                            )}
                        />
                    )}
                </div>
            </section>

            <Modal
                titulo={t('admin.cursos.modulos.titulo', { curso: cursoModulos?.nombre ?? '' })}
                descripcion={t('admin.cursos.modulos.descripcion')}
                abierto={cursoModulos !== null}
                alCerrar={() => setCursoModulos(null)}
            >
                {cursoModulos && (
                    <PanelModulos
                        key={cursoModulos.cursoId}
                        cursoId={cursoModulos.cursoId}
                        puedeModificar={
                            puedeModificar && cursoModulos.activo && esGestionable(cursoModulos)
                        }
                    />
                )}
            </Modal>

            <Modal
                titulo={t('admin.cursos.activos.titulo', { curso: cursoActivos?.nombre ?? '' })}
                abierto={cursoActivos !== null}
                alCerrar={() => setCursoActivos(null)}
            >
                {cursoActivos && (
                    <PanelActivos
                        key={cursoActivos.cursoId}
                        cursoId={cursoActivos.cursoId}
                        puedeModificar={
                            puedeModificar && cursoActivos.activo && esGestionable(cursoActivos)
                        }
                    />
                )}
            </Modal>

            <Modal
                titulo={t('admin.cursos.versiones.titulo', { curso: cursoVersiones?.nombre ?? '' })}
                abierto={cursoVersiones !== null}
                alCerrar={() => setCursoVersiones(null)}
            >
                {cursoVersiones && (
                    <PanelVersiones
                        key={cursoVersiones.cursoId}
                        cursoId={cursoVersiones.cursoId}
                        puedeModificar={
                            puedeModificar && cursoVersiones.activo && esGestionable(cursoVersiones)
                        }
                        alRestaurar={recargar}
                    />
                )}
            </Modal>

            <Modal
                titulo={t('admin.cursos.clonar.titulo', { curso: cursoAClonar?.nombre ?? '' })}
                abierto={cursoAClonar !== null}
                alCerrar={() => setCursoAClonar(null)}
            >
                <div className="flex flex-col gap-4">
                    <CampoTexto
                        etiqueta={t('admin.cursos.clonar.nombre')}
                        identificador="nombreCopia"
                        maxLength={150}
                        value={nombreCopia}
                        onChange={(evento) => setNombreCopia(evento.target.value)}
                    />

                    <div className="flex flex-wrap gap-3">
                        <Boton
                            type="button"
                            cargando={clonando}
                            onClick={() => void clonar()}
                        >
                            {t('admin.cursos.clonar.confirmar')}
                        </Boton>
                        <BotonAccion grande onClick={() => setCursoAClonar(null)}>
                            {t('admin.cursos.clonar.cancelar')}
                        </BotonAccion>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
