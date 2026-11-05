import { type ChangeEvent, type ReactNode, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { empresasApi, padronApi } from '../api/empresasApi';
import { useSesion } from '../contexto/useSesion';
import { useMensajeDeError } from '../hooks/useMensajeDeError';
import { PERMISOS } from '../rutas/itemsMenu';
import { indiceDeColumnas, parsearCsv } from '../servicios/csv';
import type {
    FilaPadronRequest,
    ResultadoFilaPadronResponse,
    ResultadoPadronResponse,
} from '../tipos/empresas';
import type { EmpresaResponse } from '../tipos/usuarios';
import { Alerta } from './Alerta';
import { Boton } from './Boton';
import { CampoArchivo, CampoSelect } from './Campos';
import { type ColumnaAbm, TablaAbm } from './TablaAbm';

const FILAS_MAXIMAS = 25;

interface CifraProps {
    titulo: string;
    valor: ReactNode;
    tono?: 'normal' | 'bueno' | 'malo';
}

function Cifra({ titulo, valor, tono = 'normal' }: CifraProps) {
    const color =
        tono === 'bueno' ? 'text-primario' : tono === 'malo' ? 'text-error' : 'text-texto';

    return (
        <div className="rounded-2xl border border-borde bg-superficie p-6">
            <p className="m-0 text-sm font-semibold text-texto-suave">{titulo}</p>
            <p className={`mt-2 text-2xl font-semibold ${color}`}>{valor}</p>
        </div>
    );
}

interface ImportadorPadronProps {
    alImportar: () => void;
}

export function ImportadorPadron({ alImportar }: ImportadorPadronProps) {
    const { t } = useTranslation();
    const { tienePermiso } = useSesion();

    const [empresas, setEmpresas] = useState<EmpresaResponse[]>([]);
    const [empresaId, setEmpresaId] = useState('');
    const [nombreArchivo, setNombreArchivo] = useState<string | null>(null);
    const [filas, setFilas] = useState<FilaPadronRequest[]>([]);
    const [resultado, setResultado] = useState<ResultadoPadronResponse | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [procesando, setProcesando] = useState(false);

    const alcanceTotal = tienePermiso(PERMISOS.usuarioVerTodasLasEmpresas);

    const mensajeDeError = useMensajeDeError();

    useEffect(() => {
        if (!alcanceTotal) {
            return;
        }

        empresasApi.listar().then(setEmpresas).catch(() => setEmpresas([]));
    }, [alcanceTotal]);

    function limpiar() {
        setResultado(null);
        setExito(null);
        setError(null);
    }

    async function elegirArchivo(evento: ChangeEvent<HTMLInputElement>) {
        const archivo = evento.target.files?.[0] ?? null;

        limpiar();
        setFilas([]);
        setNombreArchivo(archivo === null ? null : archivo.name);

        if (archivo === null) {
            return;
        }

        try {
            const contenido = await archivo.text();
            const lineas = parsearCsv(contenido);

            if (lineas.length < 2) {
                setError(t('admin.padron.errorColumnas'));
                return;
            }

            const columnas = indiceDeColumnas(lineas[0].celdas);

            if (
                columnas.nombre === undefined ||
                columnas.apellido === undefined ||
                columnas.email === undefined
            ) {
                setError(t('admin.padron.errorColumnas'));
                return;
            }

            const celda = (celdas: string[], indice: number | undefined) =>
                indice === undefined ? null : (celdas[indice] ?? '').trim() || null;

            setFilas(
                lineas.slice(1).map((linea) => ({
                    nombre: celda(linea.celdas, columnas.nombre) ?? '',
                    apellido: celda(linea.celdas, columnas.apellido) ?? '',
                    email: celda(linea.celdas, columnas.email) ?? '',
                    documento: celda(linea.celdas, columnas.documento),
                    departamento: celda(linea.celdas, columnas.departamento),
                })),
            );
        } catch {
            setError(t('admin.padron.errorArchivo'));
        }
    }

    async function procesar(confirmar: boolean) {
        setError(null);
        setExito(null);
        setProcesando(true);

        try {
            const respuesta = await padronApi.procesar({
                empresaId: empresaId === '' ? 0 : Number(empresaId),
                confirmar,
                filas,
            });

            setResultado(respuesta);

            if (respuesta.confirmado) {
                setExito(t('admin.padron.exito', { invitadas: respuesta.validas }));
                setFilas([]);
                setNombreArchivo(null);
                alImportar();
            }
        } catch (excepcion) {
            setResultado(null);
            setError(mensajeDeError(excepcion, 'admin.padron.errorProcesar'));
        } finally {
            setProcesando(false);
        }
    }

    const columnas: ColumnaAbm<ResultadoFilaPadronResponse>[] = [
        { encabezado: t('admin.padron.col.fila'), numerica: true, celda: (fila) => fila.fila },
        {
            encabezado: t('admin.padron.col.empleado'),
            celda: (fila) =>
                `${fila.apellido ?? ''}${fila.apellido && fila.nombre ? ', ' : ''}${fila.nombre ?? ''}` ||
                t('comun.valor.vacio'),
        },
        {
            encabezado: t('comun.campo.email'),
            celda: (fila) => fila.email ?? t('comun.valor.vacio'),
        },
        {
            encabezado: t('admin.padron.col.motivo'),
            celda: (fila) =>
                fila.valida ? (
                    <span className="rounded-full bg-info-fondo px-2.5 py-0.5 text-xs font-semibold text-primario">
                        {t('admin.padron.filaOk')}
                    </span>
                ) : (
                    <span className="text-error">{fila.motivo}</span>
                ),
        },
    ];

    const listoParaValidar = filas.length > 0 && (!alcanceTotal || empresaId !== '');
    const hayValidas = resultado !== null && !resultado.confirmado && resultado.validas > 0;

    return (
        <div className="mt-4 flex flex-col gap-4">
            <p className="text-sm text-texto-suave">{t('admin.padron.descripcion')}</p>

            <div className="flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {alcanceTotal && (
                <div className="w-full sm:w-96">
                    <CampoSelect
                        etiqueta={t('comun.campo.empresa')}
                        identificador="padronEmpresa"
                        value={empresaId}
                        onChange={(evento) => {
                            limpiar();
                            setEmpresaId(evento.target.value);
                        }}
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

            <CampoArchivo
                etiqueta={t('admin.padron.archivo')}
                identificador="padronArchivo"
                accept=".csv,text/csv"
                archivo={nombreArchivo}
                onChange={(evento) => void elegirArchivo(evento)}
            />

            <div>
                <p className="ayuda">{t('admin.padron.ayudaColumnas')}</p>
                <p className="ayuda">{t('admin.padron.ayudaTope', { tope: FILAS_MAXIMAS })}</p>
            </div>

            <div className="flex flex-wrap gap-3">
                <Boton
                    type="button"
                    cargando={procesando}
                    disabled={!listoParaValidar}
                    onClick={() => void procesar(false)}
                >
                    {t('admin.padron.validar')}
                </Boton>

                {hayValidas && (
                    <button
                        type="button"
                        onClick={() => void procesar(true)}
                        disabled={procesando}
                        className="rounded-lg border border-primario bg-primario px-4 py-2 text-sm font-semibold text-white hover:opacity-90 disabled:opacity-60"
                    >
                        {t('admin.padron.confirmar')}
                    </button>
                )}
            </div>

            {resultado !== null && (
                <>
                    <div className="grid gap-6 sm:grid-cols-3">
                        <Cifra titulo={t('admin.padron.cifra.leidas')} valor={resultado.leidas} />
                        <Cifra
                            titulo={t('admin.padron.cifra.aInvitar')}
                            valor={resultado.validas}
                            tono="bueno"
                        />
                        <Cifra
                            titulo={t('admin.padron.cifra.conError')}
                            valor={resultado.conError}
                            tono={resultado.conError > 0 ? 'malo' : 'normal'}
                        />
                    </div>

                    {!resultado.confirmado && resultado.validas === 0 && (
                        <p className="text-sm text-error">{t('admin.padron.sinValidas')}</p>
                    )}

                    <TablaAbm
                        columnas={columnas}
                        filas={resultado.filas}
                        claveDe={(fila) => fila.fila}
                        inactiva={(fila) => !fila.valida}
                        mensajeVacio={t('admin.padron.vacio')}
                    />
                </>
            )}
        </div>
    );
}
