import type { InputHTMLAttributes, ReactNode, Ref, SelectHTMLAttributes } from 'react';

interface CampoTextoProps extends InputHTMLAttributes<HTMLInputElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
}

export function CampoTexto({ etiqueta, identificador, error, ...resto }: CampoTextoProps) {
    const idError = `${identificador}-error`;

    return (
        <div className="campo">
            <label htmlFor={identificador}>{etiqueta}</label>
            <input
                id={identificador}
                name={identificador}
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? idError : undefined}
                className={error ? 'con-error' : undefined}
                {...resto}
            />
            {error && (
                <span id={idError} className="campo-error">
                    {error}
                </span>
            )}
        </div>
    );
}

interface CampoSelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
    children: ReactNode;
}

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

interface CampoArchivoProps extends InputHTMLAttributes<HTMLInputElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
    archivo?: string | null;
    ref?: Ref<HTMLInputElement>;
}

export function CampoArchivo({
    etiqueta,
    identificador,
    error,
    archivo,
    ...resto
}: CampoArchivoProps) {
    const idError = `${identificador}-error`;

    return (
        <div className="campo">
            <label htmlFor={identificador}>{etiqueta}</label>
            <input
                id={identificador}
                name={identificador}
                type="file"
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? idError : undefined}
                className={error ? 'con-error' : undefined}
                {...resto}
            />
            {archivo && <span className="ayuda">{archivo}</span>}
            {error && (
                <span id={idError} className="campo-error">
                    {error}
                </span>
            )}
        </div>
    );
}
