import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../api/clienteHttp';
import { encuestasApi } from '../api/encuestasApi';
import { useLocalizacion } from '../contexto/useLocalizacion';
import { obtenerTokenVisitante } from '../servicios/consultaPublica';
import type { EncuestaResponse } from '../tipos/encuestas';
import { Alerta } from './Alerta';
import { CampoCaptcha } from './CampoCaptcha';
import { GraficoBarras } from './GraficoBarras';

export function EncuestaPublica() {
    const { t } = useTranslation();
    const { idiomaActual } = useLocalizacion();

    const [encuesta, setEncuesta] = useState<EncuestaResponse | null>(null);
    const [elegida, setElegida] = useState<number | null>(null);
    const [tokenCaptcha, setTokenCaptcha] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [votando, setVotando] = useState(false);
    const [reinicioCaptcha, setReinicioCaptcha] = useState(0);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        let vigente = true;

        encuestasApi
            .listarPublicas(idiomaActual, obtenerTokenVisitante())
            .then((lista) => {
                if (vigente) {
                    setEncuesta(lista[0] ?? null);
                }
            })
            .catch(() => {
                if (vigente) {
                    setEncuesta(null);
                }
            });

        return () => {
            vigente = false;
        };
    }, [idiomaActual]);

    if (encuesta === null) {
        return null;
    }

    const respondida = encuesta.opcionElegidaId !== null;

    async function votar() {
        if (encuesta === null) {
            return;
        }

        if (elegida === null) {
            setError(t('publico.encuesta.sinOpcion'));

            return;
        }

        setError(null);
        setVotando(true);

        try {
            setEncuesta(
                await encuestasApi.responderPublica(encuesta.encuesta.encuestaId, {
                    opcionEncuestaId: elegida,
                    tokenVisitante: obtenerTokenVisitante(),
                    tokenCaptcha,
                }),
            );
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'publico.encuesta.error'));
            setReinicioCaptcha((numero) => numero + 1);
            setTokenCaptcha('');
        } finally {
            setVotando(false);
        }
    }

    return (
        <section className="mx-auto max-w-2xl rounded-2xl border border-borde bg-superficie p-6">
            <p className="m-0 text-xs font-semibold tracking-wide text-texto-suave uppercase">
                {t('publico.encuesta.titulo')}
            </p>
            <h2 className="mt-2 text-lg font-semibold text-texto">{encuesta.encuesta.pregunta}</h2>

            {encuesta.encuesta.descripcion && (
                <p className="mt-2 text-sm whitespace-pre-line text-texto-suave">
                    {encuesta.encuesta.descripcion}
                </p>
            )}

            {respondida ? (
                <div className="mt-4">
                    <p className="m-0 text-sm font-semibold text-exito">{t('publico.encuesta.gracias')}</p>

                    <div className="mt-3">
                        <GraficoBarras
                            datos={encuesta.opciones.map((opcion) => ({
                                etiqueta: opcion.texto,
                                valor: opcion.total,
                                destacado: opcion.opcionEncuestaId === encuesta.opcionElegidaId,
                            }))}
                            etiquetaSerie={t('publico.encuesta.totalVotos', {
                                cantidad: encuesta.totalRespuestas,
                            })}
                            mensajeVacio={t('comun.grafico.sinDatos')}
                        />
                    </div>
                </div>
            ) : (
                <>
                    <fieldset className="mt-4 flex flex-col gap-2 border-0 p-0">
                        <legend className="sr-only">{encuesta.encuesta.pregunta}</legend>

                        {encuesta.opciones.map((opcion) => (
                            <label
                                key={opcion.opcionEncuestaId}
                                className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-borde px-3 py-2 text-sm text-texto hover:bg-fondo"
                            >
                                <input
                                    type="radio"
                                    name="encuesta-publica"
                                    className="size-4"
                                    checked={elegida === opcion.opcionEncuestaId}
                                    onChange={() => setElegida(opcion.opcionEncuestaId)}
                                />
                                {opcion.texto}
                            </label>
                        ))}
                    </fieldset>

                    <div className="mt-4">
                        <CampoCaptcha onCambio={setTokenCaptcha} reiniciar={reinicioCaptcha} />
                    </div>

                    <div className="mt-2">
                        <Alerta tipo="error" mensaje={error} />
                    </div>

                    <button
                        type="button"
                        disabled={votando || elegida === null || tokenCaptcha === ''}
                        onClick={() => void votar()}
                        className="mt-3 rounded-lg bg-primario px-4 py-2 text-sm font-semibold text-white hover:bg-primario-hover disabled:opacity-50"
                    >
                        {votando ? t('comun.estado.procesando') : t('publico.encuesta.votar')}
                    </button>
                </>
            )}
        </section>
    );
}
