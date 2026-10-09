import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { ErrorApi } from '../../api/clienteHttp';
import { encuestasApi } from '../../api/encuestasApi';
import { Alerta } from '../../componentes/Alerta';
import { Boton } from '../../componentes/Boton';
import { GraficoBarras, type DatoGrafico } from '../../componentes/GraficoBarras';
import { useLocalizacion } from '../../contexto/useLocalizacion';
import type { EncuestaResponse } from '../../tipos/encuestas';

/**
 * Encuestas vigentes (Formulario, puntos 3 y 10).
 *
 * Cada una se responde una sola vez. Mientras el usuario no vota ve las opciones
 * y, apenas responde, el backend devuelve los totales y en el mismo lugar aparece
 * el gráfico con el resultado.
 */
export function Encuestas() {
    const { t } = useTranslation();
    const { idiomaActual, formatearFecha } = useLocalizacion();

    const [encuestas, setEncuestas] = useState<EncuestaResponse[]>([]);
    const [elegidas, setElegidas] = useState<Record<number, number>>({});
    const [votando, setVotando] = useState<number | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [exito, setExito] = useState<string | null>(null);
    const [cargando, setCargando] = useState(true);

    const mensajeDeError = useCallback(
        (excepcion: unknown, clave: string) => (excepcion instanceof ErrorApi ? excepcion.message : t(clave)),
        [t],
    );

    useEffect(() => {
        encuestasApi
            .listarVigentes(idiomaActual)
            .then(setEncuestas)
            .catch((excepcion: unknown) => setError(mensajeDeError(excepcion, 'encuestas.error')))
            .finally(() => setCargando(false));
    }, [idiomaActual, mensajeDeError]);

    async function responder(item: EncuestaResponse) {
        const encuestaId = item.encuesta.encuestaId;
        const opcionEncuestaId = elegidas[encuestaId];

        setError(null);
        setExito(null);

        if (!opcionEncuestaId) {
            setError(t('encuestas.sinOpcion'));

            return;
        }

        setVotando(encuestaId);

        try {
            const actualizada = await encuestasApi.responder(encuestaId, { opcionEncuestaId });

            setEncuestas((lista) =>
                lista.map((otra) => (otra.encuesta.encuestaId === encuestaId ? actualizada : otra)),
            );
            setExito(t('encuestas.exitoVotar'));
        } catch (excepcion) {
            setError(mensajeDeError(excepcion, 'encuestas.errorVotar'));
        } finally {
            setVotando(null);
        }
    }

    function datosDelGrafico(item: EncuestaResponse): DatoGrafico[] {
        return item.opciones.map((opcion) => ({
            etiqueta: opcion.texto,
            valor: opcion.total,
            destacado: opcion.opcionEncuestaId === item.opcionElegidaId,
        }));
    }

    function textoDeLaElegida(item: EncuestaResponse): string {
        const elegida = item.opciones.find((opcion) => opcion.opcionEncuestaId === item.opcionElegidaId);

        return elegida === undefined ? '' : elegida.texto;
    }

    return (
        <div className="mx-auto max-w-4xl">
            <h1 className="text-2xl font-semibold tracking-tight text-texto sm:text-3xl">{t('encuestas.titulo')}</h1>
            <p className="mt-2 text-sm text-texto-suave">{t('encuestas.descripcion')}</p>

            <div className="mt-6 flex flex-col gap-3">
                <Alerta tipo="error" mensaje={error} />
                <Alerta tipo="exito" mensaje={exito} />
            </div>

            {cargando && <p className="mt-6 text-sm text-texto-suave">{t('encuestas.cargando')}</p>}

            {!cargando && encuestas.length === 0 && (
                <p className="mt-6 rounded-2xl border border-dashed border-borde bg-superficie px-6 py-10 text-center text-sm text-texto-suave">
                    {t('encuestas.vacio')}
                </p>
            )}

            <div className="mt-6 flex flex-col gap-6">
                {encuestas.map((item) => {
                    const encuestaId = item.encuesta.encuestaId;
                    const respondida = item.opcionElegidaId !== null;

                    return (
                        <section key={encuestaId} className="rounded-2xl border border-borde bg-superficie p-6">
                            <h2 className="m-0 text-lg font-semibold text-texto">{item.encuesta.pregunta}</h2>

                            {item.encuesta.descripcion && (
                                <p className="mt-2 text-sm whitespace-pre-line text-texto-suave">
                                    {item.encuesta.descripcion}
                                </p>
                            )}

                            <p className="mt-1 text-xs text-texto-suave">
                                {t('encuestas.vence', { fecha: formatearFecha(item.encuesta.fechaVencimiento) })}
                                {' · '}
                                {t('encuestas.totalVotos', { total: item.totalRespuestas })}
                            </p>

                            {respondida ? (
                                <div className="mt-4">
                                    <p className="m-0 text-sm font-semibold text-texto">
                                        {t('encuestas.tuRespuesta', { opcion: textoDeLaElegida(item) })}
                                    </p>

                                    <div className="mt-3">
                                        <GraficoBarras
                                            datos={datosDelGrafico(item)}
                                            etiquetaSerie={t('encuestas.totalVotos', {
                                                total: item.totalRespuestas,
                                            })}
                                            mensajeVacio={t('comun.grafico.sinDatos')}
                                        />
                                    </div>
                                </div>
                            ) : (
                                <fieldset className="mt-4 flex flex-col gap-2 border-0 p-0">
                                    <legend className="sr-only">{item.encuesta.pregunta}</legend>

                                    {item.opciones.map((opcion) => (
                                        <label
                                            key={opcion.opcionEncuestaId}
                                            className="flex cursor-pointer items-center gap-2.5 rounded-lg border border-borde px-3 py-2 text-sm text-texto hover:bg-fondo"
                                        >
                                            <input
                                                type="radio"
                                                name={`encuesta-${encuestaId}`}
                                                className="size-4"
                                                checked={elegidas[encuestaId] === opcion.opcionEncuestaId}
                                                onChange={() =>
                                                    setElegidas({
                                                        ...elegidas,
                                                        [encuestaId]: opcion.opcionEncuestaId,
                                                    })
                                                }
                                            />
                                            {opcion.texto}
                                        </label>
                                    ))}

                                    <div className="mt-2">
                                        <Boton
                                            type="button"
                                            cargando={votando === encuestaId}
                                            onClick={() => void responder(item)}
                                        >
                                            {t('encuestas.votar')}
                                        </Boton>
                                    </div>
                                </fieldset>
                            )}
                        </section>
                    );
                })}
            </div>
        </div>
    );
}
