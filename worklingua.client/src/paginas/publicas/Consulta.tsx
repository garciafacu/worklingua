import { useCallback, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Link, useSearchParams } from 'react-router-dom';
import { ErrorApi } from '../../api/clienteHttp';
import { contactoApi } from '../../api/faqsApi';
import { Alerta } from '../../componentes/Alerta';
import { HiloTicket } from '../../componentes/HiloTicket';
import type { TicketResponse } from '../../tipos/tickets';

export function Consulta() {
    const { t } = useTranslation();
    const [parametros] = useSearchParams();
    const token = parametros.get('token') ?? '';

    const [consulta, setConsulta] = useState<TicketResponse | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [cargando, setCargando] = useState(token !== '');

    const mensajeDeError = useCallback(
        (excepcion: unknown) => {
            if (excepcion instanceof ErrorApi) {
                return excepcion.estado === 404
                    ? t('publico.consulta.noEncontrada')
                    : excepcion.message;
            }

            return t('publico.consulta.error');
        },
        [t],
    );

    useEffect(() => {
        let vigente = true;

        if (token === '') {
            return;
        }

        contactoApi
            .obtenerConsulta(token)
            .then((respuesta) => {
                if (vigente) {
                    setConsulta(respuesta);
                    setError(null);
                }
            })
            .catch((excepcion) => {
                if (vigente) {
                    setError(mensajeDeError(excepcion));
                }
            })
            .finally(() => {
                if (vigente) {
                    setCargando(false);
                }
            });

        return () => {
            vigente = false;
        };
    }, [token, mensajeDeError]);

    const mensajeError = token === '' ? t('publico.consulta.sinToken') : error;

    return (
        <div className="mx-auto max-w-3xl px-4 py-12 sm:py-16">
            <h1 className="text-3xl font-semibold tracking-tight text-texto sm:text-4xl">
                {t('publico.consulta.titulo')}
            </h1>
            <p className="mt-3 text-base text-texto-suave">{t('publico.consulta.descripcion')}</p>

            {mensajeError !== null && (
                <div className="mt-6">
                    <Alerta tipo="error" mensaje={mensajeError} />
                </div>
            )}

            {cargando && (
                <p className="mt-6 text-sm text-texto-suave">{t('publico.consulta.cargando')}</p>
            )}

            {consulta !== null && (
                <section className="mt-8 rounded-2xl border border-borde bg-superficie p-6 sm:p-8">
                    <HiloTicket
                        ticket={consulta}
                        vista="cliente"
                        alEnviar={async (texto) =>
                            setConsulta(await contactoApi.responderConsulta({ token, texto }))
                        }
                    />
                </section>
            )}

            <p className="mt-8 text-sm text-texto-suave">
                <Link to="/contacto" className="font-semibold text-primario hover:underline">
                    {t('publico.consulta.volverContacto')}
                </Link>
            </p>
        </div>
    );
}
