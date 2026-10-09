import type { ReactNode, SelectHTMLAttributes } from 'react';

interface CampoSelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
    children: ReactNode;
}

/** Selector con etiqueta y mensaje de error, gemelo de CampoTexto. */
export function CampoSelect({ etiqueta, identificador, error, children, ...resto }: CampoSelectProps) {
    const idError = `${identificador}-error`;

    return (
        <div className="campo">
            <label htmlFor={identificador}>{etiqueta}</label>
            <select
                id={identificador}
                name={identificador}
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? idError : undefined}
                className={error ? 'con-error' : undefined}
                {...resto}
            >
                {children}
            </select>
            {error && (
                <span id={idError} className="campo-error">
                    {error}
                </span>
            )}
        </div>
    );
}
