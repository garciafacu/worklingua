import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';

interface BuscadorBarraProps {
    /** Pantalla de resultados: `/buscar` en el sitio público, `/inicio/buscar` en el privado. */
    rutaResultados: string;
    /** Distingue los `id` cuando hay dos barras en la misma página. */
    identificador: string;
    className?: string;
    alBuscar?: () => void;
}

/**
 * Campo de búsqueda compacto de los encabezados. No busca por sí mismo: lleva
 * el término a la pantalla de resultados, que es la que consulta la API.
 */
export function BuscadorBarra({ rutaResultados, identificador, className = '', alBuscar }: BuscadorBarraProps) {
    const { t } = useTranslation();
    const navegar = useNavigate();
    const [texto, setTexto] = useState('');

    function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();

        const termino = texto.trim();

        if (termino === '') {
            return;
        }

        navegar(`${rutaResultados}?${new URLSearchParams({ texto: termino }).toString()}`);
        setTexto('');
        alBuscar?.();
    }

    return (
        <form onSubmit={manejarEnvio} role="search" className={className}>
            <label htmlFor={identificador} className="sr-only">
                {t('busqueda.barra.etiqueta')}
            </label>

            <input
                id={identificador}
                type="search"
                value={texto}
                onChange={(evento) => setTexto(evento.target.value)}
                placeholder={t('busqueda.barra.placeholder')}
                maxLength={200}
                className="w-full rounded-lg border border-borde bg-superficie px-3 py-1.5 text-sm text-texto placeholder:text-texto-suave hover:bg-fondo"
            />
        </form>
    );
}
