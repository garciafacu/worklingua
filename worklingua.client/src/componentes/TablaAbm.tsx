import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';

export interface ColumnaAbm<T> {
    encabezado: string;
    celda: (fila: T) => ReactNode;
    numerica?: boolean;
}

interface TablaAbmProps<T> {
    columnas: ColumnaAbm<T>[];
    filas: T[];
    claveDe: (fila: T) => number | string;
    inactiva?: (fila: T) => boolean;
    acciones?: (fila: T) => ReactNode;
    mensajeVacio: string;
}

const CLASE_CELDA = 'px-3 py-2 align-middle';

export function TablaAbm<T>({
    columnas,
    filas,
    claveDe,
    inactiva,
    acciones,
    mensajeVacio,
}: TablaAbmProps<T>) {
    const { t } = useTranslation();

    if (filas.length === 0) {
        return <p className="text-sm text-texto-suave">{mensajeVacio}</p>;
    }

    return (
        <div className="overflow-x-auto rounded-2xl border border-borde bg-superficie">
            <table className="w-full border-collapse text-sm">
                <thead>
                    <tr className="border-b border-borde text-left">
                        {columnas.map((columna) => (
                            <th
                                key={columna.encabezado}
                                scope="col"
                                className={`${CLASE_CELDA} text-xs font-semibold tracking-wide text-texto-suave uppercase ${
                                    columna.numerica ? 'text-right' : ''
                                }`}
                            >
                                {columna.encabezado}
                            </th>
                        ))}

                        {acciones && (
                            <th
                                scope="col"
                                className={`${CLASE_CELDA} text-right text-xs font-semibold tracking-wide text-texto-suave uppercase`}
                            >
                                {t('comun.tabla.acciones')}
                            </th>
                        )}
                    </tr>
                </thead>

                <tbody>
                    {filas.map((fila) => (
                        <tr
                            key={claveDe(fila)}
                            className={`border-b border-borde last:border-b-0 ${
                                inactiva?.(fila) ? 'text-texto-suave' : 'text-texto'
                            }`}
                        >
                            {columnas.map((columna) => (
                                <td
                                    key={columna.encabezado}
                                    className={`${CLASE_CELDA} ${columna.numerica ? 'text-right tabular-nums' : ''}`}
                                >
                                    {columna.celda(fila)}
                                </td>
                            ))}

                            {acciones && (
                                <td className={`${CLASE_CELDA} text-right whitespace-nowrap`}>
                                    {acciones(fila)}
                                </td>
                            )}
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
